using System;
using FrooxEngine;
using HarmonyLib;

namespace NeoFrost.Patches;

#nullable disable

[HarmonyPatch(typeof(WorkerManager))]
public static class WorkerManagerPatches
{
    [HarmonyPatch(typeof(WorkerManager), nameof(WorkerManager.GetType))]
    [HarmonyPrefix]
    public static bool GetTypePrefix(string typename, ref Type __result)
    {
        if (typename == null)
        {
            __result = null;
            return false;
        }

        return true;
    }
}