using System;

namespace VentureValheim.Scaling;

public partial class ItemConfiguration : IItemConfiguration
{
    private void InitializeBaseArmorValues()
    {
        AddBaseItemValue(ItemType.Shoulder, 1f);
        AddBaseItemValue(ItemType.PrimativeArmor, 2f);
        AddBaseItemValue(ItemType.Helmet, 4f);
        AddBaseItemValue(ItemType.Chest, 4f);
        AddBaseItemValue(ItemType.Legs, 4f);
        AddBaseItemValue(ItemType.HelmetMedium, 3f);
        AddBaseItemValue(ItemType.ChestMedium, 3f);
        AddBaseItemValue(ItemType.LegsMedium, 3f);
        AddBaseItemValue(ItemType.HelmetRobe, 2f);
        AddBaseItemValue(ItemType.ChestRobe, 2f);
        AddBaseItemValue(ItemType.LegsRobe, 2f);

        AddBaseItemValue(ItemType.BucklerShield, 8f);
        AddBaseItemValue(ItemType.Shield, 10f);
        AddBaseItemValue(ItemType.TowerShield, 15f);
        AddBaseItemValue(ItemType.MagicShield, 10f);
    }

    private void InitializeArmor()
    {
        // Capes are by default all the same level
        //AddItemConfiguration("CapeDeerHide", WorldConfiguration.Biome.Meadow, ItemType.Shoulder);
        //AddItemConfiguration("CapeOdin", WorldConfiguration.Biome.Meadow, ItemType.Shoulder);
        //AddItemConfiguration("CapeTrollHide", WorldConfiguration.Biome.Meadow, ItemType.Shoulder);
        //AddItemConfiguration("CapeWolf", WorldConfiguration.Biome.Meadow, ItemType.Shoulder);
        //AddItemConfiguration("CapeLox", WorldConfiguration.Biome.Meadow, ItemType.Shoulder);
        //AddItemConfiguration("CapeLinen", WorldConfiguration.Biome.Meadow, ItemType.Shoulder);
        //AddItemConfiguration("CapeFeather", WorldConfiguration.Biome.Meadow, ItemType.Shoulder);
        //AddItemConfiguration("CapeAsh", WorldConfiguration.Biome.Meadow, ItemType.Shoulder);
        //AddItemConfiguration("CapeAsksvin", WorldConfiguration.Biome.Meadow, ItemType.Shoulder);

        AddItemConfiguration("ArmorRagsChest", WorldConfiguration.Biome.Meadow, ItemType.PrimativeArmor);
        AddItemConfiguration("ArmorRagsLegs", WorldConfiguration.Biome.Meadow, ItemType.PrimativeArmor);

        AddItemConfiguration("HelmetLeather", WorldConfiguration.Biome.Meadow, ItemType.Helmet);
        AddItemConfiguration("ArmorLeatherChest", WorldConfiguration.Biome.Meadow, ItemType.Chest);
        AddItemConfiguration("ArmorLeatherLegs", WorldConfiguration.Biome.Meadow, ItemType.Legs);

        AddItemConfiguration("HelmetBronze", WorldConfiguration.Biome.BlackForest, ItemType.Helmet);
        AddItemConfiguration("ArmorBronzeChest", WorldConfiguration.Biome.BlackForest, ItemType.Chest);
        AddItemConfiguration("ArmorBronzeLegs", WorldConfiguration.Biome.BlackForest, ItemType.Legs);

        AddItemConfiguration("HelmetTrollLeather", WorldConfiguration.Biome.BlackForest, ItemType.HelmetRobe);
        AddItemConfiguration("ArmorTrollLeatherChest", WorldConfiguration.Biome.BlackForest, ItemType.ChestRobe);
        AddItemConfiguration("ArmorTrollLeatherLegs", WorldConfiguration.Biome.BlackForest, ItemType.LegsRobe);

        AddItemConfiguration("HelmetIron", WorldConfiguration.Biome.Swamp, ItemType.Helmet);
        AddItemConfiguration("ArmorIronChest", WorldConfiguration.Biome.Swamp, ItemType.Chest);
        AddItemConfiguration("ArmorIronLegs", WorldConfiguration.Biome.Swamp, ItemType.Legs);

        AddItemConfiguration("HelmetRoot", WorldConfiguration.Biome.Swamp, ItemType.HelmetRobe);
        AddItemConfiguration("ArmorRootChest", WorldConfiguration.Biome.Swamp, ItemType.ChestRobe);
        AddItemConfiguration("ArmorRootLegs", WorldConfiguration.Biome.Swamp, ItemType.LegsRobe);

        AddItemConfiguration("HelmetFenring", WorldConfiguration.Biome.Mountain, ItemType.HelmetRobe);
        AddItemConfiguration("ArmorFenringChest", WorldConfiguration.Biome.Mountain, ItemType.ChestRobe);
        AddItemConfiguration("ArmorFenringLegs", WorldConfiguration.Biome.Mountain, ItemType.LegsRobe);

        AddItemConfiguration("HelmetDrake", WorldConfiguration.Biome.Mountain, ItemType.Helmet);
        AddItemConfiguration("ArmorWolfChest", WorldConfiguration.Biome.Mountain, ItemType.Chest);
        AddItemConfiguration("ArmorWolfLegs", WorldConfiguration.Biome.Mountain, ItemType.Legs);

        AddItemConfiguration("HelmetPadded", WorldConfiguration.Biome.Plain, ItemType.Helmet);
        AddItemConfiguration("ArmorPaddedCuirass", WorldConfiguration.Biome.Plain, ItemType.Chest);
        AddItemConfiguration("ArmorPaddedGreaves", WorldConfiguration.Biome.Plain, ItemType.Legs);

        AddItemConfiguration("HelmetMage", WorldConfiguration.Biome.Mistland, ItemType.HelmetRobe);
        AddItemConfiguration("ArmorMageChest", WorldConfiguration.Biome.Mistland, ItemType.ChestRobe);
        AddItemConfiguration("ArmorMageLegs", WorldConfiguration.Biome.Mistland, ItemType.LegsRobe);

        AddItemConfiguration("HelmetCarapace", WorldConfiguration.Biome.Mistland, ItemType.Helmet);
        AddItemConfiguration("ArmorCarapaceChest", WorldConfiguration.Biome.Mistland, ItemType.Chest);
        AddItemConfiguration("ArmorCarapaceLegs", WorldConfiguration.Biome.Mistland, ItemType.Legs);

        AddItemConfiguration("HelmetFlametal", WorldConfiguration.Biome.AshLand, ItemType.Helmet);
        AddItemConfiguration("ArmorFlametalChest", WorldConfiguration.Biome.AshLand, ItemType.Chest);
        AddItemConfiguration("ArmorFlametalLegs", WorldConfiguration.Biome.AshLand, ItemType.Legs);

        AddItemConfiguration("HelmetAshlandsMediumHood", WorldConfiguration.Biome.AshLand, ItemType.HelmetMedium);
        AddItemConfiguration("ArmorAshlandsMediumChest", WorldConfiguration.Biome.AshLand, ItemType.ChestMedium);
        AddItemConfiguration("ArmorAshlandsMediumlegs", WorldConfiguration.Biome.AshLand, ItemType.LegsMedium);

        AddItemConfiguration("HelmetMage_Ashlands", WorldConfiguration.Biome.AshLand, ItemType.HelmetRobe);
        AddItemConfiguration("ArmorMageChest_Ashlands", WorldConfiguration.Biome.AshLand, ItemType.ChestRobe);
        AddItemConfiguration("ArmorMageLegs_Ashlands", WorldConfiguration.Biome.AshLand, ItemType.LegsRobe);
    }

