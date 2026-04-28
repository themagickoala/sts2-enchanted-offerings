namespace EnchantedOfferings;

internal static class Settings
{
    internal static bool  Enabled        = true;
    internal static float ModChance      = 10f;
    internal static float CommonWeight   = 3f;
    internal static float UncommonWeight = 2f;
    internal static float RareWeight     = 1f;
    internal static float RarityBias     = 1f;
    internal static bool  ModifyStarter  = true;
    internal static bool  ModifyInstant  = true;
    internal static bool  ModifyShop     = true;

    internal static void Set(string key, float value)
    {
        switch (key)
        {
            case "commonWeight":   CommonWeight   = value; break;
            case "uncommonWeight": UncommonWeight = value; break;
            case "rareWeight":     RareWeight     = value; break;
            case "rarityBias":     RarityBias     = value; break;
        }
    }

    internal static void Set(string key, bool value)
    {
        switch (key)
        {
            case "modifyStarter": ModifyStarter = value; break;
            case "modifyInstant": ModifyInstant = value; break;
            case "modifyShop":    ModifyShop    = value; break;
        }
    }
}
