using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace VentureValheim.Progression
{
    public class CreatureConfiguration
    {
        private CreatureConfiguration() { }
        private static readonly CreatureConfiguration _instance = new CreatureConfiguration();

        public static CreatureConfiguration Instance
        {
            get => _instance;
        }

        public class CreatureClassification
        {
            public string Name;
            public WorldConfiguration.Biome BiomeType;
            public WorldConfiguration.Difficulty CreatureDifficulty;
            public List<string>? Attacks;

            public CreatureClassification(string name, WorldConfiguration.Biome biomeType, WorldConfiguration.Difficulty creatureDifficulty, List<string>? attacks)
            {
                Name = name;
                BiomeType = biomeType;
                CreatureDifficulty = creatureDifficulty;
                Attacks = attacks;
            }
        }

        private Dictionary<string, CreatureClassification> _creatureData;
        private int[] _baseHealth = { 5, 10, 30, 50, 200, 500 };
        private int[] _baseDamage = { 0, 5, 10, 12, 15, 20 };

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
                    return Instance._baseHealth[0];
                case WorldConfiguration.Difficulty.Novice:
                    return Instance._baseHealth[1];
                case WorldConfiguration.Difficulty.Average:
                    return Instance._baseHealth[2];
                case WorldConfiguration.Difficulty.Intermediate:
                    return Instance._baseHealth[3];
                case WorldConfiguration.Difficulty.Expert:
                    return Instance._baseHealth[4];
                case WorldConfiguration.Difficulty.Boss:
                    return Instance._baseHealth[5];
                default:
                    // Set default to "Average" difficulty
                    return Instance._baseHealth[2];
            }
        }

        /// <summary>
        /// Calculates the health based on the assigned biome and difficulty of a Creature
        /// </summary>
        /// <param name="cc"></param>
        /// <returns></returns>
        public int CalculateHealth(CreatureClassification cc)
        {
            var scale = WorldConfiguration.GetBiomeScaling(cc.BiomeType);
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
                    return Instance._baseDamage[0];
                case WorldConfiguration.Difficulty.Novice:
                    return Instance._baseDamage[1];
                case WorldConfiguration.Difficulty.Average:
                    return Instance._baseDamage[2];
                case WorldConfiguration.Difficulty.Intermediate:
                    return Instance._baseDamage[3];
                case WorldConfiguration.Difficulty.Expert:
                    return Instance._baseDamage[4];
                case WorldConfiguration.Difficulty.Boss:
                    return Instance._baseDamage[5];
                default:
                    // Set default to "Average" difficulty
                    return Instance._baseDamage[2];
            }
        }

        /// <summary>
        /// Set the base health value for a Character object based on auto-scaling.
        /// </summary>
        /// <param name="character"></param>
        public void Configure(ref Character character)
        {
            var creatureClass = _creatureData[character.name];
            if (creatureClass != null)
            {
                float health = WorldConfiguration.PrettifyNumber(CalculateHealth(creatureClass));
                ProgressionPlugin.GetProgressionLogger().LogDebug($"{character.name} with {character.m_health} health updated to {health}.");
                character.m_health = health;
            }
            else
            {
                ProgressionPlugin.GetProgressionLogger().LogWarning(
                    $"Failed to configure {character.name}: No creature configuration found. Did you forget to define your custom creatures?");
            }
        }

        /// <summary>
        /// Finds the original creature prefab and configures the health and damage
        /// </summary>
        /// <param name="cc"></param>
        public void ConfigureAttacks(CreatureClassification cc)
        {
            if (cc.CreatureDifficulty == WorldConfiguration.Difficulty.Vanilla || cc.Attacks == null || cc.Attacks.Count < 1)
            {
                return;
            }

            try
            {
                float maxTotalDamage = GetMaxCreatureDamage(cc);

                if (maxTotalDamage <= 0)
                {
                    ProgressionPlugin.GetProgressionLogger().LogDebug($"Attacks for {cc.Name} have no damage components. Skipping.");
                    return;
                }

                for (var lcv = 0; lcv < cc.Attacks.Count; lcv++)
                {
                    ItemDrop? item = null;
                    if (!cc.Attacks[lcv].IsNullOrWhiteSpace())
                    {
                        item = ProgressionAPI.GetItemDrop(cc.Attacks[lcv]);
                    }

                    if (item == null)
                    {
                        ProgressionPlugin.GetProgressionLogger().LogWarning(
                            $"Failed to configure \"{cc.Attacks[lcv]}\" for {cc.Name}. Did you forget to define your custom creature attacks?");
                    }
                    else
                    {
                        var original = item.m_itemData.m_shared.m_damages;
                        float sumDamage = ItemConfiguration.Instance.GetTotalDamage(original, false);
                        var baseTotalDamage = GetBaseTotalDamage(cc.CreatureDifficulty);
                        item.m_itemData.m_shared.m_damages = ItemConfiguration.Instance.CalculateCreatureDamageTypes(cc.BiomeType, original, baseTotalDamage, maxTotalDamage);

                        float newSumDamage = ItemConfiguration.Instance.GetTotalDamage(item.m_itemData.m_shared.m_damages, false);
                        ProgressionPlugin.GetProgressionLogger().LogDebug(
                            $"{item.name} updated with new scaled damage values. Total damage changed from {sumDamage} to {newSumDamage}");
                    }
                }
            }
            catch (Exception e)
            {
                ProgressionPlugin.GetProgressionLogger().LogDebug($"Error configuring attacks for {cc.Name}. Skipping.");
                ProgressionPlugin.GetProgressionLogger().LogDebug(e);
            }
        }

        /// <summary>
        /// Calculates the maximum damage done by any one attack in the CreatureClassification.
        /// </summary>
        /// <param name="cc"></param>
        /// <returns>Exception if the CreatureClassification Attacks list is null.</returns>
        private float GetMaxCreatureDamage(CreatureClassification cc)
        {
            float maxDamage = 0;

            for (var lcv = 0; lcv < cc.Attacks.Count; lcv++)
            {
                ItemDrop? item = null;
                if (!cc.Attacks[lcv].IsNullOrWhiteSpace())
                {
                    item = ProgressionAPI.GetItemDrop(cc.Attacks[lcv]);
                }

                if (item != null)
                {
                    var damage = ItemConfiguration.Instance.GetTotalDamage(item.m_itemData.m_shared.m_damages, false);

                    if (damage > maxDamage)
                    {
                        maxDamage = damage;
                    }
                }
            }

            return maxDamage;
        }

        /// <summary>
        /// Set the default values for Vanilla creatures using the Creature Prefab Name. (TODO enable configuration of custom creatures)
        /// </summary>
        public void Initialize()
        {
            // Notes: Many attacks do not do any damage, if that changes they will need to be added in

            _creatureData = new Dictionary<string, CreatureClassification>();

            // Meadow Defaults
            var biome = WorldConfiguration.Biome.Meadow;
            AddCreatureConfiguration("Eikthyr", biome, WorldConfiguration.Difficulty.Boss,
                new List<string> {"Eikthyr_antler", "Eikthyr_charge", "Eikthyr_stomp"});
            AddCreatureConfiguration("Boar_piggy", biome, WorldConfiguration.Difficulty.Harmless, null);
            AddCreatureConfiguration("Boar", biome, WorldConfiguration.Difficulty.Novice,
                new List<string> {"boar_base_attack"});
            AddCreatureConfiguration("Deer", biome, WorldConfiguration.Difficulty.Novice, null);
            AddCreatureConfiguration("Greyling", biome, WorldConfiguration.Difficulty.Average,
                new List<string> {"Greyling_attack"});
            AddCreatureConfiguration("Neck", biome, WorldConfiguration.Difficulty.Novice,
                new List<string> {"Neck_BiteAttack"});

            // Black Forest Defaults
            biome = WorldConfiguration.Biome.BlackForest;
            AddCreatureConfiguration("gd_king", biome, WorldConfiguration.Difficulty.Boss,
                new List<string> {"gd_king_shoot", "gd_king_stomp"});
            AddCreatureConfiguration("Ghost", biome, WorldConfiguration.Difficulty.Average,
                new List<string> {"Ghost_attack"});
            AddCreatureConfiguration("Greydwarf", biome, WorldConfiguration.Difficulty.Novice,
                new List<string> {"Greydwarf_attack", "Greydwarf_throw"});
            AddCreatureConfiguration("Greydwarf_Elite", biome, WorldConfiguration.Difficulty.Intermediate,
                new List<string> {"Greydwarf_elite_attack"});
            AddCreatureConfiguration("Greydwarf_Shaman", biome, WorldConfiguration.Difficulty.Average,
                new List<string> {"Greydwarf_shaman_attack" });
            AddCreatureConfiguration("Skeleton", biome, WorldConfiguration.Difficulty.Average,
                new List<string> {"skeleton_bow"});
            AddCreatureConfiguration("Skeleton_NoArcher", biome, WorldConfiguration.Difficulty.Average,
                new List<string> {"skeleton_sword"});
            AddCreatureConfiguration("Skeleton_Poison", biome, WorldConfiguration.Difficulty.Intermediate,
                new List<string> {"skeleton_mace"});
            AddCreatureConfiguration("TentaRoot", biome, WorldConfiguration.Difficulty.Average,
                new List<string> {"tentaroot_attack"});
            AddCreatureConfiguration("Troll", biome, WorldConfiguration.Difficulty.Expert,
                new List<string> {"troll_groundslam", "troll_log_swing_h", "troll_log_swing_v", "troll_punch", "troll_throw" });

            // Swamp Defaults
            biome = WorldConfiguration.Biome.Swamp;
            AddCreatureConfiguration("Bonemass", biome, WorldConfiguration.Difficulty.Boss,
                new List<string> {"bonemass_attack_aoe", "bonemass_attack_punch"});
            AddCreatureConfiguration("Abomination", biome, WorldConfiguration.Difficulty.Expert,
                new List<string> {"Abomination_attack1", "Abomination_attack2", "Abomination_attack3"});
            AddCreatureConfiguration("Blob", biome, WorldConfiguration.Difficulty.Average,
                new List<string> {"blob_attack_aoe"});
            AddCreatureConfiguration("BlobElite", biome, WorldConfiguration.Difficulty.Intermediate,
                new List<string> {"blobelite_attack_aoe"});
            AddCreatureConfiguration("Draugr_Elite", biome, WorldConfiguration.Difficulty.Intermediate,
                new List<string> {"draugr_sword"});
            AddCreatureConfiguration("Draugr", biome, WorldConfiguration.Difficulty.Average,
                new List<string> {"draugr_axe"});
            AddCreatureConfiguration("Draugr_Ranged", biome, WorldConfiguration.Difficulty.Average,
                new List<string> {"draugr_arrow", "draugr_bow"});
            AddCreatureConfiguration("Leech", biome, WorldConfiguration.Difficulty.Novice,
                new List<string> {"Leech_BiteAttack"});
            AddCreatureConfiguration("Surtling", biome, WorldConfiguration.Difficulty.Novice,
                new List<string> {"imp_fireball_attack"});
            AddCreatureConfiguration("Wraith", biome, WorldConfiguration.Difficulty.Intermediate,
                new List<string> {"wraith_melee"});

            // Mountain Defaults
            biome = WorldConfiguration.Biome.Mountain;
            AddCreatureConfiguration("Dragon", biome, WorldConfiguration.Difficulty.Boss,
                new List<string> {"dragon_bite", "dragon_claw_left", "dragon_claw_right", "dragon_coldbreath", "dragon_spit_shotgun"});
            AddCreatureConfiguration("Bat", biome, WorldConfiguration.Difficulty.Novice,
                new List<string> {"bat_melee"});
            AddCreatureConfiguration("Fenring_Cultist", biome, WorldConfiguration.Difficulty.Intermediate,
                new List<string> {"Fenring_attack_fireclaw", "Fenring_attack_fireclaw_double", "Fenring_attack_flames"});
            AddCreatureConfiguration("Fenring", biome, WorldConfiguration.Difficulty.Average,
                new List<string> {"Fenring_attack_claw", "Fenring_attack_jump"});
            AddCreatureConfiguration("Hatchling", biome, WorldConfiguration.Difficulty.Average,
                new List<string> {"hatchling_spit_cold"});
            AddCreatureConfiguration("StoneGolem", biome, WorldConfiguration.Difficulty.Expert,
                new List<string> {"stonegolem_attack_doublesmash", "stonegolem_attack1_spike", "stonegolem_attack2_left_groundslam", "stonegolem_attack3_spikesweep"});
            AddCreatureConfiguration("Ulv", biome, WorldConfiguration.Difficulty.Average,
                new List<string> {"Ulv_attack1_bite", "Ulv_attack2_slash"});
            AddCreatureConfiguration("Wolf_cub", biome, WorldConfiguration.Difficulty.Harmless, null);
            AddCreatureConfiguration("Wolf", biome, WorldConfiguration.Difficulty.Novice,
                new List<string> {"Wolf_Attack1", "Wolf_Attack2", "Wolf_Attack3"});

            // Plain Defaults
            biome = WorldConfiguration.Biome.Plain;
            AddCreatureConfiguration("GoblinKing", biome, WorldConfiguration.Difficulty.Boss,
                new List<string> {"GoblinKing_Beam", "GoblinKing_Nova"});
            AddCreatureConfiguration("BlobTar", biome, WorldConfiguration.Difficulty.Novice,
                new List<string> {"blobtar_attack"});
            AddCreatureConfiguration("Deathsquito", biome, WorldConfiguration.Difficulty.Novice,
                new List<string> {"Deathsquito_sting"});
            AddCreatureConfiguration("Goblin", biome, WorldConfiguration.Difficulty.Average,
                new List<string> {"GoblinClub", "GoblinSword", "GoblinTorch"});
            AddCreatureConfiguration("GoblinArcher", biome, WorldConfiguration.Difficulty.Average,
                new List<string> { "GoblinSpear"});
            AddCreatureConfiguration("GoblinShaman", biome, WorldConfiguration.Difficulty.Novice,
                new List<string> {"GoblinShaman_attack_fireball", "GoblinShaman_attack_poke", "GoblinShaman_Staff_Bones", "GoblinShaman_Staff_Feathers"});
            AddCreatureConfiguration("GoblinBrute", biome, WorldConfiguration.Difficulty.Intermediate,
                new List<string> {"GoblinBrute_Attack", "GoblinBrute_RageAttack", "GoblinBrute_Taunt"});
            AddCreatureConfiguration("Lox_Calf", biome, WorldConfiguration.Difficulty.Harmless, null);
            AddCreatureConfiguration("Lox", biome, WorldConfiguration.Difficulty.Intermediate,
                new List<string> {"lox_bite", "lox_stomp"});

            // Ocean Defaults
            biome = WorldConfiguration.Biome.Ocean;
            AddCreatureConfiguration("Serpent", biome, WorldConfiguration.Difficulty.Expert,
                new List<string> {"Serpent_attack", "Serpent_taunt"});

            // TODO add options for loading configurations from a file after defaults are set
        }

        /// <summary>
        /// Adds a new CreatureClassification for scaling or replaces the existing if a configuration already exists.
        /// </summary>
        /// <param name="name">Prefab Name</param>
        /// <param name="biome"></param>
        /// <param name="difficulty"></param>
        /// <param name="attacks"></param>
        public void AddCreatureConfiguration(string name, WorldConfiguration.Biome biome, WorldConfiguration.Difficulty difficulty, List<string>? attacks)
        {
            try
            {
                _creatureData.Add(name, new CreatureClassification(name, biome, difficulty, attacks));
            }
            catch (Exception e)
            {
                // Replace exisitng configuration
                _creatureData[name] = new CreatureClassification(name, biome, difficulty, attacks);
            }
        }

        /// <summary>
        /// Apply Auto-Scaling to all Creatures found in the game by Prefab name.
        /// </summary>
        private static void UpdateCreatures()
        {
            var prefabs = ZNetScene.m_instance.m_prefabs;
            for (int lcv = 0; lcv < prefabs.Count; lcv++)
            {
                try
                {
                    Character character = prefabs[lcv].GetComponent<Character>();
                    if (character != null)
                    {
                        Instance.Configure(ref character);
                    }
                }
                catch (Exception e)
                {
                    ProgressionPlugin.GetProgressionLogger().LogDebug($"No configuration found for GameObject, skipping: {prefabs[lcv].name}.");
                }
            }

            // TODO check for duplicated values for attacks, warn or remove these configurations to prevent overriding data multiple times

            foreach (CreatureClassification data in Instance._creatureData.Values)
            {
                Instance.ConfigureAttacks(data);
            }
        }

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        public static class Patch_ObjectDB_Awake
        {
            private static void Prefix()
            {
                if (!ProgressionPlugin.Instance.GetAutoScaleCreatures())
                {
                    return;
                }

                try
                {
                    if (SceneManager.GetActiveScene().name.Equals("main"))
                    {
                        if (WorldConfiguration.Instance.GetWorldScale() != (int)WorldConfiguration.Scaling.Vanilla)
                        {
                            ProgressionPlugin.GetProgressionLogger().LogDebug("Updating Creature Configurations with auto-scaling...");
                            UpdateCreatures();
                        }
                    }
                    else
                    {
                        ProgressionPlugin.GetProgressionLogger().LogDebug("Skipping generating data because not in the main scene.");
                    }
                }
                catch (Exception e)
                {
                    ProgressionPlugin.GetProgressionLogger().LogError($"Failure configuring Creature data. Game may behave unexpectedly.");
                    ProgressionPlugin.GetProgressionLogger().LogError(e);
                }
            }
        }
    }
}