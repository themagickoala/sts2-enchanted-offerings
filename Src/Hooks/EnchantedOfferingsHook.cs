using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace EnchantedOfferings;

internal class EnchantedOfferingsHook : AbstractModel
{
    // AfterMapGenerated doesn't receive IRunState, so we pull it from RunManager.
    private static readonly PropertyInfo _runManagerState =
        typeof(RunManager).GetProperty("State", BindingFlags.NonPublic | BindingFlags.Instance)!;

    public override bool ShouldReceiveCombatHooks => false;

    public override Task AfterMapGenerated(ActMap map, int actIndex)
    {
        if (!Settings.ModifyStarter || actIndex != 0) return Task.CompletedTask;

        var runState = _runManagerState.GetValue(RunManager.Instance) as IRunState;
        if (runState == null) return Task.CompletedTask;

        foreach (var player in runState.Players)
            foreach (var card in player.Deck.Cards)
                EnchantmentPool.TryEnchantInPlace(card, runState);

        return Task.CompletedTask;
    }

    public override bool TryModifyCardRewardOptionsLate(
        Player player, List<CardCreationResult> cardRewards, CardCreationOptions options)
    {
        foreach (var result in cardRewards)
            EnchantmentPool.TryEnchant(result.Card, result, player.RunState);
        return true;
    }

    public override void ModifyMerchantCardCreationResults(
        Player player, List<CardCreationResult> cards)
    {
        if (!Settings.ModifyShop) return;
        foreach (var result in cards)
            EnchantmentPool.TryEnchant(result.Card, result, player.RunState);
    }

    public override bool TryModifyCardBeingAddedToDeck(CardModel card, out CardModel? newCard)
    {
        newCard = null;
        if (!Settings.ModifyInstant) return false;
        if (!EnchantmentPool.TryEnchantInPlace(card, card.Owner!.RunState)) return false;

        newCard = card;
        return true;
    }
}
