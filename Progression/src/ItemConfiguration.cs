using System;
using System.Collections.Generic;
using BepInEx;
using static VentureValheim.Progression.ItemConfiguration;

namespace VentureValheim.Progression
{
    public interface IItemConfiguration
    {
        public ItemCategory GetItemCategory(ItemType itemType);
        public float GetTotalDamage(HitData.DamageTypes OriginalDamage, bool playerItem);
        public HitData.DamageTypes CalculateCreatureDamageTypes(
            WorldConfiguration.BiomeData biomeData, HitData.DamageTypes OriginalDamage, float baseTotalDamage, float maxTotalDamage);
        public HitData.DamageTypes CalculateItemDamageTypes(WorldConfiguration.BiomeData biomeData, HitData.DamageTypes OriginalDamage, float baseTotalDamage);
        public void UpdateWeapon(ItemDrop item, HitData.DamageTypes? value, int upgrades, HitData.DamageTypes? upgradeValue, bool playerItem = true);
        public void UpdateArmor(ItemDrop item, float value, int upgrades, float upgradeValue);
        public void UpdateShield(ItemDrop item, float value, int upgrades, float upgradeValue);
        public void UpdateItems();
        public void VanillaReset();
    }

    public class ItemConfiguration : IItemConfiguration
    {
        static ItemConfiguration() { }
        protected ItemConfiguration() { }
        private static readonly IItemConfiguration _instance = new ItemConfiguration();

        public static ItemConfiguration Instance
        {
            get => _instance as ItemConfiguration;
        }

        public enum ItemCategory
        {
            Undefined = -1,
            Weapon = 0,
            Armor = 1,
            Shield = 2,
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
            PrimativeArmor = 31
        }

        public ItemCategory GetItemCategory(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.BucklerShield:
                case ItemType.Shield:
                case ItemType.TowerShield:
                    return ItemCategory.Shield;
                case ItemType.Shoulder:
                case ItemType.PrimativeArmor:
                case ItemType.Helmet:
                case ItemType.Chest:
                case ItemType.Legs:
                case ItemType.Utility:
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
                    return ItemCategory.Weapon;
                default:
                    return ItemCategory.Undefined;
            }
        }

        protected float GetBaseItemValue(ItemType itemType)
        {
            try
            {
                return _itemBaseValues[itemType];
            }
            catch
            {
                return 0;
            }
        }

        public void AddBaseItemValue(ItemType type, float value, bool overrideData = false)
        {
            try
            {
                _itemBaseValues.Add(type, value);
            }
            catch
            {
                if (overrideData)
                {
                    _itemBaseValues[type] = value;
                }
            }
        }

        public class ItemClassification
        {
            public string Name;
            public WorldConfiguration.Biome BiomeType;
            public ItemType ItemType;
            public ItemCategory ItemCategory;
            public float ItemValue;
            public int VanillaUpgradeLevels;
            public float VanillaValue;
            public float VanillaUpgradeValue;
            public HitData.DamageTypes? VanillaDamageValue;
            public HitData.DamageTypes? VanillaUpgradeDamageValue;

            public ItemClassification(string name, WorldConfiguration.Biome biomeType, ItemType itemType, ItemCategory itemCategory, float value)
            {
                Name = name;
                BiomeType = biomeType;
                ItemType = itemType;
                ItemCategory = itemCategory;
                ItemValue = value;
                VanillaUpgradeLevels = 0;
                VanillaValue = 0;
                VanillaUpgradeValue = 0;
                VanillaDamageValue = null;
                VanillaUpgradeDamageValue = null;
            }

            public void UpdateItem(WorldConfiguration.Biome biomeType, ItemType itemType, ItemCategory itemCategory, float value)
            {
                BiomeType = biomeType;
                ItemType = itemType;
                ItemCategory = itemCategory;
                ItemValue = value;
            }
        }

        private Dictionary<ItemType, float> _itemBaseValues = new Dictionary<ItemType, float>();
        private Dictionary<string, ItemClassification> _itemData = new Dictionary<string, ItemClassification>();
        protected bool _vanillaBackupCreated;

        #region Damage

        /// <summary>
        /// Get the total damage for a Player or Creature.
        /// </summary>
        /// <param name="OriginalDamage"></param>
        /// <param name="playerItem">True if for the Player</param>
        /// <returns></returns>
        public float GetTotalDamage(HitData.DamageTypes OriginalDamage, bool playerItem)
        {
            var damage = OriginalDamage.m_damage +
                OriginalDamage.m_blunt +
                OriginalDamage.m_slash +
                OriginalDamage.m_pierce +
                OriginalDamage.m_fire +
                OriginalDamage.m_frost +
                OriginalDamage.m_lightning +
                OriginalDamage.m_poison +
                OriginalDamage.m_spirit;

            /*if (playerItem)
            {
                return damage +
                OriginalDamage.m_chop +
                OriginalDamage.m_pickaxe;
            }*/

            return damage;
        }

