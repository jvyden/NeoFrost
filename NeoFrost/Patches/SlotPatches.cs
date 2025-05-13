using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using FrooxEngine;
using HarmonyLib;
using NeoFrost.Load;

namespace NeoFrost.Patches;

[HarmonyPatch(typeof(Slot))]
public static class SlotPatches
{
    private static readonly MethodInfo LoadTypeVersions = AccessTools.Method(typeof(LoadControl), nameof(LoadControl.LoadTypeVersions));
    private static readonly MethodInfo LoadObjectExtrasMethod = AccessTools.Method(typeof(SlotLoader), nameof(SlotLoader.LoadObjectExtras));
    
    [HarmonyPatch(typeof(Slot), nameof(Slot.LoadObject))]
    [HarmonyTranspiler, HarmonyDebug]
    public static IEnumerable<CodeInstruction> LoadObjectTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        bool found = false;
     
        List<CodeInstruction> code = new(instructions);
        for (int i = 0; i < code.Count; i++)
        {
            if (!found && code[i].Calls(LoadTypeVersions))
            {
                found = true;
                
                yield return code[i];

                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Call, LoadObjectExtrasMethod);
                continue;
            }

            yield return code[i];
        }
    }
}