using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CloudX.Shared;
using NeoFrost.Types.Conversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NeoFrost.Types;

#nullable disable

[JsonObject(MemberSerialization.OptIn)]
[Serializable]
[NeosType(typeof(Friend))]
public class ResoniteContact : IResonite
{
    [JsonProperty(PropertyName = "id")]
    [JsonPropertyName("id")]
    public string ContactUserId { get; set; }

    [JsonProperty(PropertyName = "ownerId")]
    [JsonPropertyName("ownerId")]
    public string OwnerId { get; set; }

    [JsonProperty(PropertyName = "contactUsername")]
    [JsonPropertyName("contactUsername")]
    public string ContactUsername { get; set; }

    [JsonProperty(PropertyName = "alternateUsernames")]
    [JsonPropertyName("alternateUsernames")]
    public List<string> AlternateUsernames { get; set; }

    [JsonProperty(PropertyName = "contactStatus")]
    [JsonPropertyName("contactStatus")]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public FriendStatus ContactStatus { get; set; }

    [JsonProperty(PropertyName = "isAccepted")]
    [JsonPropertyName("isAccepted")]
    public bool IsAccepted { get; set; }

    [JsonProperty(PropertyName = "latestMessageTime")]
    [JsonPropertyName("latestMessageTime")]
    public DateTime LatestMessageTime { get; set; }

    [JsonProperty(PropertyName = "profile")]
    [JsonPropertyName("profile")]
    public UserProfile Profile { get; set; }

    [JsonProperty(PropertyName = "isMigrated")]
    [JsonPropertyName("isMigrated")]
    public bool IsMigrated { get; set; }

    [JsonProperty(PropertyName = "isCounterpartMigrated")]
    [JsonPropertyName("isCounterpartMigrated")]
    public bool IsCounterpartMigrated { get; set; }

    public object ToNeos()
    {
        return new Friend
        {
            AlternateUsernames = AlternateUsernames,
            FriendStatus = ContactStatus,
            FriendUserId = ContactUserId,
            FriendUsername = ContactUsername,
            IsAccepted = IsAccepted,
            LatestMessageTime = LatestMessageTime,
            OwnerId = OwnerId,
            Profile = Profile,
            UserStatus = null,
        };
    }

    public void FromNeos(object original)
    {
        if (original is not Friend obj)
            throw new InvalidCastException();

        this.AlternateUsernames = obj.AlternateUsernames;
        this.ContactStatus = obj.FriendStatus;
        this.ContactUserId = obj.FriendUserId;
        this.ContactUsername = obj.FriendUsername;
        this.IsAccepted = obj.IsAccepted;
        this.LatestMessageTime = obj.LatestMessageTime;
        this.OwnerId = obj.OwnerId;
        this.Profile = obj.Profile;
    }
}