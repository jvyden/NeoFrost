using System;
using System.Linq;
using BaseX;
using HarmonyLib;
using NeoFrost.Extensions;
using NeoFrost.Load;

namespace NeoFrost.Patches;

#nullable disable

[HarmonyPatch("WorkerSaveLoad", "ExtractWorker")]
public static class WorkerSaveLoadPatches
{
    private static readonly Type WorkerDataType =
        AccessTools.TypeByName("WorkerSaveLoad").GetNestedType("WorkerData", AccessTools.all);
    
    [HarmonyPrefix]
    public static bool Prefix(DataTreeNode node, ref object __result)
    {
        DataTreeDictionary dict = (DataTreeDictionary)node;
        DataTreeValue dataVal = (DataTreeValue)dict["Type"];
        if (dataVal.Value is long val)
        {
            SlotExtraInfo info = SlotLoader.SlotStore.First(s => s.Node == node || s.Node.Contains(node));
            object workerData = Activator.CreateInstance(WorkerDataType, info.Types[(int)val].TypeName, dict["Data"]);
            __result = workerData;
            return false;
        }
        return true;
    }
}