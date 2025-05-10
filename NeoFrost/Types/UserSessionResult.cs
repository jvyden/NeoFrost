using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudX.Shared;
using Newtonsoft.Json;

namespace NeoFrost.Types;

#nullable disable

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class UserSessionResult
{
    [JsonProperty("entity")]
    [JsonPropertyName("entity")]
    public ResoniteUserSession Entity { get; set; }
    
    [JsonProperty("configFiles")]
    [JsonPropertyName("configFiles")]
    public List<object> ConfigFiles { get; set; }
}