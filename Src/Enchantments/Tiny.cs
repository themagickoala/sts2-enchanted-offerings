using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace EnchantedOfferings;

public sealed class Tiny : CustomEnchantmentModel
{
    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card)) return false;
        if (card.EnergyCost.CostsX) return false;
        if (card.EnergyCost.GetWithModifiers(CostModifiers.None) < 1) return false;
        return card.DynamicVars.Values.Any(v => v is DamageVar or BlockVar);
    }

    protected override void OnEnchant()
    {
        base.Card.EnergyCost.UpgradeBy(-1);
    }

    public override decimal EnchantDamageMultiplicative(decimal originalDamage, ValueProp props)
    {
        if (!props.IsPoweredAttack()) return 1m;
        return 0.75m;
    }

    public override decimal EnchantBlockMultiplicative(decimal originalBlock, ValueProp props)
    {
        if (!props.IsPoweredCardOrMonsterMoveBlock()) return 1m;
        return 0.75m;
    }
}
