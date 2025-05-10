using System;
using System.Text.Json.Serialization;
using CloudX.Shared;
using Newtonsoft.Json;

namespace NeoFrost.Types;

#nullable disable

[JsonObject(MemberSerialization.OptIn)]
[Serializable]
public class ResoniteUserSession
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
}