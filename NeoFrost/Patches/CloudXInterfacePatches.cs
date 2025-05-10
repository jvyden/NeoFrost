using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text.Json;
using BaseX;
using CloudX.Shared;
using HarmonyLib;
using NeoFrost.Types;
using NeoFrost.Types.LoginMethods;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
        Mappings.MapResource(ref resource);
        UniLog.Warning($"{method} {CloudXInterface.NEOS_API}/{resource}", false);
        return true;
    }

    /// <summary>
    /// Enables support for resdb links
    /// </summary>
    [HarmonyPatch(typeof(CloudXInterface), "IsValidNeosDBUri")]
    [HarmonyPrefix]
    public static bool IsValidNeosDBUriPrefix(Uri uri, ref bool __result)
    {
        if (uri.Scheme != "resdb") return true;

        __result = uri.Segments.Length >= 2;
        return false;
    }

    /// <summary>
    /// Enables support for resdb links
    /// </summary>
    [HarmonyPatch(typeof(CloudXInterface), "FilterNeosURL")]
    [HarmonyPrefix]
    public static bool FilterNeosURL(Uri assetURL, ref Uri __result)
    {
        if (assetURL.Scheme != "resdb") return true;

        if(assetURL.Segments.Length >= 2 && assetURL.Segments[1].Contains("."))
            assetURL = new Uri("resdb:///" + Path.GetFileNameWithoutExtension(assetURL.Segments[1]) + assetURL.Query);
        
        return false;
    }

    /// <summary>
    /// Enables support for retrieving assets from the resonite asset database
    /// </summary>
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

    /// <summary>
    /// Properly sets the authentication header to use the correct prefix
    /// </summary>
    [HarmonyPatch(typeof(CloudXInterface), "set_CurrentSession")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Set_CurrentSession_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction? instr in instructions)
        {
            if (instr.opcode == OpCodes.Ldstr && instr.operand is string str && str == "neos")
                yield return new CodeInstruction(OpCodes.Ldstr, "res");
            else
                yield return instr;
        }
    }
    
    private static readonly MediaTypeHeaderValue JSON_MEDIA_TYPE = new("application/json")
    {
        CharSet = "utf-8"
    };

    // private static readonly Lazy<JsonSerializerOptions> SERIALIZER_OPTIONS = new(() => new JsonSerializerOptions
    // {
    //     PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    //     Converters =
    //     {
    //         new LoginConverter()
    //     }
    // }, LazyThreadSafetyMode.None);
    
    /// <summary>
    /// Overrides the serializer with our own implementation.
    /// </summary>
    [HarmonyPatch(typeof(CloudXInterface), "AddBody")]
    [HarmonyPrefix]
    public static bool AddBodyPrefix(HttpRequestMessage message, ref object? entity)
    {
        try
        {
            Func<MemoryStream> memoryStreamAllocator = CloudXInterface.MemoryStreamAllocator;
            MemoryStream memoryStream = memoryStreamAllocator?.Invoke() ?? new MemoryStream();
            
            using (Utf8JsonWriter utf8JsonWriter = new(memoryStream))
            {
                JsonSerializer.Serialize(utf8JsonWriter, entity, entity?.GetType() ?? typeof(object), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters =
                    {
                        new LoginConverter()
                    }
                });
            }
        
            memoryStream.Seek(0L, SeekOrigin.Begin);
            message.Content = new StreamContent(memoryStream)
            {
                Headers = 
                {
                    ContentType = JSON_MEDIA_TYPE
                }
            };
        }
        catch (Exception ex)
        {
            UniLog.Error($"Exception serializing {entity?.GetType()} for request: {message.RequestUri}\n{entity}\n{ex}");
            throw;
        }
        
        return false;
    }
}