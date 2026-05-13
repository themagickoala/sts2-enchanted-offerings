using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace EnchantedOfferings;

internal class EnchantedOfferingsSettingsMessage : ICustomMessage
{
    // Effective settings for the current run — read by pool and hooks.
    // Defaults match config defaults; overwritten by SyncFromConfig() on host/single
    // and by HandleMessage() on clients.
    public static bool Enabled = true;
    public static float ModChance = 10f;
    public static float CommonWeight = 3f;
    public static float UncommonWeight = 2f;
    public static float RareWeight = 1f;
    public static float RarityBias = 1f;
    public static bool ModifyStarter = true;
    public static bool ModifyShop = true;
    public static bool ModifyInstant = true;

    // Message payload fields
    public bool MsgEnabled;
    public float MsgModChance;
    public float MsgCommonWeight;
    public float MsgUncommonWeight;
    public float MsgRareWeight;
    public float MsgRarityBias;
    public bool MsgModifyStarter;
    public bool MsgModifyShop;
    public bool MsgModifyInstant;

    public bool ShouldBroadcast => true;

    public static void SyncFromConfig()
    {
        Enabled = EnchantedOfferingsConfig.Enabled;
        ModChance = EnchantedOfferingsConfig.ModChance;
        CommonWeight = EnchantedOfferingsConfig.CommonWeight;
        UncommonWeight = EnchantedOfferingsConfig.UncommonWeight;
        RareWeight = EnchantedOfferingsConfig.RareWeight;
        RarityBias = EnchantedOfferingsConfig.RarityBias;
        ModifyStarter = EnchantedOfferingsConfig.ModifyStarter;
        ModifyShop = EnchantedOfferingsConfig.ModifyShop;
        ModifyInstant = EnchantedOfferingsConfig.ModifyInstant;
    }

    public static EnchantedOfferingsSettingsMessage FromConfig() => new()
    {
        MsgEnabled = EnchantedOfferingsConfig.Enabled,
        MsgModChance = EnchantedOfferingsConfig.ModChance,
        MsgCommonWeight = EnchantedOfferingsConfig.CommonWeight,
        MsgUncommonWeight = EnchantedOfferingsConfig.UncommonWeight,
        MsgRareWeight = EnchantedOfferingsConfig.RareWeight,
        MsgRarityBias = EnchantedOfferingsConfig.RarityBias,
        MsgModifyStarter = EnchantedOfferingsConfig.ModifyStarter,
        MsgModifyShop = EnchantedOfferingsConfig.ModifyShop,
        MsgModifyInstant = EnchantedOfferingsConfig.ModifyInstant,
    };

    public void HandleMessage(ulong senderId)
    {
        Enabled = MsgEnabled;
        ModChance = MsgModChance;
        CommonWeight = MsgCommonWeight;
        UncommonWeight = MsgUncommonWeight;
        RareWeight = MsgRareWeight;
        RarityBias = MsgRarityBias;
        ModifyStarter = MsgModifyStarter;
        ModifyShop = MsgModifyShop;
        ModifyInstant = MsgModifyInstant;
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBool(MsgEnabled);
        writer.WriteFloat(MsgModChance);
        writer.WriteFloat(MsgCommonWeight);
        writer.WriteFloat(MsgUncommonWeight);
        writer.WriteFloat(MsgRareWeight);
        writer.WriteFloat(MsgRarityBias);
        writer.WriteBool(MsgModifyStarter);
        writer.WriteBool(MsgModifyShop);
        writer.WriteBool(MsgModifyInstant);
    }

    public void Deserialize(PacketReader reader)
    {
        MsgEnabled = reader.ReadBool();
        MsgModChance = reader.ReadFloat();
        MsgCommonWeight = reader.ReadFloat();
        MsgUncommonWeight = reader.ReadFloat();
        MsgRareWeight = reader.ReadFloat();
        MsgRarityBias = reader.ReadFloat();
        MsgModifyStarter = reader.ReadBool();
        MsgModifyShop = reader.ReadBool();
        MsgModifyInstant = reader.ReadBool();
    }
}
