using System;
using System.Collections.Generic;
using BepInEx;
using System.Linq;
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

        public static readonly HashSet<int> IgnoreLocationHashes = new HashSet<int>
        {
            "Mystical_Well0".GetStableHashCode(), // Monsterlabz
            "Wayshrine".GetStableHashCode(), // Azumatt
            "Wayshrine_Ashlands".GetStableHashCode(), // Azumatt
            "Wayshrine_Frost".GetStableHashCode(), // Azumatt
            "Wayshrine_Plains".GetStableHashCode(), // Azumatt
            "Wayshrine_Skull".GetStableHashCode(), // Azumatt
            "Wayshrine_Skull_2".GetStableHashCode() // Azumatt
        };

        public HashSet<int> CustomIgnoreLocationHashes = new HashSet<int>();

        public struct LocationPosition
        {
            public bool IsSkyLocation;
            public bool IsDungeon;
            public DungeonGenerator DungeonGenerator;
            public Vector3 SkyPosition;
            public float SkyDistance;
            public Vector3 GroundPosition;
            public float GroundDistance;
            public Vector3 GeneratorPosition;

            public LocationPosition(LocationProxy loc, ZoneSystem.ZoneLocation zone, Location location)
            {
                var dg = location.m_generator;
                IsSkyLocation = location.m_hasInterior ||
                    (dg != null && dg.transform.position.y > LOCATION_MINIMUM);
                IsDungeon = false;
                GroundPosition = loc.transform.position;
                SkyPosition = loc.transform.position;
                GroundDistance = zone.m_exteriorRadius;
                SkyDistance = zone.m_interiorRadius;

                float maxDistance = Mathf.Max(GroundDistance, SkyDistance);
                GeneratorPosition = location.m_generator != null ?
                    location.m_generator.transform.localPosition : Vector3.zero;
                DungeonGenerator = GetDungeonGeneratorInBounds(GroundPosition + GeneratorPosition, maxDistance);

                if (DungeonGenerator != null)
                {
                    IsDungeon = true;
                    if (IsSkyLocation)
                    {
                        SkyDistance = Instance.GetDungeonRadius(DungeonGenerator);
                    }
                    else
                    {
                        // Add buffer room to ground dungeons
                        GroundDistance += 5;
                    }
                }
            }
        }

        public struct PlayerActivity
        {
            public bool SkyActivity;
            public bool GroundActivity;

            public PlayerActivity(bool sky, bool ground)
            {
                SkyActivity = sky;
                GroundActivity = ground;
            }
        }

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
        /// <returns></returns>
        private static PlayerActivity GetPlayerActivity(LocationPosition position, int hash)
        {
            bool exemptLocation = hash == LocationResetPlugin.Hash_HildirTower;
            PlayerActivity activity = new PlayerActivity(false, false);

            var list = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int lcv = 0; lcv < list.Length; lcv++)
            {
                var obj = list[lcv];
                if (obj.transform.position.y >= LOCATION_MINIMUM)
                {
                    if (position.IsSkyLocation && !activity.SkyActivity && 
                        InBounds(obj.transform.position, position.SkyPosition, position.SkyDistance))
                    {
                        activity.SkyActivity = IsPlayerActiveObject(obj);
                    }
                }
                else
                {
                    if (!activity.GroundActivity &&
                        InBounds(obj.transform.position, position.GroundPosition, position.GroundDistance))
                    {
                        activity.GroundActivity = IsPlayerActiveObject(obj, exemptLocation);
                    }
                }
            }

            return activity;
        }

        /// <summary>
        /// Returns true if the object is of type TombStone, Player,
        /// or is a player placed Piece. Special rules for exempt locations
        /// such that only crafting station and fireplace player pieces are considered.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static bool IsPlayerActiveObject(GameObject obj, bool exemptLocation = false)
        {
            var piece = obj.GetComponent<Piece>();
            if (piece != null)
            {
                bool exempt = exemptLocation &&
                    (obj.GetComponent<CraftingStation>() == null &&
                    obj.GetComponent<Fireplace>() == null);

                if (!exempt && piece.GetCreator() != 0L)
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
            var mag = GetMaximumDistance(delta.x, delta.z);
            return mag <= distance;
        }

        /// <summary>
        /// Returns the first DungeonGenerator in bounds if exists.
        /// </summary>
        /// <param name="center"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static DungeonGenerator GetDungeonGeneratorInBounds(Vector3 center, float distance)
        {
            var list = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int lcv = 0; lcv < list.Length; lcv++)
            {
                var obj = list[lcv];
                var dg = obj.GetComponent<DungeonGenerator>();

                if (dg != null && InBounds(center, obj.transform.position, distance))
                {
                    return dg;
                }
            }

            return null;
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
            if (obj.GetComponent<RandomFlyingBird>() != null)
            {
                return false;
            }

            return obj.GetComponent<Destructible>() ||
                obj.GetComponent<MineRock>() ||
                obj.GetComponent<MineRock5>() ||
                obj.GetComponent<ItemDrop>() ||
                obj.GetComponent<Piece>() ||
                obj.GetComponent<Pickable>() ||
                obj.GetComponent<Character>() ||
                obj.GetComponent<CreatureSpawner>() ||
                obj.GetComponent<WearNTear>() ||
                obj.GetComponent<SpawnArea>() ||
                obj.GetComponent<RandomSpawn>();
        }

        /// <summary>
        /// Returns whether an object can be destroyed and respawned.
        /// Used for sky locations.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static bool QualifyingSkyObject(GameObject obj)
        {
            return !(obj.GetComponent<DungeonGenerator>() ||
                obj.GetComponent<LocationProxy>() ||
                obj.GetComponent<Player>());
        }

        /// <summary>
        /// Setup custom ignored locations from the config.
        /// </summary>
        /// <param name="config"></param>
        public static void SetCustomIgnoreLocationHashes(string config)
        {
            Instance.CustomIgnoreLocationHashes = new HashSet<int>();

            if (!config.IsNullOrWhiteSpace())
            {
                List<string> keys = config.Split(',').ToList();
                for (var lcv = 0; lcv < keys.Count; lcv++)
                {
                    Instance.CustomIgnoreLocationHashes.Add(keys[lcv].GetStableHashCode());
                }
            }
        }

        #endregion

        #region Reset Logic

        /// <summary>
        /// Deletes an object from the ZNetScene.
        /// </summary>
        /// <param name="obj"></param>
        private static void DeleteObject(ref GameObject obj)
        {
            var nview = obj.GetComponent<ZNetView>();
            if (nview != null && nview.GetZDO() != null)
            {
                nview.GetZDO().SetOwner(ZDOMan.GetSessionID());
            }

            ZNetScene.instance.Destroy(obj);
        }

        /// <summary>
        /// If the object is a door, closes it if it has key requirements.
        /// </summary>
        /// <param name="obj"></param>
        public static void TryResetDoor(GameObject obj)
        {
            var door = obj.GetComponent<Door>();

            if (door != null && door.m_keyItem != null)
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"Attempting to reset a door at {obj.transform.position}.");
                if (door.m_nview != null && door.m_nview.GetZDO() != null)
                {
                    door.m_nview.GetZDO().Set(ZDOVars.s_state, 0);
                    door.UpdateState();
                }
            }
        }

        /// <summary>
        /// Deletes all objects in the given location based on player activity and location type.
        /// Resets keyed doors on ground if applicable.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="activity"></param>
        public static void DeleteLocation(LocationPosition position, PlayerActivity activity)
        {
            var list = SceneManager.GetActiveScene().GetRootGameObjects();

            for (int lcv = 0; lcv < list.Length; lcv++)
            {
                var obj = list[lcv];

                if (obj.transform.position.y < LOCATION_MINIMUM)
                {
                    // ground object
                    if (!activity.GroundActivity && 
                        InBounds(position.GroundPosition, obj.transform.position, position.GroundDistance))
                    {
                        if (!obj.GetComponent<Player>() && QualifyingObject(obj))
                        {
                            DeleteObject(ref obj);
                        }
                        else
                        {
                            TryResetDoor(obj);
                        }
                    }
                }
                else if (position.IsSkyLocation)
                {
                    // sky object
                    if (!activity.SkyActivity && QualifyingSkyObject(obj) && 
                        InBounds(position.SkyPosition, obj.transform.position, position.SkyDistance))
                    {
                        DeleteObject(ref obj);
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to reset the location.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="hash"></param>
        /// <param name="force">True to bypass time constraint and player activity checks</param>
        public void TryReset(LocationProxy loc, int hash, bool force = false)
        {
            LocationResetPlugin.LocationResetLogger.LogDebug($"Trying to reset Location with hash {hash} at: {loc.transform.position}");

            int seed = 0;
            if (loc.m_nview != null && loc.m_nview.GetZDO() != null)
            {
                seed = loc.m_nview.GetZDO().GetInt(ZDOVars.s_seed);
            }

            if (seed == 0 || hash == 0)
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"There was an issue getting the location hash or seed, abort.");
                return;
            }

            if (ModCompat.MVBPInstalled && hash == LocationResetPlugin.Hash_StartTemple)
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"Cannot reset start temple when using More Vanilla Build Prefabs.");
                return;
            }

            if (!force && !loc.NeedsReset(hash))
            {
                return;
            }

            ZoneSystem.ZoneLocation zone = ZoneSystem.instance.GetLocation(hash);
            if (zone == null)
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"There was an issue getting the zone location, abort.");
                return;
            }

            // Load and Release not needed?
            //zone.m_prefab.Load();
            Location location = zone.m_prefab.Asset.GetComponent<Location>();

            TryResetAfterLoadPrefab(loc, zone, location, hash, seed, force);

            //zone.m_prefab.Release();
        }

        private void TryResetAfterLoadPrefab(LocationProxy loc, ZoneSystem.ZoneLocation zone, Location location, int hash, int seed, bool force)
        {
            if (location == null)
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"There was an issue getting the location, abort.");
                return;
            }

            LocationPosition position = new LocationPosition(loc, zone, location);

            if (ModCompat.DungeonSplitterInstalled && position.IsSkyLocation)
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"Cannot reset sky locations when using Dungeon Splitter.");
                return;
            }

            if (position.DungeonGenerator != null)
            {
                if (position.DungeonGenerator.m_nview == null || !position.DungeonGenerator.m_nview.IsOwner())
                {
                    LocationResetPlugin.LocationResetLogger.LogDebug($"Needs a reset but does not own the DG object! Skipping.");
                    return;
                }
            }
            else if (!LocationResetPlugin.GetResetGroundLocations() && !position.IsSkyLocation)
            {
                // Do not reset ground locations when config is off
                return;
            }

            PlayerActivity playerActivity;
            if (force)
            {
                playerActivity = new PlayerActivity(false, false);
            }
            else
            {
                playerActivity = GetPlayerActivity(position, hash);
            }

            bool skipGround = LocationResetPlugin.GetSkipPlayerGroundPieceCheck() && position.IsSkyLocation;
            if ((!skipGround && playerActivity.GroundActivity) || playerActivity.SkyActivity)
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"There is player activity here! Skipping.");
                return;
            }

            if (!loc.SetLastResetNow())
            {
                LocationResetPlugin.LocationResetLogger.LogDebug($"There was an issue setting the reset time, abort.");
                return;
            }

            Reset(loc, zone, location, seed, position, playerActivity);
            LocationResetPlugin.LocationResetLogger.LogInfo($"Done regenerating location {zone.m_prefabName} at: {loc.transform.position}");
        }

        /// <summary>
        /// Deletes and regenerates the game objects in range of the Location.
        /// </summary>
        /// <param name="loc">LocationProxy needing a reset</param>
        /// <param name="zone">ZoneLocation of the LocationProxy</param>
        /// <param name="seed"></param>
        /// <param name="position"></param>
        /// <param name="activity"></param>
        private void Reset(LocationProxy loc, ZoneSystem.ZoneLocation zone, Location location,
            int seed, LocationPosition position, PlayerActivity activity)
        {
            DeleteLocation(position, activity);

            if (!activity.GroundActivity)
            {
                TerrainReset.ResetTerrain(position.GroundPosition, position.GroundDistance);
            }

            Regenerate(loc, zone, location, seed, position, activity);
        }

        /// <summary>
        /// Rerolls and generates the Location objects.
        /// </summary>
        /// <param name="loc">LocationProxy needing a reset</param>
        /// <param name="zone">ZoneLocation of the LocationProxy</param>/// <param name="location">Location of the ZoneLocation</param>
        /// <param name="seed"></param>
        /// <param name="position"></param>
        private void Regenerate(LocationProxy loc, ZoneSystem.ZoneLocation zone, Location location,
            int seed, LocationPosition position, PlayerActivity activity)
        {
            // Prepare
            ZNetView[] zNetViews = Utils.GetEnabledComponentsInChildren<ZNetView>(zone.m_prefab.Asset);
            RandomSpawn[] randomSpawns = Utils.GetEnabledComponentsInChildren<RandomSpawn>(zone.m_prefab.Asset);
            for (int lcv = 0; lcv < randomSpawns.Length; lcv++)
            {
                randomSpawns[lcv].Prepare();
            }

            Vector3 originalPosition = zone.m_prefab.Asset.transform.position;
            Quaternion originalRotation = zone.m_prefab.Asset.transform.rotation;
            zone.m_prefab.Asset.transform.position = Vector3.zero;
            zone.m_prefab.Asset.transform.rotation = Quaternion.identity;
            
            WearNTear.m_randomInitialDamage = (location != null && location.m_applyRandomDamage) ? true : false;

            // Regenerate original dungeon if exists
            if (position.DungeonGenerator != null)
            {
                position.DungeonGenerator.m_originalPosition = position.GeneratorPosition;

                // Note: this will always cause a randomized radial camp due to vanilla algorithm.
                // Seed is always the same and UnityEngine.Random.InitState is called beforehand
                // so I am unsure why it would generate a different hash.
                position.DungeonGenerator.Generate(ZoneSystem.SpawnMode.Full);
            }

            // Regenerate rest of location
            UnityEngine.Random.InitState(seed);
            foreach (RandomSpawn randomSpawn in randomSpawns)
            {
                randomSpawn.Randomize(position.GroundPosition);
            }

            int count = 0;
            int countIgnored = 0;

            foreach (ZNetView obj in zNetViews)
            {
                if (obj.gameObject.activeSelf)
                {
                    // Do not regenerate items that have not been deleted
                    if (obj.transform.position.y < LOCATION_MINIMUM)
                    {
                        // Ground object
                        if (activity.GroundActivity || !QualifyingObject(obj.gameObject))
                        {
                            countIgnored++;
                            continue;
                        }
                    }
                    else if (activity.SkyActivity || obj.GetComponent<DungeonGenerator>())
                    {
                        countIgnored++;
                        continue;
                    }

                    Vector3 objPosition = loc.transform.position + loc.transform.rotation * obj.gameObject.transform.position;
                    Quaternion objRotation = loc.transform.rotation * obj.gameObject.transform.rotation;

                    GameObject gameObject = UnityEngine.Object.Instantiate(obj.gameObject, objPosition, objRotation);
                    gameObject.GetComponent<ZNetView>().HoldReferenceTo(zone.m_prefab);
                    gameObject.SetActive(value: true);
                    count++;
                }
                else
                {
                    countIgnored++;
                }
            }

            LocationResetPlugin.LocationResetLogger.LogDebug($"Spawned {count} objects, ignored {countIgnored}.");

            // Cleanup
            for (int lcv = 0; lcv < randomSpawns.Length; lcv++)
            {
                randomSpawns[lcv].Reset();
            }

            for (int lcv = 0; lcv < zNetViews.Length; lcv++)
            {
                zNetViews[lcv].gameObject.SetActive(value: true);
            }

            zone.m_prefab.Asset.transform.position = originalPosition;
            zone.m_prefab.Asset.transform.rotation = originalRotation;

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