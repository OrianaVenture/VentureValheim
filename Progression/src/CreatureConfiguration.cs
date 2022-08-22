using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;
using static Character;

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

        [Serializable]
        public class CreatureData
        {
            public string m_name;
            public Faction m_faction;
            public string m_defeatSetGlobalKey;
            public float m_health;

            public CreatureData(string name, Faction faction, string defeatSetGlobalKey, float health)
            {
                m_name = name;
                m_faction = faction;
                m_defeatSetGlobalKey = defeatSetGlobalKey;
                m_health = health;
            }
        }

        public class CreatureClassification
        {
            public string Name;
            public int BiomeType;
            public WorldConfiguration.Difficulty CreatureDifficulty;

            public CreatureClassification(string name, int biomeType, WorldConfiguration.Difficulty creatureDifficulty)
            {
                Name = name;
                BiomeType = biomeType;
                CreatureDifficulty = creatureDifficulty;
            }

            /// <summary>
            /// Calculates the base health based on the assigned biome and difficulty of a Creature
            /// </summary>
            /// <returns></returns>
            public int CalculateHealth()
            {
                var scale = WorldConfiguration.GetBiomeScaling(BiomeType);
                int baseHealth;
                switch (CreatureDifficulty)
                {
                    case WorldConfiguration.Difficulty.Harmless:
                        baseHealth = Instance.GetBaseHealth(0);
                        break;
                    case WorldConfiguration.Difficulty.Novice:
                        baseHealth = Instance.GetBaseHealth(1);
                        break;
                    case WorldConfiguration.Difficulty.Average:
                        baseHealth = Instance.GetBaseHealth(2);
                        break;
                    case WorldConfiguration.Difficulty.Intermediate:
                        baseHealth = Instance.GetBaseHealth(3);
                        break;
                    case WorldConfiguration.Difficulty.Expert:
                        baseHealth = Instance.GetBaseHealth(4);
                        break;
                    case WorldConfiguration.Difficulty.Boss:
                        baseHealth = Instance.GetBaseHealth(5);
                        break;
                    default:
                        // Set defualt to "Average" difficulty
                        baseHealth = Instance.GetBaseHealth(2);
                        break;
                }

                return (int)(scale * baseHealth);
            }
        }

        private Dictionary<string, CreatureClassification> _creatureData;
        private int[] BaseHealth = { 5, 10, 30, 50, 200, 500 };

        /// <summary>
        /// Replaces the base health distribution values are in the correct format.
        /// </summary>
        /// <param name="values"></param>
        public void SetBaseHealth(int[] values)
        {
            if (values != null && values.Length == 6)
            {
                BaseHealth = values;
            }
        }

        /// <summary>
        /// Get the base health for scaling from the default list
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int GetBaseHealth(int index)
        {
            if (index < 0 || index >= BaseHealth.Length)
            {
                ProgressionPlugin.VentureProgressionLogger.LogWarning("Index out of range, using Average Health Value.");
                return BaseHealth[2];
            }
            return BaseHealth[index];
        }

        /// <summary>
        /// Set the base health value for a Character object based on auto-scaling
        /// </summary>
        /// <param name="chracter"></param>
        public void Configure(ref Character character)
        {
            if (WorldConfiguration.Instance.GetWorldScale() == (int)WorldConfiguration.Scaling.Vanilla)
            {
                return; // TODO consider removing this check
            }

            var creatureClass = _creatureData[character.m_name];
            float health;
            if (creatureClass != null)
            {
                health = WorldConfiguration.PrettifyNumber(creatureClass.CalculateHealth());
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"{character.m_name} with {character.m_health} health updated to {health}.");
                character.m_health = health;
            }
            else
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Failed to configure {character.m_name}: No creature configuration found. Did you forget to define your custom creatures?");
            }
        }

        /// <summary>
        /// Set the default values for Vanilla creatures. (TODO enable configuration of custom creatures)
        /// </summary>
        public void Initialize()
        {
            _creatureData = new Dictionary<string, CreatureClassification>();

            // Meadow Defaults
            var biome = WorldConfiguration.Biome.Meadow;
            AddCreatureConfiguration("$enemy_eikthyr", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("$enemy_boarpiggy", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("$enemy_boar", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("$enemy_deer", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("$enemy_greyling", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("$enemy_neck", biome, WorldConfiguration.Difficulty.Novice);

            // Black Forest Defaults
            biome = WorldConfiguration.Biome.BlackForest;
            AddCreatureConfiguration("$enemy_gdking", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("$enemy_ghost", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("$enemy_greydwarf", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("$enemy_greydwarfbrute", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("$enemy_greydwarfshaman", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("$enemy_skeleton", biome, WorldConfiguration.Difficulty.Average); // Check this
            AddCreatureConfiguration("$enemy_skeletonpoison", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("Root", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("$enemy_troll", biome, WorldConfiguration.Difficulty.Expert);

            // Swamp Defaults
            biome = WorldConfiguration.Biome.Swamp;
            AddCreatureConfiguration("$enemy_bonemass", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("$enemy_abomination", biome, WorldConfiguration.Difficulty.Expert);
            AddCreatureConfiguration("$enemy_blob", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("$enemy_blobelite", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("$enemy_draugrelite", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("$enemy_draugr", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("$enemy_leech", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("$enemy_surtling", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("$enemy_wraith", biome, WorldConfiguration.Difficulty.Novice);

            // Mountain Defaults
            biome = WorldConfiguration.Biome.Mountain;
            AddCreatureConfiguration("$enemy_dragon", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("$enemy_bat", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("$enemy_fenringcultist", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("$enemy_fenring", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("$enemy_drake", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("$enemy_stonegolem", biome, WorldConfiguration.Difficulty.Expert);
            AddCreatureConfiguration("$enemy_ulv", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("$enemy_wolfcub", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("$enemy_wolf", biome, WorldConfiguration.Difficulty.Novice);

            // Plain Defaults
            biome = WorldConfiguration.Biome.Plain;
            AddCreatureConfiguration("$enemy_goblinking", biome, WorldConfiguration.Difficulty.Boss);
            AddCreatureConfiguration("$enemy_blobtar", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("$enemy_deathsquito", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("$enemy_goblin", biome, WorldConfiguration.Difficulty.Average);
            AddCreatureConfiguration("$enemy_goblinshaman", biome, WorldConfiguration.Difficulty.Novice);
            AddCreatureConfiguration("$enemy_goblinbrute", biome, WorldConfiguration.Difficulty.Intermediate);
            AddCreatureConfiguration("$enemy_loxcalf", biome, WorldConfiguration.Difficulty.Harmless);
            AddCreatureConfiguration("$enemy_lox", biome, WorldConfiguration.Difficulty.Intermediate);

            // Ocean Defaults (Uses Meadow's difficulty by default)
            biome = WorldConfiguration.Biome.Ocean;
            AddCreatureConfiguration("$enemy_serpent", biome, WorldConfiguration.Difficulty.Expert);

            // TODO add options for loading configurations from a file after defauts are set
        }

        public void Initialize(int[] baseHealthValues)
        {
            SetBaseHealth(baseHealthValues);
            Initialize();
        }

        /// <summary>
        /// Adds a new CreatureClassification for scaling or replaces the existing if a configuration already exists.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="biome"></param>
        /// <param name="difficulty"></param>
        public void AddCreatureConfiguration(string name, WorldConfiguration.Biome biome, WorldConfiguration.Difficulty difficulty)
        {
            try
            {
                _creatureData.Add(name, new CreatureClassification(name, (int)biome, difficulty));
            }
            catch (Exception e)
            {
                ProgressionPlugin.VentureProgressionLogger.LogWarning($"Creature Configuration already exists, replacing value for {name}.");
                _creatureData[name] = new CreatureClassification(name, (int)biome, difficulty);
            }
        }

        /// <summary>
        /// Apply Auto-Scaling to all Creatures found in the game.
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
                    ProgressionPlugin.VentureProgressionLogger.LogDebug($"No configuration found for Character, skipping: {prefabs[lcv].name}.");
                }
            }
        }

        private static void GenerateData(bool overwrite)
        {
            var path = $"{Paths.ConfigPath}{Path.DirectorySeparatorChar}{"ItemDropData"}";
            if (!Directory.Exists(path) || overwrite == true)
            {
                Directory.CreateDirectory(path);

                foreach (GameObject obj in ObjectDB.instance.m_items)
                {
                    try
                    {
                        ItemDrop itemDrop = obj.GetComponent<ItemDrop>();
                        var filePath = $"{path}{Path.DirectorySeparatorChar}{itemDrop.name}.json";
                        File.WriteAllText(filePath, JsonUtility.ToJson(itemDrop, true));
                        ProgressionPlugin.VentureProgressionLogger.LogDebug($"{itemDrop.name} data written to file: {filePath}.");
                    }
                    catch (Exception e)
                    {
                        ProgressionPlugin.VentureProgressionLogger.LogDebug($"Failed to write to file for GameObject: {obj.name}.");
                    }
                }
            }

            path = $"{Paths.ConfigPath}{Path.DirectorySeparatorChar}{"RecipeData"}";
            if (!Directory.Exists(path) || overwrite == true)
            {
                Directory.CreateDirectory(path);

                foreach (Recipe obj in ObjectDB.instance.m_recipes)
                {
                    try
                    {
                        ItemDrop itemDrop = obj.m_item.GetComponent<ItemDrop>();
                        var filePath = $"{path}{Path.DirectorySeparatorChar}{itemDrop.name}.json";
                        File.WriteAllText(filePath, JsonUtility.ToJson(itemDrop, true));
                        ProgressionPlugin.VentureProgressionLogger.LogDebug($"{itemDrop.name} data written to file: {filePath}.");
                    }
                    catch (Exception e)
                    {
                        ProgressionPlugin.VentureProgressionLogger.LogDebug($"Failed to write to file for GameObject: {obj.name}.");
                    }
                }
            }


            path = $"{Paths.ConfigPath}{Path.DirectorySeparatorChar}{"CreatureData"}";
            if (!Directory.Exists(path) || overwrite == true)
            {
                Directory.CreateDirectory(path);

                foreach (GameObject obj in ZNetScene.m_instance.m_prefabs)
                {
                    try
                    {
                        Character character = obj.GetComponent<Character>();
                        if (character != null)
                        {
                            //_creatures.Add(obj);
                            var filePath = $"{path}{Path.DirectorySeparatorChar}{character.name}.json";
                            File.WriteAllText(filePath, JsonUtility.ToJson(character, true));
                            ProgressionPlugin.VentureProgressionLogger.LogDebug($"{obj.name} data written to file: {filePath}.");
                        }

                    }
                    catch (Exception e)
                    {
                        ProgressionPlugin.VentureProgressionLogger.LogDebug($"Failed to write to file for GameObject: {obj.name}.");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        public static class Patch_ObjectDB_Awake
        {
            private static void Prefix()
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug("CreatureConfiguration.Patch_ObjectDB_Awake called.");
                var regenerateData = false; // TODO add config options for this maybe?

                if (SceneManager.GetActiveScene().name.Equals("main"))
                {
                    GenerateData(regenerateData);
                    if (WorldConfiguration.Instance.GetWorldScale() != (int)WorldConfiguration.Scaling.Vanilla)
                    {
                        UpdateCreatures();
                    }
                }
                else
                {
                    ProgressionPlugin.VentureProgressionLogger.LogDebug("Skipping generating data because not in the main scene.");
                }
            }
        }
    }
}