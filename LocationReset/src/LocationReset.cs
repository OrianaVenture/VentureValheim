using System;
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

        public const int TROLL_CAVE_HASH = -1965863430; // TrollCave02
        public const float TROLL_CAVE_RADIUS = 30f;
        public const float LOCATION_MINIMUM = 4000f;
        public const string LAST_RESET = "VV_LastReset";

        /// <summary>
        /// Returns the current in-game day.
        /// </summary>
        /// <returns></returns>
        public static int GetGameDay()
        {
            return EnvMan.instance.GetCurrentDay();
        }

        /// <summary>
        /// Determines if the local player is within 30 units of the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static bool LocalPlayerInRange(Vector3 position)
        {
            var zoneSize = new Vector3(30f, 0f, 30f);
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
        /// </summary>
        /// <param name="position"></param>
        /// <param name="zoneSize"></param>
        /// <returns></returns>
        public static bool PlayerActivity(Vector3 position, Vector3 zoneSize)
        {
            if (LocationResetPlugin.GetSkipPlayerGroundPieceCheck())
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
                    ZNetView netView = obj.GetComponent<ZNetView>();
                    if (netView != null)
                    {
                        netView.GetZDO()?.SetOwner(ZDOMan.instance.GetMyID());
                    }

                    ZNetScene.instance.Destroy(obj);
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

        #region Dungeons

        /// <summary>
        /// Attempts to reset the Dungeon.
        /// </summary>
        /// <param name="dg"></param>
        public void TryReset(DungeonGenerator dg)
        {
            LocationResetPlugin.LocationResetLogger.LogDebug($"Trying to reset DungeonGenerator located at: {dg.transform.position}");

            if (dg.transform.position.y < LOCATION_MINIMUM)
            {
                // DungeonGenerator is not a sky dungeon, can not reset
                // TODO Update for ground DungeonGenerator support
                return;
            }

            if (!dg.NeedsReset())
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"DungeonGenerator does not need a reset.");
                return;
            }

            Vector3 position = dg.transform.position + dg.m_zoneCenter;
            Vector3 zoneSize = dg.m_zoneSize;

            if (PlayerActivity(position, zoneSize))
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
            DeleteLocation(position, zoneSize);

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

            if (!loc.NeedsReset())
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"LocationProxy does not need a reset.");
                return;
            }

            Vector3 position = loc.transform.position;
            float radius = TROLL_CAVE_RADIUS; // TODO update for more location support
            Vector3 zoneSize = new Vector3(radius, radius, radius);

            if (PlayerActivity(position, zoneSize))
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"There is player activity here! Will not reset.");
                return;
            }

            Reset(loc, position, zoneSize);

            LocationResetPlugin.LocationResetLogger.LogDebug($"Done regenerating location for LocationProxy located at: {loc.transform.position}.");
        }

        /// <summary>
        /// Deletes and regenerates the game objects in range of the Location.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="position"></param>
        /// <param name="zoneSize"></param>
        public void Reset(LocationProxy loc, Vector3 position, Vector3 zoneSize)
        {
            // Destroy location
            DeleteLocation(position, zoneSize);

            // Regenerate location
            Regenerate(loc);
            loc.SetLastResetNow();
        }

        /// <summary>
        /// Rerolls and generates the Location objects.
        /// </summary>
        /// <param name="loc"></param>
        public void Regenerate(LocationProxy loc)
        {
            int location = loc.m_nview.GetZDO().GetInt("location");
            var zone = ZoneSystem.instance.GetLocation(location);
            int seed = loc.m_nview.GetZDO().GetInt("seed");

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
                if (obj.transform.position.y < LOCATION_MINIMUM)
                {
                    // Do not regenerate items that have not been deleted
                    continue;
                }

                if (obj.gameObject.activeSelf)
                {
                    Vector3 objPosition = loc.transform.position + loc.transform.rotation * obj.gameObject.transform.position;
                    Quaternion objRotation = obj.gameObject.transform.rotation * loc.transform.rotation;

                    GameObject gameObject = UnityEngine.Object.Instantiate(obj.gameObject, objPosition, objRotation);
                    gameObject.SetActive(value: true);
                    gameObject.GetComponent<ZNetView>().GetZDO().SetPGWVersion(ZoneSystem.instance.m_pgwVersion);
                }
            }

            SnapToGround.SnappAll();
        }

        #endregion

        #region Patches

        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Load))]
        public static class Patch_DungeonGenerator_Load
        {
            private static void Postfix(DungeonGenerator __instance)
            {
                var reset = __instance.gameObject.GetComponent<DungeonGeneratorReset>();

                if (reset == null)
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

                if (location != null && location == TROLL_CAVE_HASH)
                {
                    var reset = __instance.gameObject.GetComponent<LocationProxyReset>();

                    if (reset == null)
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
            private static void Postfix(ZoneSystem __instance, ZoneLocation location)
            {
                var name = location.m_prefabName;
                var hash = name.GetStableHashCode();
                LocationResetPlugin.LocationResetLogger.LogDebug($"Patch_ZoneSystem_SpawnLocation: {name}, {hash} @ {__instance.transform.position}.");
            }
        }*/

        #endregion
    }
}