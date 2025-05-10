using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace NeoFrost.Types.LoginMethods;

#nullable disable

// [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
[Serializable]
public class SessionTokenLogin : LoginAuthentication
{
    public SessionTokenLogin()
    {
    }

    public SessionTokenLogin(string sessionToken)
    {
        SessionToken = sessionToken;
    }
    
    [JsonProperty(PropertyName = "$type", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("$type")]
    public string Type { get; set; } = "sessionToken";

    [JsonProperty(PropertyName = "sessionToken", NullValueHandling = NullValueHandling.Ignore)]
    [JsonPropertyName("sessionToken")]
    public string SessionToken { get; set; }
}