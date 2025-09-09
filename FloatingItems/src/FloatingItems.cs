using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using Jotunn;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.FloatingItems;

public class FloatingItems
{
    private FloatingItems()
    {
    }
    private static readonly FloatingItems _instance = new FloatingItems();

    public static FloatingItems Instance
    {
        get => _instance;
    }

    private static string TrophyPrefab = "trophy";
    private static string MeatPrefab = "meat";
    private static string MeadPrefab = "mead";
    private static string CookedPrefab = "cooked";
    private static string HidePrefab = "hide";
    private static string JutePrefab = "jute";
    private static string LeatherPrefab = "leather";
    private static string PeltPrefab = "pelt";
    private static string PiePrefab = "pie";

    private HashSet<string> FloatingPrefabs = new HashSet<string>();
    private HashSet<string> SinkingPrefabs = new HashSet<string>();
    private HashSet<string> ItemsPrefabs = new HashSet<string>
    {
        "helmetdverger",
        "helmetyule",
        "beltstrength",
        "barleywine",
        "barleyflour",
        "linenthread"
    };

    private HashSet<string> TreasurePrefabs = new HashSet<string>
    {
        "amber",
        "amberpearl",
        "coins",
        "ruby",
        "silvernecklace"
    };

    private static HashSet<string> FloatingAddedPrefabs = new HashSet<string>();
    private static HashSet<string> FloatingDisabledPrefabs = new HashSet<string>();

    private static bool _objectDBReady = false;

    /// <summary>
    /// Read and apply configurations.
    /// </summary>
    public static void Update()
    {
        var floatingPrefabsString = FloatingItemsPlugin.GetFloatingItems();
        Instance.FloatingPrefabs = new HashSet<string>();

        if (!floatingPrefabsString.IsNullOrWhiteSpace())
        {
            var prefabs = floatingPrefabsString.Split(',');
            for (int lcv = 0; lcv < prefabs.Length; lcv++)
            {
                Instance.FloatingPrefabs.Add(prefabs[lcv].Trim().ToLower());
            }
        }

        var sinkingPrefabsString = FloatingItemsPlugin.GetSinkingItems();
        Instance.SinkingPrefabs = new HashSet<string>();

        if (!sinkingPrefabsString.IsNullOrWhiteSpace())
        {
            var prefabs = sinkingPrefabsString.Split(',');
            for (int lcv = 0; lcv < prefabs.Length; lcv++)
            {
                Instance.SinkingPrefabs.Add(prefabs[lcv].Trim().ToLower());
            }
        }
    }

    /// <summary>
    /// Returns true if the item qualifies as a meat item type.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static bool IsMeat(string name)
    {
        return name.Contains(MeatPrefab) ||
            name.Contains("necktail") ||
            name.Contains("morgenheart") ||
            name.Contains("morgensinew");
    }

    /// <summary>
    /// Returns true if the item qualifies as a hide item type.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static bool IsHide(string name)
    {
        return name.Contains(LeatherPrefab) ||
            name.Contains(JutePrefab) ||
            name.Contains(HidePrefab) ||
            name.Contains(PeltPrefab) ||
            name.Equals("wolfhairbundle");
    }

    /// <summary>
    /// Returns true if the item qualifies as a treasure item type.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static bool IsTreasure(string name)
    {
        return Instance.TreasurePrefabs.Contains(name);
    }

    /// <summary>
    /// Returns true if the item has a recipe or is a another qualifying player item.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private static bool IsPlayerGear(GameObject item)
    {
        var name = item.name.ToLower();
        if (Instance.ItemsPrefabs.Contains(name) || 
            name.Contains(MeadPrefab) || 
            name.Contains(CookedPrefab) ||
            name.Contains(PiePrefab))
        {
            return true;
        }
        else
        {
            var itemDrop = item.GetComponent<ItemDrop>();
            
            if (item != null)
            {
                if (itemDrop.m_itemData.m_shared.m_food > 0f ||
                    ObjectDB.instance.GetRecipe(itemDrop.m_itemData) != null)
                {
                    return true;
                }

            }
        }

        return false;
    }

