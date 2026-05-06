using System.Reflection;
using BaseLib.Config;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Runs;

namespace EnchantedOfferings;

[ModInitializer("Initialize")]
public class ModEntry
{
    public static void Initialize()
    {
        new Harmony("EnchantedOfferings").PatchAll(Assembly.GetExecutingAssembly());

        ModHelper.SubscribeForRunStateHooks("EnchantedOfferings", GetRunHooks);
        ModConfigRegistry.Register("EnchantedOfferings", new EnchantedOfferingsConfig());
    }

    private static IEnumerable<AbstractModel> GetRunHooks(RunState runState)
    {
        yield return ModelDb.GetById<EnchantedOfferingsHook>(ModelDb.GetId<EnchantedOfferingsHook>());
    }
}
