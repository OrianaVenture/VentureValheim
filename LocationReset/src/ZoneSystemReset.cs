using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace VentureValheim.LocationReset
{
    public class ZoneSystemReset
    {
        private static readonly int LeviathanHash = "Leviathan".GetStableHashCode();
        private static readonly int SpawnSystemHash = "_ZoneCtrl".GetStableHashCode();
        private static List<ZoneSystem.ZoneVegetation> ResetVegetation;
        private static ZoneSystemResetComponent ResetComponent;

        public class ZoneSystemResetComponent : MonoBehaviour
        {
            private Dictionary<Vector2i, IEnumerator> resetCoroutines = new Dictionary<Vector2i, IEnumerator>();

            /// <summary>
            /// Reset coroutine that waits until a zone is fully loaded then checks
            /// if any leviathans are present, tries to reset them or spawn new ones if none.
            /// </summary>
            /// <param name="zoneID"></param>
            /// <param name="root"></param>
            /// <returns></returns>
            public IEnumerator WaitForReset(Vector2i zoneID, GameObject root)
            {
                yield return new WaitForSeconds(3);
                yield return null;
                int tries = 0;

                while (root != null)
                {
                    if (ZNetScene.instance.IsAreaReady(root.transform.position))
                    {
                        bool reset = false;
                        List<ZDO> objects = new List<ZDO>();
                        ZDOMan.instance.FindObjects(zoneID, objects);

                        if (objects != null)
                        {
                            int leviathansInZone = 0;
                            bool zoneOwner = false;
                            foreach (var obj in objects)
                            {
                                if (obj.m_prefab == LeviathanHash)
                                {
                                    // Count Leviathans in zone
                                    var levi = ZNetScene.instance.FindInstance(obj);
                                    var leviathan = levi?.transform.root.GetComponent<Leviathan>();
                                    if (leviathan != null && leviathan.CheckDelete(out bool deleted))
                                    {
                                        if (!deleted)
                                        {
                                            leviathansInZone++;
                                        }

                                        reset = true;
                                    }
                                    else
                                    {
                                        // Support multiple Leviathans
                                        leviathansInZone++;
                                        reset = false;
                                    }
                                }
                                else if (obj.m_prefab == SpawnSystemHash)
                                {
                                    // Check zone ownership
                                    zoneOwner = obj.IsOwner();
                                }
                            }

                            if (leviathansInZone == 0 && zoneOwner)
                            {
                                // TODO: Figure out how to delete a Leviathan and regenerate it in the same pass
                                // Currently is not spawning right after deleting them
                                reset = true;
                                ResetZone(zoneID, root);
                            }
                        }

                        if (reset || tries > 100)
                        {
                            break;
                        }

                        tries++;
                    }

                    yield return new WaitForSeconds(1);
                }

                resetCoroutines.Remove(zoneID);
            }

            /// <summary>
            /// Adds a reset coroutine for the given zone.
            /// </summary>
            /// <param name="zoneID"></param>
            /// <param name="root"></param>
            public void AddResetWatcher(Vector2i zoneID, GameObject root)
            {
                if (resetCoroutines.ContainsKey(zoneID))
                {
                    return;
                }

                var coroutine = WaitForReset(zoneID, root);
                resetCoroutines.Add(zoneID, coroutine);
                StartCoroutine(coroutine);
            }

            /// <summary>
            /// Respawns any Leviathan vegetation in a given zone.
            /// </summary>
            /// <param name="zoneID"></param>
            /// <param name="root"></param>
            private void ResetZone(Vector2i zoneID, GameObject root)
            {
                ZoneSystem zoneSystem = gameObject.GetComponent<ZoneSystem>();
                Heightmap heightmap = root.GetComponentInChildren<Heightmap>();

                if (zoneSystem != null && heightmap != null)
                {
                    zoneSystem.m_tempClearAreas.Clear();
                    zoneSystem.m_tempSpawnedObjects.Clear();

                    Vector3 zonePos = zoneSystem.GetZonePos(zoneID);

                    var vegetation = zoneSystem.m_vegetation;
                    zoneSystem.m_vegetation = ResetVegetation;
                    zoneSystem.PlaceVegetation(zoneID, zonePos, root.transform, heightmap,
                        zoneSystem.m_tempClearAreas, ZoneSystem.SpawnMode.Full, zoneSystem.m_tempSpawnedObjects);

                    zoneSystem.m_vegetation = vegetation;
                }
            }

            public void OnDestroy()
            {
                if (resetCoroutines != null)
                {
                    foreach (var coroutine in resetCoroutines.Values)
                    {
                        StopCoroutine(coroutine);
                    }
                }
            }
        }

        /// <summary>
        /// Find the Leviathan vegetation entry and create new list for restoring it later.
        /// Add the new reset manager component to the ZoneSystem.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
        public static class Patch_ZoneSystem_Start
        {
            private static void Postfix(ZoneSystem __instance)
            {
                if (LocationResetPlugin.GetEnableLeviathanReset())
                {
                    if (ResetVegetation == null)
                    {
                        foreach (var veg in __instance.m_vegetation)
                        {
                            if (veg.m_prefab != null && veg.m_prefab.name == "Leviathan")
                            {
                                ResetVegetation = new List<ZoneSystem.ZoneVegetation>() { veg };
                                break;
                            }
                        }
                    }

                    ResetComponent = __instance.gameObject.GetComponent<ZoneSystemResetComponent>();

                    if (ResetComponent == null)
                    {
                        ResetComponent = __instance.gameObject.AddComponent<ZoneSystemResetComponent>();
                    }
                }
            }
        }

        /// <summary>
        /// Add reset watcher for each zone after it is loaded.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnZone))]
        public static class Patch_ZoneSystem_SpawnZone
        {
            private static void Postfix(ZoneSystem __instance, Vector2i zoneID, ref GameObject root, bool __result)
            {
                if (LocationResetPlugin.GetEnableLeviathanReset())
                {
                    if (__result == false || __instance == null || zoneID == null || root == null)
                    {
                        return;
                    }

                    if (Player.m_localPlayer != null)
                    {
                        ResetComponent?.AddResetWatcher(zoneID, root);
                    }
                }
            }
        }
    }
}
