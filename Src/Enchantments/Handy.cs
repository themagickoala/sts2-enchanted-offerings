using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;

namespace EnchantedOfferings;

public sealed class Handy : CustomEnchantmentModel
{
    public override bool HasExtraCardText => true;
    public override bool ShowAmount => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new SummonVar(0m) };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.Static(StaticHoverTip.SummonDynamic, base.DynamicVars.Summon) };

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card)) return false;
        return !card.EnergyCost.CostsX;
    }

    public override void RecalculateValues()
    {
        base.DynamicVars.Summon.BaseValue = base.Amount;
    }

    protected override void OnEnchant()
    {
        base.Card.EnergyCost.UpgradeBy(1);
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        await OstyCmd.Summon(choiceContext, base.Card.Owner, base.DynamicVars.Summon.BaseValue, base.Card);
    }
}
