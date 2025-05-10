using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CloudX.Shared;
using HarmonyLib;

namespace NeoFrost.Patches;

[HarmonyPatch]
public static class CloudXInterfaceHubConnectPatch
{
    public static MethodBase TargetMethod()
    {
        Type? nestedType = typeof(CloudXInterface).GetNestedType("<>c__DisplayClass175_0", BindingFlags.NonPublic);
        return nestedType!.GetMethod("<ConnectToHub>b__0", BindingFlags.NonPublic | BindingFlags.Instance)!;
    }
    
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction? instr in instructions)
        {
            if (instr.opcode == OpCodes.Ldstr && instr.operand is string str && str == "neos ")
                yield return new CodeInstruction(OpCodes.Ldstr, "res ");
            else
                yield return instr;
        }
    }
}