        /// <summary>
        /// Scales the damage given the original damage, new total base damage,
        /// and the original maximum total damage of any of this Creature's attack.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="OriginalDamage"></param>
        /// <param name="baseTotalDamage">New base damage to be scaled.</param>
        /// <param name="maxTotalDamage">Maximum damage by any attack this creature has.</param>
        /// <returns></returns>
        public HitData.DamageTypes CalculateCreatureDamageTypes(WorldConfiguration.BiomeData biomeData, HitData.DamageTypes OriginalDamage, float baseTotalDamage, float maxTotalDamage)
        {
            Normalize(ref OriginalDamage);

            // Consider the maximum total damage this creature can do when auto-scaling for multiple attacks.
            float totalDamage = GetTotalDamage(OriginalDamage, false);
            float ratio = DamageRatio(totalDamage, maxTotalDamage);

            var multiplier = ratio * biomeData.ScaleValue;

            return CalculateDamageTypesFinal(OriginalDamage, baseTotalDamage, multiplier, false);
        }

        /// <summary>
        /// Scales the damage given the original damage, new total base damage,
        /// and the original maximum total damage of any of this Creature's attack.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="OriginalDamage"></param>
        /// <param name="baseTotalDamage">New base damage to be scaled.</param>
        /// <returns></returns>
        public HitData.DamageTypes CalculateItemDamageTypes(WorldConfiguration.BiomeData biomeData, HitData.DamageTypes OriginalDamage, float baseTotalDamage)
        {
            Normalize(ref OriginalDamage);

            return CalculateDamageTypesFinal(OriginalDamage, baseTotalDamage, biomeData.ScaleValue, true);
        }

        /// <summary>
        /// Scales the damage given the original damage and new total base damage.
        /// </summary>
        /// <param name="OriginalDamage"></param>
        /// <param name="baseTotalDamage"></param>
        /// <param name="multiplier"></param>
        /// <param name="playerItem"></param>
        /// <returns></returns>
        private HitData.DamageTypes CalculateDamageTypesFinal(HitData.DamageTypes OriginalDamage, float baseTotalDamage, float multiplier, bool playerItem = true)
        {
            HitData.DamageTypes damageTypes = new HitData.DamageTypes();
            var sum = GetTotalDamage(OriginalDamage, playerItem);

            /*if (playerItem)
            {
                // If a mob item leave the chop and pickaxe damage alone because it's a little weird for auto-scaling
                damageTypes.m_chop = ScaleDamage(sum, OriginalDamage.m_chop, baseTotalDamage, multiplier);
                damageTypes.m_pickaxe = ScaleDamage(sum, OriginalDamage.m_pickaxe, baseTotalDamage, multiplier);
            }*/

            // Do not scale chop or pickaxe damage, makes mining stupid
            damageTypes.m_chop = OriginalDamage.m_chop;
            damageTypes.m_pickaxe = OriginalDamage.m_pickaxe;

            damageTypes.m_damage = ScaleDamage(sum, OriginalDamage.m_damage, baseTotalDamage, multiplier);
            damageTypes.m_blunt = ScaleDamage(sum, OriginalDamage.m_blunt, baseTotalDamage, multiplier);
            damageTypes.m_slash = ScaleDamage(sum, OriginalDamage.m_slash, baseTotalDamage, multiplier);
            damageTypes.m_pierce = ScaleDamage(sum, OriginalDamage.m_pierce, baseTotalDamage, multiplier);
            damageTypes.m_fire = ScaleDamage(sum, OriginalDamage.m_fire, baseTotalDamage, multiplier);
            damageTypes.m_frost = ScaleDamage(sum, OriginalDamage.m_frost, baseTotalDamage, multiplier);
            damageTypes.m_lightning = ScaleDamage(sum, OriginalDamage.m_lightning, baseTotalDamage, multiplier);
            damageTypes.m_poison = ScaleDamage(sum, OriginalDamage.m_poison, baseTotalDamage, multiplier);
            damageTypes.m_spirit = ScaleDamage(sum, OriginalDamage.m_spirit, baseTotalDamage, multiplier);

            return damageTypes;
        }

        /// <summary>
        /// Returns the percentage of the maximum damage done by any one damage type given the total damage
        /// </summary>
        /// <param name="totalDamage"></param>
        /// <param name="maxDamageOfAnyAttackType"></param>
        /// <returns></returns>
        protected float DamageRatio(float totalDamage, float maxDamageOfAnyAttackType)
        {
            if (totalDamage == 0f || maxDamageOfAnyAttackType == 0f)
            {
                return 0f;
            }

            return totalDamage / maxDamageOfAnyAttackType;
        }

