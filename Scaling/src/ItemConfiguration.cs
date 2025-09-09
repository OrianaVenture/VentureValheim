using System;
using System.Collections.Generic;
using BepInEx;

namespace VentureValheim.Scaling;

public interface IItemConfiguration
{
    public float GetTotalDamage(HitData.DamageTypes OriginalDamage);
    public HitData.DamageTypes CalculateCreatureDamageTypes(
        float biomeScale, HitData.DamageTypes OriginalDamage,
        float baseTotalDamage, float maxTotalDamage);
    public HitData.DamageTypes CalculateItemDamageTypes(float biomeScale,
        HitData.DamageTypes originalDamage, float baseTotalDamage);
    public void UpdateWeapon(ref ItemDrop item, HitData.DamageTypes? value, 
        int? upgrades, HitData.DamageTypes? upgradeValue, bool playerItem = true);
    public void UpdateArmor(ref ItemDrop item, float? value, int? upgrades, float? upgradeValue);
    public void UpdateShield(ref ItemDrop item, float? value, int? upgrades, float? upgradeValue);
    public void UpdateItems();
    public void VanillaReset();
}

public partial class ItemConfiguration : IItemConfiguration
{
    static ItemConfiguration() { }
    protected ItemConfiguration() { }
    private static readonly IItemConfiguration _instance = new ItemConfiguration();

    public static ItemConfiguration Instance
    {
        get => _instance as ItemConfiguration;
    }

    protected Dictionary<ItemType, float> _itemBaseValues = new Dictionary<ItemType, float>();
    protected Dictionary<string, ItemClassification> _itemData = new Dictionary<string, ItemClassification>();

