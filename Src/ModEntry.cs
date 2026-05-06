using System.Reflection;
using System.Runtime.InteropServices;
using BaseLib.Config;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Runs;

namespace EnchantedOfferings;

[ModInitializer("Initialize")]
public class ModEntry
{
    [DllImport("libdl.so.2", EntryPoint = "dlopen")]
    private static extern IntPtr Dlopen(string filename, int flags);

    private const int RTLD_NOW = 2;
    private const int RTLD_GLOBAL = 256;

    public static void Initialize()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            if (Dlopen("libgcc_s.so.1", RTLD_NOW | RTLD_GLOBAL) == IntPtr.Zero)
                GD.PrintErr("[EnchantedOfferings] Warning: could not preload libgcc_s.so.1 — Harmony patches may fail");

        new Harmony("EnchantedOfferings").PatchAll(Assembly.GetExecutingAssembly());

        ModHelper.SubscribeForRunStateHooks("EnchantedOfferings", GetRunHooks);
        ModConfigRegistry.Register("EnchantedOfferings", new EnchantedOfferingsConfig());
    }

    private static IEnumerable<AbstractModel> GetRunHooks(RunState runState)
    {
        yield return ModelDb.GetById<EnchantedOfferingsHook>(ModelDb.GetId<EnchantedOfferingsHook>());
    }
}
