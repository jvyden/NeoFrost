using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using NeoFrost.Types.LoginMethods;
using Newtonsoft.Json;

namespace NeoFrost.Types;

#nullable disable

// [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[Serializable]
public class ResoniteLoginCredentials
{
    [JsonProperty("ownerId")]
    [JsonPropertyName("ownerId")]
    public string OwnerId { get; set; }
    
    [JsonPropertyName("username")]
    [JsonProperty("username")]
    public string Username { get; set; }
    
    [JsonPropertyName("email")]
    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonPropertyName("authentication")]
    [JsonProperty("authentication")]
    public LoginAuthentication Authentication { get; set; }
    
    [JsonPropertyName("secretMachineId")]
    [JsonProperty("secretMachineId")]
    public string SecretMachineId { get; set; }
    
    [JsonPropertyName("rememberMe")]
    [JsonProperty("rememberMe")]
    public bool RememberMe { get; set; }
    
    [JsonPropertyName("machineBound")]
    [JsonProperty("machineBound")]
    public bool MachineBound { get; set; }

    public void Preprocess()
    {
        this.Username = this.Username?.Trim();
        this.Email = this.Email?.Trim()?.ToLower();
    }
}