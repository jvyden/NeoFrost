using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace NeoFrost.Types.LoginMethods;

#nullable disable

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class PasswordLogin : LoginAuthentication
{
    public PasswordLogin()
    {
    }

    public PasswordLogin(string password)
    {
        Password = password;
    }

    [JsonProperty(PropertyName = "$type", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("$type")]
    public string Type { get; set; } = "password";

    [JsonProperty(PropertyName = "password", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("password")]
    public string Password { get; set; }
    
    [JsonProperty(PropertyName = "recoveryCode", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("recoveryCode")]
    public string RecoveryCode { get; set; }
}