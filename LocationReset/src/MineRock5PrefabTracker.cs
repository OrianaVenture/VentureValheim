
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.LocationReset
{
    /// <summary>
    ///     Custom component to hold MineRock5 root prefab name due to renaming in MineRock5.Awake
    ///     causing issues with detecting prefab name from a raycast at spawned MineRock5 objects
    /// </summary>
    internal class MineRock5PrefabTracker: MonoBehaviour
    {
        public string m_prefabName;
    }

    [HarmonyPatch]
    internal static class ZoneSystemPatches
    {
        private static readonly HashSet<string> MineRock5PrefabsNames = new();

        /// <summary>
        /// Adds MineRock5Trackers to all MineRock5 prefabs to encapsulate the root prefab name after they spawn.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
        public static void ZoneSystem_Start_Postfix()
        {
            // If loading into game world and prefabs from other mods have not been added
            if (SceneManager.GetActiveScene().name != "main" || !ZNetScene.instance)
            {
                return;
            }

            foreach (var prefab in ZNetScene.instance.m_prefabs)
            {
                // Check if prefab is a root prefab without a parent and if it has already been tracked and if it should be tracked
                if (!prefab.transform.parent && !MineRock5PrefabsNames.Contains(prefab.name) && prefab.GetComponent<MineRock5>())
                {
                    MineRock5PrefabsNames.Add(prefab.name);
                    prefab.AddMineRock5PrefabTracker();   
                }
            }


        }

    }
}
