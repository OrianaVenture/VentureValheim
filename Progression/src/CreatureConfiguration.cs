using System;
using System.Collections.Generic;
using UnityEngine;
using static VentureValheim.Progression.CreatureConfiguration;

namespace VentureValheim.Progression
{
    public interface ICreatureConfiguration
    {
        public bool ContainsCreature(string key);
        public void SetBaseHealth(int[] values);
        public int GetBaseHealth(WorldConfiguration.Difficulty difficulty);
        public int CalculateHealth(CreatureClassification cc, float scale);
        public void SetBaseDamage(int[] values);
        public int GetBaseTotalDamage(WorldConfiguration.Difficulty difficulty);
        public void AddCreatureConfiguration(string name, WorldConfiguration.Biome biome, WorldConfiguration.Difficulty difficulty, bool overrideData = false);
        public void UpdateCreatures();
        public void VanillaReset();
    }

    public class CreatureConfiguration : ICreatureConfiguration
    {
        static CreatureConfiguration() { }
        protected CreatureConfiguration() { }
        private static readonly CreatureConfiguration _instance = new CreatureConfiguration();

        public static CreatureConfiguration Instance
        {
            get => _instance;
        }

        public class CreatureClassification
        {
            public string Name { get; private set; }
            public WorldConfiguration.Biome BiomeType { get; private set; }
            public WorldConfiguration.Difficulty CreatureDifficulty { get; private set; }
            public Dictionary<string, HitData.DamageTypes> VanillaAttacks { get; private set; }
            public float VanillaHealth { get; private set; }

            public CreatureClassification(string name, WorldConfiguration.Biome biomeType, WorldConfiguration.Difficulty creatureDifficulty)
            {
                Name = name;
                BiomeType = biomeType;
                CreatureDifficulty = creatureDifficulty;
                VanillaHealth = -1;
                VanillaAttacks = null;
            }

            public void SetVanillaHealth(float health)
            {
                VanillaHealth = health;
            }

            public void SetVanillaAttacks(GameObject[] attacks)
            {
                VanillaAttacks = new Dictionary<string, HitData.DamageTypes>();

                for (int lcv = 0; lcv < attacks.Length; lcv++)
                {
                    var item = attacks[lcv].GetComponent<ItemDrop>();
                    if (item != null)
                    {
                        var name = item.m_itemData.m_shared.m_name;
                        var damage = item.m_itemData.m_shared.m_damages;
                        if (!AddVanillaAttack(name, damage))
                        {
                            ProgressionPlugin.GetProgressionLogger().LogDebug($"Attack {name} was not added to creature {Name}. No damage component.");
                        }
                    }
                    else
                    {
                        ProgressionPlugin.GetProgressionLogger().LogDebug($"Attack for {Name} was not added to creature {Name}. ItemDrop not found.");
                    }
                }
            }

            /// <summary>
            /// Adds the given vanilla DamageTypes for an attack.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="damage"></param>
            /// <returns>True if the attack is added to the list.</returns>
            private bool AddVanillaAttack(string name, HitData.DamageTypes damage)
            {
                try
                {
                    if (ItemConfiguration.Instance.GetTotalDamage(damage, false) > 0)
                    {
                        VanillaAttacks.Add(name, damage);
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                    // TODO optionally log warning
                }
                return false;
            }
        }

        private Dictionary<string, CreatureClassification> _creatureData = new Dictionary<string, CreatureClassification>();
        protected bool _vanillaBackupCreated = false;
        private int[] _baseHealth = { 5, 10, 30, 50, 200, 500 };
        private int[] _baseDamage = { 0, 5, 10, 12, 15, 20 };

        public bool ContainsCreature(string key)
        {
            return _creatureData.ContainsKey(key);
        }

        /// <summary>
        /// Replaces the base health distribution values if in the correct format.
        /// </summary>
        /// <param name="values"></param>
        public void SetBaseHealth(int[] values)
        {
            if (values != null && values.Length == 6)
            {
                _baseHealth = values;
            }
        }

        /// <summary>
        /// Get the base health for scaling from the default list
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public int GetBaseHealth(WorldConfiguration.Difficulty difficulty)
        {
            switch (difficulty)
            {
                case WorldConfiguration.Difficulty.Harmless:
                    return _baseHealth[0];
                case WorldConfiguration.Difficulty.Novice:
                    return _baseHealth[1];
                case WorldConfiguration.Difficulty.Average:
                    return _baseHealth[2];
                case WorldConfiguration.Difficulty.Intermediate:
                    return _baseHealth[3];
                case WorldConfiguration.Difficulty.Expert:
                    return _baseHealth[4];
                case WorldConfiguration.Difficulty.Boss:
                    return _baseHealth[5];
                default:
                    // Set default to "Average" difficulty
                    return _baseHealth[2];
            }
        }

        /// <summary>
        /// Calculates the health based on the assigned biome and difficulty of a Creature
        /// </summary>
        /// <param name="cc"></param>
        /// <returns></returns>
        public int CalculateHealth(CreatureClassification cc, float scale)
        {
            int baseHealth = GetBaseHealth(cc.CreatureDifficulty);

            return (int)(scale * baseHealth);
        }

