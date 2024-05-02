namespace VentureValheim.Scaling
{
    public enum ItemCategory
    {
        Undefined = -1,
        Weapon = 0,
        Armor = 1,
        Shield = 2
    }

    public enum ItemType
    {
        Undefined = -1,
        None = 0,
        Shield = 1,
        Helmet = 2,
        Chest = 3,
        Legs = 4,
        Shoulder = 5,
        Utility = 6,
        Tool = 7,
        PickAxe = 8,
        Axe = 9,
        Bow = 10,
        Ammo = 11,
        Sword = 20,
        Knife = 21,
        Mace = 22,
        Sledge = 23,
        Atgeir = 25,
        Battleaxe = 26,
        Primative = 27,
        Spear = 28,
        TowerShield = 29,
        BucklerShield = 30,
        PrimativeArmor = 31,
        Bolt = 32,
        Crossbow = 33,
        HelmetRobe = 34,
        ChestRobe = 35,
        LegsRobe = 36,
        Fist = 37,
        TurretBolt = 38,
        GemAxe = 39,
        GemBow = 40,
        GemSword = 41,
        GemKnife = 42,
        GemMace = 43,
        GemSledge = 44,
        GemAtgeir = 45,
        GemBattleaxe = 46,
        GemSpear = 47,
        GemCrossbow = 48,
        HelmetMedium = 49,
        ChestMedium = 50,
        LegsMedium = 51,
        StaffRapid = 52,
        StaffSlow = 53,
        MagicShield = 54
    }

    public class ItemClassification
    {
        public string Name { get; private set; }
        public WorldConfiguration.Biome BiomeType { get; private set; }
        public ItemType ItemType { get; private set; }
        public ItemCategory ItemCategory { get; private set; }

        public int? VanillaUpgradeLevels { get; private set; }
        public float? VanillaValue { get; private set; }
        public float? VanillaUpgradeValue { get; private set; }
        public HitData.DamageTypes? VanillaDamageValue { get; private set; }
        public HitData.DamageTypes? VanillaUpgradeDamageValue { get; private set; }

        public bool Overridden { get; private set; }
        public float? OverrideValue { get; private set; }
        protected CreatureOverrides.AttackOverride OverrideDamageValue { get; private set; }
        protected CreatureOverrides.AttackOverride OverrideUpgradeDamageValue { get; private set; }
        protected float? OverrideUpgradeValue { get; private set; }
        protected int? OverrideUpgradeLevels { get; private set; }

        public ItemClassification(string name, WorldConfiguration.Biome? biomeType, ItemType? itemType)
        {
            Reset();

            Name = name;
            if (biomeType != null)
            {
                BiomeType = biomeType.Value;
            }

            if (itemType != null)
            {
                ItemType = itemType.Value;
            }

            ItemCategory = GetItemCategory(itemType);

            VanillaUpgradeLevels = null;
            VanillaValue = null;
            VanillaDamageValue = null;
            VanillaUpgradeValue = null;
            VanillaUpgradeDamageValue = null;
        }

        /// <summary>
        /// Reset the custom values.
        /// </summary>
        public void Reset()
        {
            BiomeType = WorldConfiguration.Biome.Undefined;
            ItemType = ItemType.Undefined;
            ItemCategory = ItemCategory.Undefined;

            Overridden = false;
            OverrideUpgradeLevels = null;
            OverrideValue = null;
            OverrideDamageValue = null;
            OverrideUpgradeValue = null;
            OverrideUpgradeDamageValue = null;
        }

        /// <summary>
        /// Returns true if the scaling factor is Vanilla or if set to Custom and this entry is not overriden.
        /// </summary>
        /// <returns></returns>
        public virtual bool IgnoreScaling()
        {
            if (WorldConfiguration.Instance.WorldScale == WorldConfiguration.Scaling.Vanilla ||
                (WorldConfiguration.Instance.WorldScale == WorldConfiguration.Scaling.Custom && Overridden == false))
            {
                return true;
            }

            return false;
        }

        public void UpdateItem(WorldConfiguration.Biome? biomeType, ItemType? itemType)
        {
            if (biomeType != null)
            {
                BiomeType = biomeType.Value;
            }
            if (itemType != null)
            {
                ItemType = itemType.Value;
                ItemCategory = GetItemCategory(itemType);
            }
        }

        public void OverrideItem(ItemOverrides.ItemOverride item)
        {
            Overridden = true;

            WorldConfiguration.Biome? biome = null;

            if (item.biome != null)
            {
                biome = (WorldConfiguration.Biome)item.biome;
            }

            ItemType? itemType = null;

            if (item.itemType != null)
            {
                itemType = (ItemType)item.itemType;
            }

            UpdateItem(biome, itemType);

            OverrideUpgradeLevels = item.quality;
            OverrideValue = item.value;
            OverrideDamageValue = item.damageValue;
            OverrideUpgradeValue = item.upgradeValue;
            OverrideUpgradeDamageValue = item.upgradeDamageValue;
        }

        public static ItemCategory GetItemCategory(ItemType? itemType)
        {
            if (itemType == null)
            {
                return ItemCategory.Undefined;
            }

            switch (itemType)
            {
                case ItemType.BucklerShield:
                case ItemType.Shield:
                case ItemType.TowerShield:
                case ItemType.MagicShield:
                    return ItemCategory.Shield;
                case ItemType.Shoulder:
                case ItemType.PrimativeArmor:
                case ItemType.Helmet:
                case ItemType.Chest:
                case ItemType.Legs:
                case ItemType.HelmetRobe:
                case ItemType.ChestRobe:
                case ItemType.LegsRobe:
                case ItemType.Utility:
                case ItemType.HelmetMedium:
                case ItemType.ChestMedium:
                case ItemType.LegsMedium:
                    return ItemCategory.Armor;
                case ItemType.Primative:
                case ItemType.Knife:
                case ItemType.Ammo:
                case ItemType.PickAxe:
                case ItemType.Sword:
                case ItemType.Mace:
                case ItemType.Spear:
                case ItemType.Axe:
                case ItemType.Sledge:
                case ItemType.Atgeir:
                case ItemType.Battleaxe:
                case ItemType.Bow:
                case ItemType.Tool:
                case ItemType.Bolt:
                case ItemType.Crossbow:
                case ItemType.Fist:
                case ItemType.TurretBolt:
                case ItemType.GemAxe:
                case ItemType.GemBow:
                case ItemType.GemSword:
                case ItemType.GemKnife:
                case ItemType.GemMace:
                case ItemType.GemSledge:
                case ItemType.GemAtgeir:
                case ItemType.GemBattleaxe:
                case ItemType.GemSpear:
                case ItemType.GemCrossbow:
                case ItemType.StaffRapid:
                case ItemType.StaffSlow:
                    return ItemCategory.Weapon;
                default:
                    return ItemCategory.Undefined;
            }
        }

        private bool ItemTypeDefined()
        {
            return ItemType != ItemType.Undefined && ItemType != ItemType.None;
        }

        /// <summary>
        /// If overridden returns that value, otherwise calulates the new scaled value.
        /// </summary>
        /// <returns></returns>
        public float? GetValue()
        {
            if (IgnoreScaling())
            {
                return VanillaValue;
            }
            else if (OverrideValue != null)
            {
                return OverrideValue;
            }
            else if (BiomeType != WorldConfiguration.Biome.Undefined && ItemTypeDefined())
            {
                var scale = WorldConfiguration.Instance.GetBiomeScaling(BiomeType);
                return (int)(GetBaseValue() * scale);
            }

            return VanillaValue;
        }

        /// <summary>
        /// Shorthand call for the class.
        /// </summary>
        /// <returns></returns>
        private float GetBaseValue()
        {
            return ItemConfiguration.Instance.GetBaseItemValue(ItemType);
        }

        public float? GetUpgradeValue()
        {
            if (IgnoreScaling())
            {
                return VanillaUpgradeValue;
            }
            else if (OverrideUpgradeValue != null)
            {
                return OverrideUpgradeValue;
            }
            else if (BiomeType != WorldConfiguration.Biome.Undefined && ItemTypeDefined())
            {
                var levels = GetUpgradeLevels();
                if (levels != null)
                {
                    return ItemConfiguration.Instance.CalculateUpgradeValue(BiomeType, GetBaseValue(), levels.Value);
                }
            }

            return VanillaUpgradeValue;
        }

        public int? GetUpgradeLevels()
        {
            if (OverrideUpgradeLevels != null)
            {
                return OverrideUpgradeLevels;
            }

            return VanillaUpgradeLevels;
        }

        /// <summary>
        /// If the damage is overridden returns the custom DamageTypes or calculated DamageTypes from total damage.
        /// If not overridden returns the calculated DamageTypes.
        /// </summary>
        /// <returns></returns>
        public HitData.DamageTypes? GetDamageValue()
        {
            if (IgnoreScaling())
            {
                return VanillaDamageValue;
            }
            else if (OverrideDamageValue != null)
            {
                if (OverrideDamageValue.totalDamage != null && VanillaDamageValue != null)
                {
                    return ItemConfiguration.Instance.CalculateDamageTypesFinal(
                        VanillaDamageValue.Value, OverrideDamageValue.totalDamage.Value, 1);
                }
                else
                {
                    var damage = CreatureOverrides.GetDamageTypes(OverrideDamageValue);
                    // Prevent an overwrite of chop and pickaxe damage to reduce confusion hopefully
                    if (VanillaDamageValue != null)
                    {
                        if (OverrideDamageValue.pickaxe == null)
                        {
                            damage.m_pickaxe = VanillaDamageValue.Value.m_pickaxe;
                        }
                        if (OverrideDamageValue.chop == null)
                        {
                            damage.m_chop = VanillaDamageValue.Value.m_chop;
                        }
                    }

                    return damage;
                }
            }
            else if (BiomeType != WorldConfiguration.Biome.Undefined && VanillaDamageValue != null && ItemTypeDefined())
            {
                var biome = WorldConfiguration.Instance.GetBiome(BiomeType);
                return ItemConfiguration.Instance.CalculateItemDamageTypes(biome.ScaleValue, VanillaDamageValue.Value, GetBaseValue());
            }

            return VanillaDamageValue;
        }

        public HitData.DamageTypes? GetUpgradeDamageValue()
        {
            if (IgnoreScaling())
            {
                return VanillaUpgradeDamageValue;
            }
            else if (OverrideUpgradeDamageValue != null)
            {
                if (OverrideUpgradeDamageValue.totalDamage != null && VanillaDamageValue != null)
                {
                    return ItemConfiguration.Instance.CalculateDamageTypesFinal(
                        VanillaUpgradeDamageValue.Value, OverrideUpgradeDamageValue.totalDamage.Value, 1);
                }
                else
                {
                    return CreatureOverrides.GetDamageTypes(OverrideUpgradeDamageValue);
                }
            }
            else if (BiomeType != WorldConfiguration.Biome.Undefined && VanillaDamageValue != null && ItemTypeDefined())
            {
                return ItemConfiguration.Instance.CalculateUpgradeValue(BiomeType, VanillaUpgradeDamageValue.Value, GetBaseValue(), GetUpgradeLevels().Value);
            }

            return VanillaUpgradeDamageValue;
        }

        public void SetVanillaData(float value, int upgrades, float upgradeValue)
        {
            VanillaValue ??= value;

            VanillaUpgradeLevels ??= upgrades;

            VanillaUpgradeValue ??= upgradeValue;
        }

        public void SetVanillaData(HitData.DamageTypes damage, int upgrades, HitData.DamageTypes upgradeDamage)
        {
            VanillaDamageValue ??= damage;

            VanillaUpgradeLevels ??= upgrades;

            VanillaUpgradeDamageValue ??= upgradeDamage;
        }
    }
}