    private void InitializeShields()
    {
        AddItemConfiguration("ShieldBronzeBuckler", WorldConfiguration.Biome.BlackForest, ItemType.BucklerShield);
        AddItemConfiguration("ShieldIronBuckler", WorldConfiguration.Biome.Swamp, ItemType.BucklerShield);
        AddItemConfiguration("ShieldCarapaceBuckler", WorldConfiguration.Biome.Mistland, ItemType.BucklerShield);

        AddItemConfiguration("ShieldWood", WorldConfiguration.Biome.Meadow, ItemType.Shield);
        AddItemConfiguration("ShieldBanded", WorldConfiguration.Biome.Swamp, ItemType.Shield);
        AddItemConfiguration("ShieldSilver", WorldConfiguration.Biome.Swamp, ItemType.Shield);
        AddItemConfiguration("ShieldBlackmetal", WorldConfiguration.Biome.Plain, ItemType.Shield);
        AddItemConfiguration("ShieldCarapace", WorldConfiguration.Biome.Mistland, ItemType.Shield);
        AddItemConfiguration("ShieldFlametal", WorldConfiguration.Biome.AshLand, ItemType.Shield);

        AddItemConfiguration("ShieldSerpentscale", WorldConfiguration.Biome.Ocean, ItemType.TowerShield);
        AddItemConfiguration("ShieldWoodTower", WorldConfiguration.Biome.Meadow, ItemType.TowerShield);
        AddItemConfiguration("ShieldBoneTower", WorldConfiguration.Biome.BlackForest, ItemType.TowerShield);
        AddItemConfiguration("ShieldIronTower", WorldConfiguration.Biome.Swamp, ItemType.TowerShield);
        AddItemConfiguration("ShieldBlackmetalTower", WorldConfiguration.Biome.Plain, ItemType.TowerShield);
        AddItemConfiguration("ShieldFlametalTower", WorldConfiguration.Biome.AshLand, ItemType.TowerShield);

        AddItemConfiguration("StaffShield", WorldConfiguration.Biome.Mistland, ItemType.MagicShield);
    }

