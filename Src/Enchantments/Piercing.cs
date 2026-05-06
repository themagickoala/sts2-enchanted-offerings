using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using BaseLib.Abstracts;

namespace EnchantedOfferings;

public sealed class Piercing : CustomEnchantmentModel
{
    public override bool HasExtraCardText => true;

    // DamageProps has a private setter — set the backing field directly
    private static readonly FieldInfo _damagePropsField =
        typeof(AttackCommand).GetField("<DamageProps>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!;

    public override bool CanEnchantCardType(CardType cardType) =>
        cardType == CardType.Attack;

    public override Task BeforeAttack(AttackCommand command)
    {
        if (command.ModelSource == base.Card)
        {
            var current = (ValueProp)_damagePropsField.GetValue(command)!;
            _damagePropsField.SetValue(command, current | ValueProp.Unblockable);
        }
        return Task.CompletedTask;
    }
}
