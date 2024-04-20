using System.Collections.Generic;
using BepInEx;

namespace VentureValheim.Progression
{
    public interface IKeyManager
    {
        string BlockedGlobalKeys { get; }
        string AllowedGlobalKeys { get; }
        string EnforcedGlobalKeys { get; }
        string BlockedPrivateKeys { get; }
        string AllowedPrivateKeys { get; }
        string EnforcedPrivateKeys { get; }
        HashSet<string> BlockedGlobalKeysList { get; }
        HashSet<string> AllowedGlobalKeysList { get; }
        HashSet<string> EnforcedGlobalKeysList { get; }
        HashSet<string> BlockedPrivateKeysList { get; }
        HashSet<string> AllowedPrivateKeysList { get; }
        HashSet<string> EnforcedPrivateKeysList { get; }
        HashSet<string> PrivateKeysList { get; }
        public int GetPublicBossKeysCount();
        public int GetPrivateBossKeysCount();
        public bool BlockGlobalKey(bool blockAll, string globalKey);
        public bool HasPrivateKey(string key);
        public bool HasGlobalKey(string key);
    }

    public partial class KeyManager : IKeyManager
    {
        static KeyManager() { }
        protected KeyManager()
        {
            ResetServer();
            ResetPlayer();
        }

        protected static readonly IKeyManager _instance = new KeyManager();

        public static KeyManager Instance
        {
            get => _instance as KeyManager;
        }

        public HashSet<string> PrivateKeysList { get; protected set; }
        public Dictionary<long, HashSet<string>> ServerPrivateKeysList { get; protected set; }

        public const string RPCNAME_ServerListKeys = "VV_ServerListKeys";
        public const string RPCNAME_ServerSetPrivateKeys = "VV_ServerSetPrivateKeys";
        public const string RPCNAME_ServerSetPrivateKey = "VV_ServerSetPrivateKey";
        public const string RPCNAME_ServerRemovePrivateKey = "VV_ServerRemovePrivateKey";
        public const string RPCNAME_SetPrivateKey = "VV_SetPrivateKey";
        public const string RPCNAME_RemovePrivateKey = "VV_RemovePrivateKey";
        public const string RPCNAME_ResetPrivateKeys = "VV_ResetPrivateKeys";

        public const string PLAYER_SAVE_KEY = "VV_PrivateKeys";

        // List of keys to ignore for other mod support
        HashSet<string> IgnoredModKeys = new()
        {
            "season_winter", // Seasonality
            "season_fall", // Seasonality
            "season_summer", // Seasonality
            "season_spring" // Seasonality
        };

        /// <summary>
        /// Checks if a key is a vanilla world modifier.
        /// </summary>
        /// <param name="key">Must be lowercase</param>
        /// <returns></returns>
        private bool IsWorldModifier(string key)
        {
            ZoneSystem.GetKeyValue(key, out _, out GlobalKeys gk);
            if (gk < GlobalKeys.NonServerOption || gk == GlobalKeys.activeBosses)
            {
                return true;
            }

            return false;
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

            if (IsWorldModifier(globalKey) || IgnoredModKeys.Contains(globalKey))
            {
                ProgressionPlugin.VentureProgressionLogger.LogDebug($"{globalKey} is non-qualifying, can not block.");
                return false;
            }

            if (blockAll)
            {
                return AllowedGlobalKeysList.Count <= 0 || !AllowedGlobalKeysList.Contains(globalKey);
            }

            return (BlockedGlobalKeysList.Count > 0) && BlockedGlobalKeysList.Contains(globalKey);
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
            if (key.IsNullOrWhiteSpace() || IsWorldModifier(key) || IgnoredModKeys.Contains(key))
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
                return true;
            }

            return PrivateKeysList.Contains(key);
        }

        /// <summary>
        /// Returns whether the global key is unlocked or
        /// the Player has unlocked the given key when using private keys.
        /// World Modifier keys should not be passed into this method.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKey(string key)
        {
            return (ProgressionConfiguration.Instance.GetUsePrivateKeys() && HasPrivateKey(key)) || HasGlobalKey(key);
        }

