using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BaseX;
using CloudX.Shared;
using FrooxEngine;
using HarmonyLib;
using NeoFrost.Extensions;
using NeoFrost.Types;
using NeoFrost.Types.LoginMethods;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
// using Newtonsoft.Json;
using User = CloudX.Shared.User;

namespace NeoFrost.Patches;

[HarmonyPatch(typeof(CloudXInterface))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class CloudXInterfaceLoginPatch
{
    /// <summary>
    /// Rewrite of the login function to use the new login scheme.
    /// This is far from trivial to do so with a transpiler as many generics and async calls are here at play.
    /// </summary>
    private static async Task<CloudResult<UserSession>> LoginAsync(string credential, string password, string sessionToken, string secretMachineId, bool rememberMe, string recoverCode, string totp)
    {
        CloudXInterface @this = Engine.Current.Cloud;
        await @this.Logout(false);

        ResoniteLoginCredentials credentials = new()
        {
            MachineBound = false,
            SecretMachineId = secretMachineId,
            RememberMe = rememberMe,
        };

        if (credential.StartsWith("U-"))
            credentials.OwnerId = credential;
        else if (credential.Contains('@'))
            credentials.Email = credential;
        else
            credentials.Username = credential;

        if (!string.IsNullOrEmpty(password))
            credentials.Authentication = new PasswordLogin(password);
        else if (!string.IsNullOrEmpty(sessionToken))
            credentials.Authentication = new SessionTokenLogin(sessionToken);
        
        Task<CryptoData> cryptoTask = Task.Run(delegate
        {
            RSACryptoServiceProvider provider = new(2048);
            RSAParameters parameters = provider.ExportParameters(false);
            return new CryptoData(provider, parameters);
        });

        CloudResult resultStr = await @this.POST("userSessions", credentials, null, totp);
        UniLog.Log(resultStr.Content);

        // UserSessionResult obj = JsonSerializer.Deserialize<UserSessionResult>(resultStr.Content)!;
        // UniLog.Log("RESERIALIZE (system): " + JsonSerializer.Serialize(obj));
        // for some stupid fucking reason only newtonsoft can handle this shit but system can only handle others i can't even
        UserSessionResult obj = JsonConvert.DeserializeObject<UserSessionResult>(resultStr.Content)!;
        // UniLog.Log("RESERIALIZE (newtonsoft): " + JsonConvert.SerializeObject(obj));

        CloudResult<UserSessionResult> result = new(obj, resultStr.State, resultStr.Content);

        ResoniteUserSession resoSession = result.Entity.Entity;
        UserSession session = new()
        {
            UserId = resoSession.UserId,
            RememberMe = resoSession.RememberMe,
            SecretMachineId = secretMachineId,
            SessionToken = resoSession.SessionToken,
            SessionCreated = resoSession.SessionCreated,
            SessionExpire = resoSession.SessionExpire
        };

        if (result.IsOK)
        {
            CryptoData crypto = cryptoTask.Result;
            
            CryptoProvider.SetValue(@this, crypto.Provider);
            PublicKey.Invoke(@this, [crypto.Parameters]);
            CurrentSession.Invoke(@this, [session]);

            @this.CurrentUser = new User
            {
                Id = @this.CurrentSession.UserId,
                Email = credentials.Email,
                Username = credentials.Username
            };

            await ((Task)ConnectToHub.Invoke(@this, [])).ConfigureAwait(false);
            await @this.UpdateCurrentUserInfo();
            await @this.UpdateCurrentUserMemberships();
            FriendsUpdate.Invoke(@this.Friends, []);
            OnLogin.Invoke(@this, []);
        }
        else if (result.Content != "TOTP")
        {
            UniLog.Warning($"Error logging in: {result.State}\n{result.Content}");
            return new CloudResult<UserSession>(null!, result.State, result.Content);
        }

        return new CloudResult<UserSession>(session, result.State, result.Content);
    }

    private static readonly FieldInfo CryptoProvider = AccessTools.Field(typeof(CloudXInterface), "_cryptoProvider");
    private static readonly MethodInfo PublicKey = AccessTools.PropertySetter(typeof(CloudXInterface), "PublicKey");
    private static readonly MethodInfo CurrentSession = AccessTools.PropertySetter(typeof(CloudXInterface), "CurrentSession");

    private static readonly MethodInfo ConnectToHub = AccessTools.Method(typeof(CloudXInterface), "ConnectToHub");
    private static readonly MethodInfo OnLogin = AccessTools.Method(typeof(CloudXInterface), "OnLogin");
    private static readonly MethodInfo FriendsUpdate = AccessTools.Method(typeof(FriendManager), "Update");
    
    private readonly struct CryptoData(RSACryptoServiceProvider provider, RSAParameters parameters)
    {
        public readonly RSACryptoServiceProvider Provider = provider;
        public readonly RSAParameters Parameters = parameters;
    }
    
    [HarmonyPatch(typeof(CloudXInterface), "Login")]
    [HarmonyPrefix]
    public static bool LoginPrefix(string credential, string password, string sessionToken, string secretMachineId, bool rememberMe, string recoverCode, string totp, ref Task<CloudResult<UserSession>> __result)
    {
        __result = LoginAsync(credential, password, sessionToken, secretMachineId, rememberMe, recoverCode, totp);
        return false;
    }
}