using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Runs;

namespace EnchantedOfferings;

internal static class EnchantmentPool
{
    private enum PoolRarity { Common, Uncommon, Rare }

    private record PoolEntry(EnchantmentModel Enchantment, int Amount, PoolRarity Rarity, Func<CardModel, bool>? Filter = null);

    private static readonly Lazy<PoolEntry[]> _pool = new(BuildPool);

    // UpdateEntry() re-fires this hook on still-stocked shop slots after every purchase.
    // Track result objects by identity so we only roll once per Populate() cycle.
    private static readonly ConditionalWeakTable<CardCreationResult, object?> _seen = new();

    // Cards that went through TryEnchant (reward/shop flow) must not get a second roll
    // when TryModifyCardBeingAddedToDeck fires as the player picks/buys them.
    private static readonly ConditionalWeakTable<CardModel, object?> _processedCards = new();

    private static PoolEntry[] BuildPool() => new[]
    {
        // Common
        new PoolEntry(ModelDb.Enchantment<Nimble>(),            1, PoolRarity.Common),
        new PoolEntry(ModelDb.Enchantment<Sharp>(),             1, PoolRarity.Common),
        new PoolEntry(ModelDb.Enchantment<Swift>(),             1, PoolRarity.Common),
        new PoolEntry(ModelDb.Enchantment<Steady>(),            0, PoolRarity.Common),
        new PoolEntry(ModelDb.Enchantment<Vigorous>(),          3, PoolRarity.Common),
        new PoolEntry(ModelDb.Enchantment<Slither>(),           0, PoolRarity.Common),
        new PoolEntry(ModelDb.Enchantment<Tiny>(),              0, PoolRarity.Common),
        new PoolEntry(ModelDb.Enchantment<Wave>(),              5, PoolRarity.Common, c => c.Type == CardType.Attack),
        new PoolEntry(ModelDb.Enchantment<Piercing>(),          0, PoolRarity.Common),
        // Uncommon
        new PoolEntry(ModelDb.Enchantment<Handy>(),             5, PoolRarity.Uncommon),
        new PoolEntry(ModelDb.Enchantment<Noxious>(),           2, PoolRarity.Uncommon),
        new PoolEntry(ModelDb.Enchantment<Corrupted>(),         0, PoolRarity.Uncommon),
        new PoolEntry(ModelDb.Enchantment<Inky>(),              0, PoolRarity.Uncommon, c => c.Type == CardType.Attack),
        new PoolEntry(ModelDb.Enchantment<Sown>(),              1, PoolRarity.Uncommon),
        new PoolEntry(ModelDb.Enchantment<Nimble>(),            2, PoolRarity.Uncommon),
        new PoolEntry(ModelDb.Enchantment<Sharp>(),             2, PoolRarity.Uncommon),
        new PoolEntry(ModelDb.Enchantment<Swift>(),             2, PoolRarity.Uncommon),
        new PoolEntry(ModelDb.Enchantment<RoyallyApproved>(),   0, PoolRarity.Uncommon),
        new PoolEntry(ModelDb.Enchantment<SlumberingEssence>(), 0, PoolRarity.Uncommon),
        // Rare
        new PoolEntry(ModelDb.Enchantment<Nimble>(),            3, PoolRarity.Rare),
        new PoolEntry(ModelDb.Enchantment<Sharp>(),             3, PoolRarity.Rare),
        new PoolEntry(ModelDb.Enchantment<Swift>(),             3, PoolRarity.Rare),
        new PoolEntry(ModelDb.Enchantment<Glam>(),              0, PoolRarity.Rare),
        new PoolEntry(ModelDb.Enchantment<Goopy>(),             0, PoolRarity.Rare),
        new PoolEntry(ModelDb.Enchantment<Momentum>(),          4, PoolRarity.Rare),
        new PoolEntry(ModelDb.Enchantment<SoulsPower>(),        0, PoolRarity.Rare),
    };

    // Bias exponent: rare cards skew toward rarer enchantments.
    // Rare card + Rare enchant => bias^2; Rare card + Uncommon => bias^1; Common card => no bias.
    private static float BiasExponent(PoolRarity enchantRarity, CardRarity cardRarity)
    {
        int cardIdx = cardRarity switch
        {
            CardRarity.Uncommon => 1,
            CardRarity.Rare     => 2,
            _                   => 0,
        };
        int enchantIdx = (int)enchantRarity;
        return MathF.Max(0, enchantIdx + cardIdx - 2);
    }

    private static float WeightFor(PoolEntry entry, CardRarity cardRarity)
    {
        float baseWeight = entry.Rarity switch
        {
            PoolRarity.Common   => EnchantedOfferingsConfig.CommonWeight,
            PoolRarity.Uncommon => EnchantedOfferingsConfig.UncommonWeight,
            PoolRarity.Rare     => EnchantedOfferingsConfig.RareWeight,
            _                   => 1f,
        };
        return baseWeight * MathF.Pow(EnchantedOfferingsConfig.RarityBias, BiasExponent(entry.Rarity, cardRarity));
    }

    private static PoolEntry? TryPickEntry(CardModel card, IRunState runState)
    {
        var eligible = _pool.Value
            .Where(e => e.Enchantment.CanEnchant(card) && (e.Filter == null || e.Filter(card)))
            .ToList();

        if (eligible.Count == 0) return null;

        float totalWeight = eligible.Sum(e => WeightFor(e, card.Rarity));
        float pick = runState.Rng.Niche.NextFloat(totalWeight);

        float cumulative = 0f;
        foreach (var entry in eligible)
        {
            cumulative += WeightFor(entry, card.Rarity);
            if (pick < cumulative)
                return entry;
        }

        return null;
    }

    internal static bool TryEnchant(CardModel card, CardCreationResult result, IRunState runState)
    {
        if (!EnchantedOfferingsConfig.Enabled) return false;
        if (!_seen.TryAdd(result, null)) return false;
        _processedCards.TryAdd(card, null);
        if (runState.Rng.Niche.NextFloat(100f) >= EnchantedOfferingsConfig.ModChance) return false;

        var entry = TryPickEntry(card, runState);
        if (entry == null) return false;

        var enchanted = runState.CloneCard(card);
        CardCmd.Enchant(entry.Enchantment.ToMutable(), enchanted, entry.Amount);
        result.ModifyCard(enchanted);
        return true;
    }

    internal static bool TryEnchantInPlace(CardModel card, IRunState runState)
    {
        if (_processedCards.TryGetValue(card, out _)) return false;
        if (runState.Rng.Niche.NextFloat(100f) >= EnchantedOfferingsConfig.ModChance) return false;

        var entry = TryPickEntry(card, runState);
        if (entry == null) return false;

        CardCmd.Enchant(entry.Enchantment.ToMutable(), card, entry.Amount);
        return true;
    }

    // Used by the bundle screen Harmony patch — enchants before the player sees the card,
    // and marks as processed so TryModifyCardBeingAddedToDeck doesn't re-enchant on selection.
    internal static void TryEnchantForDisplay(CardModel card, IRunState runState)
    {
        _processedCards.TryAdd(card, null);
        if (runState.Rng.Niche.NextFloat(100f) >= EnchantedOfferingsConfig.ModChance) return;

        var entry = TryPickEntry(card, runState);
        if (entry == null) return;

        CardCmd.Enchant(entry.Enchantment.ToMutable(), card, entry.Amount);
    }
}
