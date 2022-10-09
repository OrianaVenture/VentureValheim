using System;
using BepInEx;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.Progression
{
    [PublicAPI]
    public class ProgressionAPI
    {
        private ProgressionAPI()
        {
        }
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
        public void AddCustomBiome(Heightmap.Biome biome, float scale, bool overrideBiome = false)
        {
            var internalBiome = GetProgressionBiome(biome);
            WorldConfiguration.Instance.AddCustomBiome(internalBiome, scale, overrideBiome);
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
        public static ItemDrop? GetItemDrop(string name)
        {
            ItemDrop? item = null;
            try
            {
                item = ObjectDB.instance.GetItemPrefab(name.GetStableHashCode()).GetComponent<ItemDrop>(); // Try hash code
            }
            catch
            {
                item = ObjectDB.instance.GetItemPrefab(name).GetComponent<ItemDrop>(); // Failed, try slow search
            }
            return item;
        }

        /// <summary>
        /// Prints out useful game data to json files
        /// </summary>
        /// <param name="overwrite"></param>
        public static void GenerateData(bool overwrite = false)
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
                        ProgressionPlugin.GetProgressionLogger().LogDebug($"{itemDrop.name} data written to file: {filePath}.");

                        HitData.DamageTypes damage = itemDrop.m_itemData.m_shared.m_damages;
                        filePath = $"{path}{Path.DirectorySeparatorChar}{itemDrop.name}.damage.json";
                        File.WriteAllText(filePath, JsonUtility.ToJson(damage, true));
                        ProgressionPlugin.GetProgressionLogger().LogDebug($"{itemDrop.name} damage data written to file: {filePath}.");
                    }
                    catch (Exception e)
                    {
                        ProgressionPlugin.GetProgressionLogger().LogDebug($"Failed to write to file for GameObject: {obj.name}.");
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
                        ProgressionPlugin.GetProgressionLogger().LogDebug($"{itemDrop.name} data written to file: {filePath}.");
                    }
                    catch (Exception e)
                    {
                        ProgressionPlugin.GetProgressionLogger().LogDebug($"Failed to write to file for GameObject: {obj.name}.");
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
                            var filePath = $"{path}{Path.DirectorySeparatorChar}{character.name}.json";
                            File.WriteAllText(filePath, JsonUtility.ToJson(character, true));
                            ProgressionPlugin.GetProgressionLogger().LogDebug($"{obj.name} data written to file: {filePath}.");
                        }

                    }
                    catch (Exception e)
                    {
                        ProgressionPlugin.GetProgressionLogger().LogDebug($"Failed to write to file for GameObject: {obj.name}.");
                    }
                }
            }
        }

        public static bool IsInTheMainScene()
        {
            return SceneManager.GetActiveScene().name.Equals("main");
        }
    }
}