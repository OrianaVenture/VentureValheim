using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace VentureValheim.MiningCaves;

public class CaveManager
{
    public static void AddPrefabs()
    {
        // Disable the terrain modifiers located on the copper node frac
        GameObject frac = PrefabManager.Instance.GetPrefab("rock4_copper_frac");
        if (frac != null)
        {
            TerrainModifier[] modifiers = frac.gameObject.GetComponentsInChildren<TerrainModifier>();
            foreach (TerrainModifier modifier in modifiers)
            {
                modifier.enabled = false;
            }
        }

        PrefabManager.Instance.AddPrefab(new CustomPrefab(MiningCavesPlugin.CavesBundle, "VV_rock4_copper", true)); // Legacy Copper Ore
        PrefabManager.Instance.AddPrefab(new CustomPrefab(MiningCavesPlugin.CavesBundle, "VV_MineRock_Copper", true));
        PrefabManager.Instance.AddPrefab(new CustomPrefab(MiningCavesPlugin.CavesBundle, "VV_MineRock_Tin", true));
        PrefabManager.Instance.AddPrefab(new CustomPrefab(MiningCavesPlugin.CavesBundle, "VV_MineRock_Iron", true));

        PrefabManager.OnVanillaPrefabsAvailable -= AddPrefabs;
    }

    public static void AddMiningCaves()
    {
        // Copper & Tin Cave
        string copperTinCaveName = "VV_CopperTinCave";
        GameObject copperTinCave = ZoneManager.Instance.CreateLocationContainer(MiningCavesPlugin.CavesBundle, copperTinCaveName);
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
        string silverCaveName = "VV_SilverCave";
        GameObject silverCave = ZoneManager.Instance.CreateLocationContainer(MiningCavesPlugin.CavesBundle, silverCaveName);
        LocationConfig silverCaveLocConfig = new LocationConfig();
        silverCaveLocConfig.Biome = Heightmap.Biome.Mountain;
        silverCaveLocConfig.Quantity = 50;
        silverCaveLocConfig.ExteriorRadius = 16;
        silverCaveLocConfig.HasInterior = true;
        silverCaveLocConfig.InteriorRadius = 40;
        silverCaveLocConfig.MinAltitude = 90;
        silverCaveLocConfig.MaxAltitude = 2000;
        silverCaveLocConfig.Priotized = true;
        silverCaveLocConfig.ClearArea = true;

        CustomLocation silverCaveLoc = new CustomLocation(silverCave, true, silverCaveLocConfig);
        ZoneManager.Instance.AddCustomLocation(silverCaveLoc);

        // Tar Cave
        string tarCaveName = "VV_TarCave";
        GameObject tarCave = ZoneManager.Instance.CreateLocationContainer(MiningCavesPlugin.CavesBundle, tarCaveName);
        LocationConfig tarCaveLocConfig = new LocationConfig();
        tarCaveLocConfig.Biome = Heightmap.Biome.Plains;
        tarCaveLocConfig.Quantity = 50;
        tarCaveLocConfig.ExteriorRadius = 25;
        tarCaveLocConfig.HasInterior = true;
        tarCaveLocConfig.InteriorRadius = 50;
        tarCaveLocConfig.MinAltitude = 10;
        tarCaveLocConfig.MaxAltitude = 2000;
        tarCaveLocConfig.Priotized = true;
        tarCaveLocConfig.MinDistanceFromSimilar = 300f;
        tarCaveLocConfig.ClearArea = true;

        CustomLocation tarCaveLoc = new CustomLocation(tarCave, true, tarCaveLocConfig);
        ZoneManager.Instance.AddCustomLocation(tarCaveLoc);

        ZoneManager.OnVanillaLocationsAvailable -= AddMiningCaves;
    }
}
