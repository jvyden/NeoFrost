using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
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
        
        CloudResult<List<ResoniteContact>> contacts = await @this.GET<List<ResoniteContact>>($"users/{userId}/contacts{query}", timeout);
        List<Friend> friends = contacts.Entity.Select(c => (Friend)c.ToNeos()).ToList();

        return new CloudResult<List<Friend>>(friends, contacts.State, contacts.Content);
    }
}