        /// <summary>
        /// Scale the given newDamage by the multiplier if the original value is greater than 0.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="baseTotalDamage"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        protected float ScaleDamage(float originalSum, float original, float baseTotalDamage, float multiplier)
        {
            if (original <= 0f || baseTotalDamage <= 0f)
            {
                return 0f;
            }

            var value = baseTotalDamage * multiplier * (original / originalSum);
            return (int)value;
        }

        /// <summary>
        /// Sets all negative damage values to 0, use to normalize data for calculations.
        /// </summary>
        /// <param name="OriginalDamage"></param>
        private void Normalize(ref HitData.DamageTypes OriginalDamage)
        {
            OriginalDamage.m_damage = Normalize(OriginalDamage.m_damage);
            OriginalDamage.m_blunt = Normalize(OriginalDamage.m_blunt);
            OriginalDamage.m_slash = Normalize(OriginalDamage.m_slash);
            OriginalDamage.m_pierce = Normalize(OriginalDamage.m_pierce);
            OriginalDamage.m_chop = Normalize(OriginalDamage.m_chop);
            OriginalDamage.m_pickaxe = Normalize(OriginalDamage.m_pickaxe);
            OriginalDamage.m_fire = Normalize(OriginalDamage.m_fire);
            OriginalDamage.m_frost = Normalize(OriginalDamage.m_frost);
            OriginalDamage.m_lightning = Normalize(OriginalDamage.m_lightning);
            OriginalDamage.m_poison = Normalize(OriginalDamage.m_poison);
            OriginalDamage.m_spirit = Normalize(OriginalDamage.m_spirit);
        }

        private float Normalize(float num)
        {
            if (num < 0f)
            {
                return 0f;
            }

            return num;
        }

        /// <summary>
        /// Calculates the item upgrade values such that an item that is fully upgraded will be
        /// as strong as the next biome's natural item.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="original"></param>
        /// <param name="baseTotalDamage">The item's new base total damage</param>
        /// <param name="quality">The item's max quality, 1 indicated no upgrades for the item</param>
        /// <returns></returns>
        protected HitData.DamageTypes CalculateUpgradeValue(WorldConfiguration.Biome biome, HitData.DamageTypes original, float baseTotalDamage, int quality)
        {
            var scale = WorldConfiguration.Instance.GetBiomeScaling(biome);
            var nextScale = WorldConfiguration.Instance.GetNextBiomeScale(biome);

            return CalculateUpgradeValue(scale, nextScale, original, baseTotalDamage, quality);
        }

        /// <summary>
        /// Calculates the item upgrade values such that an item that is fully upgraded will be
        /// as strong as the next biome's natural item.
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="nextScale"></param>
        /// <param name="original"></param>
        /// <param name="baseTotalDamage">The item's new base total damage</param>
        /// <param name="quality">The item's max quality, 1 indicated no upgrades for the item</param>
        /// <returns></returns>
        protected HitData.DamageTypes CalculateUpgradeValue(float scale, float nextScale, HitData.DamageTypes original, float baseTotalDamage, int quality)
        {
            if (quality <= 1)
            {
                // This item has no upgrades
                return new HitData.DamageTypes();
            }

            var startValue = baseTotalDamage * scale;
            var endValue = baseTotalDamage * nextScale;
            var range = endValue - startValue;

            if (range > 0f)
            {
                // Round up
                float damagePerLevel = (float)Math.Round((float)range / quality);
                // TODO, should items equal the next biome's item at max quality?
                return CalculateDamageTypesFinal(original, damagePerLevel, 1f, true);
            }

            return new HitData.DamageTypes();
        }

        #endregion

        #region Armor

        /// <summary>
        /// Calculates the new item armor or block value based off global scale.
        /// </summary>
        /// <param name="ic"></param>
        /// <returns></returns>
        protected float CalculateArmor(ItemClassification ic)
        {
            var scale = WorldConfiguration.Instance.GetBiomeScaling(ic.BiomeType);
            return (int)(ic.ItemValue * scale);
        }

        /// <summary>
        /// Calculates the item upgrade values such that an item that is fully upgraded will be
        /// as strong as the next biome's natural item.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="baseValue">The item's base value</param>
        /// <param name="quality">The item's max quality, 1 indicated no upgrades for the item</param>
        /// <returns></returns>
        protected float CalculateUpgradeValue(WorldConfiguration.Biome biome, float baseValue, int quality)
        {
            var scale = WorldConfiguration.Instance.GetBiomeScaling(biome);
            var nextScale = WorldConfiguration.Instance.GetNextBiomeScale(biome);

            return CalculateUpgradeValue(scale, nextScale, baseValue, quality);
        }

