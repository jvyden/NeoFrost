using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using BaseX;
using CloudX.Shared;
using FrooxEngine;
using HarmonyLib;

namespace NeoFrost.Patches;

[HarmonyPatch(typeof(CloudXInterface))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CloudXInterfacePatches
{
    [HarmonyPatch(typeof(CloudXInterface), "get_" + nameof(CloudXInterface.NEOS_API))]
    [HarmonyPrefix]
    public static bool ApiPrefix(ref string __result)
    {
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
        UniLog.Warning($"{method} {CloudXInterface.NEOS_API}/{resource}", false);
        return true;
    }

    [HarmonyPatch(typeof(CloudXInterface), "IsValidNeosDBUri")]
    [HarmonyPrefix]
    public static bool IsValidNeosDBUriPrefix(Uri uri, ref bool __result)
    {
        if (uri.Scheme != "resdb") return true;

        __result = uri.Segments.Length >= 2;
        return false;
    }

    [HarmonyPatch(typeof(CloudXInterface), "FilterNeosURL")]
    [HarmonyPrefix]
    public static bool FilterNeosURL(Uri assetURL, ref Uri __result)
    {
        if (assetURL.Scheme != "resdb") return true;

        if(assetURL.Segments.Length >= 2 && assetURL.Segments[1].Contains("."))
            assetURL = new Uri("resdb:///" + Path.GetFileNameWithoutExtension(assetURL.Segments[1]) + assetURL.Query);
        
        return false;
    }

    [HarmonyPatch(typeof(CloudXInterface), "NeosDBToHttp")]
    [HarmonyPrefix]
    public static bool NeosDBToHttp(Uri neosdb, NeosDB_Endpoint endpoint, ref Uri __result)
    {
        string signature = CloudXInterface.NeosDBSignature(neosdb);
        string query = CloudXInterface.NeosDBQuery(neosdb);

        if (string.IsNullOrEmpty(query))
        {
            __result = new Uri("https://assets.resonite.com/" + signature);
            return false;
        }

        __result = new Uri("https://variants.resonite.com/" + signature);
        return false;
    }
}