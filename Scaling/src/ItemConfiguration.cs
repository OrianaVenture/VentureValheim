using System;
using System.Collections.Generic;
using BepInEx;

namespace VentureValheim.Scaling
{
    public interface IItemConfiguration
    {
        public float GetTotalDamage(HitData.DamageTypes OriginalDamage);
        public HitData.DamageTypes CalculateCreatureDamageTypes(
            float biomeScale, HitData.DamageTypes OriginalDamage, float baseTotalDamage, float maxTotalDamage);
        public HitData.DamageTypes CalculateItemDamageTypes(float biomeScale, HitData.DamageTypes originalDamage, float baseTotalDamage);
        public void UpdateWeapon(ref ItemDrop item, HitData.DamageTypes? value, int? upgrades, HitData.DamageTypes? upgradeValue, bool playerItem = true);
        public void UpdateArmor(ref ItemDrop item, float? value, int? upgrades, float? upgradeValue);
        public void UpdateShield(ref ItemDrop item, float? value, int? upgrades, float? upgradeValue);
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

        #region Damage

        /// <summary>
        /// Get the total damage, excluding chop and pickaxe damage.
        /// </summary>
        /// <param name="originalDamage"></param>
        /// <returns></returns>
        public float GetTotalDamage(HitData.DamageTypes originalDamage)
        {
            var damage = originalDamage.m_damage +
                originalDamage.m_blunt +
                originalDamage.m_slash +
                originalDamage.m_pierce +
                originalDamage.m_fire +
                originalDamage.m_frost +
                originalDamage.m_lightning +
                originalDamage.m_poison +
                originalDamage.m_spirit;

            return damage;
        }

        /// <summary>
        /// Scales the damage given the original damage, new total base damage,
        /// and the original maximum total damage of any of this Creature's attack.
        /// </summary>
        /// <param name="biomeScale"></param>
        /// <param name="originalDamage"></param>
        /// <param name="baseTotalDamage">New base damage to be scaled.</param>
        /// <param name="maxTotalDamage">Maximum damage by any attack this creature has.</param>
        /// <returns></returns>
        public HitData.DamageTypes CalculateCreatureDamageTypes(float biomeScale, HitData.DamageTypes originalDamage, float baseTotalDamage, float maxTotalDamage)
        {
            Normalize(ref originalDamage);

            // Consider the maximum total damage this creature can do when auto-scaling for multiple attacks.
            float ratio = DamageRatio(GetTotalDamage(originalDamage), maxTotalDamage);

            var multiplier = ratio * biomeScale;

            return CalculateDamageTypesFinal(originalDamage, baseTotalDamage, multiplier);
        }

        /// <summary>
        /// Scales the damage given the original damage, new total base damage,
        /// and the original maximum total damage of any of this Creature's attack.
        /// </summary>
        /// <param name="biomeScale"></param>
        /// <param name="originalDamage"></param>
        /// <param name="baseTotalDamage">New base damage to be scaled.</param>
        /// <returns></returns>
        public HitData.DamageTypes CalculateItemDamageTypes(float biomeScale, HitData.DamageTypes originalDamage, float baseTotalDamage)
        {
            Normalize(ref originalDamage);

            return CalculateDamageTypesFinal(originalDamage, baseTotalDamage, biomeScale);
        }

