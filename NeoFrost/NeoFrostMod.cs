using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using BaseX;
using CloudX.Shared;
using HarmonyLib;
using Microsoft.Win32.SafeHandles;
using NeoFrost.Patches;
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

    #region Native Windows Kernel32 Stuff

    [DllImport("kernel32.dll",
        EntryPoint = "GetStdHandle",
        SetLastError = true,
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "AllocConsole",
        SetLastError = true,
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    private static extern int AllocConsole();

    private const int STD_OUTPUT_HANDLE = -11;

    #endregion

    #region Native Windows Console Stuff

    public static void SetupNativeConsole()
    {
        AllocConsole();
        SetupNativeConsoleStream();
    }

    // Windows fuckery to redirect
    // the Console.Write output to our new console window
    public static void SetupNativeConsoleStream()
    {
        IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
        SafeFileHandle fileHandle = new(handle, true);

        FileStream fileStream = new(fileHandle, FileAccess.Write);
        Encoding encoding = Encoding.GetEncoding(Encoding.ASCII.CodePage);

        StreamWriter output = new(fileStream, encoding);
        output.AutoFlush = true;

        Console.SetOut(output);
    }

    #endregion

    public override void OnEngineInit()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, ev) =>
        {
            UniLog.Error(ev.ExceptionObject.ToString());
            Console.Out.Flush();
        };
        
        // Adapted from https://github.com/JanoschABR/NativeConsole
        #if DEBUG
        // Setup the native console
        SetupNativeConsole();

        // Add the UniLog output to the console
        UniLog.OnLog += msg => {
            Console.WriteLine($"[I] {msg}");
        };
        // UniLog.OnWarning += (msg) => {
        // Console.WriteLine($"[W] {msg}");
        // };
        UniLog.OnError += msg => {
            Console.WriteLine($"[E] {msg}");
        };
        #endif
        
        Harmony harmony = new(Name);
        harmony.PatchAll();

        CloudXInterface.UseNewtonsoftJson = false;
        #if DEBUG
        CloudXInterface.DEBUG_REQUESTS = true;
        CloudXInterface.DEBUG_UPLOAD = true;
        #endif
    }
}