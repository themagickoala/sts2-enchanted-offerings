using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

namespace EnchantedOfferings;

[HarmonyPatch(typeof(NChooseABundleSelectionScreen), nameof(NChooseABundleSelectionScreen.ShowScreen))]
static class BundleScreenPatch
{
    static void Prefix(IReadOnlyList<IReadOnlyList<CardModel>> bundles)
    {
        if (!EnchantedOfferingsConfig.Enabled || !EnchantedOfferingsConfig.ModifyInstant) return;
        foreach (var bundle in bundles)
            foreach (var card in bundle)
            {
                var runState = card.Owner?.RunState;
                if (runState != null)
                    EnchantmentPool.TryEnchantForDisplay(card, runState);
            }
    }
}
