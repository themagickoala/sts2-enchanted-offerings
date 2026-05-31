using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;

namespace EnchantedOfferings;

public sealed class Roundabout : CustomEnchantmentModel
{
    public override bool HasExtraCardText => true;

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card)) return false;
        if (card.Keywords.Contains(CardKeyword.Exhaust)) return false;
        bool isZeroEnergy = !card.EnergyCost.CostsX && card.EnergyCost.GetWithModifiers(CostModifiers.None) <= 0;
        if (isZeroEnergy && card.BaseStarCost <= 0) return false;
        return true;
    }
}
