using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace VentureValheim.MiningCaves
{
    public class CaveManager
    {
        private static Dictionary<Type, Dictionary<string, UnityEngine.Object>> dictionaryCache = null;

        public static void AddMiningCaves()
        {
            ClearPrefabCache(typeof(Material));
            ClearPrefabCache(typeof(GameObject));

            AssetBundle bundle = AssetUtils.LoadAssetBundleFromResources("vv_miningcaves", Assembly.GetExecutingAssembly());

            // Copper & Tin Cave
            var copperTinCaveName = "VV_CopperTinCave";
            GameObject copperTinCave = bundle.LoadAsset<GameObject>(copperTinCaveName);
            LocationConfig copperTinCaveLocConfig = new LocationConfig();
            copperTinCaveLocConfig.Biome = Heightmap.Biome.BlackForest;
            copperTinCaveLocConfig.Quantity = 50;
            copperTinCaveLocConfig.ExteriorRadius = 12;
            copperTinCaveLocConfig.HasInterior = true;
            copperTinCaveLocConfig.InteriorRadius = 35;
            copperTinCaveLocConfig.MinAltitude = 3;
            copperTinCaveLocConfig.MaxAltitude = 1000;
            copperTinCaveLocConfig.Priotized = true;

            CustomLocation copperTinCaveLoc = new CustomLocation(copperTinCave, true, copperTinCaveLocConfig);
            ZoneManager.Instance.AddCustomLocation(copperTinCaveLoc);

            // Silver Cave
            var silverCaveName = "VV_SilverCave";
            GameObject silverCave = bundle.LoadAsset<GameObject>(silverCaveName);
            LocationConfig silverCaveLocConfig = new LocationConfig();
            silverCaveLocConfig.Biome = Heightmap.Biome.Mountain;
            silverCaveLocConfig.Quantity = 35;
            silverCaveLocConfig.ExteriorRadius = 20;
            silverCaveLocConfig.HasInterior = true;
            silverCaveLocConfig.InteriorRadius = 40;
            silverCaveLocConfig.MinAltitude = 90;
            silverCaveLocConfig.MaxAltitude = 2000;
            silverCaveLocConfig.Priotized = true;

            CustomLocation silverCaveLoc = new CustomLocation(silverCave, true, silverCaveLocConfig);
            ZoneManager.Instance.AddCustomLocation(silverCaveLoc);

            ZoneManager.OnVanillaLocationsAvailable -= AddMiningCaves;
        }

        /// <summary>
        /// Clears the Jotunn PrefabManager Cache of the specified type to fix some mocks not resolving.
        /// </summary>
        /// <param name="t"></param>
        private static void ClearPrefabCache(Type t)
        {
            if (dictionaryCache == null)
            {
                var cacheField = AccessTools.Field(typeof(PrefabManager.Cache), "dictionaryCache") as FieldInfo;
                if (cacheField != null)
                {
                    var dict = cacheField.GetValue(null) as Dictionary<Type, Dictionary<string, UnityEngine.Object>>;
                    if (dict != null)
                    {
                        dictionaryCache = dict;
                    }
                    else
                    {
                        MiningCavesPlugin.MiningCavesLogger.LogDebug($"Couldn't get value of PrefabManager.Cache.dictionaryCache");
                    }
                }
                else
                {
                    MiningCavesPlugin.MiningCavesLogger.LogDebug($"Couldn't find memberinfo for PrefabManager.Cache.dictionaryCache");
                }
            }

            if (dictionaryCache != null)
            {
                MiningCavesPlugin.MiningCavesLogger.LogDebug($"Clearing cache for type {t}.");
                dictionaryCache.Remove(t);
            }
        }
    }
}
