using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using BaseLib.Abstracts;

namespace EnchantedOfferings;

public sealed class Cunning : CustomEnchantmentModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromKeyword(CardKeyword.Sly) };

    public override bool CanEnchant(CardModel card)
    {
        if (!base.CanEnchant(card)) return false;
        return !card.Keywords.Contains(CardKeyword.Sly);
    }

    protected override void OnEnchant()
    {
        base.Card.AddKeyword(CardKeyword.Sly);
    }
}
