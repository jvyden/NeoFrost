using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FrooxEngine;
using HarmonyLib;

namespace NeoFrost.Patches;

[HarmonyPatch(typeof(GatherJob))]
public static class GatherJobPatches
{
    /// <summary>
    /// Enables support for resdb assets. 
    /// </summary>
    [HarmonyPatch(typeof(GatherJob), "StartInternal")]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> StartInternalTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new(instructions);

        for (int i = 0; i < code.Count; i++)
        {
            if (code[i].opcode != OpCodes.Ldstr || (string)code[i].operand != "neosdb") continue;
            // expecting a string equality check following, e.g. callvirt string.Equals or op_Equality

            code.InsertRange(i + 2, [
                // || scheme == "resdb"
                new CodeInstruction(OpCodes.Ldloc_S, 4),
                new CodeInstruction(OpCodes.Ldstr, "resdb"),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(string), "op_Equality")),
                new CodeInstruction(OpCodes.Or)
            ]);

            i += 3;
        }

        return code;
    }

    private static readonly MethodInfo AssetURL = AccessTools.PropertySetter(typeof(GatherJob), nameof(GatherJob.AssetURL));

    [HarmonyPatch(typeof(GatherJob), "StartInternal")]
    [HarmonyPrefix]
    public static bool StartInternalPrefix(GatherJob __instance)
    {
        if(__instance.AssetURL.AbsolutePath.EndsWith("neoshader"))
            AssetURL.Invoke(__instance, [new Uri(__instance.AssetURL, __instance.AssetURL.AbsolutePath.Replace("neoshader", "unityshader"))]);
        return true;
    }
}