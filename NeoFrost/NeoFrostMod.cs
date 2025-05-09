using HarmonyLib;
using NeosModLoader;

namespace NeoFrost;

public class NeoFrostMod : NeosMod
{
    static NeoFrostMod()
    {
        Harmony.DEBUG = true;
    }
    
    public override string Name => "NeoFrost";
    public override string Author => "jvyden";
    public override string Version => "1.0.0";

    public override void OnEngineInit()
    {
        Harmony harmony = new(Name);
        harmony.PatchAll();
    }
}