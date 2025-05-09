using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using BaseX;
using Brotli;
using HarmonyLib;
using Newtonsoft.Json.Bson;

namespace NeoFrost.Patches;

[HarmonyPatch(typeof(DataTreeConverter))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class DataTreeConverterPatches
{
    [HarmonyPatch(typeof(DataTreeConverter), "Load", typeof(string), typeof(string))]
    [HarmonyPrefix]
    public static bool LoadPrefix(string file, string ext, ref DataTreeDictionary __result)
    {
        // ReSharper disable once RedundantAssignment
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        ext ??= Path.GetExtension(file).ToLower().Replace(".", "");

        if (ext != "brson")
            return true;

        using FileStream fs = File.OpenRead(file);
        bool success = TryReadHeader(fs, out int version, out ulong compression);
        if(success)
            UniLog.Log($"Reading brotli asset: version={version},compression={compression}");

        __result = FromBrotli(fs);
        return false;
    }

    private static readonly MethodInfo DataTreeConverter_Read = AccessTools.Method(typeof(DataTreeConverter), "Read");

    private static void DecompressFromBrotli(Stream inStream, Stream destStream)
    {
        using BrotliStream brotliStream = new(inStream, CompressionMode.Decompress, true);
        brotliStream.CopyTo(destStream);
        destStream.Flush();
    }
    
    private static bool TryReadHeader(Stream stream, out int version, out ulong compression)
    {
        using BinaryReader binaryReader = new(stream, Encoding.UTF8, true);
        for (int i = 0; i < "FrDT".Length; i++)
        {
            if (binaryReader.ReadByte() == (byte)"FrDT"[i]) continue;

            version = -1;
            compression = 0;
            return false;
        }
        
        version = binaryReader.ReadInt32();
        compression = binaryReader.Read7BitEncoded();
        
        return true;
    }
    
    private static DataTreeDictionary FromBrotli(FileStream stream)
    {
        UniLog.Log("MAKING BROTLI STREAM");
        
        using MemoryStream ms = new();
        DecompressFromBrotli(stream, ms);
        ms.Seek(0, SeekOrigin.Begin);
        
        UniLog.Log("MAKING BSON READER");
        
        using BsonDataReader bsonDataReader = new(ms);
        bsonDataReader.CloseInput = false;

        UniLog.Log("DECOMPRESSING BROTLI STREAM");
        DataTreeDictionary dataTreeDictionary = (DataTreeDictionary)DataTreeConverter_Read.Invoke(null, [bsonDataReader]);
        UniLog.Log("DECOMPRESSED BROTLI STREAM");
        return dataTreeDictionary;
    }
}