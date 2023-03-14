using BepInEx;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace VentureValheim.Progression
{
    public interface IProgressionAPI
    {
    }

    [PublicAPI]
    public class ProgressionAPI : IProgressionAPI
    {
        static ProgressionAPI() { }
        protected ProgressionAPI() { }
        private static readonly ProgressionAPI _instance = new ProgressionAPI();

        public static ProgressionAPI Instance
        {
            get => _instance;
        }

        public void Initialize()
        {
        }

        /// <summary>
        /// Adds a configuration for the given biome with the specified scaling order.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="order"></param>
        /// <param name="overrideBiome"></param>
        public void AddCustomBiome(Heightmap.Biome biome, int order, bool overrideBiome = false)
        {
            var internalBiome = GetProgressionBiome(biome);
            WorldConfiguration.Instance.AddBiome(internalBiome, order, overrideBiome);
        }

        /// <summary>
        /// Adds a configuration for the given biome with the specified custom scaling value.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="scale"></param>
        /// <param name="overrideBiome"></param>
        public void AddCustomBiome(Heightmap.Biome biome, int order, float scale, bool overrideBiome = false)
        {
            var internalBiome = GetProgressionBiome(biome);
            WorldConfiguration.Instance.AddCustomBiome(internalBiome, scale, order, overrideBiome);
        }

        /// <summary>
        /// Convert game Heightmap.Biome to internal Biome type
        /// </summary>
        /// <param name="biome"></param>
        /// <returns></returns>
        public WorldConfiguration.Biome GetProgressionBiome(Heightmap.Biome biome)
        {
            switch (biome)
            {
                case Heightmap.Biome.Meadows:
                    return WorldConfiguration.Biome.Meadow;
                case Heightmap.Biome.BlackForest:
                    return WorldConfiguration.Biome.BlackForest;
                case Heightmap.Biome.Swamp:
                    return WorldConfiguration.Biome.Swamp;
                case Heightmap.Biome.Mountain:
                    return WorldConfiguration.Biome.Mountain;
                case Heightmap.Biome.Plains:
                    return WorldConfiguration.Biome.Plain;
                case Heightmap.Biome.AshLands:
                    return WorldConfiguration.Biome.AshLand;
                case Heightmap.Biome.DeepNorth:
                    return WorldConfiguration.Biome.DeepNorth;
                case Heightmap.Biome.Ocean:
                    return WorldConfiguration.Biome.Ocean;
                case Heightmap.Biome.Mistlands:
                    return WorldConfiguration.Biome.Mistland;
            }

            return WorldConfiguration.Biome.Undefined;
        }

        /// <summary>
        /// Convert game ItemDrop.ItemData.ItemType to internal ItemType type
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /*public ItemConfiguration.ItemType GetProgressionItemType(ItemDrop.ItemData.ItemType item)
        {
            switch (item)
            {
                case ItemDrop.ItemData.ItemType.None:
                    return ItemConfiguration.ItemType.None;
                case ItemDrop.ItemData.ItemType.Material:
                    return ItemConfiguration.ItemType.Material;
                case ItemDrop.ItemData.ItemType.Consumable:
                    return ItemConfiguration.ItemType.Consumable;
                case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                    return ItemConfiguration.ItemType.OneHandedWeapon;
                case ItemDrop.ItemData.ItemType.Bow:
                    return ItemConfiguration.ItemType.Bow;
                case ItemDrop.ItemData.ItemType.Shield:
                    return ItemConfiguration.ItemType.Shield;
                case ItemDrop.ItemData.ItemType.Helmet:
                    return ItemConfiguration.ItemType.Helmet;
                case ItemDrop.ItemData.ItemType.Chest:
                    return ItemConfiguration.ItemType.Chest;
                case ItemDrop.ItemData.ItemType.Ammo:
                    return ItemConfiguration.ItemType.Ammo;
                case ItemDrop.ItemData.ItemType.Customization:
                    return ItemConfiguration.ItemType.Customization;
                case ItemDrop.ItemData.ItemType.Legs:
                    return ItemConfiguration.ItemType.Legs;
                case ItemDrop.ItemData.ItemType.Hands:
                    return ItemConfiguration.ItemType.Hands;
                case ItemDrop.ItemData.ItemType.Trophie:
                    return ItemConfiguration.ItemType.Trophy;
                case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                    return ItemConfiguration.ItemType.TwoHandedWeapon;
                case ItemDrop.ItemData.ItemType.Torch:
                    return ItemConfiguration.ItemType.Torch;
                case ItemDrop.ItemData.ItemType.Misc:
                    return ItemConfiguration.ItemType.Misc;
                case ItemDrop.ItemData.ItemType.Shoulder:
                    return ItemConfiguration.ItemType.Shoulder;
                case ItemDrop.ItemData.ItemType.Utility:
                    return ItemConfiguration.ItemType.Utility;
                case ItemDrop.ItemData.ItemType.Tool:
                    return ItemConfiguration.ItemType.Tool;
                case ItemDrop.ItemData.ItemType.Attach_Atgeir:
                    return ItemConfiguration.ItemType.AttachAtgeir;
            }

            return ItemConfiguration.ItemType.Undefined;
        }*/

        /// <summary>
        /// Attempts to get the ItemDrop by the given name's hashcode, if not found searches by string.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ItemDrop GetItemDrop(string name)
        {
            ItemDrop item = null;

            if (!name.IsNullOrWhiteSpace())
            {
                try
                {
                    // Try hash code
                    item = ObjectDB.instance.GetItemPrefab(name.GetStableHashCode())?.GetComponent<ItemDrop>();
                }
                catch
                {
                    // Failed, try slow search
                    item = ObjectDB.instance.GetItemPrefab(name)?.GetComponent<ItemDrop>();
                }
            }

            return item;
        }

        /// <summary>
        /// Attempts to get the Humanoid by the given name's hashcode, if not found searches by string.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Humanoid GetHumanoid(string name)
        {
            Humanoid character = null;

            if (!name.IsNullOrWhiteSpace())
            {
                try
                {
                    // Try hash code
                    var gameObject = ZNetScene.m_instance.m_namedPrefabs[name.GetStableHashCode()];
                    character = gameObject.GetComponent<Humanoid>();
                }
                catch
                {
                    // Failed, try slow search
                    var prefabs = ZNetScene.m_instance.m_prefabs;
                    for (int lcv = 0; lcv < prefabs.Count; lcv++)
                    {
                        if (prefabs[lcv].name.Equals(name))
                        {
                            character = prefabs[lcv].GetComponent<Humanoid>();
                        }
                    }
                }
            }

            return character;
        }

        /// <summary>
        /// Prints out useful game data to json files.
        /// </summary>
        /// <param name="overwrite"></param>
        public void GenerateData(bool overwrite = false)
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
                    }
                    catch
                    {
                        ProgressionPlugin.VentureProgressionLogger.LogDebug($"Failed to write to file for GameObject: {obj.name}.");
                    }
                }
            }

            /*path = $"{Paths.ConfigPath}{Path.DirectorySeparatorChar}{"RecipeData"}";
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
                    }
                    catch
                    {
                        ProgressionPlugin.VentureProgressionLogger.LogDebug($"Failed to write to file for GameObject: {obj.name}.");
                    }
                }
            }*/


            path = $"{Paths.ConfigPath}{Path.DirectorySeparatorChar}{"CreatureData"}";
            if (!Directory.Exists(path) || overwrite == true)
            {
                Directory.CreateDirectory(path);

                foreach (GameObject obj in ZNetScene.m_instance.m_prefabs)
                {
                    try
                    {
                        Humanoid character = obj.GetComponent<Humanoid>();
                        if (character != null)
                        {
                            var filePath = $"{path}{Path.DirectorySeparatorChar}{character.name}.json";
                            File.WriteAllText(filePath, JsonUtility.ToJson(character, true));

                            var items = new Dictionary<string, GameObject>();
                            if (character.m_defaultItems != null)
                            {
                                foreach (var item in character.m_defaultItems)
                                {
                                    if (!items.ContainsKey(item.name))
                                    {
                                        items.Add(item.name, item);
                                    }
                                }
                            }

                            if (character.m_randomWeapon != null)
                            {
                                foreach (var item in character.m_randomWeapon)
                                {
                                    if (!items.ContainsKey(item.name))
                                    {
                                        items.Add(item.name, item);
                                    }
                                }
                            }

                            if (character.m_randomSets != null)
                            {
                                for (int lcv = 0; lcv < character.m_randomSets.Length; lcv++)
                                {
                                    if (character.m_randomSets[lcv].m_items != null)
                                    {
                                        foreach (var item in character.m_randomSets[lcv].m_items)
                                        {
                                            if (!items.ContainsKey(item.name))
                                            {
                                                items.Add(item.name, item);
                                            }
                                        }
                                    }
                                }
                            }

                            if (items != null)
                            {
                                foreach (var item in items.Values)
                                {
                                    var component = item.GetComponent<ItemDrop>();
                                    if (component != null)
                                    {
                                        var itemPath = $"{path}{Path.DirectorySeparatorChar}{character.name}.{item.name}.json";
                                        File.WriteAllText(itemPath, JsonUtility.ToJson(component.m_itemData?.m_shared?.m_damages, true));
                                    }
                                    else
                                    {
                                        ProgressionPlugin.VentureProgressionLogger.LogDebug($"Failed to write to file for GameObject: {item.name}.");
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        ProgressionPlugin.VentureProgressionLogger.LogDebug($"Failed to write to file for GameObject: {obj.name}.");
                    }
                }
            }
        }

        public bool IsInTheMainScene()
        {
            return SceneManager.GetActiveScene().name.Equals("main");
        }

        /// <summary>
        /// Converts a comma separated string to a HashSet of strings.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public HashSet<string> StringToSet(string str)
        {
            var set = new HashSet<string>();

            if (!str.IsNullOrWhiteSpace())
            {
                List<string> keys = str.Split(',').ToList();
                for (var lcv = 0; lcv < keys.Count; lcv++)
                {
                    set.Add(keys[lcv].Trim());
                }
            }

            return set;
        }

        /// <summary>
        /// Converts a comma separated string to a Dictionary of strings.
        /// If odd number of items ignores the last item.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public Dictionary<string, string> StringToDictionary(string str)
        {
            var dict = new Dictionary<string, string>();

            if (!str.IsNullOrWhiteSpace())
            {
                List<string> keys = str.Split(',').ToList();
                for (var lcv = 0; lcv < keys.Count - 1; lcv += 2)
                {
                    var key = keys[lcv].Trim();
                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, keys[lcv + 1].Trim());
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// Converts a comma separated string to an int[].
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public int[] StringToIntArray(string str)
        {
            if (!str.IsNullOrWhiteSpace())
            {
                var list = str.Split(',');
                var copy = new int[list.Length];
                for (var lcv = 0; lcv < list.Length; lcv++)
                {
                    copy[lcv] = int.Parse(list[lcv].Trim());
                }

                return copy;
            }

            return null;
        }

        /// <summary>
        /// Method to determine if a Global Key exists to bypass patches.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool GetGlobalKey(string key)
        {
            return ZoneSystem.instance.m_globalKeys.Contains(key);
        }

        /// <summary>
        /// Method to add a global key to bypass patches.
        /// </summary>
        /// <param name="key"></param>
        public void AddGlobalKey(string key)
        {
            if (key.IsNullOrWhiteSpace())
            {
                return;
            }

            if (!ZoneSystem.instance.m_globalKeys.Contains(key))
            {
                ZoneSystem.instance.m_globalKeys.Add(key);
                ZoneSystem.instance.SendGlobalKeys(ZRoutedRpc.Everybody);
            }
        }

        /// <summary>
        /// Attempts to find the player id with the given name.
        /// Case-insensitive, ignores whitespace.
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public long GetPlayerID(string playerName)
        {
            var nameSimple = playerName.Trim().ToLower();
            var players = ZNet.instance.GetPlayerList();

            for (int lcv = 0; lcv < players.Count; lcv++)
            {
                var player = players[lcv].m_name.Trim().ToLower();
                if (player.Equals(nameSimple))
                {
                    return players[lcv].m_characterID.m_userID;
                }
            }

            return 0L;
        }

        /// <summary>
        /// Returns the player name of the client player if exists.
        /// </summary>
        /// <returns></returns>
        public string GetLocalPlayerName()
        {
            var profile = Game.instance.GetPlayerProfile();
            if (profile != null)
            {
                return profile.m_playerName;
            }

            return "";
        }

        /// <summary>
        /// Rounds a number up or down to the nearest roundTo value. Rounds up to the nearest 5 by default.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="roundTo">5 by default</param>
        /// <param name="roundUp">true by default</param>
        /// <returns></returns>
        public int PrettifyNumber(int number, int roundTo = 5, bool roundUp = true)
        {
            var remainder = number % roundTo;
            if (remainder == 0)
            {
                return number;
            }

            if (roundUp)
            {
                return number + (roundTo - remainder);
            }
            else
            {
                return number - remainder;
            }
        }
    }
}