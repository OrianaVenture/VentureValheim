using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.Progression
{
    public interface IKeyManager
    {
        string BlockedGlobalKeys { get; }
        string AllowedGlobalKeys { get; }
        string BlockedPrivateKeys { get; }
        string AllowedPrivateKeys { get; }
        HashSet<string> BlockedGlobalKeysList { get; }
        HashSet<string> AllowedGlobalKeysList { get; }
        HashSet<string> BlockedPrivateKeysList { get; }
        HashSet<string> AllowedPrivateKeysList { get; }
        HashSet<string> PrivateKeysList { get; }
        public int GetPublicBossKeysCount();
        public int GetPrivateBossKeysCount();
        public bool BlockGlobalKey(bool blockAll, string globalKey);
        public bool HasPrivateKey(string key);
    }

    public class KeyManager : IKeyManager
    {
        static KeyManager() { }
        protected KeyManager()
        {
            ServerPrivateKeysList = new Dictionary<string, HashSet<string>>();

            ResetPlayer();
        }

        protected static readonly IKeyManager _instance = new KeyManager();

        public static KeyManager Instance
        {
            get => _instance as KeyManager;
        }

        public string BlockedGlobalKeys { get; protected set; }
        public string AllowedGlobalKeys { get; protected set; }
        public string BlockedPrivateKeys { get; protected set; }
        public string AllowedPrivateKeys { get; protected set; }
        public HashSet<string> BlockedGlobalKeysList  { get; protected set; }
        public HashSet<string> AllowedGlobalKeysList  { get; protected set; }
        public HashSet<string> BlockedPrivateKeysList { get; protected set; }
        public HashSet<string> AllowedPrivateKeysList { get; protected set; }
        public HashSet<string> PrivateKeysList { get; protected set; }
        public Dictionary<string, HashSet<string>> ServerPrivateKeysList { get; protected set; }

        public readonly string[] BossKeys = new string[TOTAL_BOSSES]
            { "defeated_eikthyr", "defeated_gdking", "defeated_bonemass", "defeated_dragon", "defeated_goblinking", "defeated_queen" };
        public const int TOTAL_BOSSES = 6;

        public const string RPCNAME_ServerSetPrivateKeys = "VV_ServerSetPrivateKeys";
        public const string RPCNAME_ServerSetPrivateKey = "VV_ServerSetPrivateKey";
        public const string RPCNAME_ServerRemovePrivateKey = "VV_ServerRemovePrivateKey";
        public const string RPCNAME_SetPrivateKey = "VV_SetPrivateKey";
        public const string RPCNAME_RemovePrivateKey = "VV_RemovePrivateKey";
        public const string RPCNAME_ResetPrivateKeys = "VV_ResetPrivateKeys";

        public const string PLAYER_SAVE_KEY = "VV_PrivateKeys";

        private static string _filepath = "";

        private static int _cachedPublicBossKeys = -1;
        private static int _cachedPrivateBossKeys = -1;

        private static float _lastUpdateTime = 0f;
        private static readonly float _updateInterval = 10f;

        protected void ResetPlayer()
        {
            BlockedGlobalKeys = "";
            AllowedGlobalKeys = "";
            BlockedGlobalKeysList = new HashSet<string>();
            AllowedGlobalKeysList = new HashSet<string>();

            BlockedPrivateKeys = "";
            AllowedPrivateKeys = "";
            BlockedPrivateKeysList = new HashSet<string>();
            AllowedPrivateKeysList = new HashSet<string>();

            PrivateKeysList = new HashSet<string>();

            _filepath = "";
            _cachedPublicBossKeys = -1;
            _cachedPrivateBossKeys = -1;
        }

        public int GetPublicBossKeysCount()
        {
            Update();
            return _cachedPublicBossKeys;
        }

        public int GetPrivateBossKeysCount()
        {
            Update();
            return _cachedPrivateBossKeys;
        }

        private void UpdateConfigs(float delta)
        {
            UpdateGlobalKeyConfiguration(ProgressionConfiguration.Instance.GetBlockedGlobalKeys(), ProgressionConfiguration.Instance.GetAllowedGlobalKeys());
            UpdatePrivateKeyConfiguration(ProgressionConfiguration.Instance.GetBlockedPrivateKeys(), ProgressionConfiguration.Instance.GetAllowedPrivateKeys());
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"Updating cached Key Information: {delta} time passed.");
        }

        /// <summary>
        /// Updates class data if chached values have expired.
        /// </summary>
        private float Update()
        {
            var time = Time.time;
            var delta = time - _lastUpdateTime;

            if (delta  >= _updateInterval)
            {
                UpdateConfigs(delta);
                _cachedPublicBossKeys = CountPublicBossKeys();
                _cachedPrivateBossKeys = CountPrivateBossKeys();

                _lastUpdateTime = time;
            }

            return delta;
        }

        /// <summary>
        /// Set the values for BlockedGlobalKeysList and AllowedGlobalKeysList if changed.
        /// </summary>
        /// <param name="blockedGlobalKeys"></param>
        /// <param name="allowedGlobalKeys"></param>
        protected void UpdateGlobalKeyConfiguration(string blockedGlobalKeys, string allowedGlobalKeys)
        {
            if (!BlockedGlobalKeys.Equals(blockedGlobalKeys))
            {
                BlockedGlobalKeys = blockedGlobalKeys;
                BlockedGlobalKeysList = ProgressionAPI.Instance.StringToSet(blockedGlobalKeys);
            }

            if (!AllowedGlobalKeys.Equals(allowedGlobalKeys))
            {
                AllowedGlobalKeys = allowedGlobalKeys;
                AllowedGlobalKeysList = ProgressionAPI.Instance.StringToSet(allowedGlobalKeys);
            }
        }

        /// <summary>
        /// Set the values for BlockedPrivateKeysList and AllowedPrivateKeysList if changed.
        /// </summary>
        /// <param name="blockedPrivateKeys"></param>
        /// <param name="allowedPrivateKeys"></param>
        protected void UpdatePrivateKeyConfiguration(string blockedPrivateKeys, string allowedPrivateKeys)
        {
            if (!BlockedPrivateKeys.Equals(blockedPrivateKeys))
            {
                BlockedPrivateKeys = blockedPrivateKeys;
                BlockedPrivateKeysList = ProgressionAPI.Instance.StringToSet(blockedPrivateKeys);
            }

            if (!AllowedPrivateKeys.Equals(allowedPrivateKeys))
            {
                AllowedPrivateKeys = allowedPrivateKeys;
                AllowedPrivateKeysList = ProgressionAPI.Instance.StringToSet(allowedPrivateKeys);
            }
        }

        private string GetOldFilePath(string original)
        {
            return original + ".oldkeys";
        }

        private string GetFilePath(string original)
        {
            return original + ".keys";
        }

        /// <summary>
        /// Whether to block a Global Key based on configuration settings.
        /// </summary>
        /// <param name="blockAll"></param>
        /// <param name="globalKey"></param>
        /// <returns>True when default blocked and does not exist in the allowed list,
        /// or when default unblocked and key is in the blocked list.</returns>
        public bool BlockGlobalKey(bool blockAll, string globalKey)
        {
            if (globalKey.IsNullOrWhiteSpace())
            {
                return true;
            }

            if (blockAll)
            {
                return (AllowedGlobalKeysList.Count > 0) ? !AllowedGlobalKeysList.Contains(globalKey) : true;
            }

            return (BlockedGlobalKeysList.Count > 0) ? BlockedGlobalKeysList.Contains(globalKey) : false;
        }

        /// <summary>
        /// Whether to block a private Key based on configuration settings.
        /// If instance is a dedicated server returns true.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool BlockPrivateKey(string key)
        {
            if (ProgressionConfiguration.Instance.GetUsePrivateKeys() && !ZNet.instance.IsDedicated())
            {
                return PrivateKeyIsBlocked(key);
            }

            return true;
        }

        /// <summary>
        /// Whether to block a private Key based on list configurations.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>True if blocked and in the blocked list,
        /// false when allowed and in the allowed list or both lists are empty.</returns>
        protected bool PrivateKeyIsBlocked(string key)
        {
            if (key.IsNullOrWhiteSpace())
            {
                return true;
            }

            if (BlockedPrivateKeysList.Count > 0)
            {
                return BlockedPrivateKeysList.Contains(key);
            }
            else if (AllowedPrivateKeysList.Count > 0)
            {
                return !AllowedPrivateKeysList.Contains(key);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns whether the Player has unlocked the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasPrivateKey(string key)
        {
            if (key.IsNullOrWhiteSpace())
            {
                return false;
            }

            return PrivateKeysList.Contains(key);
        }

        /// <summary>
        /// Returns whether the Player has unlocked the given key, or false if no keys are recorded.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name">Player name</param>
        /// <returns></returns>
        public bool ServerHasPrivateKey(string key, string name)
        {
            if (ServerPrivateKeysList.ContainsKey(name))
            {
                var set = ServerPrivateKeysList[name];
                if (set != null)
                {
                    return set.Contains(key);
                }
            }

            return false;
        }

        /// <summary>
        /// Returns whether the indicated player has the required keys for a random event.
        /// Random events are decided by the server.
        /// </summary>
        /// <param name="name">Player name</param>
        /// <param name="ev"></param>
        /// <returns></returns>
        public bool PlayerHasPrivateEventKey(string name, RandomEvent ev)
        {
            for (int lcv = 0; lcv < ev.m_requiredGlobalKeys.Count; lcv++)
            {
                if (!Instance.ServerHasPrivateKey(ev.m_requiredGlobalKeys[lcv], name))
                {
                    return false;
                }
            }

            for (int lcv = 0; lcv < ev.m_notRequiredGlobalKeys.Count; lcv++)
            {
                if (Instance.ServerHasPrivateKey(ev.m_notRequiredGlobalKeys[lcv], name))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Counts all the boss keys in the Player's private list.
        /// </summary>
        /// <returns></returns>
        protected int CountPrivateBossKeys()
        {
            int count = 0;

            for (int lcv = 0; lcv < BossKeys.Length; lcv++)
            {
                if (PrivateKeysList.Contains(BossKeys[lcv]))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Counts all the boss keys in the public list.
        /// </summary>
        /// <returns></returns>
        private int CountPublicBossKeys()
        {
            int count = 0;

            for (int lcv = 0; lcv < BossKeys.Length; lcv++)
            {
                if (HasGlobalKey(BossKeys[lcv]))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Shorthand for checking the global key list.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual bool HasGlobalKey(string key)
        {
            return ProgressionAPI.Instance.GetGlobalKey(key);
        }

        #region Add Private Key

        /// <summary>
        /// Adds the given key to the Player's private list.
        /// If added sends a response back to the server for tracking.
        /// </summary>
        /// <param name="key"></param>
        private void AddPrivateKey(string key)
        {
            if (key.IsNullOrWhiteSpace())
            {
                return;
            }

            bool added = PrivateKeysList.Add(key);
            if (added)
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Adding Private Key {key}.");
                SendPrivateKeyToServer(key);
            }
        }

        /// <summary>
        /// If the playerName is empty adds the key to the current player,
        /// else sends the key to the target player.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="playerName"></param>
        private void AddPrivateKey(string key, string playerName)
        {
            if (playerName.IsNullOrWhiteSpace())
            {
                AddPrivateKey(key);
            }
            else
            {
                var player = ProgressionAPI.Instance.GetPlayerByName(playerName);

                if (player != null)
                {
                    SendPrivateKey(player, key);
                }
            }
        }

        /// <summary>
        /// Invokes the RPC to add a key to a player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        private void SendPrivateKey(Player player, string key)
        {
            player.m_nview.InvokeRPC(RPCNAME_SetPrivateKey, key);
        }

        /// <summary>
        /// RPC for adding a private key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="key"></param>
        private void RPC_SetPrivateKey(long sender, string key)
        {
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"Got private key {key} from {sender}: adding to list.");
            AddPrivateKey(key);
        }

        #endregion

        #region Remove Private Key

        /// <summary>
        /// Removes the given key from the Player's private list.
        /// If removed sends a response back to the server for tracking.
        /// </summary>
        /// <param name="key"></param>
        private void RemovePrivateKey(string key)
        {
            bool removed = PrivateKeysList.Remove(key);
            if (removed)
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Removing Private Key {key}.");
                SendRemovePrivateKeyFromServer(key);
            }
        }

        /// <summary>
        /// Remove private key method for commands.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="playerName"></param>
        private void RemovePrivateKey(string key, string playerName)
        {
            if (playerName.IsNullOrWhiteSpace())
            {
                RemovePrivateKey(key);
            }
            else
            {
                var player = ProgressionAPI.Instance.GetPlayerByName(playerName);

                if (player != null)
                {
                    SendRemovePrivateKey(player, key);
                }
            }
        }

        /// <summary>
        /// Invokes the RPC to remove the given player's private key.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        private void SendRemovePrivateKey(Player player, string key)
        {
            player.m_nview.InvokeRPC(RPCNAME_RemovePrivateKey, key);
        }

        /// <summary>
        /// RPC to remove a private key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="key"></param>
        private void RPC_RemovePrivateKey(long sender, string key)
        {
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"Got private key {key} from {sender}: removing from list.");
            RemovePrivateKey(key);
        }

        #endregion

        #region Reset Private Keys

        /// <summary>
        /// Resets the keys for the Player's private list.
        /// Sends a response back to the server for tracking.
        /// </summary>
        private void ResetPrivateKeys()
        {
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"Resetting Private Keys.");

            PrivateKeysList = new HashSet<string>();
            SendPrivateKeysToServer(PrivateKeysList);
        }

        /// <summary>
        /// Reset private keys method for commands.
        /// </summary>
        /// <param name="playerName"></param>
        private void ResetPrivateKeys(string playerName)
        {
            if (playerName.IsNullOrWhiteSpace())
            {
                ResetPrivateKeys();
            }
            else
            {
                var player = ProgressionAPI.Instance.GetPlayerByName(playerName);
                if (player != null)
                {
                    SendResetPrivateKeys(player);
                }
            }
        }

        /// <summary>
        /// Invokes the RPC to reset the given player's private keys
        /// </summary>
        /// <param name="player"></param>
        private void SendResetPrivateKeys(Player player)
        {
            player.m_nview.InvokeRPC(RPCNAME_ResetPrivateKeys);
        }

        /// <summary>
        /// RPC for reseting private keys.
        /// </summary>
        /// <param name="sender"></param>
        private void RPC_ResetPrivateKeys(long sender)
        {
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"Got reset keys command from {sender}.");
            ResetPrivateKeys();
        }

        #endregion

        #region Server Keys

        /// <summary>
        /// Sends the private key data to the server for tracking.
        /// </summary>
        /// <param name="keys"></param>
        private void SendPrivateKeysToServer(HashSet<string> keys)
        {
            string setString = string.Join<string>(",", keys);
            ZRoutedRpc.instance.InvokeRoutedRPC(RPCNAME_ServerSetPrivateKeys, setString, ProgressionAPI.Instance.GetLocalPlayerName());
        }

        /// <summary>
        /// Sets the Server keys for a player for tracking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keys">Comma-seperated string of keys</param>
        /// <param name="name">Player name</param>
        private void RPC_ServerSetPrivateKeys(long sender, string keys, string name)
        {
            if (!name.IsNullOrWhiteSpace())
            {
                var set = ProgressionAPI.Instance.StringToSet(keys);
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Updating Server Player: {set.Count} keys found for peer {sender}: \"{name}\".");
                SetServerKeys(name, set);
            }
        }

        /// <summary>
        /// Sends the private key to the server for tracking.
        /// </summary>
        /// <param name="key"></param>
        private void SendPrivateKeyToServer(string key)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(RPCNAME_ServerSetPrivateKey, key, ProgressionAPI.Instance.GetLocalPlayerName());
        }

        /// <summary>
        /// Adds a Server key for a player for tracking.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="key"></param>
        private void RPC_ServerSetPrivateKey(long sender, string key, string name)
        {
            if (!name.IsNullOrWhiteSpace())
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Updating Server Player: Adding key {key} for peer {sender}: \"{name}\".");
                SetServerKey(name, key);
            }
        }

        /// <summary>
        /// Sends the private key to remove data on the server for tracking.
        /// </summary>
        /// <param name="key"></param>
        private void SendRemovePrivateKeyFromServer(string key)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(RPCNAME_ServerRemovePrivateKey, key, ProgressionAPI.Instance.GetLocalPlayerName());
        }

        /// <summary>
        /// Removes a Server key for a player for tracking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="key"></param>
        private void RPC_ServerRemovePrivateKey(long sender, string key, string name)
        {
            if (!name.IsNullOrWhiteSpace())
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Updating Server Player: Removing key {key} for peer {sender}: \"{name}\".");
                RemoveServerKey(name, key);
            }
        }

        /// <summary>
        /// Records the private key set for a player in the server dataset.
        /// </summary>
        /// <param name="name">Player name</param>
        /// <param name="keys"></param>
        private void SetServerKeys(string name, HashSet<string> keys)
        {
            if (ServerPrivateKeysList.ContainsKey(name))
            {
                ServerPrivateKeysList[name] = keys;
            }
            else
            {
                ServerPrivateKeysList.Add(name, keys);
            }
        }

        /// <summary>
        /// Adds the private key for a player to the server dataset.
        /// </summary>
        /// <param name="name">Player name</param>
        /// <param name="key"></param>
        private void SetServerKey(string name, string key)
        {
            if (ServerPrivateKeysList.ContainsKey(name))
            {
                ServerPrivateKeysList[name].Add(key);
            }
            else
            {
                var set = new HashSet<string>
                {
                    key
                };
                ServerPrivateKeysList.Add(name, set);
            }
        }

        /// <summary>
        /// Removes the private key for a player from the server dataset
        /// </summary>
        /// <param name="name">Player name</param>
        /// <param name="key"></param>
        private void RemoveServerKey(string name, string key)
        {
            if (ServerPrivateKeysList.ContainsKey(name))
            {
                ServerPrivateKeysList[name].Remove(key);
            }
        }

        #endregion

        /// <summary>
        /// Get private keys as a comma-separated string.
        /// </summary>
        /// <returns></returns>
        private string GetPrivateKeysString()
        {
            return string.Join<string>(",", PrivateKeysList);
        }

        /// <summary>
        /// Loads the saved file given the file path has been initialized.
        /// </summary>
        /// <param name="filesource"></param>
        /// <returns></returns>
        private HashSet<string> LoadFile(FileHelpers.FileSource filesource)
        {
            // TODO depreciate this in the future
            FileReader fileReader = null;
            HashSet<string> keys = new HashSet<string>();
            try
            {
                fileReader = new FileReader(GetFilePath(_filepath), filesource);

                byte[] data;

                BinaryReader binary = fileReader.m_binary;
                int count = binary.ReadInt32();
                data = binary.ReadBytes(count);
                int count2 = binary.ReadInt32();
                binary.ReadBytes(count2);

                var package = new ZPackage(data);

                int length = package.ReadInt();
                for (int lcv = 0; lcv < length; lcv++)
                {
                    string key = package.ReadString();
                    keys.Add(key);
                }

                fileReader.Dispose();

                // Upgrade to new mod version by deleting files if they exist
                ProgressionPlugin.VentureProgressionLogger.LogInfo("Performing mod version upgrade. Deleting key files, they are no longer used!");
                File.Delete(GetFilePath(_filepath));
                File.Delete(GetOldFilePath(_filepath));
            }
            catch
            {
                fileReader?.Dispose();
            }

            return keys;
        }

        /// <summary>
        /// Set the file path if not already defined.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Returns true if the file path is defined.</returns>
        protected bool SetFilePaths(string path)
        {
            if (_filepath.IsNullOrWhiteSpace())
            {
                if(!path.IsNullOrWhiteSpace())
                {
                    _filepath = path;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        #region Patches

        /// <summary>
        /// Skips the original ZoneSystem.SetGlobalKey method if a key is blocked.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetGlobalKey))]
        public static class Patch_ZoneSystem_SetGlobalKey
        {
            [HarmonyPriority(Priority.Last)]
            private static bool Prefix(string name)
            {
                Instance.Update();
                if (Instance.BlockGlobalKey(ProgressionConfiguration.Instance.GetBlockAllGlobalKeys(), name))
                {
                    ProgressionPlugin.VentureProgressionLogger.LogDebug($"Skipping adding global key: {name}.");
                    return false; // Skip adding the global key
                }

                ProgressionPlugin.VentureProgressionLogger.LogDebug($"Adding global key: {name}.");
                return true; // Continue adding the global key
            }

            private static void Postfix(string name)
            {
                if (Player.m_localPlayer != null && !Instance.BlockPrivateKey(name))
                {
                    List<Player> nearbyPlayers = new List<Player>();
                    Player.GetPlayersInRange(Player.m_localPlayer.transform.position, 100, nearbyPlayers);

                    if (nearbyPlayers != null && nearbyPlayers.Count == 0)
                    {
                        ProgressionPlugin.VentureProgressionLogger.LogDebug($"No players in range to send key!");
                    }
                    else
                    {
                        for (int lcv = 0; lcv < nearbyPlayers.Count; lcv++)
                        {
                            ProgressionPlugin.VentureProgressionLogger.LogDebug(
                                    $"Attempting to send private key: {name} to \"{nearbyPlayers[lcv].GetPlayerName()}\".");
                            Instance.SendPrivateKey(nearbyPlayers[lcv], name);
                        }
                    }
                }
                else
                {
                    ProgressionPlugin.VentureProgressionLogger.LogDebug($"Skipping adding private key: {name}.");
                }
            }
        }

        /// <summary>
        /// If using private keys, returns true if the key is in the global list when
        /// the instance is a dedicated server, or true if the local player has the private key.
        /// If not using private keys uses default behavior.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetGlobalKey))]
        public static class Patch_ZoneSystem_GetGlobalKey
        {
            private static void Postfix(string name, ref bool __result)
            {
                if (ProgressionConfiguration.Instance.GetUsePrivateKeys() && !ZNet.instance.IsDedicated())
                {
                    __result = Instance.HasPrivateKey(name);
                }
            }
        }

        /// <summary>
        /// Registers RPCs for each Player when created.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        public static class Patch_Player_Awake
        {
            private static void Postfix(Player __instance)
            {
                if (ProgressionAPI.Instance.IsInTheMainScene() && __instance.m_nview != null)
                {
                    ProgressionPlugin.VentureProgressionLogger.LogDebug($"Registering RPCs for key managment.");

                    __instance.m_nview.Register(RPCNAME_SetPrivateKey, new Action<long, string>(Instance.RPC_SetPrivateKey));
                    __instance.m_nview.Register(RPCNAME_RemovePrivateKey, new Action<long, string>(Instance.RPC_RemovePrivateKey));
                    __instance.m_nview.Register(RPCNAME_ResetPrivateKeys, new Action<long>(Instance.RPC_ResetPrivateKeys));
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Save))]
        public static class Patch_Player_Save
        {
            private static void Prefix(Player __instance)
            {
                if (__instance.m_customData.ContainsKey(PLAYER_SAVE_KEY))
                {
                    __instance.m_customData[PLAYER_SAVE_KEY] = Instance.GetPrivateKeysString();
                }
                else
                {
                    __instance.m_customData.Add(PLAYER_SAVE_KEY, Instance.GetPrivateKeysString());
                }
            }
        }

        /// <summary>
        /// Load private keys from the player file if the data exists,
        /// fallback try to load a legacy save file. Cleans up private keys
        /// based off configurations then syncs the data with the server.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.Load))]
        public static class Patch_Player_Load
        {
            private static void Postfix(Player __instance)
            {
                if (!ProgressionAPI.Instance.IsInTheMainScene())
                {
                    return;
                }

                ProgressionPlugin.VentureProgressionLogger.LogInfo("Starting Player Key Management. Cleaning up private keys!");

                Instance.ResetPlayer();
                Instance.UpdateConfigs(0f);

                HashSet<string> loadedKeys = new HashSet<string>();

                if (__instance.m_customData.ContainsKey(PLAYER_SAVE_KEY))
                {
                    loadedKeys = ProgressionAPI.Instance.StringToSet(__instance.m_customData[PLAYER_SAVE_KEY]);
                }
                else
                {
                    // Upgrade mod version by reading file when not yet in custom data
                    var profile = Game.instance.GetPlayerProfile();
                    if (Instance.SetFilePaths(profile.GetPath()))
                    {
                        loadedKeys = Instance.LoadFile(profile.m_fileSource);
                    }
                }

                foreach (var key in loadedKeys)
                {
                    if (!Instance.BlockPrivateKey(key))
                    {
                        Instance.PrivateKeysList.Add(key);
                    }
                }

                // Sync data on connect
                Instance.SendPrivateKeysToServer(Instance.PrivateKeysList);
            }
        }

        /// <summary>
        /// Register RPCs and perform a server key cleanup when starting up.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
        public static class Patch_ZoneSystem_Start
        {
            private static void Postfix(ZoneSystem __instance)
            {
                if (ZNet.instance.IsServer())
                {
                    ProgressionPlugin.VentureProgressionLogger.LogInfo("Starting Server Key Management. Cleaning up public keys!");

                    var keys = __instance.GetGlobalKeys();
                    var blockAll = ProgressionConfiguration.Instance.GetBlockAllGlobalKeys();

                    for (int lcv = 0; lcv < keys.Count; lcv++)
                    {
                        if (Instance.BlockGlobalKey(blockAll, keys[lcv]))
                        {
                            __instance.m_globalKeys.Remove(keys[lcv]);
                        }
                    }

                    ZRoutedRpc.instance.Register(RPCNAME_ServerSetPrivateKeys, new Action<long, string, string>(Instance.RPC_ServerSetPrivateKeys));
                    ZRoutedRpc.instance.Register(RPCNAME_ServerSetPrivateKey, new Action<long, string, string>(Instance.RPC_ServerSetPrivateKey));
                    ZRoutedRpc.instance.Register(RPCNAME_ServerRemovePrivateKey, new Action<long, string, string>(Instance.RPC_ServerRemovePrivateKey));
                }
            }
        }

        /// <summary>
        /// Tack on the check private keys logic to the CheckBase method.
        /// </summary>
        [HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.CheckBase))]
        public static class Patch_RandEventSystem_CheckBase
        {
            private static void Postfix(ref bool __result, RandomEvent ev, ZDO zdo)
            {
                if (__result && ProgressionConfiguration.Instance.GetUsePrivateKeys())
                {
                    var player = zdo.GetString("playerName", "");
                    if (!player.IsNullOrWhiteSpace())
                    {
                        __result = Instance.PlayerHasPrivateEventKey(player, ev);
                    }
                }
            }
        }

        /// <summary>
        /// Trick the random event system to enter the CheckBase patch if using private keys.
        /// </summary>
        [HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.HaveGlobalKeys))]
        public static class Patch_RandEventSystem_HaveGlobalKeys
        {
            [HarmonyPriority(Priority.Last)]
            private static bool Prefix()
            {
                if (ProgressionConfiguration.Instance.GetUsePrivateKeys())
                {
                    return false; // Skip main method to save some calculations
                }

                return true;
            }

            // TODO set priority if there are mod conflicts
            private static void Postfix(ref bool __result)
            {
                if (ProgressionConfiguration.Instance.GetUsePrivateKeys())
                {
                    __result = true;
                }
            }
        }

        /// <summary>
        /// Enables all of Haldor's items by bypassing key checking.
        /// </summary>
        [HarmonyPatch(typeof(Trader), nameof(Trader.GetAvailableItems))]
        public static class Patch_Trader_GetAvailableItems
        {
            [HarmonyPriority(Priority.Last)]
            private static bool Prefix()
            {
                if (ProgressionConfiguration.Instance.GetUnlockAllHaldorItems())
                {
                    return false; // Skip main method to save some calculations
                }

                return true;
            }

            [HarmonyPriority(Priority.First)]
            private static void Postfix(Trader __instance, ref List<Trader.TradeItem> __result)
            {
                if (ProgressionConfiguration.Instance.GetUnlockAllHaldorItems())
                {
                    __result = new List<Trader.TradeItem>(__instance.m_items);
                }
            }
        }

        /// <summary>
        /// Adds commands for managing player keys.
        /// </summary>
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
        private static class Patch_Terminal_InitTerminal
        {
            [HarmonyPriority(Priority.First)]
            private static void Prefix(out bool __state)
            {
                __state = Terminal.m_terminalInitialized;
            }

            private static void Postfix(bool __state)
            {
                if (__state)
                {
                    return;
                }

                ProgressionPlugin.VentureProgressionLogger.LogInfo("Adding Terminal Commands for key management.");

                new Terminal.ConsoleCommand("setglobalkey", "[name]", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length >= 2)
                    {
                        ProgressionAPI.Instance.AddGlobalKey(args[1]);
                        args.Context.AddString($"Setting global key {args[1]}.");
                    }
                    else
                    {
                        args.Context.AddString("Syntax: setglobalkey [key]");
                    }
                }, isCheat: true, isNetwork: false, onlyServer: true);
                new Terminal.ConsoleCommand("setprivatekey", "[name] [optional: player name]", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length >= 3)
                    {
                        Instance.AddPrivateKey(args[1], args[2]);
                        args.Context.AddString($"Setting private key {args[1]} for player {args[2]}.");
                    }
                    else if (args.Length == 2)
                    {
                        Instance.AddPrivateKey(args[1]);
                        args.Context.AddString($"Setting private key {args[1]}.");
                    }
                    else
                    {
                        args.Context.AddString("Syntax: setprivatekey [key]");
                    }
                }, isCheat: true, isNetwork: false, onlyServer: true);
                new Terminal.ConsoleCommand("removeprivatekey", "[name] [optional: player name]", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length >= 3)
                    {
                        Instance.RemovePrivateKey(args[1], args[2]);
                        args.Context.AddString($"Removing private key {args[1]} for player {args[2]}.");
                    }
                    else if (args.Length == 2)
                    {
                        Instance.RemovePrivateKey(args[1]);
                        args.Context.AddString($"Removing private key {args[1]}.");
                    }
                    else
                    {
                        args.Context.AddString("Syntax: removeprivatekey [key] [optional: player name]");
                    }
                }, isCheat: true, isNetwork: false, onlyServer: true);
                new Terminal.ConsoleCommand("resetprivatekeys", "[optional: player name]", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length >= 2)
                    {
                        Instance.ResetPrivateKeys(args[1]);
                        args.Context.AddString($"Private keys cleared for player {args[1]}.");
                    }
                    else if (args.Length == 1)
                    {
                        Instance.ResetPrivateKeys();
                        args.Context.AddString("Private keys cleared");
                    }
                    else
                    {
                        args.Context.AddString("Syntax: resetprivatekeys [optional: player name]");
                    }
                }, isCheat: true, isNetwork: false, onlyServer: true);
                new Terminal.ConsoleCommand("listprivatekeys", "", delegate (Terminal.ConsoleEventArgs args)
                {
                    args.Context.AddString($"Total Keys {Instance.PrivateKeysList.Count}");
                    foreach (string key in Instance.PrivateKeysList)
                    {
                        args.Context.AddString(key);
                    }
                }, isCheat: true, isNetwork: false, onlyServer: false);
                new Terminal.ConsoleCommand("listserverkeys", "", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (ZNet.instance.IsServer())
                    {
                        args.Context.AddString($"Total Players Recorded This Session: {Instance.ServerPrivateKeysList.Count}");

                        foreach (var set in Instance.ServerPrivateKeysList)
                        {
                            var numKeys = set.Value?.Count ?? 0;

                            args.Context.AddString($"Player {set.Key} has {numKeys} keys");

                            if (set.Value != null)
                            {
                                foreach (string key in set.Value)
                                {
                                    args.Context.AddString(key);
                                }
                            }
                        }
                    }
                    else
                    {
                        args.Context.AddString($"You are not the server, no data available.");
                    }
                }, isCheat: true, isNetwork: false, onlyServer: true);
            }
        }

        #endregion
    }
}