        protected float CalculateUpgradeValue(float scale, float nextScale, float baseValue, int quality)
        {
            if (quality <= 1)
            {
                // This item has no upgrades
                return 0f;
            }

            var startValue = (int)(baseValue * scale);
            var endValue = (int)(baseValue * nextScale);
            var range = endValue - startValue;

            if (range > 0f)
            {
                // Round up
                float value = (float)Math.Round((float)range / quality);
                return value; // TODO, should items equal the next biome's item at max quality?
            }

            return 0f;
        }

        #endregion

        #region Game Configuration

        /// <summary>
        /// Set the default values for Vanilla Player Items.
        /// </summary>
        public void Initialize()
        {
            // TODO
            // BombOoze

            if (_vanillaBackupCreated) return;

            InitializeBaseValues();
            InitializeWeapons();
            InitializeArmor();
            InitializeShields();

            // TODO ability to override the number of upgrades?
            // TODO add options for loading configurations from a file after defaults are set

            CreateVanillaBackup();
        }

        private void InitializeWeapons()
        {
            AddItemConfiguration("Club", WorldConfiguration.Biome.Meadow, ItemType.Primative);
            AddItemConfiguration("AxeStone", WorldConfiguration.Biome.Meadow, ItemType.Primative);
            AddItemConfiguration("PickaxeStone", WorldConfiguration.Biome.Meadow, ItemType.Primative);

            AddItemConfiguration("AxeFlint", WorldConfiguration.Biome.Meadow, ItemType.Axe);
            AddItemConfiguration("AxeBronze", WorldConfiguration.Biome.BlackForest, ItemType.Axe);
            AddItemConfiguration("AxeIron", WorldConfiguration.Biome.Swamp, ItemType.Axe);
            AddItemConfiguration("AxeBlackMetal", WorldConfiguration.Biome.Plain, ItemType.Axe);

            AddItemConfiguration("PickaxeAntler", WorldConfiguration.Biome.Meadow, ItemType.PickAxe);
            AddItemConfiguration("PickaxeBronze", WorldConfiguration.Biome.BlackForest, ItemType.PickAxe);
            AddItemConfiguration("PickaxeIron", WorldConfiguration.Biome.Swamp, ItemType.PickAxe);

            AddItemConfiguration("KnifeChitin", WorldConfiguration.Biome.Ocean, ItemType.Knife);
            AddItemConfiguration("KnifeFlint", WorldConfiguration.Biome.Meadow, ItemType.Knife);
            AddItemConfiguration("KnifeCopper", WorldConfiguration.Biome.BlackForest, ItemType.Knife);
            AddItemConfiguration("KnifeSilver", WorldConfiguration.Biome.Mountain, ItemType.Knife);
            AddItemConfiguration("KnifeBlackMetal", WorldConfiguration.Biome.Plain, ItemType.Knife);

            AddItemConfiguration("MaceBronze", WorldConfiguration.Biome.BlackForest, ItemType.Mace);
            AddItemConfiguration("MaceIron", WorldConfiguration.Biome.Swamp, ItemType.Mace);
            AddItemConfiguration("MaceSilver", WorldConfiguration.Biome.Mountain, ItemType.Mace);
            AddItemConfiguration("MaceNeedle", WorldConfiguration.Biome.Plain, ItemType.Mace);

            AddItemConfiguration("SwordBronze", WorldConfiguration.Biome.BlackForest, ItemType.Sword);
            AddItemConfiguration("SwordIron", WorldConfiguration.Biome.Swamp, ItemType.Sword);
            AddItemConfiguration("SwordIronFire", WorldConfiguration.Biome.Swamp, ItemType.Sword);
            AddItemConfiguration("SwordSilver", WorldConfiguration.Biome.Mountain, ItemType.Sword);
            AddItemConfiguration("SwordBlackmetal", WorldConfiguration.Biome.Plain, ItemType.Sword);

            AddItemConfiguration("AtgeirBronze", WorldConfiguration.Biome.BlackForest, ItemType.Atgeir);
            AddItemConfiguration("AtgeirIron", WorldConfiguration.Biome.Swamp, ItemType.Atgeir);
            AddItemConfiguration("AtgeirBlackmetal", WorldConfiguration.Biome.Plain, ItemType.Atgeir);

            AddItemConfiguration("Battleaxe", WorldConfiguration.Biome.Swamp, ItemType.Battleaxe);
            AddItemConfiguration("BattleaxeCrystal", WorldConfiguration.Biome.Mountain, ItemType.Battleaxe);

            AddItemConfiguration("SledgeStagbreaker", WorldConfiguration.Biome.Meadow, ItemType.Sledge);
            AddItemConfiguration("SledgeIron", WorldConfiguration.Biome.Swamp, ItemType.Sledge);

            AddItemConfiguration("SpearChitin", WorldConfiguration.Biome.Ocean, ItemType.Spear);
            AddItemConfiguration("SpearFlint", WorldConfiguration.Biome.Meadow, ItemType.Spear);
            AddItemConfiguration("SpearBronze", WorldConfiguration.Biome.BlackForest, ItemType.Spear);
            AddItemConfiguration("SpearElderbark", WorldConfiguration.Biome.Swamp, ItemType.Spear);
            AddItemConfiguration("SpearWolfFang", WorldConfiguration.Biome.Mountain, ItemType.Spear);

            AddItemConfiguration("Bow", WorldConfiguration.Biome.Meadow, ItemType.Bow);
            AddItemConfiguration("BowFineWood", WorldConfiguration.Biome.BlackForest, ItemType.Bow);
            AddItemConfiguration("BowHuntsman", WorldConfiguration.Biome.Swamp, ItemType.Bow);
            AddItemConfiguration("BowDraugrFang", WorldConfiguration.Biome.Mountain, ItemType.Bow);

            AddItemConfiguration("ArrowWood", WorldConfiguration.Biome.Meadow, ItemType.Ammo);
            AddItemConfiguration("ArrowFlint", WorldConfiguration.Biome.Meadow, ItemType.Ammo);
            AddItemConfiguration("ArrowFire", WorldConfiguration.Biome.Meadow, ItemType.Ammo);
            AddItemConfiguration("ArrowBronze", WorldConfiguration.Biome.BlackForest, ItemType.Ammo);
            AddItemConfiguration("ArrowIron", WorldConfiguration.Biome.Plain, ItemType.Ammo);
            AddItemConfiguration("ArrowSilver", WorldConfiguration.Biome.Mountain, ItemType.Ammo);
            AddItemConfiguration("ArrowPoison", WorldConfiguration.Biome.Mountain, ItemType.Ammo);
            AddItemConfiguration("ArrowObsidian", WorldConfiguration.Biome.Mountain, ItemType.Ammo);
            AddItemConfiguration("ArrowFrost", WorldConfiguration.Biome.Mountain, ItemType.Ammo);
            AddItemConfiguration("ArrowNeedle", WorldConfiguration.Biome.Plain, ItemType.Ammo);
        }

