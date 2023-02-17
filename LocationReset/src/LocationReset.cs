using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.LocationReset
{
    public class LocationReset
    {
        static LocationReset() { }
        private LocationReset() { }
        private static readonly LocationReset _instance = new LocationReset();

        public static LocationReset Instance
        {
            get => _instance;
        }

        public const float LOCATION_MINIMUM = 4000f;
        public const string LAST_RESET = "VV_LastReset";

        public static readonly HashSet<int> DungeonHashes = new HashSet<int>
        {
            "WoodVillage1".GetStableHashCode(),
            "WoodFarm1".GetStableHashCode(),
            "Crypt2".GetStableHashCode(),
            "Crypt3".GetStableHashCode(),
            "Crypt4".GetStableHashCode(),
            "SunkenCrypt4".GetStableHashCode(),
            "MountainCave02".GetStableHashCode(),
            "GoblinCamp2".GetStableHashCode(),
            "Mistlands_DvergrBossEntrance1".GetStableHashCode(),
            "Mistlands_DvergrTownEntrance1".GetStableHashCode(),
            "Mistlands_DvergrTownEntrance2".GetStableHashCode()
        };

        public static readonly HashSet<int> SkyLocationHashes = new HashSet<int>
        {
            "TrollCave02".GetStableHashCode()
        };

        /// <summary>
        /// Get the trigger range for resetting a location by its id hash.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static float GetResetRange(int hash)
        {
            return SkyLocationHashes.Contains(hash) ? 30f : 100f;
        }

        /// <summary>
        /// Get the trigger range for resetting a dungeon by transform height.
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public static float GetResetRange(float height)
        {
            return height >= LOCATION_MINIMUM ? 30f : 100f;
        }

        /// <summary>
        /// Returns the current in-game day.
        /// </summary>
        /// <returns></returns>
        public static int GetGameDay()
        {
            return EnvMan.instance.GetCurrentDay();
        }

        /// <summary>
        /// Determines if the local player is within range of the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool LocalPlayerInRange(Vector3 position, float resetRange)
        {
            var zoneSize = new Vector3(resetRange, 0f, resetRange);
            var player = Player.m_localPlayer;
            if (player == null)
            {
                return false;
            }

            return InBounds(position, player.gameObject.transform.position, zoneSize);
        }

        /// <summary>
        /// Determines if the local player is beyond 200 units of the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static bool LocalPlayerBeyondRange(Vector3 position)
        {
            var zoneSize = new Vector3(200f, 0f, 200f);
            var player = Player.m_localPlayer;
            if (player == null)
            {
                return true;
            }

            return !InBounds(position, player.gameObject.transform.position, zoneSize);
        }

        /// <summary>
        /// Returns true if there is player activity based off config settings.
        /// Locations on the ground will always perform a ground player activity check.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="zoneSize"></param>
        /// <param name="skyLocation"></param>
        /// <returns></returns>
        public static bool PlayerActivity(Vector3 position, Vector3 zoneSize, bool skyLocation)
        {
            if (LocationResetPlugin.GetSkipPlayerGroundPieceCheck() && skyLocation)
            {
                return PlayerActivityNoGroundPieceCheck(position, zoneSize);
            }
            else
            {
                return PlayerActivityGroundPieceCheck(position, zoneSize);
            }
        }

        /// <summary>
        /// Returns true if there are player placed pieces or tombstones near the location
        /// on the ground or the sky, or if there are players in the sky.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="zoneSize"></param>
        /// <returns></returns>
        private static bool PlayerActivityGroundPieceCheck(Vector3 position, Vector3 zoneSize)
        {
            var list = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int lcv = 0; lcv < list.Length; lcv++)
            {
                var obj = list[lcv];
                if (InBounds(obj.transform.position, position, zoneSize))
                {
                    var piece = obj.GetComponent<Piece>();
                    if (piece != null)
                    {
                        if (piece.GetCreator() != 0L)
                        {
                            return true;
                        }
                    }
                    else if (obj.GetComponent<TombStone>() != null)
                    {
                        return true;
                    }
                    else if (obj.transform.position.y >= LOCATION_MINIMUM && obj.GetComponent<Player>() != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if there are player placed pieces, tombstones, or players
        /// in the sky at the location.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="zoneSize"></param>
        /// <returns></returns>
        private static bool PlayerActivityNoGroundPieceCheck(Vector3 position, Vector3 zoneSize)
        {
            var list = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int lcv = 0; lcv < list.Length; lcv++)
            {
                var obj = list[lcv];
                if (obj.transform.position.y >= LOCATION_MINIMUM && InBounds(obj.transform.position, position, zoneSize))
                {
                    var piece = obj.GetComponent<Piece>();
                    if (piece != null)
                    {
                        if (piece.GetCreator() != 0L)
                        {
                            return true;
                        }
                    }
                    else if (obj.GetComponent<TombStone>() != null)
                    {
                        return true;
                    }
                    else if (obj.GetComponent<Player>() != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Deletes all objects in the given sky location unless they are of type
        /// DungeonGenerator or LocationProxy.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="zoneSize"></param>
        public static void DeleteLocation(Vector3 position, Vector3 zoneSize)
        {
            var list = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int lcv = 0; lcv < list.Length; lcv++)
            {
                var obj = list[lcv];

                if (obj.transform.position.y < LOCATION_MINIMUM)
                {
                    continue;
                }

                if (obj.GetComponent<DungeonGenerator>() != null || obj.GetComponent<LocationProxy>() != null)
                {
                    // Do not destroy the generators
                    continue;
                }

                if (InBounds(obj.transform.position, position, zoneSize))
                {
                    DeleteObject(ref obj);
                }
            }
        }

        /// <summary>
        /// Deletes all objects in the given ground location given it is a qualifying object.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="zoneSize"></param>
        public static void DeleteGroundLocation(Vector3 position, Vector3 zoneSize)
        {
            var list = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int lcv = 0; lcv < list.Length; lcv++)
            {
                var obj = list[lcv];

                if (obj.transform.position.y >= LOCATION_MINIMUM || obj.GetComponent<Player>() != null)
                {
                    // Do not destroy the Players or sky objects
                    continue;
                }

                if (QualifyingObject(obj) && InBounds(obj.transform.position, position, zoneSize))
                {
                    DeleteObject(ref obj);
                }
            }
        }

        /// <summary>
        /// Determines if two positions are in x, z range of each other.
        /// </summary>
        /// <param name="position1"></param>
        /// <param name="position2"></param>
        /// <param name="zoneSize"></param>
        /// <returns></returns>
        public static bool InBounds(Vector3 position1, Vector3 position2, Vector3 zoneSize)
        {
            var delta = position1 - position2;

            if (Math.Abs(delta.x) <= zoneSize.x &&
                Math.Abs(delta.z) <= zoneSize.z)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether an object can be destroyed and respawned.
        /// Used for ground locations.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static bool QualifyingObject(GameObject obj)
        {
            return obj.GetComponent<Piece>() ||
                obj.GetComponent<Pickable>() ||
                obj.GetComponent<Character>() ||
                obj.GetComponent<CreatureSpawner>() ||
                obj.GetComponent<WearNTear>() ||
                obj.GetComponent<SpawnArea>() ||
                obj.GetComponent<RandomSpawn>();
        }

        /// <summary>
        /// Deletes an object from the ZNetScene.
        /// </summary>
        /// <param name="obj"></param>
        private static void DeleteObject(ref GameObject obj)
        {
            obj.GetComponent<ZNetView>()?.GetZDO()?.SetOwner(ZDOMan.instance.GetMyID());

            ZNetScene.instance.Destroy(obj);
        }

        #region Dungeons

        /// <summary>
        /// Attempts to reset the Dungeon.
        /// </summary>
        /// <param name="dg"></param>
        public void TryReset(DungeonGenerator dg)
        {
            LocationResetPlugin.LocationResetLogger.LogDebug($"Trying to reset DungeonGenerator located at: {dg.transform.position}, center: {dg.m_zoneCenter}");

            if (!dg.NeedsReset())
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"DungeonGenerator does not need a reset.");
                return;
            }

            Vector3 position = dg.transform.position + dg.m_zoneCenter;
            Vector3 zoneSize = dg.m_zoneSize;
            bool skyLocation = position.y >= LOCATION_MINIMUM;

            if (PlayerActivity(position, zoneSize, skyLocation))
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"There is player activity here! Will not reset.");
                return;
            }

            Reset(dg, position, zoneSize);
            LocationResetPlugin.LocationResetLogger.LogDebug($"Done regenerating location for DungeonGenerator located at: {dg.transform.position}.");
        }

        /// <summary>
        /// Deletes and regenerates the game objects in range of the Dungeon.
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="position"></param>
        /// <param name="zoneSize"></param>
        public void Reset(DungeonGenerator dg, Vector3 position, Vector3 zoneSize)
        {
            // Destroy location
            if (dg.transform.position.y < LOCATION_MINIMUM)
            {
                // This location is a Village or Fuling Camp
                DeleteGroundLocation(position, zoneSize);
            }
            else
            {
                // Sky dungeon
                DeleteLocation(position, zoneSize);
            }

            // Regenerate location
            dg.Generate(ZoneSystem.SpawnMode.Full);
            dg.SetLastResetNow();
        }

        #endregion

        #region Locations

        /// <summary>
        /// Attempts to reset the location.
        /// </summary>
        /// <param name="loc"></param>
        public void TryReset(LocationProxy loc)
        {
            LocationResetPlugin.LocationResetLogger.LogDebug($"Trying to reset LocationProxy located at: {loc.transform.position}");

            int hash = loc.m_nview?.GetZDO()?.GetInt("location") ?? 0;

            if (!loc.NeedsReset(hash))
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"LocationProxy does not need a reset.");
                return;
            }

            Vector3 position = loc.transform.position;
            ZoneSystem.ZoneLocation zone = ZoneSystem.instance.GetLocation(hash);
            float locationRadius = Mathf.Max(zone.m_exteriorRadius, zone.m_interiorRadius) + 3f; // Add buffer to range
            Vector3 zoneSize = new Vector3(locationRadius, locationRadius, locationRadius);

            if (PlayerActivity(position, zoneSize, SkyLocationHashes.Contains(hash)))
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"There is player activity here! Will not reset.");
                return;
            }

            Reset(loc, position, zoneSize, hash);

            LocationResetPlugin.LocationResetLogger.LogDebug($"Done regenerating location for LocationProxy located at: {loc.transform.position}.");
        }

        /// <summary>
        /// Deletes and regenerates the game objects in range of the Location.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="position"></param>
        /// <param name="zoneSize"></param>
        /// <param name="locationHash"></param>
        public void Reset(LocationProxy loc, Vector3 position, Vector3 zoneSize, int locationHash)
        {
            // Destroy location
            if (!SkyLocationHashes.Contains(locationHash))
            {
                DeleteGroundLocation(position, zoneSize);
            }
            else
            {
                DeleteLocation(position, zoneSize);
            }

            // Regenerate location
            Regenerate(loc, locationHash);
            loc.SetLastResetNow();
        }

        /// <summary>
        /// Rerolls and generates the Location objects.
        /// </summary>
        /// <param name="loc"></param>
        public void Regenerate(LocationProxy loc, int locationHash)
        {
            var zone = ZoneSystem.instance.GetLocation(locationHash);
            int seed = loc.m_nview?.GetZDO()?.GetInt("seed") ?? 0;

            foreach (ZNetView obj in zone.m_netViews)
            {
                obj.gameObject.SetActive(value: true);
            }

            UnityEngine.Random.InitState(seed);
            foreach (RandomSpawn randomSpawn in zone.m_randomSpawns)
            {
                randomSpawn.Randomize();
            }

            WearNTear.m_randomInitialDamage = zone.m_location.m_applyRandomDamage;

            foreach (ZNetView obj in zone.m_netViews)
            {
                if (obj.gameObject.activeSelf)
                {
                    // Do not regenerate items that have not been deleted
                    if (SkyLocationHashes.Contains(locationHash))
                    {
                        if (obj.transform.position.y < LOCATION_MINIMUM)
                        {
                            continue;
                        }
                    }
                    else if (!QualifyingObject(obj.gameObject))
                    {
                        continue;
                    }

                    Vector3 objPosition = loc.transform.position + loc.transform.rotation * obj.gameObject.transform.position;
                    Quaternion objRotation = obj.gameObject.transform.rotation * loc.transform.rotation;

                    GameObject gameObject = UnityEngine.Object.Instantiate(obj.gameObject, objPosition, objRotation);
                    gameObject.SetActive(value: true);
                    gameObject.GetComponent<ZNetView>()?.GetZDO()?.SetPGWVersion(ZoneSystem.instance.m_pgwVersion);
                }
            }

            WearNTear.m_randomInitialDamage = false;
            SnapToGround.SnappAll();
        }

        #endregion

        #region Patches

        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Load))]
        public static class Patch_DungeonGenerator_Load
        {
            private static void Postfix(DungeonGenerator __instance)
            {
                if (__instance.gameObject.GetComponent<DungeonGeneratorReset>() == null)
                {
                    __instance.gameObject.AddComponent<DungeonGeneratorReset>();
                }
            }
        }

        [HarmonyPatch(typeof(LocationProxy), nameof(LocationProxy.SpawnLocation))]
        public static class Patch_LocationProxy_SpawnLocation
        {
            private static void Postfix(LocationProxy __instance)
            {
                var location = __instance.m_nview?.GetZDO()?.GetInt("location");

                if (!LocationResetPlugin.GetResetGroundLocations() && location != null && !SkyLocationHashes.Contains(location.Value))
                {
                    // Do not reset ground locations when config is off
                    return;
                }

                // Do not respawn location proxies for dungeons
                if (location != null && !DungeonHashes.Contains(location.Value))
                {
                    if (__instance.gameObject.GetComponent<LocationProxyReset>() == null)
                    {
                        __instance.gameObject.AddComponent<LocationProxyReset>();
                    }
                }
            }
        }

        // Helps to find the location names
        /*[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
        public static class Patch_ZoneSystem_SpawnLocation
        {
            private static void Postfix(ZoneSystem __instance, ZoneSystem.ZoneLocation location)
            {
                var name = location.m_prefabName;
                var hash = name.GetStableHashCode();
                LocationResetPlugin.LocationResetLogger.LogDebug($"Location spawned with data: {name}, {hash}.");
            }
        }*/

        #endregion
    }
}