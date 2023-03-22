using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.FloatingItems
{
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

        private HashSet<string> FloatingPrefabs = new HashSet<string>();
        private HashSet<string> SinkingPrefabs = new HashSet<string>();
        private HashSet<string> ItemsPrefabs = new HashSet<string>
        {
            "helmetdverger",
            "helmetyule",
            "beltstrength",
            "barleywine",
            "barleyflour",
            "fishandbread",
            "loxpie",
            "bread",
            "honeyglazedchicken",
            "magicallystuffedshroom",
            "mistharesupreme",
            "linenthread"
        };

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

            var sinkingPrefabsString = FloatingItemsPlugin.GetFloatingItems();
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
        /// Returns true if the item has a recipe or is a another qualifying player item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool IsPlayerGear(GameObject item)
        {
            var name = item.name.ToLower();
            if (Instance.ItemsPrefabs.Contains(name) || name.Contains(MeadPrefab) || name.Contains(CookedPrefab))
            {
                return true;
            }
            else
            {
                var itemDrop = item.GetComponent<ItemDrop>();
                if (item != null && ObjectDB.instance.GetRecipe(itemDrop.m_itemData) != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Applies or removes floating effect to all configured items.
        /// </summary>
        private static void EnableFloatingItems()
        {
            Update();

            foreach (var item in ObjectDB.instance.m_items)
            {
                var name = item.name.ToLower();

                if (item.gameObject.GetComponent<Floating>() != null)
                {
                    if (Instance.SinkingPrefabs.Contains(name))
                    {
                        RemoveFloatingComponent(item);
                    }
                    continue;
                }

                if (!Instance.SinkingPrefabs.Contains(name))
                {
                    if (Instance.FloatingPrefabs.Contains(name))
                    {
                        ApplyFloatingComponent(item);
                    }
                    else if (FloatingItemsPlugin.GetFloatTrophies() && name.Contains(TrophyPrefab))
                    {
                        ApplyFloatingComponent(item);
                    }
                    else if (FloatingItemsPlugin.GetFloatMeat() && name.Contains(MeatPrefab))
                    {
                        ApplyFloatingComponent(item);
                    }
                    else if (FloatingItemsPlugin.GetFloatHides() &&
                        (name.Contains(LeatherPrefab) || name.Contains(JutePrefab) || name.Contains(HidePrefab) || 
                        name.Contains(PeltPrefab) || name.Equals("wolfhairbundle")))
                    {
                        ApplyFloatingComponent(item);
                    }
                    else if (FloatingItemsPlugin.GetFloatGearAndCraftable() && IsPlayerGear(item))
                    {
                        ApplyFloatingComponent(item);
                    }
                }
            }
        }

        private static void ApplyFloatingComponent(GameObject item)
        {
            var floating = item.gameObject.AddComponent<Floating>();
            floating.m_waterLevelOffset = 0.7f;
        }

        private static void RemoveFloatingComponent(GameObject item)
        {
            var floating = item.gameObject.GetComponent<Floating>();
            GameObject.Destroy(floating);
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
                    EnableFloatingItems();
                    FloatingItemsPlugin.FloatingItemsLogger.LogInfo("Done applying buoyancy.");
                }
            }
        }

        /// <summary>
        /// Helper method to identify floating items.
        /// </summary>
        private static void ListFloatingItems()
        {
            foreach (var obj in ObjectDB.instance.m_items)
            {
                if (obj == null) continue;

                var component = obj.GetComponent<Floating>();
                if (component != null)
                {
                    FloatingItemsPlugin.FloatingItemsLogger.LogDebug($"Found floating component on {obj.name}.");
                }
            }
        }
    }
}