        private void InitializeArmor()
        {
            AddItemConfiguration("ArmorRagsChest", WorldConfiguration.Biome.Meadow, ItemType.Primative);
            AddItemConfiguration("ArmorRagsLegs", WorldConfiguration.Biome.Meadow, ItemType.Primative);

            AddItemConfiguration("HelmetLeather", WorldConfiguration.Biome.Meadow, ItemType.Helmet);
            AddItemConfiguration("ArmorLeatherChest", WorldConfiguration.Biome.Meadow, ItemType.Chest);
            AddItemConfiguration("ArmorLeatherLegs", WorldConfiguration.Biome.Meadow, ItemType.Legs);

            AddItemConfiguration("HelmetBronze", WorldConfiguration.Biome.BlackForest, ItemType.Helmet);
            AddItemConfiguration("ArmorBronzeChest", WorldConfiguration.Biome.BlackForest, ItemType.Chest);
            AddItemConfiguration("ArmorBronzeLegs", WorldConfiguration.Biome.BlackForest, ItemType.Legs);

            AddItemConfiguration("HelmetTrollLeather", WorldConfiguration.Biome.BlackForest, ItemType.Helmet);
            AddItemConfiguration("ArmorTrollLeatherChest", WorldConfiguration.Biome.BlackForest, ItemType.Chest);
            AddItemConfiguration("ArmorTrollLeatherLegs", WorldConfiguration.Biome.BlackForest, ItemType.Legs);

            AddItemConfiguration("HelmetIron", WorldConfiguration.Biome.Swamp, ItemType.Helmet);
            AddItemConfiguration("ArmorIronChest", WorldConfiguration.Biome.Swamp, ItemType.Chest);
            AddItemConfiguration("ArmorIronLegs", WorldConfiguration.Biome.Swamp, ItemType.Legs);

            AddItemConfiguration("HelmetRoot", WorldConfiguration.Biome.Swamp, ItemType.Helmet);
            AddItemConfiguration("ArmorRootChest", WorldConfiguration.Biome.Swamp, ItemType.Chest);
            AddItemConfiguration("ArmorRootLegs", WorldConfiguration.Biome.Swamp, ItemType.Legs);

            AddItemConfiguration("HelmetFenring", WorldConfiguration.Biome.Mountain, ItemType.Helmet);
            AddItemConfiguration("ArmorFenringChest", WorldConfiguration.Biome.Mountain, ItemType.Chest);
            AddItemConfiguration("ArmorFenringLegs", WorldConfiguration.Biome.Mountain, ItemType.Legs);

            AddItemConfiguration("HelmetDrake", WorldConfiguration.Biome.Mountain, ItemType.Helmet);
            AddItemConfiguration("ArmorWolfChest", WorldConfiguration.Biome.Mountain, ItemType.Chest);
            AddItemConfiguration("ArmorWolfLegs", WorldConfiguration.Biome.Mountain, ItemType.Legs);

            AddItemConfiguration("HelmetPadded", WorldConfiguration.Biome.Plain, ItemType.Helmet);
            AddItemConfiguration("ArmorPaddedCuirass", WorldConfiguration.Biome.Plain, ItemType.Chest);
            AddItemConfiguration("ArmorPaddedGreaves", WorldConfiguration.Biome.Plain, ItemType.Legs);
        }

