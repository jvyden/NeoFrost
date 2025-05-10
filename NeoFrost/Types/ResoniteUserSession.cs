using System;
using System.Text.Json.Serialization;
using CloudX.Shared;
using NeoFrost.Types.Conversion;
using Newtonsoft.Json;

namespace NeoFrost.Types;

#nullable disable

[JsonObject(MemberSerialization.OptIn)]
[Serializable]
public class ResoniteUserSession : IResonite
{
    [JsonProperty("userId")]
    [JsonPropertyName("userId")]
    public string UserId { get; set; }
    
    [JsonProperty("token")]
    [JsonPropertyName("token")]
    public string SessionToken { get; set; }
    
    [JsonProperty("created")]
    [JsonPropertyName("created")]
    public DateTime SessionCreated { get; set; }
    
    [JsonProperty("expire")]
    [JsonPropertyName("expire")]
    public DateTime SessionExpire { get; set; }
    
    // [JsonPropertyName("secretMachineIdHash")]
    // public string SecretMachineIdHash { get; set; }
    
    [JsonProperty("rememberMe")]
    [JsonPropertyName("rememberMe")]
    public bool RememberMe { get; set; }

    public object ToNeos()
    {
        return new UserSession
        {
            UserId = this.UserId,
            RememberMe = this.RememberMe,
            // SecretMachineId = secretMachineId,
            SessionToken = this.SessionToken,
            SessionCreated = this.SessionCreated,
            SessionExpire = this.SessionExpire
        };
    }

    public void FromNeos(object original)
    {
        if (original is not UserSession obj)
            throw new InvalidCastException();
        
        this.UserId = obj.UserId;
        this.RememberMe = obj.RememberMe;
        this.SessionToken = obj.SessionToken;
        this.SessionCreated = obj.SessionCreated;
        this.SessionExpire = obj.SessionExpire;
    }

    public Type NeosType => typeof(UserSession);
}