        /// <summary>
        /// Scales the damage given the original damage and new total base damage.
        /// </summary>
        /// <param name="OriginalDamage"></param>
        /// <param name="baseTotalDamage"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public HitData.DamageTypes CalculateDamageTypesFinal(HitData.DamageTypes OriginalDamage, float baseTotalDamage, float multiplier)
        {
            HitData.DamageTypes damageTypes = new HitData.DamageTypes();
            var sum = GetTotalDamage(OriginalDamage);

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
        public HitData.DamageTypes CalculateUpgradeValue(WorldConfiguration.Biome biome, HitData.DamageTypes original, float baseTotalDamage, int quality)
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
        /// <param name="quality">The item's max quality, 1 indicates no upgrades for the item</param>
        /// <returns></returns>
        protected HitData.DamageTypes CalculateUpgradeValue(float scale, float nextScale, HitData.DamageTypes original, float baseTotalDamage, int quality)
        {
            if (quality <= 1)
            {
                // This item has no upgrades
                return original;
            }

            var startValue = baseTotalDamage * scale;
            var endValue = baseTotalDamage * nextScale;
            var range = endValue - startValue;

            if (range > 0f)
            {
                // Round up
                float damagePerLevel = (float)Math.Round((float)range / quality);
                // TODO, should items equal the next biome's item at max quality?
                return CalculateDamageTypesFinal(original, damagePerLevel, 1f);
            }

            return original;
        }

        #endregion

        #region Armor

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
            // BombBile

            foreach (ItemClassification data in _itemData.Values)
            {
                data.Reset();
            }

            InitializeBaseValues();

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

        private void InitializeWeapons()
        {
            AddItemConfiguration("Club", WorldConfiguration.Biome.Meadow, ItemType.Primative);
            AddItemConfiguration("AxeStone", WorldConfiguration.Biome.Meadow, ItemType.Primative);
            AddItemConfiguration("PickaxeStone", WorldConfiguration.Biome.Meadow, ItemType.Primative);

            AddItemConfiguration("AxeFlint", WorldConfiguration.Biome.Meadow, ItemType.Axe);
            AddItemConfiguration("AxeBronze", WorldConfiguration.Biome.BlackForest, ItemType.Axe);
            AddItemConfiguration("AxeIron", WorldConfiguration.Biome.Swamp, ItemType.Axe);
            AddItemConfiguration("AxeBlackMetal", WorldConfiguration.Biome.Plain, ItemType.Axe);
            AddItemConfiguration("AxeJotunBane", WorldConfiguration.Biome.Mistland, ItemType.Axe);

            AddItemConfiguration("PickaxeAntler", WorldConfiguration.Biome.Meadow, ItemType.PickAxe);
            AddItemConfiguration("PickaxeBronze", WorldConfiguration.Biome.BlackForest, ItemType.PickAxe);
            AddItemConfiguration("PickaxeIron", WorldConfiguration.Biome.Swamp, ItemType.PickAxe);
            AddItemConfiguration("PickaxeBlackMetal", WorldConfiguration.Biome.Plain, ItemType.PickAxe);

            AddItemConfiguration("KnifeChitin", WorldConfiguration.Biome.Ocean, ItemType.Knife);
            AddItemConfiguration("KnifeFlint", WorldConfiguration.Biome.Meadow, ItemType.Knife);
            AddItemConfiguration("KnifeCopper", WorldConfiguration.Biome.BlackForest, ItemType.Knife);
            AddItemConfiguration("KnifeSilver", WorldConfiguration.Biome.Mountain, ItemType.Knife);
            AddItemConfiguration("KnifeBlackMetal", WorldConfiguration.Biome.Plain, ItemType.Knife);
            AddItemConfiguration("KnifeSkollAndHati", WorldConfiguration.Biome.Mistland, ItemType.Knife);

            AddItemConfiguration("MaceBronze", WorldConfiguration.Biome.BlackForest, ItemType.Mace);
            AddItemConfiguration("MaceIron", WorldConfiguration.Biome.Swamp, ItemType.Mace);
            AddItemConfiguration("MaceSilver", WorldConfiguration.Biome.Mountain, ItemType.Mace);
            AddItemConfiguration("MaceNeedle", WorldConfiguration.Biome.Plain, ItemType.Mace);

            AddItemConfiguration("SwordBronze", WorldConfiguration.Biome.BlackForest, ItemType.Sword);
            AddItemConfiguration("SwordIron", WorldConfiguration.Biome.Swamp, ItemType.Sword);
            AddItemConfiguration("SwordIronFire", WorldConfiguration.Biome.Swamp, ItemType.Sword);
            AddItemConfiguration("SwordSilver", WorldConfiguration.Biome.Mountain, ItemType.Sword);
            AddItemConfiguration("SwordBlackmetal", WorldConfiguration.Biome.Plain, ItemType.Sword);
            AddItemConfiguration("SwordMistwalker", WorldConfiguration.Biome.Mistland, ItemType.Sword);

            AddItemConfiguration("AtgeirBronze", WorldConfiguration.Biome.BlackForest, ItemType.Atgeir);
            AddItemConfiguration("AtgeirIron", WorldConfiguration.Biome.Swamp, ItemType.Atgeir);
            AddItemConfiguration("AtgeirBlackmetal", WorldConfiguration.Biome.Plain, ItemType.Atgeir);
            AddItemConfiguration("AtgeirHimminAfl", WorldConfiguration.Biome.Mistland, ItemType.Atgeir);

            AddItemConfiguration("Battleaxe", WorldConfiguration.Biome.Swamp, ItemType.Battleaxe);
            AddItemConfiguration("BattleaxeCrystal", WorldConfiguration.Biome.Mountain, ItemType.Battleaxe);
            AddItemConfiguration("THSwordKrom", WorldConfiguration.Biome.Mistland, ItemType.Battleaxe);

            AddItemConfiguration("SledgeStagbreaker", WorldConfiguration.Biome.Meadow, ItemType.Sledge);
            AddItemConfiguration("SledgeIron", WorldConfiguration.Biome.Swamp, ItemType.Sledge);
            AddItemConfiguration("SledgeDemolisher", WorldConfiguration.Biome.Mistland, ItemType.Sledge);

            AddItemConfiguration("SpearChitin", WorldConfiguration.Biome.Ocean, ItemType.Spear);
            AddItemConfiguration("SpearFlint", WorldConfiguration.Biome.Meadow, ItemType.Spear);
            AddItemConfiguration("SpearBronze", WorldConfiguration.Biome.BlackForest, ItemType.Spear);
            AddItemConfiguration("SpearElderbark", WorldConfiguration.Biome.Swamp, ItemType.Spear);
            AddItemConfiguration("SpearWolfFang", WorldConfiguration.Biome.Mountain, ItemType.Spear);
            AddItemConfiguration("SpearCarapace", WorldConfiguration.Biome.Mistland, ItemType.Spear);

            AddItemConfiguration("FistFenrirClaw", WorldConfiguration.Biome.Mountain, ItemType.Fist);

            AddItemConfiguration("Bow", WorldConfiguration.Biome.Meadow, ItemType.Bow);
            AddItemConfiguration("BowFineWood", WorldConfiguration.Biome.BlackForest, ItemType.Bow);
            AddItemConfiguration("BowHuntsman", WorldConfiguration.Biome.Swamp, ItemType.Bow);
            AddItemConfiguration("BowDraugrFang", WorldConfiguration.Biome.Mountain, ItemType.Bow);
            AddItemConfiguration("BowSpineSnap", WorldConfiguration.Biome.Mistland, ItemType.Bow);

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
            AddItemConfiguration("ArrowCarapace", WorldConfiguration.Biome.Mistland, ItemType.Ammo);

            AddItemConfiguration("CrossbowArbalest", WorldConfiguration.Biome.Mistland, ItemType.Crossbow);

            AddItemConfiguration("BoltBone", WorldConfiguration.Biome.BlackForest, ItemType.Bolt);
            AddItemConfiguration("BoltIron", WorldConfiguration.Biome.Swamp, ItemType.Bolt);
            AddItemConfiguration("BoltBlackmetal", WorldConfiguration.Biome.Plain, ItemType.Bolt);
            AddItemConfiguration("BoltCarapace", WorldConfiguration.Biome.Mistland, ItemType.Bolt);
        }

        private void InitializeArmor()
        {
            // TODO: Add support for shoulder items

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

            AddItemConfiguration("ShieldSerpentscale", WorldConfiguration.Biome.Ocean, ItemType.TowerShield);
            AddItemConfiguration("ShieldWoodTower", WorldConfiguration.Biome.Meadow, ItemType.TowerShield);
            AddItemConfiguration("ShieldBoneTower", WorldConfiguration.Biome.BlackForest, ItemType.TowerShield);
            AddItemConfiguration("ShieldIronTower", WorldConfiguration.Biome.Swamp, ItemType.TowerShield);
            AddItemConfiguration("ShieldBlackmetalTower", WorldConfiguration.Biome.Plain, ItemType.TowerShield);
        }

        private void InitializeBaseValues()
        {
            AddBaseItemValue(ItemType.Shoulder, 1f);
            AddBaseItemValue(ItemType.PrimativeArmor, 2f);
            AddBaseItemValue(ItemType.Helmet, 4f);
            AddBaseItemValue(ItemType.Chest, 4f);
            AddBaseItemValue(ItemType.Legs, 4f);
            AddBaseItemValue(ItemType.HelmetRobe, 2f);
            AddBaseItemValue(ItemType.ChestRobe, 2f);
            AddBaseItemValue(ItemType.LegsRobe, 2f);
            AddBaseItemValue(ItemType.BucklerShield, 8f);
            AddBaseItemValue(ItemType.Shield, 10f);
            AddBaseItemValue(ItemType.TowerShield, 15f);
            AddBaseItemValue(ItemType.Primative, 8f);
            AddBaseItemValue(ItemType.Knife, 8f);
            AddBaseItemValue(ItemType.Fist, 8f);
            AddBaseItemValue(ItemType.Ammo, 10f);
            AddBaseItemValue(ItemType.Bolt, 10f);
            AddBaseItemValue(ItemType.PickAxe, 10f);
            AddBaseItemValue(ItemType.Sword, 12f);
            AddBaseItemValue(ItemType.Mace, 12f);
            AddBaseItemValue(ItemType.Spear, 12f);
            AddBaseItemValue(ItemType.Axe, 12f);
            AddBaseItemValue(ItemType.Sledge, 15f);
            AddBaseItemValue(ItemType.Atgeir, 15f);
            AddBaseItemValue(ItemType.Battleaxe, 15f);
            AddBaseItemValue(ItemType.Bow, 18f);
            AddBaseItemValue(ItemType.Crossbow, 45f);
            AddBaseItemValue(ItemType.Tool, 0f);
            AddBaseItemValue(ItemType.Utility, 0f);
            AddBaseItemValue(ItemType.None, 0f);
            AddBaseItemValue(ItemType.Undefined, 0f);
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
                ItemDrop item = ScalingAPI.GetItemDrop(data.Name);

                if (item == null)
                {
                    ScalingPlugin.VentureScalingLogger.LogWarning($"Failed to configure Item: {data.Name}.");
                }
                else
                {
                    if (data.ItemCategory == ItemCategory.Weapon)
                    {
                        UpdateWeapon(ref item, data.GetDamageValue(), data.GetUpgradeLevels(), data.GetUpgradeDamageValue(), true);
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
        /// Apply Auto-Scaling to the Weapon by Prefab name.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="value">The new total damage.</param>
        public void UpdateWeapon(ref ItemDrop item, HitData.DamageTypes? value, int? upgrades, HitData.DamageTypes? upgradeValue, bool playerItem = true)
        {
            // Damage
            if (value == null)
            {
                ScalingPlugin.VentureScalingLogger.LogWarning(
                    $"{item.name} NOT updated with new scaled damage values. DamageTypes undefined.");
                return;
            }

            var original = item.m_itemData.m_shared.m_damages;
            float sumDamage = GetTotalDamage(original);
            float newSumDamage = GetTotalDamage(value.Value);
            item.m_itemData.m_shared.m_damages = value.Value;

            ScalingPlugin.VentureScalingLogger.LogDebug(
                $"{item.name}: Total damage changed from {sumDamage} to {newSumDamage}.");

            // Upgrades
            if (!playerItem)
            {
                return;
            }

            if (upgrades == null)
            {
                ScalingPlugin.VentureScalingLogger.LogWarning(
                    $"{item.name} NOT updated with new scaled upgrade damage values. Total upgrades undefined.");
                return;
            }

            if (upgradeValue == null)
            {
                ScalingPlugin.VentureScalingLogger.LogWarning(
                    $"{item.name} NOT updated with new scaled upgrade damage values. DamageTypes for item upgrades undefined.");
                return;
            }

            var quality = item.m_itemData.m_shared.m_maxQuality;
            var upgradeAmount = item.m_itemData.m_shared.m_damagesPerLevel;
            float sumDamageUpgrade = GetTotalDamage(upgradeAmount);
            float newSumDamageUpgrade = GetTotalDamage(upgradeValue.Value);

            item.m_itemData.m_shared.m_maxQuality = upgrades.Value;
            item.m_itemData.m_shared.m_damagesPerLevel = upgradeValue.Value;

            ScalingPlugin.VentureScalingLogger.LogDebug(
                $"{item.name}: Total item upgrades changed from {quality} to {upgrades}. Total upgrade damage changed from {sumDamageUpgrade} to {newSumDamageUpgrade}");
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
                $"{item.name}: Total item upgrades changed from {quality} to {upgrades}. Total upgrade armor changed from {upgradeAmount} to {upgradeValue}.");
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
                $"{item.name}: Total item upgrades changed from {quality} to {upgrades}. Total upgrade block changed from {upgradeAmount} to {upgradeValue}.");
        }

        /// <summary>
        /// Sets the vanilla data from the current value of every item currently classified.
        /// </summary>
        protected virtual void CreateVanillaBackup()
        {
            ScalingPlugin.VentureScalingLogger.LogInfo("Configuring vanilla backup for Item data...");

            foreach (ItemClassification data in _itemData.Values)
            {
                ItemDrop item = ScalingAPI.GetItemDrop(data.Name);

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
                    ScalingPlugin.VentureScalingLogger.LogDebug($"Vanilla backup for {data.Name} not created, ItemDrop not found.");
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
                ItemDrop item = ScalingAPI.GetItemDrop(data.Name);

                if (item != null)
                {
                    if (data.ItemCategory == ItemCategory.Weapon)
                    {
                        UpdateWeapon(ref item, data.VanillaDamageValue, data.VanillaUpgradeLevels, data.VanillaUpgradeDamageValue, true);
                    }
                    else if (data.ItemCategory == ItemCategory.Armor)
                    {
                        UpdateArmor(ref item, data.VanillaValue, data.VanillaUpgradeLevels, data.VanillaUpgradeValue);
                    }
                    else if (data.ItemCategory == ItemCategory.Shield)
                    {
                        UpdateShield(ref item, data.VanillaValue, data.VanillaUpgradeLevels, data.VanillaUpgradeValue);
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
                ScalingPlugin.VentureScalingLogger.LogWarning("Error loading WAP.ItemOverrides.yaml file.");
                ScalingPlugin.VentureScalingLogger.LogWarning(e);
                ScalingPlugin.VentureScalingLogger.LogWarning("Continuing without custom values...");
            }
        }

        #endregion
    }
}