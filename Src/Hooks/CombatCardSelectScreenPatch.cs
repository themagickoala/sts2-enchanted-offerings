using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

namespace EnchantedOfferings;

[HarmonyPatch(typeof(NChooseACardSelectionScreen), nameof(NChooseACardSelectionScreen.ShowScreen))]
static class CombatCardSelectScreenPatch
{
    static void Prefix(IReadOnlyList<CardModel> cards)
    {
        if (!EnchantedOfferingsSettingsMessage.Enabled) return;
        if (!EnchantedOfferingsSettingsMessage.ModifyCombatGenerated) return;
        foreach (var card in cards)
        {
            var runState = card.Owner?.RunState;
            if (card.Pile == null && runState != null)
                EnchantmentPool.TryEnchantForDisplay(card, runState);
        }
    }
}
