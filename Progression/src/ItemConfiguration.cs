using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace VentureValheim.Progression
{
    public class ItemConfiguration
    {
        private ItemConfiguration() { }
        private static readonly ItemConfiguration _instance = new ItemConfiguration();

        public static ItemConfiguration Instance
        {
            get => _instance;
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
            BucklerShield = 30
        }

        public static float GetBasePlayerDamage(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Tool:
                    return 0f;
                case ItemType.PickAxe:
                    return 10f;
                case ItemType.Primative:
                    return 8f;
                case ItemType.Knife:
                    return 8f;
                case ItemType.Sword:
                case ItemType.Mace:
                case ItemType.Spear:
                case ItemType.Axe:
                    return 12f;
                case ItemType.Bow:
                    return 22f;
                case ItemType.Ammo:
                    return 10f;
                case ItemType.Sledge:
                case ItemType.Atgeir:
                case ItemType.Battleaxe:
                    return 15f;
                default:
                    return 0f;
            }
        }

        public static float GetBasePlayerArmor(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Shield:
                    return 10f;
                case ItemType.BucklerShield:
                    return 8f;
                case ItemType.TowerShield:
                    return 15f;
                case ItemType.Helmet:
                    return 4f;
                case ItemType.Chest:
                    return 4f;
                case ItemType.Legs:
                    return 4f;
                case ItemType.Shoulder:
                    return 1f;
                case ItemType.Primative:
                    return 2f;
                case ItemType.Utility:
                default:
                    return 0f;
            }
        }

        public class ItemClassification
        {
            public string Name;
            public WorldConfiguration.Biome BiomeType;
            public ItemType ItemType;

            public ItemClassification(string name, WorldConfiguration.Biome biomeType, ItemType itemType)
            {
                Name = name;
                BiomeType = biomeType;
                ItemType = itemType;
            }
        }

        private Dictionary<string, ItemClassification> _weaponData;
        private Dictionary<string, ItemClassification> _armorData;
        private Dictionary<string, ItemClassification> _shieldData;

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

            if (playerItem)
            {
                return damage +
                OriginalDamage.m_chop +
                OriginalDamage.m_pickaxe;
            }

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
        public HitData.DamageTypes CalculateCreatureDamageTypes(WorldConfiguration.Biome biome, HitData.DamageTypes OriginalDamage, float baseTotalDamage, float maxTotalDamage)
        {
            var scale = WorldConfiguration.GetBiomeScaling((int)biome);
            Normalize(ref OriginalDamage);

            // Consider the maximum total damage this creature can do when auto-scaling for multiple attacks.
            float totalDamage = GetTotalDamage(OriginalDamage, false);
            float ratio = DamageRatio(totalDamage, maxTotalDamage);

            var multiplier = ratio * scale;

            return CalculateDamageTypesFinal(OriginalDamage, baseTotalDamage, multiplier, false);
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
        public HitData.DamageTypes CalculateItemDamageTypes(WorldConfiguration.Biome biome, HitData.DamageTypes OriginalDamage, float baseTotalDamage)
        {
            var scale = WorldConfiguration.GetBiomeScaling((int)biome);
            Normalize(ref OriginalDamage);

            return CalculateDamageTypesFinal(OriginalDamage, baseTotalDamage, scale, true);
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

            if (playerItem)
            {
                // If a mob item leave the chop and pickaxe damage alone because it's a little weird for auto-scaling
                damageTypes.m_chop = ScaleDamage(OriginalDamage.m_chop, baseTotalDamage, multiplier);
                damageTypes.m_pickaxe = ScaleDamage(OriginalDamage.m_pickaxe, baseTotalDamage, multiplier);
            }

            damageTypes.m_damage = ScaleDamage(OriginalDamage.m_damage, baseTotalDamage, multiplier);
            damageTypes.m_blunt = ScaleDamage(OriginalDamage.m_blunt, baseTotalDamage, multiplier);
            damageTypes.m_slash = ScaleDamage(OriginalDamage.m_slash, baseTotalDamage, multiplier);
            damageTypes.m_pierce = ScaleDamage(OriginalDamage.m_pierce, baseTotalDamage, multiplier);
            damageTypes.m_fire = ScaleDamage(OriginalDamage.m_fire, baseTotalDamage, multiplier);
            damageTypes.m_frost = ScaleDamage(OriginalDamage.m_frost, baseTotalDamage, multiplier);
            damageTypes.m_lightning = ScaleDamage(OriginalDamage.m_lightning, baseTotalDamage, multiplier);
            damageTypes.m_poison = ScaleDamage(OriginalDamage.m_poison, baseTotalDamage, multiplier);
            damageTypes.m_spirit = ScaleDamage(OriginalDamage.m_spirit, baseTotalDamage, multiplier);

            return damageTypes;
        }

        /// <summary>
        /// Returns the percentage of the maximum damage done by any one damage type given the total damage
        /// </summary>
        /// <param name="totalDamage"></param>
        /// <param name="maxDamageOfAnyAttackType"></param>
        /// <returns></returns>
        private float DamageRatio(float totalDamage, float maxDamageOfAnyAttackType)
        {
            if (totalDamage == 0f || maxDamageOfAnyAttackType == 0f)
            {
                return 0f;
            }

            return maxDamageOfAnyAttackType / totalDamage;
        }

        /// <summary>
        /// Scale the given newDamage by the multiplier if the original value is greater than 0.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="baseTotalDamage"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        private float ScaleDamage(float original, float baseTotalDamage, float multiplier)
        {
            if (original <= 0f || baseTotalDamage <= 0f)
            {
                return 0f;
            }
            var value = baseTotalDamage * multiplier;
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

        public float CalculateArmor(ItemClassification ic)
        {
            var scale = WorldConfiguration.GetBiomeScaling((int)ic.BiomeType);
            return (int)(GetBasePlayerArmor(ic.ItemType) * scale);
        }

        /*public float CalculateArmorPerLevel(ItemClassification ic)
        {
            return 1f;
        }*/

        /// <summary>
        /// Set the default values for Vanilla Player Items.
        /// </summary>
        public void Initialize()
        {
            _weaponData = new Dictionary<string, ItemClassification>();
            _armorData = new Dictionary<string, ItemClassification>();
            _shieldData = new Dictionary<string, ItemClassification>();

            // TODO
            // BombOoze

            // Weapons

            AddWeaponConfiguration("Club", WorldConfiguration.Biome.Meadow, ItemType.Primative);
            AddWeaponConfiguration("AxeStone", WorldConfiguration.Biome.Meadow, ItemType.Primative);
            AddWeaponConfiguration("PickaxeStone", WorldConfiguration.Biome.Meadow, ItemType.Primative);

            AddWeaponConfiguration("AxeFlint", WorldConfiguration.Biome.Meadow, ItemType.Axe);
            AddWeaponConfiguration("AxeBronze", WorldConfiguration.Biome.BlackForest, ItemType.Axe);
            AddWeaponConfiguration("AxeIron", WorldConfiguration.Biome.Swamp, ItemType.Axe);
            AddWeaponConfiguration("AxeBlackMetal", WorldConfiguration.Biome.Plain, ItemType.Axe);

            AddWeaponConfiguration("PickaxeAntler", WorldConfiguration.Biome.Meadow, ItemType.PickAxe);
            AddWeaponConfiguration("PickaxeBronze", WorldConfiguration.Biome.BlackForest, ItemType.PickAxe);
            AddWeaponConfiguration("PickaxeIron", WorldConfiguration.Biome.Swamp, ItemType.PickAxe);

            AddWeaponConfiguration("KnifeChitin", WorldConfiguration.Biome.Ocean, ItemType.Knife);
            AddWeaponConfiguration("KnifeFlint", WorldConfiguration.Biome.Meadow, ItemType.Knife);
            AddWeaponConfiguration("KnifeCopper", WorldConfiguration.Biome.BlackForest, ItemType.Knife);
            AddWeaponConfiguration("KnifeSilver", WorldConfiguration.Biome.Mountain, ItemType.Knife);
            AddWeaponConfiguration("KnifeBlackMetal", WorldConfiguration.Biome.Plain, ItemType.Knife);

            AddWeaponConfiguration("MaceBronze", WorldConfiguration.Biome.BlackForest, ItemType.Mace);
            AddWeaponConfiguration("MaceIron", WorldConfiguration.Biome.Swamp, ItemType.Mace);
            AddWeaponConfiguration("MaceSilver", WorldConfiguration.Biome.Mountain, ItemType.Mace);
            AddWeaponConfiguration("MaceNeedle", WorldConfiguration.Biome.Plain, ItemType.Mace);

            AddWeaponConfiguration("SwordBronze", WorldConfiguration.Biome.BlackForest, ItemType.Sword);
            AddWeaponConfiguration("SwordIron", WorldConfiguration.Biome.Swamp, ItemType.Sword);
            AddWeaponConfiguration("SwordIronFire", WorldConfiguration.Biome.Swamp, ItemType.Sword);
            AddWeaponConfiguration("SwordSilver", WorldConfiguration.Biome.Mountain, ItemType.Sword);
            AddWeaponConfiguration("SwordBlackmetal", WorldConfiguration.Biome.Plain, ItemType.Sword);

            AddWeaponConfiguration("AtgeirBronze", WorldConfiguration.Biome.BlackForest, ItemType.Atgeir);
            AddWeaponConfiguration("AtgeirIron", WorldConfiguration.Biome.Swamp, ItemType.Atgeir);
            AddWeaponConfiguration("AtgeirBlackmetal", WorldConfiguration.Biome.Plain, ItemType.Atgeir);

            AddWeaponConfiguration("Battleaxe", WorldConfiguration.Biome.Swamp, ItemType.Battleaxe);
            AddWeaponConfiguration("BattleaxeCrystal", WorldConfiguration.Biome.Mountain, ItemType.Battleaxe);

            AddWeaponConfiguration("SledgeStagbreaker", WorldConfiguration.Biome.Meadow, ItemType.Sledge);
            AddWeaponConfiguration("SledgeIron", WorldConfiguration.Biome.Swamp, ItemType.Sledge);

            AddWeaponConfiguration("SpearChitin", WorldConfiguration.Biome.Ocean, ItemType.Spear);
            AddWeaponConfiguration("SpearFlint", WorldConfiguration.Biome.Meadow, ItemType.Spear);
            AddWeaponConfiguration("SpearBronze", WorldConfiguration.Biome.BlackForest, ItemType.Spear);
            AddWeaponConfiguration("SpearElderbark", WorldConfiguration.Biome.Swamp, ItemType.Spear);
            AddWeaponConfiguration("SpearWolfFang", WorldConfiguration.Biome.Mountain, ItemType.Spear);

            AddWeaponConfiguration("Bow", WorldConfiguration.Biome.Meadow, ItemType.Bow);
            AddWeaponConfiguration("BowFineWood", WorldConfiguration.Biome.BlackForest, ItemType.Bow);
            AddWeaponConfiguration("BowHuntsman", WorldConfiguration.Biome.Swamp, ItemType.Bow);
            AddWeaponConfiguration("BowDraugrFang", WorldConfiguration.Biome.Mountain, ItemType.Bow);

            AddWeaponConfiguration("ArrowWood", WorldConfiguration.Biome.Meadow, ItemType.Ammo);
            AddWeaponConfiguration("ArrowFlint", WorldConfiguration.Biome.Meadow, ItemType.Ammo);
            AddWeaponConfiguration("ArrowFire", WorldConfiguration.Biome.Meadow, ItemType.Ammo);
            AddWeaponConfiguration("ArrowBronze", WorldConfiguration.Biome.BlackForest, ItemType.Ammo);
            AddWeaponConfiguration("ArrowIron", WorldConfiguration.Biome.Plain, ItemType.Ammo);
            AddWeaponConfiguration("ArrowSilver", WorldConfiguration.Biome.Mountain, ItemType.Ammo);
            AddWeaponConfiguration("ArrowPoison", WorldConfiguration.Biome.Mountain, ItemType.Ammo);
            AddWeaponConfiguration("ArrowObsidian", WorldConfiguration.Biome.Mountain, ItemType.Ammo);
            AddWeaponConfiguration("ArrowFrost", WorldConfiguration.Biome.Mountain, ItemType.Ammo);
            AddWeaponConfiguration("ArrowNeedle", WorldConfiguration.Biome.Plain, ItemType.Ammo);

            // Armor

            AddArmorConfiguration("ArmorRagsChest", WorldConfiguration.Biome.Meadow, ItemType.Primative);
            AddArmorConfiguration("ArmorRagsLegs", WorldConfiguration.Biome.Meadow, ItemType.Primative);

            AddArmorConfiguration("HelmetLeather", WorldConfiguration.Biome.Meadow, ItemType.Helmet);
            AddArmorConfiguration("ArmorLeatherChest", WorldConfiguration.Biome.Meadow, ItemType.Chest);
            AddArmorConfiguration("ArmorLeatherLegs", WorldConfiguration.Biome.Meadow, ItemType.Legs);

            AddArmorConfiguration("HelmetBronze", WorldConfiguration.Biome.BlackForest, ItemType.Helmet);
            AddArmorConfiguration("ArmorBronzeChest", WorldConfiguration.Biome.BlackForest, ItemType.Chest);
            AddArmorConfiguration("ArmorBronzeLegs", WorldConfiguration.Biome.BlackForest, ItemType.Legs);

            AddArmorConfiguration("HelmetTrollLeather", WorldConfiguration.Biome.BlackForest, ItemType.Helmet);
            AddArmorConfiguration("ArmorTrollLeatherChest", WorldConfiguration.Biome.BlackForest, ItemType.Chest);
            AddArmorConfiguration("ArmorTrollLeatherLegs", WorldConfiguration.Biome.BlackForest, ItemType.Legs);

            AddArmorConfiguration("HelmetIron", WorldConfiguration.Biome.Swamp, ItemType.Helmet);
            AddArmorConfiguration("ArmorIronChest", WorldConfiguration.Biome.Swamp, ItemType.Chest);
            AddArmorConfiguration("ArmorIronLegs", WorldConfiguration.Biome.Swamp, ItemType.Legs);

            AddArmorConfiguration("HelmetRoot", WorldConfiguration.Biome.Swamp, ItemType.Helmet);
            AddArmorConfiguration("ArmorRootChest", WorldConfiguration.Biome.Swamp, ItemType.Chest);
            AddArmorConfiguration("ArmorRootLegs", WorldConfiguration.Biome.Swamp, ItemType.Legs);

            AddArmorConfiguration("HelmetFenring", WorldConfiguration.Biome.Mountain, ItemType.Helmet);
            AddArmorConfiguration("ArmorFenringChest", WorldConfiguration.Biome.Mountain, ItemType.Chest);
            AddArmorConfiguration("ArmorFenringLegs", WorldConfiguration.Biome.Mountain, ItemType.Legs);

            AddArmorConfiguration("HelmetDrake", WorldConfiguration.Biome.Mountain, ItemType.Helmet);
            AddArmorConfiguration("ArmorWolfChest", WorldConfiguration.Biome.Mountain, ItemType.Chest);
            AddArmorConfiguration("ArmorWolfLegs", WorldConfiguration.Biome.Mountain, ItemType.Legs);

            AddArmorConfiguration("HelmetPadded", WorldConfiguration.Biome.Plain, ItemType.Helmet);
            AddArmorConfiguration("ArmorPaddedCuirass", WorldConfiguration.Biome.Plain, ItemType.Chest);
            AddArmorConfiguration("ArmorPaddedGreaves", WorldConfiguration.Biome.Plain, ItemType.Legs);

            // Shields

            AddShieldConfiguration("ShieldBronzeBuckler", WorldConfiguration.Biome.BlackForest, ItemType.BucklerShield);
            AddShieldConfiguration("ShieldIronBuckler", WorldConfiguration.Biome.Swamp, ItemType.BucklerShield);

            AddShieldConfiguration("ShieldWood", WorldConfiguration.Biome.Meadow, ItemType.Shield);
            AddShieldConfiguration("ShieldBanded", WorldConfiguration.Biome.Swamp, ItemType.Shield);
            AddShieldConfiguration("ShieldKnight", WorldConfiguration.Biome.Swamp, ItemType.Shield);
            AddShieldConfiguration("ShieldSilver", WorldConfiguration.Biome.Swamp, ItemType.Shield);
            AddShieldConfiguration("ShieldBlackmetal", WorldConfiguration.Biome.Plain, ItemType.Shield);

            AddShieldConfiguration("ShieldSerpentscale", WorldConfiguration.Biome.Ocean, ItemType.TowerShield);
            AddShieldConfiguration("ShieldWoodTower", WorldConfiguration.Biome.Meadow, ItemType.TowerShield);
            AddShieldConfiguration("ShieldBoneTower", WorldConfiguration.Biome.BlackForest, ItemType.TowerShield);
            AddShieldConfiguration("ShieldIronSquare", WorldConfiguration.Biome.Swamp, ItemType.TowerShield);
            AddShieldConfiguration("ShieldIronTower", WorldConfiguration.Biome.Swamp, ItemType.TowerShield);
            AddShieldConfiguration("ShieldBlackmetalTower", WorldConfiguration.Biome.Plain, ItemType.TowerShield);
        }

        /// <summary>
        /// Adds a new weapon ItemClassification for scaling or replaces the existing if a configuration already exists.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="biome"></param>
        /// <param name="itemType"></param>
        public void AddWeaponConfiguration(string name, WorldConfiguration.Biome biome, ItemType itemType)
        {
            try
            {
                _weaponData.Add(name, new ItemClassification(name, biome, itemType));
            }
            catch (Exception e)
            {
                _weaponData[name] = new ItemClassification(name, biome, itemType);
            }
        }

        /// <summary>
        /// Adds a new armor ItemClassification for scaling or replaces the existing if a configuration already exists.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="biome"></param>
        /// <param name="itemType"></param>
        public void AddArmorConfiguration(string name, WorldConfiguration.Biome biome, ItemType itemType)
        {
            try
            {
                _armorData.Add(name, new ItemClassification(name, biome, itemType));
            }
            catch (Exception e)
            {
                _armorData[name] = new ItemClassification(name, biome, itemType);
            }
        }

        /// <summary>
        /// Adds a new armor ItemClassification for scaling or replaces the existing if a configuration already exists.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="biome"></param>
        /// <param name="itemType"></param>
        public void AddShieldConfiguration(string name, WorldConfiguration.Biome biome, ItemType itemType)
        {
            try
            {
                _shieldData.Add(name, new ItemClassification(name, biome, itemType));
            }
            catch (Exception e)
            {
                _shieldData[name] = new ItemClassification(name, biome, itemType);
            }
        }

        /// <summary>
        /// Apply Auto-Scaling to all Creatures found in the game by Prefab name.
        /// </summary>
        private static void UpdateItems()
        {
            foreach (ItemClassification data in Instance._weaponData.Values)
            {
                ItemDrop? item = ProgressionAPI.GetItemDrop(data.Name);

                if (item == null)
                {
                    ProgressionPlugin.GetProgressionLogger().LogWarning($"Failed to configure {data.Name}.");
                }
                else
                {
                    var original = item.m_itemData.m_shared.m_damages;
                    float sumDamage = Instance.GetTotalDamage(original, false);
                    var baseTotalDamage = GetBasePlayerDamage(data.ItemType);

                    item.m_itemData.m_shared.m_damages = Instance.CalculateItemDamageTypes(data.BiomeType, original, baseTotalDamage);

                    float newSumDamage = Instance.GetTotalDamage(item.m_itemData.m_shared.m_damages, false);
                    ProgressionPlugin.GetProgressionLogger().LogDebug(
                        $"{item.name} updated with new scaled damage values. Total damage changed from {sumDamage} to {newSumDamage}");
                }
            }

            foreach (ItemClassification data in Instance._armorData.Values)
            {
                ItemDrop? item = ProgressionAPI.GetItemDrop(data.Name);

                if (item == null)
                {
                    ProgressionPlugin.GetProgressionLogger().LogWarning($"Failed to configure {data.Name}.");
                }
                else
                {
                    var original = item.m_itemData.m_shared.m_armor;
                    item.m_itemData.m_shared.m_armor = Instance.CalculateArmor(data);
                    var newArmor = item.m_itemData.m_shared.m_armor;

                    ProgressionPlugin.GetProgressionLogger().LogDebug(
                        $"{item.name} updated with new scaled armor values. Total armor changed from {original} to {newArmor}");
                }
            }

            foreach (ItemClassification data in Instance._shieldData.Values)
            {
                ItemDrop? item = ProgressionAPI.GetItemDrop(data.Name);

                if (item == null)
                {
                    ProgressionPlugin.GetProgressionLogger().LogWarning($"Failed to configure {data.Name}.");
                }
                else
                {
                    var original = item.m_itemData.m_shared.m_blockPower;
                    item.m_itemData.m_shared.m_blockPower = Instance.CalculateArmor(data);
                    var newArmor = item.m_itemData.m_shared.m_blockPower;

                    ProgressionPlugin.GetProgressionLogger().LogDebug(
                        $"{item.name} updated with new scaled block values. Total block changed from {original} to {newArmor}");
                }
            }
        }

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        public static class Patch_ObjectDB_Awake
        {
            private static void Prefix()
            {
                if (!ProgressionPlugin.Instance.GetAutoScaleItems())
                {
                    return;
                }

                if (SceneManager.GetActiveScene().name.Equals("main"))
                {
                    if (WorldConfiguration.Instance.GetWorldScale() != (int)WorldConfiguration.Scaling.Vanilla)
                    {
                        ProgressionPlugin.GetProgressionLogger().LogDebug("Updating Item Configurations with auto-scaling.");
                        UpdateItems();
                    }
                }
                else
                {
                    ProgressionPlugin.GetProgressionLogger().LogDebug("Skipping generating data because not in the main scene.");
                }
            }
        }
    }
}