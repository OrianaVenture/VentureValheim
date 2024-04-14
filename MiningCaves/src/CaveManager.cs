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
        public static void AddMiningCaves()
        {
            // Disable the terrain modifiers located on the copper node frac
            var frac = PrefabManager.Instance.GetPrefab("rock4_copper_frac");
            if (frac != null)
            {
                var modifiers = frac.gameObject.GetComponentsInChildren<TerrainModifier>();
                foreach (var modifier in modifiers)
                {
                    modifier.enabled = false;
                }
            }

            AssetBundle bundle = AssetUtils.LoadAssetBundleFromResources("vv_miningcaves", Assembly.GetExecutingAssembly());

            // Copper & Tin Cave
            var copperTinCaveName = "VV_CopperTinCave";
            GameObject copperTinCave = ZoneManager.Instance.CreateLocationContainer(bundle, copperTinCaveName);
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
            GameObject silverCave = ZoneManager.Instance.CreateLocationContainer(bundle, silverCaveName);
            LocationConfig silverCaveLocConfig = new LocationConfig();
            silverCaveLocConfig.Biome = Heightmap.Biome.Mountain;
            silverCaveLocConfig.Quantity = 50;
            silverCaveLocConfig.ExteriorRadius = 16;
            silverCaveLocConfig.HasInterior = true;
            silverCaveLocConfig.InteriorRadius = 40;
            silverCaveLocConfig.MinAltitude = 90;
            silverCaveLocConfig.MaxAltitude = 2000;
            silverCaveLocConfig.Priotized = true;

            CustomLocation silverCaveLoc = new CustomLocation(silverCave, true, silverCaveLocConfig);
            ZoneManager.Instance.AddCustomLocation(silverCaveLoc);

            ZoneManager.OnVanillaLocationsAvailable -= AddMiningCaves;
        }
    }
}
