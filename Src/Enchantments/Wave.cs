using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace EnchantedOfferings;

public sealed class Wave : EnchantmentModel
{
    public override bool HasExtraCardText => true;
    public override bool ShowAmount => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new BlockVar(0m, ValueProp.Move) };

    public override bool CanEnchantCardType(CardType cardType) =>
        cardType == CardType.Attack;

    public override void RecalculateValues()
    {
        base.DynamicVars.Block.BaseValue = base.Amount;
    }

    public override decimal EnchantDamageMultiplicative(decimal originalDamage, ValueProp props)
    {
        if (!props.IsPoweredAttack()) return 1m;
        return 0.75m;
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        await CreatureCmd.GainBlock(base.Card.Owner.Creature, base.DynamicVars.Block, cardPlay);
    }
}
