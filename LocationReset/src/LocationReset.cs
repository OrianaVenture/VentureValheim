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

        public static readonly HashSet<int> SkyLocationHashes = new HashSet<int>
        {
            "TrollCave02".GetStableHashCode(),
            "SpiderCave01".GetStableHashCode(), // Monsterlabz
            "AshlandsCave_01".GetStableHashCode(), // Monsterlabz
            "AshlandsCave_02".GetStableHashCode(), // Monsterlabz
            "Loc_MistlandsCave_DoD".GetStableHashCode(), // Horem
            "CaveDeepNorth_TW".GetStableHashCode() // Therzie Warfare
        };

        #region Utility Functions

        /// <summary>
        /// Returns the current in-game day.
        /// </summary>
        /// <returns></returns>
        public static int GetGameDay()
        {
            return EnvMan.instance.GetCurrentDay();
        }

        /// <summary>
        /// Returns true if the location/dungeon is suspended in the sky.
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="dg"></param>
        /// <returns></returns>
        public static bool IsSkyLocation(int hash, DungeonGenerator dg)
        {
            return SkyLocationHashes.Contains(hash) || (dg != null && dg.transform.position.y >= LOCATION_MINIMUM);
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
        /// Returns the closest DungeonGenerator to the point in bounds.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static DungeonGenerator GetDungeonGeneratorInBounds(Vector3 center, float distance)
        {
            // TODO evaluate if "else if" ever is reached, it is an edge case that can make this run longer
            DungeonGenerator returnDG = null;
            var list = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int lcv = 0; lcv < list.Length; lcv++)
            {
                var obj = list[lcv];
                var dg = obj.GetComponent<DungeonGenerator>();

                if (dg != null && InBounds(center, obj.transform.position, distance))
                {
                    if (returnDG == null)
                    {
                        returnDG = dg;
                    }
                    else if ((center - obj.transform.position).sqrMagnitude < (center - returnDG.transform.position).sqrMagnitude)
                    {
                        LocationResetPlugin.LocationResetLogger.LogInfo($"SELECTING CLOSER DUNGEON!! This is an edge case. " +
                            $"If you see this tell Venture and she will get excited and give you a super tester role in discord.");
                        // Select closer Dungeon
                        returnDG = dg;
                    }
                }
            }

            return returnDG;
        }

        /// <summary>
        /// Finds the dungeon center based off the existing environment box if applicable.
        /// </summary>
        /// <param name="dg"></param>
        /// <returns></returns>
        public Vector3 GetDungeonCenter(DungeonGenerator dg)
        {
            Vector3 position = dg.transform.position + dg.m_zoneCenter; // Fallback position

            Collider[] hits = Physics.OverlapBox(dg.transform.position, dg.transform.localScale / 2, Quaternion.identity);
            foreach (var hit in hits)
            {
                if (hit.gameObject.GetComponent<EnvZone>() != null)
                {
                    position = hit.gameObject.transform.position; // Optimal position
                    break;
                }
            }

            return position;
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
        /// Calculates the generous radius of a dungeon.
        /// </summary>
        /// <param name="dg"></param>
        /// <returns></returns>
        public float GetDungeonRadius(DungeonGenerator dg)
        {
            return GetMaximumDistance(dg.m_zoneSize.x / 2, dg.m_zoneSize.z / 2);
        }

        /// <summary>
        /// Returns whether an object can be destroyed and respawned.
        /// Used for ground locations.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static bool QualifyingObject(GameObject obj)
        {
            return obj.GetComponent<ItemDrop>() ||
                obj.GetComponent<Piece>() ||
                obj.GetComponent<Pickable>() ||
                obj.GetComponent<Character>() ||
                obj.GetComponent<CreatureSpawner>() ||
                obj.GetComponent<WearNTear>() ||
                obj.GetComponent<SpawnArea>() ||
                obj.GetComponent<RandomSpawn>();
        }

        #endregion

        #region Reset Logic

        /// <summary>
        /// Deletes an object from the ZNetScene.
        /// </summary>
        /// <param name="obj"></param>
        private static void DeleteObject(ref GameObject obj)
        {
            obj.GetComponent<ZNetView>()?.GetZDO()?.SetOwner(ZDOMan.GetSessionID());

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
                door.m_nview?.GetZDO()?.Set(ZDOVars.s_state, 0);
                door.UpdateState();
            }
        }

        /// <summary>
        /// Deletes all objects in the given sky location unless they are of type
        /// DungeonGenerator, LocationProxy, or Player.
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

                if (obj.GetComponent<DungeonGenerator>() != null ||
                    obj.GetComponent<LocationProxy>() != null ||
                    obj.GetComponent<Player>() != null)
                {
                    // Do not destroy the generators or Players
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
        /// Attempts to reset the location.
        /// </summary>
        /// <param name="loc"></param>
        public void TryReset(LocationProxy loc, int hash)
        {
            int seed = loc.m_nview?.GetZDO()?.GetInt(ZDOVars.s_seed) ?? 0;

            LocationResetPlugin.LocationResetLogger.LogDebug($"Trying to reset Location with hash {hash} at: {loc.transform.position}");

            if (seed == 0 || hash == 0)
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"There was an issue getting the location hash or seed, abort.");
                return;
            }

            if (!loc.NeedsReset(hash))
            {
                return;
            }

            // Location positioning
            Vector3 position = loc.transform.position;
            ZoneSystem.ZoneLocation zone = ZoneSystem.instance.GetLocation(hash);
            float locationRadius = Mathf.Max(zone.m_exteriorRadius, zone.m_interiorRadius);
            float distance = GetMaximumDistance(locationRadius, locationRadius);

            // If a dungeon override location positioning
            var dg = GetDungeonGeneratorInBounds(position, distance);
            bool skyLocation = IsSkyLocation(hash, dg);

            if (dg != null)
            {
                if (dg.m_nview == null || !dg.m_nview.IsOwner())
                {
                    LocationResetPlugin.LocationResetLogger.LogDebug($"Needs a reset but does not own the DG object! Skipping.");
                    return;
                }

                position = GetDungeonCenter(dg);
                distance = GetDungeonRadius(dg);
            }
            else if (!LocationResetPlugin.GetResetGroundLocations() && !SkyLocationHashes.Contains(hash))
            {
                // Do not reset ground locations when config is off
                return;
            }

            if (PlayerActivity(position, distance, skyLocation))
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"There is player activity here! Skipping.");
                return;
            }

            if (!loc.SetLastResetNow())
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"There was an issue setting the reset time, abort.");
                return;
            }

            Reset(loc, zone, seed, position, distance, skyLocation, dg);
            LocationResetPlugin.LocationResetLogger.LogInfo($"Done regenerating location {zone.m_prefabName} at: {loc.transform.position}");
        }

        /// <summary>
        /// Deletes and regenerates the game objects in range of the Location. Resets keyed doors in range.
        /// </summary>
        /// <param name="loc">LocationProxy needing a reset</param>
        /// <param name="zone">ZoneLocation of the LocationProxy</param>
        /// <param name="seed"></param>
        /// <param name="position">Center position for the reset</param>
        /// <param name="distance">Radius for the reset</param>
        /// <param name="skyLocation"></param>
        /// <param name="dg">DungeonGenerator of the location if any exists</param>
        public void Reset(LocationProxy loc, ZoneSystem.ZoneLocation zone, int seed, Vector3 position, float distance, bool skyLocation, DungeonGenerator dg)
        {
            // Destroy location
            if (!skyLocation)
            {
                DeleteGroundLocation(position, distance);
            }
            else
            {
                DeleteLocation(position, distance);
            }

            ResetDoors(position, distance);

            Regenerate(loc, zone, seed, skyLocation, dg);
        }

        /// <summary>
        /// Rerolls and generates the Location objects.
        /// </summary>
        /// <param name="loc">LocationProxy needing a reset</param>
        /// <param name="zone">ZoneLocation of the LocationProxy</param>
        /// <param name="seed"></param>
        /// <param name="skyLocation"></param>
        /// <param name="dg">DungeonGenerator of the location if any exists</param>
        public void Regenerate(LocationProxy loc, ZoneSystem.ZoneLocation zone, int seed, bool skyLocation, DungeonGenerator dg)
        {
            // TODO find a way to reset the ground or to remove terrain modifications from applying
            // Tar pits give infinite resets with no difficulty
            // Fuling camps dig deep pits after many resets

            WearNTear.m_randomInitialDamage = zone.m_location?.m_applyRandomDamage ?? false;

            if (dg != null)
            {
                // Don't need to perform complex check because null is vector.0, so doesn't change when not set
                dg.m_originalPosition = zone.m_generatorPosition;
                dg.Generate(ZoneSystem.SpawnMode.Full);
            }
            else
            {
                foreach (ZNetView obj in zone.m_netViews)
                {
                    obj.gameObject.SetActive(value: true);
                }

                UnityEngine.Random.InitState(seed);
                foreach (RandomSpawn randomSpawn in zone.m_randomSpawns)
                {
                    randomSpawn.Randomize();
                }

                int count = 0;

                foreach (ZNetView obj in zone.m_netViews)
                {
                    if (obj.gameObject.activeSelf)
                    {
                        // Do not regenerate items that have not been deleted
                        if (skyLocation)
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
                        count++;
                    }
                }

                LocationResetPlugin.LocationResetLogger.LogDebug($"Spawned {count} objects.");
            }

            WearNTear.m_randomInitialDamage = false;
            SnapToGround.SnappAll();
        }

        #endregion

        #region Patches

        [HarmonyPatch(typeof(LocationProxy), nameof(LocationProxy.SpawnLocation))]
        public static class Patch_LocationProxy_SpawnLocation
        {
            private static void Postfix(LocationProxy __instance)
            {
                // Start reset watcher for clients
                if (!ZNet.instance.IsDedicated())
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
            private static void Postfix(ZoneSystem.ZoneLocation location)
            {
                var name = location.m_prefabName;
                var hash = name.GetStableHashCode();
                LocationResetPlugin.LocationResetLogger.LogDebug($"Location spawned with data: {name}, {hash}.");
            }
        }*/

        #endregion
    }
}