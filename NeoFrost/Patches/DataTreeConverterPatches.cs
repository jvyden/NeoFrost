using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using BaseX;
using BrotliSharpLib;
using HarmonyLib;
using Newtonsoft.Json.Bson;

namespace NeoFrost.Patches;

[HarmonyPatch(typeof(DataTreeConverter))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class DataTreeConverterPatches
{
    [HarmonyPatch(typeof(DataTreeConverter), "Load", typeof(string), typeof(string))]
    // [HarmonyPrefix]
    public static bool LoadPrefix(string file, string ext, ref DataTreeDictionary __result)
    {
        // ReSharper disable once RedundantAssignment
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        ext ??= Path.GetExtension(file).ToLower().Replace(".", "");

        if (ext != "brson")
            return true;

        using FileStream fs = File.OpenRead(file);
        __result = FromBrotli(fs);
        return false;
    }

    private static readonly MethodInfo DataTreeConverter_Read = AccessTools.Method(typeof(DataTreeConverter), "Read");
    private static DataTreeDictionary FromBrotli(FileStream stream)
    {
        using BrotliStream brotliStream = new(stream, CompressionMode.Decompress, true);
        using BsonDataReader bsonDataReader = new(brotliStream);

        try
        {
            DataTreeDictionary dataTreeDictionary =
                (DataTreeDictionary)DataTreeConverter_Read.Invoke(null, [bsonDataReader]);
            return dataTreeDictionary;
        }
        catch (Exception e)
        {
            UniLog.Error(e.ToString(), false);
            return null!;
        }
    }
}