using BaseLib.Config;

namespace EnchantedOfferings;

internal class EnchantedOfferingsConfig : SimpleModConfig
{
    public static bool Enabled { get; set; } = true;

    [ConfigSlider(0, 100, 1)]
    public static float ModChance { get; set; } = 10f;

    [ConfigSlider(1, 20, 1)]
    public static float CommonWeight { get; set; } = 3f;

    [ConfigSlider(1, 20, 1)]
    public static float UncommonWeight { get; set; } = 2f;

    [ConfigSlider(1, 20, 1)]
    public static float RareWeight { get; set; } = 1f;

    [ConfigSlider(1, 20, 1)]
    public static float RarityBias { get; set; } = 1f;

    public static bool ModifyStarter { get; set; } = true;
    public static bool ModifyInstant { get; set; } = true;
    public static bool ModifyShop { get; set; } = true;
}