        private void InitializeShields()
        {
            AddItemConfiguration("ShieldBronzeBuckler", WorldConfiguration.Biome.BlackForest, ItemType.BucklerShield);
            AddItemConfiguration("ShieldIronBuckler", WorldConfiguration.Biome.Swamp, ItemType.BucklerShield);

            AddItemConfiguration("ShieldWood", WorldConfiguration.Biome.Meadow, ItemType.Shield);
            AddItemConfiguration("ShieldBanded", WorldConfiguration.Biome.Swamp, ItemType.Shield);
            AddItemConfiguration("ShieldSilver", WorldConfiguration.Biome.Swamp, ItemType.Shield);
            AddItemConfiguration("ShieldBlackmetal", WorldConfiguration.Biome.Plain, ItemType.Shield);

            AddItemConfiguration("ShieldSerpentscale", WorldConfiguration.Biome.Ocean, ItemType.TowerShield);
            AddItemConfiguration("ShieldWoodTower", WorldConfiguration.Biome.Meadow, ItemType.TowerShield);
            AddItemConfiguration("ShieldBoneTower", WorldConfiguration.Biome.BlackForest, ItemType.TowerShield);
            AddItemConfiguration("ShieldIronSquare", WorldConfiguration.Biome.Swamp, ItemType.TowerShield);
            AddItemConfiguration("ShieldIronTower", WorldConfiguration.Biome.Swamp, ItemType.TowerShield);
            AddItemConfiguration("ShieldBlackmetalTower", WorldConfiguration.Biome.Plain, ItemType.TowerShield);
        }

        private void InitializeBaseValues()
        {
            // TODO error handling
            AddBaseItemValue(ItemType.Shoulder, 1f);
            AddBaseItemValue(ItemType.PrimativeArmor, 2f);
            AddBaseItemValue(ItemType.Helmet, 4f);
            AddBaseItemValue(ItemType.Chest, 4f);
            AddBaseItemValue(ItemType.Legs, 4f);
            AddBaseItemValue(ItemType.BucklerShield, 8f);
            AddBaseItemValue(ItemType.Shield, 10f);
            AddBaseItemValue(ItemType.TowerShield, 15f);
            AddBaseItemValue(ItemType.Primative, 8f);
            AddBaseItemValue(ItemType.Knife, 8f);
            AddBaseItemValue(ItemType.Ammo, 10f);
            AddBaseItemValue(ItemType.PickAxe, 10f);
            AddBaseItemValue(ItemType.Sword, 12f);
            AddBaseItemValue(ItemType.Mace, 12f);
            AddBaseItemValue(ItemType.Spear, 12f);
            AddBaseItemValue(ItemType.Axe, 12f);
            AddBaseItemValue(ItemType.Sledge, 15f);
            AddBaseItemValue(ItemType.Atgeir, 15f);
            AddBaseItemValue(ItemType.Battleaxe, 15f);
            AddBaseItemValue(ItemType.Bow, 22f);
            AddBaseItemValue(ItemType.Tool, 0f);
            AddBaseItemValue(ItemType.Utility, 0f);
            AddBaseItemValue(ItemType.None, 0f);
            AddBaseItemValue(ItemType.Undefined, 0f);
        }

        /// <summary>
        /// Adds a ItemClassification for scaling or replaces the existing if a configuration already exists.
        /// Uses default base configurations for item Category and item Value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="biome"></param>
        /// <param name="itemType"></param>
        private void AddItemConfiguration(string name, WorldConfiguration.Biome biome, ItemType itemType)
        {
            AddItemConfiguration(name, biome, itemType, GetItemCategory(itemType), GetBaseItemValue(itemType));
        }