        /// <summary>
        /// Replaces the base damage distribution values if in the correct format.
        /// </summary>
        /// <param name="values"></param>
        public void SetBaseDamage(int[] values)
        {
            if (values != null && values.Length == 6)
            {
                _baseDamage = values;
            }
        }

        /// <summary>
        /// Get the base damage for scaling from the default list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetBaseTotalDamage(WorldConfiguration.Difficulty difficulty)
        {
            switch (difficulty)
            {
                case WorldConfiguration.Difficulty.Harmless:
                    return _baseDamage[0];
                case WorldConfiguration.Difficulty.Novice:
                    return _baseDamage[1];
                case WorldConfiguration.Difficulty.Average:
                    return _baseDamage[2];
                case WorldConfiguration.Difficulty.Intermediate:
                    return _baseDamage[3];
                case WorldConfiguration.Difficulty.Expert:
                    return _baseDamage[4];
                case WorldConfiguration.Difficulty.Boss:
                    return _baseDamage[5];
                default:
                    // Set default to "Average" difficulty
                    return _baseDamage[2];
            }
        }

        /// <summary>
        /// Finds the creature ItemDrop attack prefab and configures damage.
        /// </summary>
        /// <param name="cc"></param>
        private void ConfigureAttacks(CreatureClassification cc)
        {
            if (cc.VanillaAttacks == null || cc.VanillaAttacks.Count < 1)
            {
                return;
            }

            try
            {
                var values = GetMaxCreatureDamage(cc);
                float totalAttacks = values.Item1;
                float maxTotalDamage = values.Item2;

                if (maxTotalDamage <= 0)
                {
                    ProgressionPlugin.GetProgressionLogger()
                        .LogDebug($"All {totalAttacks} attacks for {cc.Name} have no damage components. Skipping attack configuration.");
                    return;
                }

                foreach (var attack in cc.VanillaAttacks)
                {
                    ItemDrop item = ProgressionAPI.Instance.GetItemDrop(attack.Key);
                    if (item != null)
                    {
                        var vanillaAttack = attack.Value;
                        float vanillaAttackSum = ItemConfiguration.Instance.GetTotalDamage(vanillaAttack, false);
                        var baseTotalDamage = GetBaseTotalDamage(cc.CreatureDifficulty);
                        var biomeData = WorldConfiguration.Instance.GetBiome(cc.BiomeType);
                        var damage = ItemConfiguration.Instance.CalculateCreatureDamageTypes(biomeData, vanillaAttack, baseTotalDamage, maxTotalDamage);
                        ConfigureAttack(item, damage);
                    }
                    else
                    {
                        ProgressionPlugin.GetProgressionLogger().LogWarning(
                            $"Failed to configure \"{attack.Key}\" for {cc.Name}. Did you forget to define your custom creature attacks?");
                    }
                }
            }
            catch (Exception e)
            {
                ProgressionPlugin.GetProgressionLogger().LogDebug($"Error configuring attacks for {cc.Name}. Skipping.");
                ProgressionPlugin.GetProgressionLogger().LogDebug(e);
            }
        }

        private void ConfigureAttack(ItemDrop item, HitData.DamageTypes? value)
        {
            ItemConfiguration.Instance.UpdateWeapon(item, value, false);
        }

        /// <summary>
        /// Calculates the maximum damage done by any one attack in the CreatureClassification.
        /// Counts the total number of attacks that do damage.
        /// </summary>
        /// <param name="cc"></param>
        /// <returns>Total non-zero attacks, highest attack value</returns>
        protected (int, float) GetMaxCreatureDamage(CreatureClassification cc)
        {
            int totalAttacks = 0;
            float maxDamage = 0;

            if (cc.VanillaAttacks != null)
            {
                foreach (var attack in cc.VanillaAttacks)
                {
                    ItemDrop item = ProgressionAPI.Instance.GetItemDrop(attack.Key);

                    if (item != null)
                    {
                        var damage = ItemConfiguration.Instance.GetTotalDamage(item.m_itemData.m_shared.m_damages, false);

                        if (damage > 0)
                        {
                            totalAttacks++;
                        }

                        if (damage > maxDamage)
                        {
                            maxDamage = damage;
                        }
                    }
                }
            }

            return (totalAttacks, maxDamage);
        }

