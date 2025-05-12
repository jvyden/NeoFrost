using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseX;
using CloudX.Shared;
using HarmonyLib;
using NeoFrost.Types;

namespace NeoFrost.Patches;

[HarmonyPatch(typeof(CloudXInterface))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class CloudXInterfaceRequestPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CloudXInterface), "GetFriends", typeof(string), typeof(DateTime?))]
    public static bool GetFriendsPrefix(ref Task<CloudResult<List<Friend>>> __result, CloudXInterface __instance, string userId, DateTime? lastStatusUpdate)
    {
        __result = GetFriends(__instance, userId, lastStatusUpdate);
        return false;
    }

    private static async Task<CloudResult<List<Friend>>> GetFriends(CloudXInterface @this, string userId, DateTime? lastStatusUpdate)
    {
        string? query = null;
        TimeSpan? timeout = null;
        if (lastStatusUpdate != null)
            query = $"?lastStatusUpdate={lastStatusUpdate.Value.ToUniversalTime():o}";
        else
            timeout = TimeSpan.FromSeconds(90.0);
        
        CloudResult<List<ResoniteContact>> result = await @this.GET<List<ResoniteContact>>($"users/{userId}/contacts{query}", timeout);
        List<Friend> data = result.Entity.Select(c => (Friend)c.ToNeos()).ToList();

        return new CloudResult<List<Friend>>(data, result.State, result.Content);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CloudXInterface), "GetMessages")]
    public static bool GetMessagesPrefix(ref Task<CloudResult<List<Message>>> __result, CloudXInterface __instance,
        DateTime? fromTime, int maxItems, string user, bool unreadOnly, TimeSpan? timeout)
    {
        __result = GetMessages(__instance, fromTime, maxItems, user, unreadOnly, timeout);
        return false;
    }

    private static async Task<CloudResult<List<Message>>> GetMessages(CloudXInterface @this, DateTime? fromTime, int maxItems, string user, bool unreadOnly, TimeSpan? timeout)
    {
        StringBuilder query = Pool.BorrowStringBuilder();
        query.Append("?maxItems=");
        query.Append(maxItems);
        if (fromTime != null)
        {
            query.Append("&fromTime=");
            query.Append(fromTime.Value.ToUniversalTime().ToString("o"));
        }
        if (!string.IsNullOrEmpty(user))
        {
            query.Append("&user=");
            query.Append(user);
        }
        if (unreadOnly)
        {
            query.Append("&unread=true");
        }
        string text = $"api/users/{@this.CurrentUser.Id}/messages{query}";
        Pool.Return(ref query);
        
        CloudResult<List<ResoniteMessage>>? result = await @this.GET<List<ResoniteMessage>>(text, timeout);
        List<Message> data = result.Entity.Select(c => (Message)c.ToNeos()).ToList();
        
        return new CloudResult<List<Message>>(data, result.State, result.Content);
    }
}