        /// <summary>
        /// Adds a ItemClassification for scaling or updates the existing if a configuration already exists.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="biome"></param>
        /// <param name="itemType"></param>
        /// <param name="category"></param>
        /// <param name="value"></param>
        private void AddItemConfiguration(string name, WorldConfiguration.Biome biome, ItemType itemType, ItemCategory category, float value)
        {
            if (!name.IsNullOrWhiteSpace())
            {
                var item = new ItemClassification(name, biome, itemType, category, value);
                try
                {
                    _itemData.Add(name, item);
                }
                catch (ArgumentException)
                {
                    _itemData[name].UpdateItem(biome, itemType, category, value);
                }
            }
        }

        /// <summary>
        /// Apply Auto-Scaling to all Items found in the game.
        /// </summary>
        public void UpdateItems()
        {
            foreach (ItemClassification data in _itemData.Values)
            {
                ItemDrop item = ProgressionAPI.Instance.GetItemDrop(data.Name);

                if (item == null)
                {
                    ProgressionPlugin.GetProgressionLogger().LogWarning($"Failed to configure Item: {data.Name}.");
                }
                else
                {
                    var upgrades = data.VanillaUpgradeLevels;

                    if (data.ItemCategory == ItemCategory.Weapon)
                    {
                        var original = item.m_itemData.m_shared.m_damages;
                        var baseTotalDamage = GetBaseItemValue(data.ItemType);
                        var biome = WorldConfiguration.Instance.GetBiome(data.BiomeType);
                        var newDamage = CalculateItemDamageTypes(biome, original, baseTotalDamage);
                        var upgradeAmount = CalculateUpgradeValue(data.BiomeType, original, baseTotalDamage, upgrades);

                        // temp patch for pickaxe and chop damages
                        var originalUpgrade = item.m_itemData.m_shared.m_damagesPerLevel;
                        upgradeAmount.m_chop = originalUpgrade.m_chop;
                        upgradeAmount.m_pickaxe = originalUpgrade.m_pickaxe;

                        UpdateWeapon(item, newDamage, upgrades, upgradeAmount, true);
                    }
                    else if (data.ItemCategory == ItemCategory.Armor)
                    {
                        var upgradeAmount = CalculateUpgradeValue(data.BiomeType, data.ItemValue, upgrades);
                        UpdateArmor(item, CalculateArmor(data), upgrades, upgradeAmount);
                    }
                    else if (data.ItemCategory == ItemCategory.Shield)
                    {
                        var upgradeAmount = CalculateUpgradeValue(data.BiomeType, data.ItemValue, upgrades);
                        UpdateShield(item, CalculateArmor(data), upgrades, upgradeAmount);
                    }
                }
            }
        }

        /// <summary>
        /// Apply Auto-Scaling to the Weapon by Prefab name.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="value">The new total damage.</param>
        public void UpdateWeapon(ItemDrop item, HitData.DamageTypes? value, int upgrades, HitData.DamageTypes? upgradeValue, bool playerItem = true)
        {
            // Damage
            if (value == null)
            {
                ProgressionPlugin.GetProgressionLogger().LogWarning(
                    $"{item.name} NOT updated with new scaled damage values. DamageTypes undefined.");
                return;
            }

            var original = item.m_itemData.m_shared.m_damages;
            float sumDamage = GetTotalDamage(original, playerItem);
            float newSumDamage = GetTotalDamage(value.Value, playerItem);
            item.m_itemData.m_shared.m_damages = value.Value;

            ProgressionPlugin.GetProgressionLogger().LogDebug(
                $"{item.name} updated with new scaled damage values. Total damage changed from {sumDamage} to {newSumDamage}.");

            // Upgrades
            if (!playerItem)
            {
                return;
            }

            if (upgradeValue == null)
            {
                ProgressionPlugin.GetProgressionLogger().LogWarning(
                    $"{item.name} NOT updated with new scaled damage values. DamageTypes for item upgrades undefined.");
                return;
            }

            var quality = item.m_itemData.m_shared.m_maxQuality;
            var upgradeAmount = item.m_itemData.m_shared.m_damagesPerLevel;
            float sumDamageUpgrade = GetTotalDamage(upgradeAmount, playerItem);
            float newSumDamageUpgrade = GetTotalDamage(upgradeValue.Value, playerItem);
            item.m_itemData.m_shared.m_maxQuality = upgrades;
            item.m_itemData.m_shared.m_damagesPerLevel = upgradeValue.Value;

            ProgressionPlugin.GetProgressionLogger().LogDebug(
                $"{item.name}: Total item upgrades changed from {quality} to {upgrades}. Total upgrade damage changed from {sumDamageUpgrade} to {newSumDamageUpgrade}");
        }

