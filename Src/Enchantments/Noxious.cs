using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using BaseLib.Abstracts;

namespace EnchantedOfferings;

public sealed class Noxious : CustomEnchantmentModel
{
    public override bool HasExtraCardText => true;
    public override bool ShowAmount => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new[] { new PowerVar<PoisonPower>(0m) };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromPower<PoisonPower>() };

    public override void RecalculateValues()
    {
        base.DynamicVars.Poison.BaseValue = base.Amount;
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        foreach (Creature enemy in base.Card.CombatState!.HittableEnemies)
        {
            await PowerCmd.Apply<PoisonPower>(choiceContext, enemy, base.DynamicVars.Poison.BaseValue, base.Card.Owner.Creature, base.Card);
        }
    }
}