        /// <summary>
        /// Set the default values for Vanilla creatures using the Creature Prefab Name. (TODO enable configuration of custom creatures)
        /// </summary>
        public void Initialize()
        {
            // Meadow Defaults
            var biome = WorldConfiguration.Biome.Meadow;
            AddCreatureConfiguration("Eikthyr", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("Boar_piggy", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("Boar", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Deer", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Greyling", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Neck", biome, WorldConfiguration.Difficulty.Novice);

            // Black Forest Defaults
            biome = WorldConfiguration.Biome.BlackForest;
            AddCreatureConfiguration("gd_king", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("Ghost", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Greydwarf", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Greydwarf_Elite", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("Greydwarf_Shaman", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Skeleton", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Skeleton_NoArcher", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Skeleton_Poison", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("TentaRoot", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Troll", biome, WorldConfiguration.Difficulty.Expert);

            // Swamp Defaults
            biome = WorldConfiguration.Biome.Swamp;
            AddCreatureConfiguration("Bonemass", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("Abomination", biome, WorldConfiguration.Difficulty.Expert);
            AddCreatureConfiguration("Blob", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("BlobElite", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("Draugr_Elite", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("Draugr", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Draugr_Ranged", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Leech", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Surtling", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Wraith", biome, WorldConfiguration.Difficulty.Intermediate);

            // Mountain Defaults
            biome = WorldConfiguration.Biome.Mountain;
            AddCreatureConfiguration("Dragon", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("Bat", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Fenring_Cultist", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("Fenring", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Hatchling", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("StoneGolem", biome, WorldConfiguration.Difficulty.Expert);
            AddCreatureConfiguration("Ulv", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Wolf_cub", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("Wolf", biome, WorldConfiguration.Difficulty.Novice);

            // Plain Defaults
            biome = WorldConfiguration.Biome.Plain;
            AddCreatureConfiguration("GoblinKing", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("BlobTar", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Deathsquito", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Goblin", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("GoblinArcher", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("GoblinShaman", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("GoblinBrute", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("Lox_Calf", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("Lox", biome, WorldConfiguration.Difficulty.Intermediate);

            // Ocean Defaults
            biome = WorldConfiguration.Biome.Ocean;
            AddCreatureConfiguration("Serpent", biome, WorldConfiguration.Difficulty.Expert);

            // TODO add options for loading configurations from a file after defaults are set

            if (_vanillaBackupCreated) return;
            CreateVanillaBackup();
        }

        /// <summary>
        /// Adds a new CreatureClassification for scaling or optionally replaces the existing if a configuration already exists.
        /// </summary>
        /// <param name="name">Prefab Name</param>
        /// <param name="biome"></param>
        /// <param name="difficulty"></param>
        public void AddCreatureConfiguration(string name, WorldConfiguration.Biome biome, WorldConfiguration.Difficulty difficulty, bool overrideData = false)
        {
            try
            {
                _creatureData.Add(name, new CreatureClassification(name, biome, difficulty));
            }
            catch
            {
                if (overrideData)
                {
                    _creatureData[name] = new CreatureClassification(name, biome, difficulty);
                }
            }
        }

        /// <summary>
        /// Apply Auto-Scaling to all Creatures found in the game by Prefab name.
        /// </summary>
        public void UpdateCreatures()
        {
            CreateVanillaBackup();

            foreach (CreatureClassification creatureClass in _creatureData.Values)
            {
                var creature = ProgressionAPI.Instance.GetHumanoid(creatureClass.Name);
                if (creature != null)
                {
                    var scale = WorldConfiguration.Instance.GetBiomeScaling(creatureClass.BiomeType);
                    float health = ProgressionAPI.Instance.PrettifyNumber(CalculateHealth(creatureClass, scale));
                    UpdateCreature(health, ref creature);
                    ConfigureAttacks(creatureClass);
                }
            }
        }

        private void UpdateCreature(float health, ref Humanoid creature)
        {
            creature.m_health = health;
            ProgressionPlugin.GetProgressionLogger().LogDebug($"{creature.name} with {creature.m_health} health updated to {health}.");
        }

        private void CreateVanillaBackup()
        {
            if (_vanillaBackupCreated) return;

            var prefabs = ZNetScene.m_instance.m_prefabs;
            for (int lcv = 0; lcv < prefabs.Count; lcv++)
            {
                try
                {
                    // Find and set base health
                    Humanoid creature = prefabs[lcv].GetComponent<Humanoid>();
                    if (creature != null)
                    {
                        var config = _creatureData[creature.name];
                        if (config != null)
                        {
                            config.SetVanillaHealth(creature.m_health);

                            // Find all attacks
                            var attacks = new List<string>();
                            var defaults = creature.m_defaultItems;

                            config.SetVanillaAttacks(creature.m_defaultItems);
                        }
                    }
                }
                catch
                {
                    ProgressionPlugin.GetProgressionLogger().LogDebug($"No configuration found for GameObject, skipping: {prefabs[lcv].name}.");
                }
            }

            _vanillaBackupCreated = true;
        }

        public void VanillaReset()
        {
            if (_vanillaBackupCreated)
            {
                foreach (CreatureClassification creatureClass in _creatureData.Values)
                {
                    var creature = ProgressionAPI.Instance.GetHumanoid(creatureClass.Name);
                    if (creature != null)
                    {
                        if (creature.m_health > 0)
                        {
                            creature.m_health = creatureClass.VanillaHealth;
                        }

                        if (creatureClass.VanillaAttacks != null)
                        {
                            foreach (var attack in creatureClass.VanillaAttacks)
                            {
                                ItemDrop item = ProgressionAPI.Instance.GetItemDrop(attack.Key);
                                if (item != null)
                                {
                                    ConfigureAttack(item, attack.Value);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                CreateVanillaBackup();
            }
        }
    }
}