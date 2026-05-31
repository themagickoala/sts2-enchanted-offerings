using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;

namespace EnchantedOfferings;

public sealed class Spiky : CustomEnchantmentModel
{
    public override bool HasExtraCardText => true;
    public override bool ShowAmount => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new PowerVar<ThornsPower>(0m) };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<ThornsPower>(),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    };

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card)) return false;
        return !card.Keywords.Contains(CardKeyword.Exhaust);
    }

    public override void RecalculateValues()
    {
        base.DynamicVars["ThornsPower"].BaseValue = base.Amount;
    }

    protected override void OnEnchant()
    {
        base.Card.AddKeyword(CardKeyword.Exhaust);
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        await PowerCmd.Apply<ThornsPower>(choiceContext, base.Card.Owner.Creature,
            base.DynamicVars["ThornsPower"].BaseValue, base.Card.Owner.Creature, base.Card);
    }
}
