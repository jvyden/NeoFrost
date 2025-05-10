using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using BaseX;
using CloudX.Shared;
using HarmonyLib;
using NeoFrost.Types;
using NeoFrost.Types.LoginMethods;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace NeoFrost.Patches;

[HarmonyPatch(typeof(CloudXInterface))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CloudXInterfacePatches
{
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_API))]
    [HarmonyPrefix]
    public static bool ApiPrefix(ref string __result)
    {
        __result = "https://api.resonite.com";
        return false;
    }
    
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_CLOUD_BLOB))]
    [HarmonyPrefix]
    public static bool CloudBlobPrefix(ref string __result)
    {
        __result = "https://assets.resonite.com/";
        return false;
    }
    
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_ASSETS))]
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_ASSETS_CDN))]
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_ASSETS_VIDEO_CDN))]
    [HarmonyPrefix]
    public static bool AssetsPrefix(ref string __result)
    {
        __result = "https://assets.resonite.com/assets/";
        return false;
    }
    
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_ASSETS_BLOB))]
    [HarmonyPrefix]
    public static bool AssetsBlobPrefix(ref string __result)
    {
        __result = "https://skyfrostfastblob.blob.core.windows.net/assets/";
        return false;
    }
    
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_THUMBNAILS_OLD))]
    [HarmonyPrefix]
    public static bool ThumbnailsOldPrefix(ref string __result)
    {
        __result = "https://skyfrostfastblob.blob.core.windows.net/thumbnails/";
        return false;
    }
    
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_THUMBNAILS_OLD))]
    [HarmonyPrefix]
    public static bool ThumbnailsPrefix(ref string __result)
    {
        __result = "https://thumbnails.resonite.com/";
        return false;
    }

    [HarmonyPatch(typeof(CloudXInterface), "CreateRequest")]
    [HarmonyPrefix]
    public static bool CreateRequestPrefix(ref string resource, HttpMethod method)
    {
        resource = resource.Replace("api/", "");
        resource = resource.Replace("G-Neos", "G-Resonite");
        resource = resource.Replace("Neos%20Essentials", "Resonite%20Essentials");
        if (resource == "stats/onlineUserStats")
            resource = "stats/onlineStats";
        UniLog.Warning($"{method} {CloudXInterface.NEOS_API}/{resource}", false);
        return true;
    }

    [HarmonyPatch(typeof(CloudXInterface), "IsValidNeosDBUri")]
    [HarmonyPrefix]
    public static bool IsValidNeosDBUriPrefix(Uri uri, ref bool __result)
    {
        if (uri.Scheme != "resdb") return true;

        __result = uri.Segments.Length >= 2;
        return false;
    }

    [HarmonyPatch(typeof(CloudXInterface), "FilterNeosURL")]
    [HarmonyPrefix]
    public static bool FilterNeosURL(Uri assetURL, ref Uri __result)
    {
        if (assetURL.Scheme != "resdb") return true;

        if(assetURL.Segments.Length >= 2 && assetURL.Segments[1].Contains("."))
            assetURL = new Uri("resdb:///" + Path.GetFileNameWithoutExtension(assetURL.Segments[1]) + assetURL.Query);
        
        return false;
    }

    [HarmonyPatch(typeof(CloudXInterface), "NeosDBToHttp")]
    [HarmonyPrefix]
    public static bool NeosDBToHttp(Uri neosdb, NeosDB_Endpoint endpoint, ref Uri __result)
    {
        string signature = CloudXInterface.NeosDBSignature(neosdb);
        string query = CloudXInterface.NeosDBQuery(neosdb);

        if (string.IsNullOrEmpty(query))
        {
            __result = new Uri("https://assets.resonite.com/" + signature);
            return false;
        }

        __result = new Uri("https://variants.resonite.com/" + signature);
        return false;
    }
    
    private static readonly MediaTypeHeaderValue JSON_MEDIA_TYPE = new("application/json")
    {
        CharSet = "utf-8"
    };

    // private static readonly Lazy<JsonSerializerOptions> SERIALIZER_OPTIONS = new(() => new JsonSerializerOptions
    // {
    //     PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //     Converters =
    //     {
    //         new LoginConverter()
    //     }
    // }, LazyThreadSafetyMode.None);
    
    [HarmonyPatch(typeof(CloudXInterface), "AddBody")]
    [HarmonyPrefix]
    public static bool AddBodyPrefix(HttpRequestMessage message, ref object? entity)
    {
        if (entity is LoginCredentials old)
        {
            ResoniteLoginCredentials resoniteLogin = new()
            {
                OwnerId = old.OwnerId,
                Username = old.Username,
                Email = old.Email,
                RememberMe = old.RememberMe,
                SecretMachineId = old.SecretMachineId,
                MachineBound = true
            };
            
            if (!string.IsNullOrEmpty(old.SessionToken))
                resoniteLogin.Authentication = new SessionTokenLogin(old.SessionToken);
            else if (!string.IsNullOrEmpty(old.Password))
                resoniteLogin.Authentication = new PasswordLogin(old.Password);

            resoniteLogin.Preprocess();
            entity = resoniteLogin;
        }
        
        try
        {
            // message.Content = new StringContent(JsonConvert.SerializeObject(entity))
            // {
            //     Headers =
            //     {
            //         ContentType = JSON_MEDIA_TYPE
            //     }
            // };

            Func<MemoryStream> memoryStreamAllocator = CloudXInterface.MemoryStreamAllocator;
            MemoryStream memoryStream = memoryStreamAllocator?.Invoke() ?? new MemoryStream();
            
            using (Utf8JsonWriter utf8JsonWriter = new(memoryStream))
            {
                JsonSerializer.Serialize(utf8JsonWriter, entity, entity?.GetType() ?? typeof(object), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters =
                    {
                        new LoginConverter()
                    }
                });
            }
        
            memoryStream.Seek(0L, SeekOrigin.Begin);
            message.Content = new StreamContent(memoryStream)
            {
                Headers = 
                {
                    ContentType = JSON_MEDIA_TYPE
                }
            };
        }
        catch (Exception ex)
        {
            UniLog.Error($"Exception serializing {entity?.GetType()} for request: {message.RequestUri}\n{entity}\n{ex}");
            throw;
        }
        
        return false;
    }
    

    [HarmonyPatch(typeof(CloudXInterface), "AddBody")]
    [HarmonyPostfix]
    public static void AddBodyPostfix(HttpRequestMessage message)
    {
        UniLog.Log(message.Content.ReadAsStringAsync().Result);
    }
}