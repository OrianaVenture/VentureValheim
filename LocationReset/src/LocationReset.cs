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
            "TrollCave02".GetStableHashCode(),
            "SpiderCave01".GetStableHashCode(), // Monsterlabz
            "AshlandsCave_01".GetStableHashCode(), // Monsterlabz
            "AshlandsCave_02".GetStableHashCode(), // Monsterlabz
            "Loc_MistlandsCave_DoD".GetStableHashCode() // Horem
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
            var player = Player.m_localPlayer;
            if (player == null)
            {
                return false;
            }

            return InBounds(position, player.gameObject.transform.position, resetRange);
        }

        /// <summary>
        /// Determines if the local player is beyond 200 units of the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static bool LocalPlayerBeyondRange(Vector3 position)
        {
            var player = Player.m_localPlayer;
            if (player == null)
            {
                return true;
            }

            return !InBounds(position, player.gameObject.transform.position, 200f);
        }

        /// <summary>
        /// Returns true if there is player activity based off config settings.
        /// Locations on the ground will always perform a ground player activity check.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="distance"></param>
        /// <param name="skyLocation"></param>
        /// <returns></returns>
        public static bool PlayerActivity(Vector3 position, float distance, bool skyLocation)
        {
            if (LocationResetPlugin.GetSkipPlayerGroundPieceCheck() && skyLocation)
            {
                return PlayerActivityNoGroundPieceCheck(position, distance);
            }
            else
            {
                return PlayerActivityGroundPieceCheck(position, distance);
            }
        }

        /// <summary>
        /// Returns true if there are player placed pieces or tombstones near the location
        /// on the ground or the sky, or if there are players in the sky.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        private static bool PlayerActivityGroundPieceCheck(Vector3 position, float distance)
        {
            var list = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int lcv = 0; lcv < list.Length; lcv++)
            {
                var obj = list[lcv];
                if (InBounds(obj.transform.position, position, distance))
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
        /// <param name="distance"></param>
        /// <returns></returns>
        private static bool PlayerActivityNoGroundPieceCheck(Vector3 position, float distance)
        {
            var list = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int lcv = 0; lcv < list.Length; lcv++)
            {
                var obj = list[lcv];
                if (obj.transform.position.y >= LOCATION_MINIMUM && InBounds(obj.transform.position, position, distance))
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
        /// <param name="distance"></param>
        public static void DeleteLocation(Vector3 position, float distance)
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

                if (InBounds(obj.transform.position, position, distance))
                {
                    DeleteObject(ref obj);
                }
            }
        }

        /// <summary>
        /// Deletes all objects in the given ground location given it is a qualifying object.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="distance"></param>
        public static void DeleteGroundLocation(Vector3 position, float distance)
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

                if (QualifyingObject(obj) && InBounds(obj.transform.position, position, distance))
                {
                    DeleteObject(ref obj);
                }
            }
        }

        /// <summary>
        /// Determines if two positions are in range of each other.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="position"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool InBounds(Vector3 center, Vector3 position, float distance)
        {
            var delta = center - position;

            if (Math.Abs(delta.x) <= distance &&
                Math.Abs(delta.z) <= distance)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculates the maximum distance a point in space can be from another: The hypotenuse of a triangle represented by x, z.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static float GetMaximumDistance(float x, float z)
        {
            return (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(z, 2));
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

        /// <summary>
        /// Closes any doors that have key requirements.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="distance"></param>
        public static void ResetDoors(Vector3 position, float distance)
        {
            var list = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int lcv = 0; lcv < list.Length; lcv++)
            {
                var obj = list[lcv];

                if (obj.transform.position.y < LOCATION_MINIMUM)
                {
                    TryResetDoor(obj, position, distance);
                }
            }
        }

        /// <summary>
        /// If the object is a door, closes it if it has key requirements.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="position"></param>
        /// <param name="distance"></param>
        public static void TryResetDoor(GameObject obj, Vector3 position, float distance)
        {
            var door = obj.GetComponent<Door>();

            if (door != null && door.m_keyItem != null && InBounds(obj.transform.position, position, distance))
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"Attempting to reset a door at {obj.transform.position}.");
                door.m_nview?.GetZDO()?.Set("state", 0);
                door.UpdateState();
            }
        }

        #region Dungeons

        /// <summary>
        /// Attempts to reset the Dungeon.
        /// </summary>
        /// <param name="dg"></param>
        public void TryReset(DungeonGenerator dg)
        {
            LocationResetPlugin.LocationResetLogger.LogDebug($"Trying to reset DungeonGenerator located at: " +
                $"{dg.transform.position}, center: {dg.m_zoneCenter}");

            // Potential fix for multiplayer issues up for consideration
            /*if (!dg.m_nview.IsOwner())
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"Player does not own the chunk for DungeonGenerator reset. Skipping.");
                return;
            }*/

            if (!dg.NeedsReset())
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"DungeonGenerator does not need a reset. Skipping.");
                return;
            }

            Vector3 position = dg.transform.position + dg.m_zoneCenter;
            // TODO: Find the center of every dungeon accurately, test radius precision
            var distance = GetMaximumDistance(dg.m_zoneSize.x / 2, dg.m_zoneSize.z / 2);
            bool skyLocation = dg.transform.position.y >= LOCATION_MINIMUM;

            if (PlayerActivity(position, distance, skyLocation))
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"There is player activity here! Skipping.");
                return;
            }

            Reset(dg, position, distance);
            LocationResetPlugin.LocationResetLogger.LogDebug($"Done regenerating location for DungeonGenerator located at: {dg.transform.position}.");
        }

        /// <summary>
        /// Deletes and regenerates the game objects in range of the Dungeon. Resets keyed doors in range.
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="position"></param>
        /// <param name="distance"></param>
        public void Reset(DungeonGenerator dg, Vector3 position, float distance)
        {
            dg.SetLastResetNow();

            // Destroy location
            if (dg.transform.position.y < LOCATION_MINIMUM)
            {
                // This location is a Village or Fuling Camp
                DeleteGroundLocation(position, distance);
            }
            else
            {
                // Sky dungeon
                DeleteLocation(position, distance);
            }

            ResetDoors(position, distance);

            // Regenerate location
            Regenerate(dg);
        }

        /// <summary>
        /// Regenerates the dungeon with initial wear and tear damage.
        /// </summary>
        /// <param name="dg"></param>
        public void Regenerate(DungeonGenerator dg)
        {
            WearNTear.m_randomInitialDamage = true;

            dg.Generate(ZoneSystem.SpawnMode.Full);

            WearNTear.m_randomInitialDamage = false;
            SnapToGround.SnappAll();
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

            // Potential fix for multiplayer issues up for consideration
            /*if (!loc.m_nview.IsOwner())
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"Player does not own the chunk for LocationProxy reset. Skipping.");
                return;
            }*/

            int hash = loc.m_nview?.GetZDO()?.GetInt("location") ?? 0;

            if (hash == 0 || !loc.NeedsReset(hash))
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"LocationProxy does not need a reset. Skipping.");
                return;
            }

            Vector3 position = loc.transform.position;
            ZoneSystem.ZoneLocation zone = ZoneSystem.instance.GetLocation(hash);
            // TODO: Find the center of every location accurately, test radius precision
            float locationRadius = Mathf.Max(zone.m_exteriorRadius, zone.m_interiorRadius);
            float distance = GetMaximumDistance(locationRadius, locationRadius);

            if (PlayerActivity(position, locationRadius, SkyLocationHashes.Contains(hash)))
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"There is player activity here! Skipping.");
                return;
            }

            Reset(loc, position, distance, hash);

            LocationResetPlugin.LocationResetLogger.LogDebug($"Done regenerating location for LocationProxy located at: {loc.transform.position}.");
        }

        /// <summary>
        /// Deletes and regenerates the game objects in range of the Location. Resets keyed doors in range.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="position"></param>
        /// <param name="distance"></param>
        /// <param name="locationHash"></param>
        public void Reset(LocationProxy loc, Vector3 position, float distance, int locationHash)
        {
            loc.SetLastResetNow();

            // Destroy location
            if (!SkyLocationHashes.Contains(locationHash))
            {
                DeleteGroundLocation(position, distance);
            }
            else
            {
                DeleteLocation(position, distance);
            }

            ResetDoors(position, distance);

            // Regenerate location
            Regenerate(loc, locationHash);
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

                if (location == null || location.Value == 0)
                {
                    return;
                }

                if (!LocationResetPlugin.GetResetGroundLocations() && !SkyLocationHashes.Contains(location.Value))
                {
                    // Do not reset ground locations when config is off
                    return;
                }

                // Do not respawn location proxies for dungeons
                if (!DungeonHashes.Contains(location.Value))
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