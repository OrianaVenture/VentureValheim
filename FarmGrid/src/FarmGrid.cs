using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.FarmGrid;

public class FarmGrid
{
    private class PlantObject
    {
        public Vector3 position;
        public float growthSize;

        public PlantObject(Vector3 position, float growthSize)
        {
            this.position = position;
            this.growthSize = growthSize;
        }
    }

    private static Dictionary<string, float> customPlants = new Dictionary<string, float>();
    private static readonly Dictionary<string, float> vanillaPlantsDefaults = new Dictionary<string, float>()
    {
        { "BlueberryBush", 0.5f },
        { "CloudberryBush", 0.5f },
        { "RaspberryBush", 0.5f },
        { "Pickable_Dandelion", 0.5f },
        { "Pickable_Fiddlehead", 0.5f },
        { "Pickable_Mushroom", 0.5f },
        { "Pickable_Mushroom_blue", 0.5f },
        { "Pickable_Mushroom_yellow", 0.5f },
        { "Pickable_SmokePuff", 0.5f },
        { "Pickable_Thistle", 0.5f }
    };

    private static Dictionary<string, float> plantsConfiguration = new Dictionary<string, float>();

    private static readonly string[] plantObjectMasks =
        { "Default", "Default_small", "item", "piece", "piece_nonsolid", "static_solid" };
    private static int plantObjectMask = LayerMask.GetMask(plantObjectMasks);

    private static Vector3 plantSnapPoint = Vector3.zero;
    private static GameObject[] farmGrid = null;
    private static bool farmGridVisible = false;
    private static PlantObject plantGhost;
    private static Vector3 plantGhostPosition = Vector3.zero;
    private static Material lineMaterial;
    private static bool freeDraw = true;

    #region Methods

    public static void SetupPlantCache()
    {
        if (ZNetScene.instance == null)
        {
            return;
        }

        plantsConfiguration = vanillaPlantsDefaults;

        foreach (GameObject obj in ZNetScene.instance.m_prefabs)
        {
            var plant = obj.GetComponent<Plant>();
            if (plant != null)
            {
                plantsConfiguration.Add(plant.name, plant.m_growRadius);

                foreach (var grownPlant in plant.m_grownPrefabs)
                {
                    plantsConfiguration.Add(grownPlant.name, plant.m_growRadius);
                }
            }
        }
    }