        /// <summary>
        /// Apply Auto-Scaling to the armor by Prefab name.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="value">The new armor value.</param>
        public void UpdateArmor(ItemDrop item, float value, int upgrades, float upgradeValue)
        {
            // Armor
            var original = item.m_itemData.m_shared.m_armor;
            item.m_itemData.m_shared.m_armor = value;

            ProgressionPlugin.GetProgressionLogger().LogDebug(
                $"{item.name} updated with new scaled armor values. Total armor changed from {original} to {value}.");

            // Upgrades
            var quality = item.m_itemData.m_shared.m_maxQuality;
            var upgradeAmount = item.m_itemData.m_shared.m_armorPerLevel;
            item.m_itemData.m_shared.m_maxQuality = upgrades;
            item.m_itemData.m_shared.m_armorPerLevel = upgradeValue;

            ProgressionPlugin.GetProgressionLogger().LogDebug(
                $"{item.name}: Total item upgrades changed from {quality} to {upgrades}. Total upgrade armor changed from {upgradeAmount} to {upgradeValue}.");
        }

        /// <summary>
        /// Apply Auto-Scaling to the shield by Prefab name.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="value">The new block power.</param>
        public void UpdateShield(ItemDrop item, float value, int upgrades, float upgradeValue)
        {
            // Shield
            var original = item.m_itemData.m_shared.m_blockPower;
            item.m_itemData.m_shared.m_blockPower = value;

            ProgressionPlugin.GetProgressionLogger().LogDebug(
                $"{item.name} updated with new scaled block values. Total block changed from {original} to {value}.");

            // Upgrades
            var quality = item.m_itemData.m_shared.m_maxQuality;
            var upgradeAmount = item.m_itemData.m_shared.m_blockPowerPerLevel;
            item.m_itemData.m_shared.m_maxQuality = upgrades;
            item.m_itemData.m_shared.m_blockPowerPerLevel = upgradeValue;

            ProgressionPlugin.GetProgressionLogger().LogDebug(
                $"{item.name}: Total item upgrades changed from {quality} to {upgrades}. Total upgrade block changed from {upgradeAmount} to {upgradeValue}.");
        }

        private void CreateVanillaBackup()
        {
            if (_vanillaBackupCreated) return;

            ProgressionPlugin.GetProgressionLogger().LogInfo("Configuring vanilla backup for Item data...");

            foreach (ItemClassification data in _itemData.Values)
            {
                ItemDrop item = ProgressionAPI.Instance.GetItemDrop(data.Name);

                if (item != null)
                {
                    if (data.ItemCategory == ItemCategory.Weapon)
                    {
                        data.VanillaDamageValue = item.m_itemData.m_shared.m_damages;
                        data.VanillaUpgradeLevels = item.m_itemData.m_shared.m_maxQuality;
                        data.VanillaUpgradeDamageValue = item.m_itemData.m_shared.m_damagesPerLevel;
                    }
                    else if (data.ItemCategory == ItemCategory.Armor)
                    {
                        data.VanillaValue = item.m_itemData.m_shared.m_armor;
                        data.VanillaUpgradeLevels = item.m_itemData.m_shared.m_maxQuality;
                        data.VanillaUpgradeValue = item.m_itemData.m_shared.m_armorPerLevel;
                    }
                    else if (data.ItemCategory == ItemCategory.Shield)
                    {
                        data.VanillaValue = item.m_itemData.m_shared.m_blockPower;
                        data.VanillaUpgradeLevels = item.m_itemData.m_shared.m_maxQuality;
                        data.VanillaUpgradeValue = item.m_itemData.m_shared.m_blockPowerPerLevel;
                    }
                }
                else
                {
                    ProgressionPlugin.GetProgressionLogger().LogDebug($"Vanilla backup for {data.Name} not created, ItemDrop not found.");
                }
            }

            _vanillaBackupCreated = true;
        }

        public void VanillaReset()
        {
            if (_vanillaBackupCreated)
            {
                foreach (ItemClassification data in _itemData.Values)
                {
                    ItemDrop item = ProgressionAPI.Instance.GetItemDrop(data.Name);

                    if (item != null)
                    {
                        if (data.ItemCategory == ItemCategory.Weapon)
                        {
                            UpdateWeapon(item, data.VanillaDamageValue, data.VanillaUpgradeLevels, data.VanillaUpgradeDamageValue, true);
                        }
                        else if (data.ItemCategory == ItemCategory.Armor)
                        {
                            UpdateArmor(item, data.VanillaValue, data.VanillaUpgradeLevels, data.VanillaUpgradeValue);
                        }
                        else if (data.ItemCategory == ItemCategory.Shield)
                        {
                            UpdateShield(item, data.VanillaValue, data.VanillaUpgradeLevels, data.VanillaUpgradeValue);
                        }
                    }
                }
            }
            else
            {
                CreateVanillaBackup();
            }
        }

        #endregion
    }
}