    /// <summary>
    /// Returns true if configurations indicate an item should float.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    private static bool ShouldFloat(string name, GameObject item)
    {
        return FloatingItemsPlugin.GetFloatEverything() ||
                Instance.FloatingPrefabs.Contains(name) ||
                (FloatingItemsPlugin.GetFloatTrophies() && name.Contains(TrophyPrefab)) ||
                (FloatingItemsPlugin.GetFloatMeat() && IsMeat(name)) ||
                (FloatingItemsPlugin.GetFloatHides() && IsHide(name)) ||
                (FloatingItemsPlugin.GetFloatGearAndCraftable() && IsPlayerGear(item)) ||
                (FloatingItemsPlugin.GetFloatTreasure() && IsTreasure(name));
    }

    /// <summary>
    /// Applies or removes floating effect to all configured items.
    /// </summary>
    public static void EnableFloatingItems()
    {
        if (!_objectDBReady)
        {
            return;
        }

        Update();

        for (int lcv = 0; lcv < ObjectDB.instance.m_items.Count; lcv++)
        {
            var item = ObjectDB.instance.m_items[lcv];
            var name = item.name.ToLower();

            if (Instance.SinkingPrefabs.Contains(name))
            {
                DisableFloatingComponent(item);
            }
            else if (ShouldFloat(name, item))
            {
                ApplyFloatingComponent(item);
            }
            else
            {
                // Restore original state
                CleanFloatingItem(name, ref item);
            }
        }

        FloatingItemsPlugin.FloatingItemsLogger.LogInfo("Done applying buoyancy.");
    }

    private static void CleanFloatingItem(string name, ref GameObject item)
    {
        if (FloatingAddedPrefabs.Contains(name))
        {
            var floating = item.gameObject.GetComponent<Floating>();
            if (floating != null)
            {
                GameObject.Destroy(floating);
            }

            FloatingAddedPrefabs.Remove(name);
        }
        else if (FloatingDisabledPrefabs.Contains(name))
        {
            var floating = item.gameObject.GetComponent<Floating>();
            if (floating != null)
            {
                floating.enabled = true;
            }

            FloatingDisabledPrefabs.Remove(name);
        }
    }

    /// <summary>
    /// Add the Floating component to an item if it does not already have one.
    /// </summary>
    /// <param name="item"></param>
    private static void ApplyFloatingComponent(GameObject item)
    {
        if (item.gameObject.GetComponentInChildren<Collider>() == null ||
            item.gameObject.GetComponent<Rigidbody>() == null)
        {
            return;
        }

        var floating = item.gameObject.GetComponent<Floating>();
        if (floating == null)
        {
            floating = item.gameObject.AddComponent<Floating>();
            floating.m_waterLevelOffset = 0.7f;
            FloatingAddedPrefabs.Add(item.gameObject.name.ToLower());
        }

        floating.enabled = true;
    }

    /// <summary>
    /// Disables the Floating component for an item if it has one.
    /// </summary>
    /// <param name="item"></param>
    private static void DisableFloatingComponent(GameObject item)
    {
        var floating = item.gameObject.GetComponent<Floating>();
        if (floating != null)
        {
            floating.enabled = false;
            FloatingDisabledPrefabs.Add(item.gameObject.name.ToLower());
        }
    }

    /// <summary>
    /// Enable floating items, priority set to low to run after other mod patches.
    /// </summary>
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    public static class Patch_ObjectDB_Awake
    {
        private static void Postfix()
        {
            if (SceneManager.GetActiveScene().name.Equals("main"))
            {
                _objectDBReady = true;
                EnableFloatingItems();
            }
            else
            {
                _objectDBReady = false;
            }
        }
    }

    /// <summary>
    /// Helper method to identify floating items.
    /// </summary>
    /*private static void ListFloatingItems()
    {
        int count = 0;
        foreach (var obj in ObjectDB.instance.m_items)
        {
            if (obj == null) continue;

            var component = obj.GetComponent<Floating>();
            if (component != null)
            {
                count++;
                //FloatingItemsPlugin.FloatingItemsLogger.LogDebug($"Found floating component on {obj.name}.");
            }
        }

        FloatingItemsPlugin.FloatingItemsLogger.LogDebug($"Found {count} floating items.");
    }*/

    [HarmonyPatch(typeof(Floating), nameof(Floating.CustomFixedUpdate))]
    public static class Patch_Floating_CustomFixedUpdate
    {
        private static bool Prefix(Floating __instance)
        {
            if (__instance.m_body == null || __instance.m_collider == null)
            {
                FloatingItemsPlugin.FloatingItemsLogger.LogDebug($"Null found: {__instance.name}. " +
                    $"{__instance.m_body == null}, " +
                    $"{__instance.m_collider == null}");
                return false;
            }

            return true;
        }
    }
}