    public static void SetupConfigurations()
    {
        customPlants.Clear();
        string[] plants = FarmGridPlugin.GetCustomPlants().Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string text in plants)
        {
            string[] entry = text.Split(new string[1] { ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (entry.Length == 2 &&
                float.TryParse(entry[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var size))
            {
                customPlants.Add(entry[0].Trim(), size);
            }
        }

        DestroyFarmGrid();
        ResetCache();
        SetGhostPlant();
    }

    private static bool HasCustomSize(string prefab, out float size)
    {
        string name = Utils.GetPrefabName(prefab);
        size = 0f;

        if (customPlants.ContainsKey(name))
        {
            size = customPlants[name];
            return true;
        }

        if (plantsConfiguration.ContainsKey(name))
        {
            size = plantsConfiguration[name];
            return true;
        }

        return false;
    }

    private static float GetCollisionRadius(PlantObject plant)
    {
        return GetPlantSpacing(plant.growthSize) * (FarmGridPlugin.GetFarmGridSections() + 1);
    }

    private static float GetPlantSpacing(float growthSize)
    {
        return growthSize * 2f + FarmGridPlugin.GetExtraPlantSpacing();
    }

    private static void DrawFarmGrid(Vector3 pos, Vector3 gridDir, float gridSize)
    {
        if (farmGrid == null || farmGrid[0] == null)
        {
            InitFarmGrid();
        }

        Vector3 pivot = new Vector3(gridDir.z, gridDir.y, 0f - gridDir.x);
        int farmGridSections = FarmGridPlugin.GetFarmGridSections();

        for (int lcv = -farmGridSections; lcv <= farmGridSections; lcv++)
        {
            GameObject grid = farmGrid[lcv + farmGridSections];
            Vector3 position = pos + gridDir * lcv * gridSize;
            DrawSegments(farmGridSections, gridSize, grid, position, pivot);

            GameObject grid2 = farmGrid[lcv + farmGridSections + (farmGridSections * 2 + 1)];
            Vector3 position2 = pos + pivot * lcv * gridSize;
            DrawSegments(farmGridSections, gridSize, grid2, position2, gridDir);
        }

        farmGridVisible = true;
    }

    private static void DrawSegments(int farmGridSections, float gridSize, GameObject grid,
        Vector3 position, Vector3 pivot)
    {
        grid.transform.position = position;
        LineRenderer component = grid.GetComponent<LineRenderer>();
        component.widthMultiplier = 0.015f;
        component.enabled = true;
        float farmGridYOffset = FarmGridPlugin.GetFarmGridYOffset();

        for (int lcv2 = -farmGridSections; lcv2 <= farmGridSections; lcv2++)
        {
            Vector3 point = position + pivot * lcv2 * gridSize;
            float groundHeight = GetGroundHeight(point);
            point.y = groundHeight + farmGridYOffset;
            component.SetPosition(lcv2 + farmGridSections, point);
        }
    }

    private static float GetGroundHeight(Vector3 linePos)
    {
        return ZoneSystem.instance.GetGroundHeight(linePos);
    }

    public static void DestroyFarmGrid()
    {
        if (farmGrid == null)
        {
            return;
        }

        for (int lcv = 0; lcv < farmGrid.Length; lcv++)
        {
            if ((bool)farmGrid[lcv])
            {
                UnityEngine.Object.Destroy(farmGrid[lcv]);
            }
        }

        farmGrid = null;
    }

    private static void InitFarmGrid()
    {
        if (!lineMaterial)
        {
            lineMaterial = Resources.FindObjectsOfTypeAll<Material>()
                .First((Material k) => k.name == "Default-Line");
        }

        int farmGridSections = FarmGridPlugin.GetFarmGridSections();
        Color farmGridColor = FarmGridPlugin.GetFarmGridColor();
        farmGrid = new GameObject[(farmGridSections * 2 + 1) * 2];

        for (int lcv = 0; lcv < farmGrid.Length; lcv++)
        {
            GameObject gameObject = new GameObject();
            LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = lineMaterial;
            lineRenderer.startColor = farmGridColor;
            lineRenderer.endColor = farmGridColor;
            lineRenderer.positionCount = farmGridSections * 2 + 1;
            farmGrid[lcv] = gameObject;
        }
    }

    private static void HideFarmGrid()
    {
        if (farmGridVisible && farmGrid != null && farmGrid[0] != null)
        {
            for (int lcv = 0; lcv < farmGrid.Length; lcv++)
            {
                LineRenderer component = farmGrid[lcv].GetComponent<LineRenderer>();
                component.enabled = false;
            }

            farmGridVisible = false;
            freeDraw = true;
        }
    }

    private static void ResetCache()
    {
        plantGhost = null;
        plantSnapPoint = Vector3.zero;
        freeDraw = true;
    }

    private static void SetGhostPlant()
    {
        Player localPlayer = Player.m_localPlayer;

        if (localPlayer == null || localPlayer.m_placementGhost == null)
        {
            return;
        }

        Collider component = localPlayer.m_placementGhost.GetComponentInChildren<Collider>();

        if (GetPlantObject(component, out var plantObject))
        {
            plantGhost = plantObject;
            plantGhostPosition = plantGhost.position;
        }
    }

    private static bool GetFarmSnapPoints()
    {
        if (plantGhost == null)
        {
            HideFarmGrid();
            ResetCache();
            return false;
        }

        if (Player.m_localPlayer == null)
        {
            HideFarmGrid();
            ResetCache();
            return false;
        }

        GameObject placementGhost = Player.m_localPlayer.m_placementGhost;

        if (placementGhost == null)
        {
            HideFarmGrid();
            ResetCache();
            return false;
        }

        // Only update when there is a significant change in placement
        float sqrMagnitude = (plantGhostPosition - placementGhost.transform.position).sqrMagnitude;
        if (((!farmGridVisible || freeDraw) && sqrMagnitude < 0.0001f) ||
            (farmGridVisible && !freeDraw && sqrMagnitude < 0.02))
        {
            if (plantSnapPoint != Vector3.zero)
            {
                placementGhost.transform.position = plantSnapPoint;
            }

            return true;
        }

        plantSnapPoint = Vector3.zero;
        plantGhost.position = placementGhost.transform.position;
        plantGhostPosition = plantGhost.position;

        List<PlantObject> otherPlants = GetOtherPlants(plantGhost.position, GetCollisionRadius(plantGhost));

        if (otherPlants.Count < 1)
        {
            HideFarmGrid();
            return true;
        }

        if (otherPlants.Count == 1)
        {
            DrawFreeGrid(otherPlants[0]);
        }
        else if (otherPlants.Count > 1)
        {
            DrawFixedGrid(otherPlants);
        }

        return true;
    }

    private static void DrawFreeGrid(PlantObject otherPlant)
    {
        if (!GetFixedGridDir(otherPlant, out Vector3 gridDir))
        {
            gridDir = plantGhost.position - otherPlant.position;
            gridDir.y = 0f;
            gridDir.Normalize();
        }

        freeDraw = true;

        float requiredSpacing = GetPlantSpacing(Mathf.Max(plantGhost.growthSize, otherPlant.growthSize));
        plantSnapPoint = otherPlant.position + gridDir * requiredSpacing;
        float groundHeight = GetGroundHeight(plantSnapPoint);
        plantSnapPoint.y = groundHeight;
        Player.m_localPlayer.m_placementGhost.transform.position = plantSnapPoint;
        DrawFarmGrid(plantSnapPoint, gridDir, requiredSpacing);
    }

    private static void DrawFixedGrid(List<PlantObject> otherPlants)
    {
        // TODO: chosen1 might not need a null check here
        if (!GetGridDir(otherPlants, out Vector3 gridDir,
                out PlantObject chosen1, out PlantObject chosen2) || chosen1 == null)
        {
            DrawFreeGrid(otherPlants[0]);
            return;
        }

        freeDraw = false;
        float maxSpacing = Mathf.Max(plantGhost.growthSize, chosen1.growthSize);

        /*if (chosen2 != null && chosen2.growthSize > maxSpacing)
        {
            // TODO: Evaluate if this should be kept
        }*/

        float requiredSpacing = GetPlantSpacing(maxSpacing);
        List<Vector3> list = GetGridLocations(plantGhost.position, chosen1.position, gridDir, requiredSpacing);
        plantSnapPoint = list[0];

        for (int lcv = 0; lcv < list.Count; lcv++)
        {
            if (!HasOverlappingPlants(list[lcv], otherPlants, plantGhost.growthSize)) // TODO
            {
                plantSnapPoint = list[lcv];
                break;
            }
        }

        float groundHeight = GetGroundHeight(plantSnapPoint);
        plantSnapPoint.y = groundHeight;
        Player.m_localPlayer.m_placementGhost.transform.position = plantSnapPoint;
        DrawFarmGrid(plantSnapPoint, gridDir, requiredSpacing);
    }

    private static List<Vector3> GetGridLocations(Vector3 position, Vector3 otherPlantPosition, Vector3 gridDir, float gridSize)
    {
        List<Vector3> grid = new List<Vector3>();
        Vector3 rightAngle = new Vector3(gridDir.z, gridDir.y, 0f - gridDir.x);

        for (int lcv1 = -2; lcv1 <= 2; lcv1++)
        {
            for (int lcv2 = -2; lcv2 <= 2; lcv2++)
            {
                if (lcv1 != 0 || lcv2 != 0)
                {
                    Vector3 location = otherPlantPosition + gridDir * lcv1 * gridSize + rightAngle * lcv2 * gridSize;
                    grid.Add(location);
                }
            }
        }

        return grid.OrderBy((Vector3 location) => (location - position).sqrMagnitude).ToList();
    }

    /// <summary>
    /// Finds the closest grid direction from set partitions.
    /// </summary>
    private static bool GetFixedGridDir(PlantObject plant, out Vector3 gridDir)
    {
        gridDir = default(Vector3);
        int farmGridFixedPartitions = FarmGridPlugin.GetFarmGridFixedPartitions();

        if (farmGridFixedPartitions != 0)
        {
            float requiredAngle = Mathf.PI * 2 / farmGridFixedPartitions;
            Vector3 delta = plantGhost.position - plant.position;
            float currentAngle = Mathf.Atan2(delta.z, delta.x);
            float newAngle = Mathf.Round(currentAngle / requiredAngle) * requiredAngle;

            gridDir.y = 0f;
            gridDir.x = Mathf.Cos(newAngle);
            gridDir.z = Mathf.Sin(newAngle);
            gridDir = gridDir.normalized;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to find an acceptable grid direction from the closest two crops.
    /// </summary>
    private static bool GetGridDir(List<PlantObject> plantList, out Vector3 gridDir,
        out PlantObject chosen1, out PlantObject chosen2)
    {
        chosen1 = plantList[0];
        chosen2 = null;

        for (int lcv1 = 0; lcv1 < 2; lcv1++)
        {
            chosen1 = plantList[lcv1];
            for (int lcv2 = 1; lcv2 < plantList.Count; lcv2++)
            {
                gridDir = chosen1.position - plantList[lcv2].position;
                gridDir.y = 0f;
                float requiredSpacing = GetPlantSpacing(
                    Mathf.Max(Mathf.Max(chosen1.growthSize, plantList[lcv2].growthSize), plantGhost.growthSize));
                float spacing = gridDir.magnitude;
                float difference = spacing - requiredSpacing;

                if (difference > -0.01f && difference < 0.01f * requiredSpacing)
                {
                    chosen2 = plantList[lcv2];
                    gridDir = gridDir.normalized;
                    return true;
                }
            }
        }

        gridDir = Vector3.zero;
        chosen1 = null;
        chosen2 = null;
        return false;
    }

    /// <summary>
    /// Gets a maximum of 24 of the closest qualifying plant objects.
    /// </summary>
    private static List<PlantObject> GetOtherPlants(Vector3 position, float collisionRadius)
    {
        int size = Physics.OverlapSphereNonAlloc(position, collisionRadius, Piece.s_pieceColliders, plantObjectMask);

        var colliders = Piece.s_pieceColliders.Take(size)
            .OrderBy((Collider plant) => (plant.transform.position - plantGhost.position).sqrMagnitude)
            .ToList();

        List<PlantObject> plants = new List<PlantObject>();

        for (int lcv = 0; lcv < colliders.Count; lcv++)
        {
            if (GetPlantObject(colliders[lcv], out var plantObject))
            {
                plants.Add(plantObject);

                if (plants.Count == 24)
                {
                    break;
                }
            }
        }

        return plants;
    }

    private static bool HasOverlappingPlants(Vector3 pos, List<PlantObject> otherPlants, float collisionRadius)
    {
        for (int lcv = 0; lcv < otherPlants.Count; lcv++)
        {
            if ((pos - otherPlants[lcv].position).magnitude <= collisionRadius + otherPlants[lcv].growthSize)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasOverlappingPlants(Vector3 pos, float collisionRadius)
    {
        int num = Physics.OverlapSphereNonAlloc(pos, collisionRadius, Piece.s_pieceColliders, plantObjectMask);

        for (int lcv = 0; lcv < num; lcv++)
        {
            if (GetPlantObject(Piece.s_pieceColliders[lcv], out var plantObject) &&
                (pos - plantObject.position).magnitude <= collisionRadius + plantObject.growthSize)
            {
                return true;
            }
        }

        return false;
    }

    private static bool GetPlantObject(Collider collider, out PlantObject plantObject)
    {
        if (collider != null)
        {
            if (HasCustomSize(collider.transform.root.name, out float size))
            {
                plantObject = new PlantObject(collider.transform.position, size);
                return true;
            }
        }

        plantObject = null;
        return false;
    }

    #endregion

    #region Patches

    [HarmonyPatch(typeof(Player), nameof(Player.SetupPlacementGhost))]
    public static class Patch_Player_SetupPlacementGhost
    {
        private static void Postfix()
        {
            ResetCache();
            SetGhostPlant();
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
    public static class Patch_Player_UpdatePlacementGhost
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList();
            bool flag = false;
            int num = 0;
            var method = AccessTools.Method(typeof(Player), nameof(Player.FindClosestSnapPoints), (Type[])null, (Type[])null);
            var customMethod = (object)AccessTools.Method(typeof(FarmGrid), nameof(GetFarmSnapPoints), (Type[])null, (Type[])null);
            for (int lcv = 0; lcv < list.Count; lcv++)
            {
                if (list[lcv].opcode == OpCodes.Brtrue)
                {
                    num = lcv;
                }
                if (CodeInstructionExtensions.Calls(list[lcv], method))
                {
                    list.InsertRange(num + 1, (IEnumerable<CodeInstruction>)(object)new CodeInstruction[2]
                    {
                        new CodeInstruction(OpCodes.Call, customMethod),
                        new CodeInstruction(OpCodes.Brtrue, list[num].operand)
                    });
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                FarmGridPlugin.FarmGridLogger.LogWarning("FarmGrid: Failed to patch UpdatePlacementGhost");
            }
            return list;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.TryPlacePiece))]
    public static class Patch_Player_PlacePiece
    {
        private static bool Prefix(ref Player __instance, ref bool __result, Piece piece)
        {
            if (GetPlantObject(__instance.m_placementGhost.GetComponent<Collider>(),
                out var plantObject) && HasOverlappingPlants(plantObject.position, plantObject.growthSize))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.SetupVisEquipment))]
    public static class Patch_Humanoid_SetupVisEquipment
    {
        private static void Postfix(ref Humanoid __instance)
        {
            if (__instance == Player.m_localPlayer &&
                (Player.m_localPlayer == null ||
                    Player.m_localPlayer.m_rightItem == null ||
                    Player.m_localPlayer.m_rightItem.m_shared == null ||
                    Player.m_localPlayer.m_rightItem.m_shared.m_name != "$item_cultivator"))
            {
                HideFarmGrid();
                ResetCache();
            }
        }
    }

    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    public static class Patch_ZNetScene_Awake
    {
        private static void Postfix()
        {
            SetupPlantCache();
        }
    }

    #endregion
}