        /// <summary>
        /// Shorthand for checking the global key list.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasGlobalKey(string key)
        {
            if (key.IsNullOrWhiteSpace())
            {
                return true;
            }

            return ProgressionAPI.GetGlobalKey(key);
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

            key = key.ToLower();
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
                SendPrivateKey(playerName, key);
            }
        }

        /// <summary>
        /// Invokes the RPC to add a key to a player.
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="key"></param>
        private void SendPrivateKey(string playerName, string key)
        {
            var id = ProgressionAPI.GetPlayerID(playerName);
            if (id != 0)
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(id, RPCNAME_SetPrivateKey, key);
            }
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
            key = key.ToLower();
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
                SendRemovePrivateKey(playerName, key);
            }
        }

        /// <summary>
        /// Invokes the RPC to remove the given player's private key.
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="key"></param>
        private void SendRemovePrivateKey(string playerName, string key)
        {
            var id = ProgressionAPI.GetPlayerID(playerName);
            if (id != 0)
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(id, RPCNAME_RemovePrivateKey, key);
            }
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
                SendResetPrivateKeys(playerName);
            }
        }

        /// <summary>
        /// Invokes the RPC to reset the given player's private keys.
        /// </summary>
        /// <param name="playerName"></param>
        private void SendResetPrivateKeys(string playerName)
        {
            var id = ProgressionAPI.GetPlayerID(playerName);
            if (id != 0)
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(id, RPCNAME_ResetPrivateKeys);
            }
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
            ZRoutedRpc.instance.InvokeRoutedRPC(RPCNAME_ServerSetPrivateKeys, setString, ProgressionAPI.GetLocalPlayerID());
        }

        /// <summary>
        /// Sets the Server keys for a player for tracking.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="keys">Comma-seperated string of keys</param>
        /// <param name="playerID">Player ID</param>
        private void RPC_ServerSetPrivateKeys(long sender, string keys, long playerID)
        {
            var set = ProgressionAPI.StringToSet(keys);
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"Updating Server Player: " +
                $"{set.Count} keys found for peer {sender}: \"{ProgressionAPI.GetPlayerName(playerID)}\".");
            SetServerKeys(playerID, set);
        }

        /// <summary>
        /// Sends the private key to the server for tracking.
        /// </summary>
        /// <param name="key"></param>
        private void SendPrivateKeyToServer(string key)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(RPCNAME_ServerSetPrivateKey, key, ProgressionAPI.GetLocalPlayerID());
        }

        /// <summary>
        /// Adds a Server key for a player for tracking.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="key"></param>
        /// <param name="playerID">Player ID</param>
        private void RPC_ServerSetPrivateKey(long sender, string key, long playerID)
        {
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"Updating Server Player: " +
                $"Adding key {key} for peer {sender}: \"{ProgressionAPI.GetPlayerName(playerID)}\".");
            SetServerKey(playerID, key);
        }

        /// <summary>
        /// Sends the private key to remove data on the server for tracking.
        /// </summary>
        /// <param name="key"></param>
        private void SendRemovePrivateKeyFromServer(string key)
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(RPCNAME_ServerRemovePrivateKey, key, ProgressionAPI.GetLocalPlayerID());
        }

        /// <summary>
        /// Removes a Server key for a player for tracking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="key"></param>
        /// <param name="playerID">Player ID</param>
        private void RPC_ServerRemovePrivateKey(long sender, string key, long playerID)
        {
            ProgressionPlugin.VentureProgressionLogger.LogDebug($"Updating Server Player: " +
                $"Removing key {key} for peer {sender}: \"{ProgressionAPI.GetPlayerName(playerID)}\".");
            RemoveServerKey(playerID, key);
        }

        /// <summary>
        /// Sends the command to list the current server keys.
        /// </summary>
        private void SendServerListKeys()
        {
            ZRoutedRpc.instance.InvokeRoutedRPC(RPCNAME_ServerListKeys);
        }

        /// <summary>
        /// Prints the current server keys to the log file.
        /// </summary>
        /// <param name="sender"></param>
        private void RPC_ServerListKeys(long sender)
        {
            foreach (var player in ServerPrivateKeysList)
            {
                string keys = "";
                foreach (var key in player.Value)
                {
                    keys += $"{key}, ";
                }

                ProgressionPlugin.VentureProgressionLogger.LogInfo($"Player {ProgressionAPI.GetPlayerName(player.Key)} has " +
                    $"{player.Value.Count} recorded keys: {keys}");
            }
        }

        /// <summary>
        /// Records the private key set for a player in the server dataset.
        /// </summary>
        /// <param name="playerID">Player ID</param>
        /// <param name="keys"></param>
        private void SetServerKeys(long playerID, HashSet<string> keys)
        {
            if (ServerPrivateKeysList.ContainsKey(playerID))
            {
                ServerPrivateKeysList[playerID] = keys;
            }
            else
            {
                ServerPrivateKeysList.Add(playerID, keys);
            }
        }

        /// <summary>
        /// Adds the private key for a player to the server dataset.
        /// </summary>
        /// <param name="playerID">Player ID</param>
        /// <param name="key"></param>
        private void SetServerKey(long playerID, string key)
        {
            if (ServerPrivateKeysList.ContainsKey(playerID))
            {
                ServerPrivateKeysList[playerID].Add(key);
            }
            else
            {
                var set = new HashSet<string>
                {
                    key
                };
                ServerPrivateKeysList.Add(playerID, set);
            }
        }

        /// <summary>
        /// Removes the private key for a player from the server dataset
        /// </summary>
        /// <param name="playerID">Player ID</param>
        /// <param name="key"></param>
        private void RemoveServerKey(long playerID, string key)
        {
            if (ServerPrivateKeysList.ContainsKey(playerID))
            {
                ServerPrivateKeysList[playerID].Remove(key);
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
    }
}
