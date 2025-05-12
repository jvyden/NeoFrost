using System;
using System.Text.Json.Serialization;
using CloudX.Shared;
using NeoFrost.Types.Conversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NeoFrost.Types;

#nullable disable

[JsonObject(MemberSerialization.OptIn)]
[Serializable]
[NeosType(typeof(Message))]
public class ResoniteMessage : IResonite
{
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public SendStatus? SendStatus { get; set; }
    
    [JsonProperty(PropertyName = "id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonProperty(PropertyName = "ownerId")]
    [JsonPropertyName("ownerId")]
    public string OwnerId { get; set; }

    [JsonProperty(PropertyName = "recipientId")]
    [JsonPropertyName("recipientId")]
    public string RecipientId { get; set; }

    [JsonProperty(PropertyName = "senderId")]
    [JsonPropertyName("senderId")]
    public string SenderId { get; set; }

    [JsonProperty(PropertyName = "senderUserSessionId")]
    [JsonPropertyName("senderUserSessionId")]
    public string SenderUserSessionId { get; set; }

    [JsonProperty(PropertyName = "messageType")]
    [JsonPropertyName("messageType")]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public ResoniteMessageType MessageType { get; set; }
    
    [JsonProperty(PropertyName = "content")]
    [JsonPropertyName("content")]
    public string Content { get; set; }
    
    [JsonProperty(PropertyName = "sendTime")]
    [JsonPropertyName("sendTime")]
    public DateTime SendTime { get; set; }

    [JsonProperty(PropertyName = "lastUpdateTime")]
    [JsonPropertyName("lastUpdateTime")]
    public DateTime LastUpdateTime { get; set; }

    [JsonProperty(PropertyName = "readTime")]
    [JsonPropertyName("readTime")]
    public DateTime? ReadTime { get; set; }

    [JsonProperty(PropertyName = "isMigrated")]
    [JsonPropertyName("isMigrated")]
    public bool IsMigrated { get; set; }

    public object ToNeos()
    {
        return new Message
        {
            Content = this.Content,
            Id = this.Id,
            LastUpdateTime = this.LastUpdateTime,
            MessageType = this.MessageType switch
            {
                ResoniteMessageType.Text => CloudX.Shared.MessageType.Text,
                ResoniteMessageType.Object => CloudX.Shared.MessageType.Object,
                ResoniteMessageType.Sound => CloudX.Shared.MessageType.Sound,
                ResoniteMessageType.SessionInvite => CloudX.Shared.MessageType.SessionInvite,
                _ => CloudX.Shared.MessageType.Text
            },
            OwnerId = this.OwnerId,
            ReadTime = this.ReadTime,
            RecipientId = this.RecipientId,
            SenderId = this.SenderId,
            SendStatus = this.SendStatus,
            SendTime = this.SendTime,
        };
    }

    public void FromNeos(object original)
    {
        if (original is not Message obj)
            throw new InvalidCastException();
        
        this.Content = obj.Content;
        this.Id = obj.Id;
        this.LastUpdateTime = obj.LastUpdateTime;
        this.MessageType = obj.MessageType switch {
            CloudX.Shared.MessageType.Text => ResoniteMessageType.Text,
            CloudX.Shared.MessageType.Object => ResoniteMessageType.Object,
            CloudX.Shared.MessageType.Sound => ResoniteMessageType.Sound,
            CloudX.Shared.MessageType.SessionInvite => ResoniteMessageType.SessionInvite,
            _ => ResoniteMessageType.Text
        };
        this.OwnerId = obj.OwnerId;
        this.ReadTime = obj.ReadTime;
        this.RecipientId = obj.RecipientId;
        this.SenderId = obj.SenderId;
        this.SendStatus = obj.SendStatus;
        this.SendTime = obj.SendTime;
    }
}