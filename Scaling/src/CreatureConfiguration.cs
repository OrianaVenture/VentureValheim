using BepInEx;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.Scaling
{
    public interface ICreatureConfiguration
    {
        public bool ContainsCreature(string key);
        public void SetBaseHealth(int[] values);
        public int GetBaseHealth(WorldConfiguration.Difficulty difficulty);
        public void SetBaseDamage(int[] values);
        public int GetBaseTotalDamage(WorldConfiguration.Difficulty difficulty);
        public void AddCreatureConfiguration(string name, WorldConfiguration.Biome? biome, WorldConfiguration.Difficulty? difficulty);
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

        protected Dictionary<string, CreatureClassification> _creatureData = new Dictionary<string, CreatureClassification>();
        private int[] _baseHealth = { 5, 10, 30, 50, 200, 500 };
        private int[] _baseDamage = { 5, 15, 20, 25, 30, 30 };

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
        /// Calculates the health from the base health and scale value.
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="baseHealth"></param>
        /// <returns></returns>
        public int CalculateHealth(float scale, int baseHealth)
        {
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
        /// Finds the creature ItemDrop attack prefabs and configures damages.
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
                var maxTotalDamage = GetMaxCreatureDamage(cc.VanillaAttacks);
                if (maxTotalDamage <= 0)
                {
                    return;
                }

                foreach (var attack in cc.VanillaAttacks)
                {
                    ScalingAPI.GetItemDrop(attack.Key, out ItemDrop item);
                    if (item != null)
                    {
                        var damage = cc.GetAttack(attack.Key, maxTotalDamage);
                        ConfigureAttack(ref item, damage);
                    }
                    else
                    {
                        ScalingPlugin.VentureScalingLogger.LogWarning(
                            $"Failed to configure \"{attack.Key}\" for {cc.Name}. ItemDrop data does not exist!");
                    }
                }
            }
            catch (Exception e)
            {
                ScalingPlugin.VentureScalingLogger.LogDebug($"Error configuring attacks for {cc.Name}. Skipping.");
                ScalingPlugin.VentureScalingLogger.LogDebug(e);
            }
        }

        private void ConfigureAttack(ref ItemDrop item, HitData.DamageTypes? value)
        {
            // Do not send upgrade information in this call, it is not used for non-player items.
            // If player items and attacks become decoupled this will need an update to
            // ensure that the item is not being updated twice.
            // If attacks can be upgraded this will need to be updated.
            ItemConfiguration.Instance.UpdateWeapon(ref item, value, 1, null, false);
        }

        /// <summary>
        /// Calculates the maximum damage done by any one attack in the CreatureClassification's original attack values.
        /// Counts the total number of attacks that do damage.
        /// </summary>
        /// <param name="attacks"></param>
        /// <returns>Highest attack value in the group</returns>
        protected float GetMaxCreatureDamage(Dictionary<string, HitData.DamageTypes> attacks)
        {
            float maxDamage = 0;

            if (attacks != null)
            {
                foreach (var attack in attacks.Values)
                {
                    float damage = ItemConfiguration.Instance.GetTotalDamage(attack);

                    if (damage > maxDamage)
                    {
                        maxDamage = damage;
                    }
                }
            }

            return maxDamage;
        }

        /// <summary>
        /// Set the default values for Vanilla creatures using the Creature Prefab Name.
        /// </summary>
        public void Initialize()
        {
            foreach (CreatureClassification data in _creatureData.Values)
            {
                data.Reset();
            }

            // Meadow Defaults
            var biome = WorldConfiguration.Biome.Meadow;
            AddCreatureConfiguration("Eikthyr", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("Boar_piggy", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("Boar", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Deer", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Greyling", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Neck", biome, WorldConfiguration.Difficulty.Harmless);

            // Black Forest Defaults
            biome = WorldConfiguration.Biome.BlackForest;
            AddCreatureConfiguration("gd_king", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("Skeleton_Hildir", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("Ghost", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("Greydwarf", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Greydwarf_Elite", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("Greydwarf_Shaman", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Skeleton", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Skeleton_NoArcher", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Skeleton_Poison", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("TentaRoot", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Troll", biome, WorldConfiguration.Difficulty.Expert);

            // Swamp Defaults
            biome = WorldConfiguration.Biome.Swamp;
            AddCreatureConfiguration("Bonemass", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("Abomination", biome, WorldConfiguration.Difficulty.Expert);
            AddCreatureConfiguration("Blob", biome, WorldConfiguration.Difficulty.Novice);
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
            AddCreatureConfiguration("Skeleton_Hildir", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("Bat", biome, WorldConfiguration.Difficulty.Harmless);
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
            AddCreatureConfiguration("Deathsquito", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("Goblin", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("GoblinArcher", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("GoblinShaman", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("GoblinBrute", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("Lox_Calf", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("Lox", biome, WorldConfiguration.Difficulty.Intermediate);

            // Ocean Defaults
            biome = WorldConfiguration.Biome.Ocean;
            AddCreatureConfiguration("Serpent", biome, WorldConfiguration.Difficulty.Expert);

            // Mistlands Defaults
            biome = WorldConfiguration.Biome.Mistland;
            AddCreatureConfiguration("SeekerQueen", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("Dverger", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("DvergerMage", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("DvergerMageFire", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("DvergerMageIce", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("DvergerMageSupport", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("Gjall", biome, WorldConfiguration.Difficulty.Expert);
            AddCreatureConfiguration("Hare", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("Seeker", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("SeekerBrood", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("SeekerBrute", biome, WorldConfiguration.Difficulty.Expert);
            AddCreatureConfiguration("Tick", biome, WorldConfiguration.Difficulty.Novice);

            // Ashlands Defaults
            biome = WorldConfiguration.Biome.AshLand;
            AddCreatureConfiguration("Fader", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("Asksvin", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("Asksvin_hatchling", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("BlobLava", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("BonemawSerpent", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("Charred_Archer", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Charred_Archer_Fader", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Charred_Mage", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("Charred_Melee", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("Charred_Melee_Dyrnwyn", biome, WorldConfiguration.Difficulty.Expert);
            AddCreatureConfiguration("Charred_Melee_Fader", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("Charred_Twitcher", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Charred_Twitcher_Summoned", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("FallenValkyrie", biome, WorldConfiguration.Difficulty.Expert);
            AddCreatureConfiguration("Morgen", biome, WorldConfiguration.Difficulty.Expert);
            AddCreatureConfiguration("Morgen_NonSleeping", biome, WorldConfiguration.Difficulty.Expert);
            AddCreatureConfiguration("Volture", biome, WorldConfiguration.Difficulty.Average);

            // Summons Defaults
            AddCreatureConfiguration("staff_greenroots_tentaroot",
                WorldConfiguration.Biome.AshLand, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Skeleton_Friendly",
                WorldConfiguration.Biome.Mistland, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("Troll_Summoned",
                WorldConfiguration.Biome.AshLand, WorldConfiguration.Difficulty.Expert);


            if (WorldConfiguration.Instance.WorldScale != WorldConfiguration.Scaling.Vanilla &&
                !ScalingConfiguration.Instance.GetAutoScaleIgnoreOverrides())
            {
                ReadCustomValues();
            }

            CreateVanillaBackup();
        }

        /// <summary>
        /// Adds a new CreatureClassification for scaling or updates the existing if a configuration already exists.
        /// </summary>
        /// <param name="name">Prefab Name</param>
        /// <param name="biome"></param>
        /// <param name="difficulty"></param>
        public void AddCreatureConfiguration(string name, WorldConfiguration.Biome? biome, WorldConfiguration.Difficulty? difficulty)
        {
            if (name.IsNullOrWhiteSpace())
            {
                return;
            }

            if (_creatureData.ContainsKey(name))
            {
                _creatureData[name].UpdateCreature(biome, difficulty);
            }
            else
            {
                _creatureData.Add(name, new CreatureClassification(name, biome, difficulty));
            }
        }

        /// <summary>
        /// Create a new entry or override an existing.
        /// </summary>
        /// <param name="creatureOverride"></param>
        public void AddCreatureConfiguration(CreatureOverrides.CreatureOverride creatureOverride)
        {
            if (creatureOverride == null || creatureOverride.name.IsNullOrWhiteSpace())
            {
                return;
            }

            AddCreatureConfiguration(creatureOverride.name, null, null);

            _creatureData[creatureOverride.name].OverrideCreature(creatureOverride);

        }

        /// <summary>
        /// Apply Auto-Scaling to all initialized Creatures found in the game by Prefab name.
        /// </summary>
        public void UpdateCreatures()
        {
            foreach (CreatureClassification cc in _creatureData.Values)
            {
                var creature = ScalingAPI.GetHumanoid(cc.Name);
                if (creature != null)
                {
                    var health = cc.GetHealth();
                    if (health != null)
                    {
                        UpdateCreature(health.Value, ref creature);
                    }

                    ConfigureAttacks(cc);
                }
            }
        }

        /// <summary>
        /// Updates a creature's health.
        /// </summary>
        /// <param name="health"></param>
        /// <param name="creature"></param>
        private void UpdateCreature(float health, ref Humanoid creature)
        {
            var original = creature.m_health;
            creature.m_health = health;
            ScalingPlugin.VentureScalingLogger.LogDebug($"{creature.name}: Health updated from {original} to {creature.m_health}.");
        }

        /// <summary>
        /// Sets the vanilla data from the current value of every creature currently classified.
        /// </summary>
        protected virtual void CreateVanillaBackup()
        {
            ScalingPlugin.VentureScalingLogger.LogInfo("Configuring vanilla backup for Creature data...");

            foreach (CreatureClassification cc in _creatureData.Values)
            {
                var creature = ScalingAPI.GetHumanoid(cc.Name);
                if (creature != null)
                {
                    var attacks = new List<GameObject>();
                    if (creature.m_defaultItems != null)
                    {
                        attacks.AddRange(creature.m_defaultItems);
                    }

                    if (creature.m_randomWeapon != null)
                    {
                        attacks.AddRange(creature.m_randomWeapon);
                    }

                    if (creature.m_randomSets != null)
                    {
                        for (int lcv = 0; lcv < creature.m_randomSets.Length; lcv++)
                        {
                            if (creature.m_randomSets[lcv].m_items != null)
                            {
                                attacks.AddRange(creature.m_randomSets[lcv].m_items);
                            }
                        }
                    }

                    cc.SetVanillaData(creature.m_health, attacks);
                }
            }
        }

        /// <summary>
        /// Reset creatures to their original values given they have been assigned.
        /// Creates a vanilla backup if not already assigned.
        /// </summary>
        public void VanillaReset()
        {
            foreach (CreatureClassification creatureClass in _creatureData.Values)
            {
                var creature = ScalingAPI.GetHumanoid(creatureClass.Name);
                if (creature != null)
                {
                    if (creatureClass.VanillaHealth != null)
                    {
                        creature.m_health = creatureClass.VanillaHealth.Value;
                    }

                    if (creatureClass.VanillaAttacks != null)
                    {
                        foreach (var attack in creatureClass.VanillaAttacks)
                        {
                            ScalingAPI.GetItemDrop(attack.Key, out ItemDrop item);
                            if (item != null)
                            {
                                ConfigureAttack(ref item, attack.Value);
                            }
                        }
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
                CreatureOverrides.CreatureOverridesList creatures = CreatureOverrides.ReadYaml();

                if (creatures != null)
                {
                    ScalingPlugin.VentureScalingLogger.LogDebug("Deserializer successfully parsed yaml data.");
                    ScalingPlugin.VentureScalingLogger.LogDebug(creatures.ToString());

                    foreach (var entry in creatures.creatures)
                    {
                        AddCreatureConfiguration(entry);
                    }
                }
            }
            catch (Exception e)
            {
                ScalingPlugin.VentureScalingLogger.LogWarning("Error loading VWS.CreatureOverrides.yaml file.");
                ScalingPlugin.VentureScalingLogger.LogWarning(e);
                ScalingPlugin.VentureScalingLogger.LogWarning("Continuing without custom values...");
            }
        }
    }
}