    /// <summary>
    /// Returns the base item value for a type or 0 if not defined.
    /// </summary>
    /// <param name="itemType"></param>
    /// <returns></returns>
    public float GetBaseItemValue(ItemType itemType)
    {
        if (_itemBaseValues.ContainsKey(itemType))
        {
            return _itemBaseValues[itemType];
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Adds a base value for an item type for scaling.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    public void AddBaseItemValue(ItemType type, float value)
    {
        if (_itemBaseValues.ContainsKey(type))
        {
            _itemBaseValues[type] = value;
        }
        else
        {
            _itemBaseValues.Add(type, value);
        }
    }

    /// <summary>
    /// Adds or overrides a base value for an item type for scaling if correctly defined.
    /// </summary>
    /// <param name="item"></param>
    public void AddBaseItemValue(ItemOverrides.BaseItemValueOverride item)
    {
        if (item.itemType != null && item.value != null)
        {
            var type = (ItemType)item.itemType;

            AddBaseItemValue(type, item.value.Value);
        }
    }

    #region Game Configuration

    /// <summary>
    /// Set the default values for Vanilla Player Items.
    /// </summary>
    public void Initialize()
    {
        foreach (ItemClassification data in _itemData.Values)
        {
            data.Reset();
        }

        InitializeBaseArmorValues();
        InitializeBaseDamageValues();

        InitializeWeapons();
        InitializeArmor();
        InitializeShields();

        if (WorldConfiguration.Instance.WorldScale != WorldConfiguration.Scaling.Vanilla &&
            !ScalingConfiguration.Instance.GetAutoScaleIgnoreOverrides())
        {
            ReadCustomValues();
        }

        CreateVanillaBackup();
    }

    /// <summary>
    /// Adds a ItemClassification for scaling or updates the existing if a configuration already exists.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="biome"></param>
    /// <param name="itemType"></param>
    public void AddItemConfiguration(string name, WorldConfiguration.Biome? biome, ItemType? itemType)
    {
        if (name.IsNullOrWhiteSpace())
        {
            return;
        }

        if (_itemData.ContainsKey(name))
        {
            _itemData[name].UpdateItem(biome, itemType);
        }
        else
        {
            _itemData.Add(name, new ItemClassification(name, biome, itemType));
        }
    }

    /// <summary>
    /// Create a new entry or override an existing.
    /// </summary>
    /// <param name="itemOverride"></param>
    public void AddItemConfiguration(ItemOverrides.ItemOverride itemOverride)
    {
        if (itemOverride == null || itemOverride.name.IsNullOrWhiteSpace())
        {
            return;
        }

        AddItemConfiguration(itemOverride.name, null, null);

        _itemData[itemOverride.name].OverrideItem(itemOverride);
    }

    /// <summary>
    /// Applies Auto-Scaling to all initialized Items found in the game.
    /// </summary>
    public void UpdateItems()
    {
        foreach (ItemClassification data in _itemData.Values)
        {
            ScalingAPI.GetItemDrop(data.Name, out ItemDrop item);

            if (item == null)
            {
                ScalingPlugin.VentureScalingLogger.LogWarning($"Failed to configure Item: {data.Name}.");
            }
            else
            {
                if (data.ItemCategory == ItemCategory.Weapon)
                {
                    UpdateWeapon(ref item, data.GetDamageValue(), data.GetUpgradeLevels(), 
                        data.GetUpgradeDamageValue(), true);
                }
                else if (data.ItemCategory == ItemCategory.Armor)
                {
                    UpdateArmor(ref item, data.GetValue(), data.GetUpgradeLevels(), data.GetUpgradeValue());
                }
                else if (data.ItemCategory == ItemCategory.Shield)
                {
                    UpdateShield(ref item, data.GetValue(), data.GetUpgradeLevels(), data.GetUpgradeValue());
                }
            }
        }
    }

    /// <summary>
    /// Sets the vanilla data from the current value of every item currently classified.
    /// </summary>
    protected virtual void CreateVanillaBackup()
    {
        ScalingPlugin.VentureScalingLogger.LogInfo("Configuring vanilla backup for Item data...");

        foreach (ItemClassification data in _itemData.Values)
        {
            ScalingAPI.GetItemDrop(data.Name, out ItemDrop item);

            if (item != null)
            {
                if (data.ItemCategory == ItemCategory.Weapon)
                {
                    var damage = item.m_itemData.m_shared.m_damages;
                    var upgrades = item.m_itemData.m_shared.m_maxQuality;
                    var upgradeDamage = item.m_itemData.m_shared.m_damagesPerLevel;
                    data.SetVanillaData(damage, upgrades, upgradeDamage);
                }
                else if (data.ItemCategory == ItemCategory.Armor)
                {
                    var value = item.m_itemData.m_shared.m_armor;
                    var upgrades = item.m_itemData.m_shared.m_maxQuality;
                    var upgradeValue = item.m_itemData.m_shared.m_armorPerLevel;
                    data.SetVanillaData(value, upgrades, upgradeValue);
                }
                else if (data.ItemCategory == ItemCategory.Shield)
                {
                    var value = item.m_itemData.m_shared.m_blockPower;
                    var upgrades = item.m_itemData.m_shared.m_maxQuality;
                    var upgradeValue = item.m_itemData.m_shared.m_blockPowerPerLevel;
                    data.SetVanillaData(value, upgrades, upgradeValue);
                }
            }
            else
            {
                ScalingPlugin.VentureScalingLogger.LogDebug(
                    $"Vanilla backup for {data.Name} not created, ItemDrop not found.");
            }
        }
    }

    /// <summary>
    /// Reset creatures to their original values given they have been assigned.
    /// Creates a vanilla backup if not already assigned.
    /// </summary>
    public void VanillaReset()
    {
        foreach (ItemClassification data in _itemData.Values)
        {
            ScalingAPI.GetItemDrop(data.Name, out ItemDrop item);

            if (item != null)
            {
                if (data.ItemCategory == ItemCategory.Weapon)
                {
                    UpdateWeapon(ref item, data.VanillaDamageValue, 
                        data.VanillaUpgradeLevels, data.VanillaUpgradeDamageValue, true);
                }
                else if (data.ItemCategory == ItemCategory.Armor)
                {
                    UpdateArmor(ref item, data.VanillaValue, 
                        data.VanillaUpgradeLevels, data.VanillaUpgradeValue);
                }
                else if (data.ItemCategory == ItemCategory.Shield)
                {
                    UpdateShield(ref item, data.VanillaValue, 
                        data.VanillaUpgradeLevels, data.VanillaUpgradeValue);
                }
            }
        }
    }

    /// <summary>
    /// Reads the yaml file and applies the custom configurations.
    /// </summary>
    protected virtual void ReadCustomValues()
    {
        try
        {
            ItemOverrides.ItemOverridesList items = ItemOverrides.ReadYaml();

            if (items != null)
            {
                ScalingPlugin.VentureScalingLogger.LogDebug("Deserializer successfully parsed yaml data.");
                ScalingPlugin.VentureScalingLogger.LogDebug(items.ToString());
                foreach (var entry in items.items)
                {
                    AddItemConfiguration(entry);
                }

                foreach (var entry in items.baseItemValues)
                {
                    AddBaseItemValue(entry);
                }
            }
        }
        catch (Exception e)
        {
            ScalingPlugin.VentureScalingLogger.LogWarning("Error loading VWS.ItemOverrides.yaml file.");
            ScalingPlugin.VentureScalingLogger.LogWarning(e);
            ScalingPlugin.VentureScalingLogger.LogWarning("Continuing without custom values...");
        }
    }

    #endregion
}