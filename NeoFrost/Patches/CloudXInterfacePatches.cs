using System;
using System.Net.Http;
using BaseX;
using CloudX.Shared;
using HarmonyLib;

namespace NeoFrost.Patches;

[HarmonyPatch(typeof(CloudXInterface))]
public class CloudXInterfacePatches
{
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_API))]
    [HarmonyPrefix]
    public static bool ApiPrefix(ref string __result)
    {
        Console.WriteLine("api");
        __result = "https://api.resonite.com";
        return false;
    }
    
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_CLOUD_BLOB))]
    [HarmonyPrefix]
    public static bool CloudBlobPrefix(ref string __result)
    {
        __result = "https://assets.resonite.com/";
        return false;
    }
    
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_ASSETS))]
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_ASSETS_CDN))]
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_ASSETS_VIDEO_CDN))]
    [HarmonyPrefix]
    public static bool AssetsPrefix(ref string __result)
    {
        __result = "https://assets.resonite.com/assets/";
        return false;
    }
    
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_ASSETS_BLOB))]
    [HarmonyPrefix]
    public static bool AssetsBlobPrefix(ref string __result)
    {
        __result = "https://skyfrostfastblob.blob.core.windows.net/assets/";
        return false;
    }
    
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_THUMBNAILS_OLD))]
    [HarmonyPrefix]
    public static bool ThumbnailsOldPrefix(ref string __result)
    {
        __result = "https://skyfrostfastblob.blob.core.windows.net/thumbnails/";
        return false;
    }
    
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_THUMBNAILS_OLD))]
    [HarmonyPrefix]
    public static bool ThumbnailsPrefix(ref string __result)
    {
        __result = "https://thumbnails.resonite.com/";
        return false;
    }

    [HarmonyPatch(typeof(CloudXInterface), "CreateRequest")]
    [HarmonyPrefix]
    public static bool CreateRequestPrefix(ref string resource, HttpMethod method)
    {
        resource = resource.Replace("api/", "");
        resource = resource.Replace("G-Neos", "G-Resonite");
        if (resource == "stats/onlineUserStats")
            resource = "stats/onlineStats";
        UniLog.Warning($"{method} {CloudXInterface.NEOS_API}/{resource}", true);
        return true;
    }
}