    /// <summary>
    /// Calculates the item upgrade values such that an item that is fully upgraded will be
    /// as strong as the next biome's natural item.
    /// </summary>
    /// <param name="biome"></param>
    /// <param name="baseValue">The item's base value</param>
    /// <param name="quality">The item's max quality, 1 indicated no upgrades for the item</param>
    /// <returns></returns>
    public float CalculateUpgradeValue(WorldConfiguration.Biome biome, float baseValue, int quality)
    {
        var scale = WorldConfiguration.Instance.GetBiomeScaling(biome);
        var nextScale = WorldConfiguration.Instance.GetNextBiomeScale(biome);

        return CalculateUpgradeValue(scale, nextScale, baseValue, quality);
    }

    /// <summary>
    /// Calcualtes the value for a single upgrade or returns 0 if invalid.
    /// </summary>
    /// <param name="scale"></param>
    /// <param name="nextScale"></param>
    /// <param name="baseValue"></param>
    /// <param name="quality"></param>
    /// <returns></returns>
    protected float CalculateUpgradeValue(float scale, float nextScale, float baseValue, int quality)
    {
        if (quality <= 1)
        {
            // This item has no upgrades
            return 0f;
        }

        var startValue = baseValue * scale;
        var endValue = baseValue * nextScale;
        var range = endValue - startValue;

        if (range > 0f)
        {
            float value = (float)Math.Round((float)range / quality, 1);
            return value;
        }

        return 0f;
    }

    /// <summary>
    /// Apply Auto-Scaling to the armor by Prefab name.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="value">The new armor value.</param>
    public void UpdateArmor(ref ItemDrop item, float? value, int? upgrades, float? upgradeValue)
    {
        // Armor
        if (value == null)
        {
            ScalingPlugin.VentureScalingLogger.LogWarning(
                $"{item.name} NOT updated with new armor value. Value undefined.");
            return;
        }

        var original = item.m_itemData.m_shared.m_armor;
        item.m_itemData.m_shared.m_armor = value.Value;

        ScalingPlugin.VentureScalingLogger.LogDebug(
            $"{item.name}: Total armor changed from {original} to {value}.");

        // Upgrades
        if (upgrades == null)
        {
            ScalingPlugin.VentureScalingLogger.LogWarning(
                $"{item.name} NOT updated with new upgrade value. Total upgrades undefined.");
            return;
        }

        if (upgradeValue == null)
        {
            ScalingPlugin.VentureScalingLogger.LogWarning(
                $"{item.name} NOT updated with new upgrade value. Value for item upgrade undefined.");
            return;
        }

        var quality = item.m_itemData.m_shared.m_maxQuality;
        var upgradeAmount = item.m_itemData.m_shared.m_armorPerLevel;
        item.m_itemData.m_shared.m_maxQuality = upgrades.Value;
        item.m_itemData.m_shared.m_armorPerLevel = upgradeValue.Value;

        ScalingPlugin.VentureScalingLogger.LogDebug(
            $"{item.name}: Total item upgrades changed from {quality} to {upgrades}. " +
            $"Total upgrade armor changed from {upgradeAmount} to {upgradeValue}.");
    }

    /// <summary>
    /// Apply Auto-Scaling to the shield by Prefab name.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="value">The new block power.</param>
    public void UpdateShield(ref ItemDrop item, float? value, int? upgrades, float? upgradeValue)
    {
        // Shield
        if (value == null)
        {
            ScalingPlugin.VentureScalingLogger.LogWarning(
                $"{item.name} NOT updated with new block value. Value undefined.");
            return;
        }

        var original = item.m_itemData.m_shared.m_blockPower;
        item.m_itemData.m_shared.m_blockPower = value.Value;

        ScalingPlugin.VentureScalingLogger.LogDebug(
            $"{item.name}: Total block changed from {original} to {value}.");

        // Upgrades
        if (upgrades == null)
        {
            ScalingPlugin.VentureScalingLogger.LogWarning(
                $"{item.name} NOT updated with new upgrade value. Total upgrades undefined.");
            return;
        }

        if (upgradeValue == null)
        {
            ScalingPlugin.VentureScalingLogger.LogWarning(
                $"{item.name} NOT updated with new upgrade value. Value for item upgrade undefined.");
            return;
        }

        var quality = item.m_itemData.m_shared.m_maxQuality;
        var upgradeAmount = item.m_itemData.m_shared.m_blockPowerPerLevel;
        item.m_itemData.m_shared.m_maxQuality = upgrades.Value;
        item.m_itemData.m_shared.m_blockPowerPerLevel = upgradeValue.Value;

        ScalingPlugin.VentureScalingLogger.LogDebug(
            $"{item.name}: Total item upgrades changed from {quality} to {upgrades}. " +
            $"Total upgrade block changed from {upgradeAmount} to {upgradeValue}.");
    }
}
