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
        HashSet<string> BlockedGlobalKeysList { get; }
        HashSet<string> AllowedGlobalKeysList { get; }
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
            ResetPlayer();
        }

        protected static readonly IKeyManager _instance = new KeyManager();

        public static KeyManager Instance
        {
            get => _instance as KeyManager;
        }

        public string BlockedGlobalKeys { get; protected set; }
        public string AllowedGlobalKeys { get; protected set; }
        public HashSet<string> BlockedGlobalKeysList  { get; protected set; }
        public HashSet<string> AllowedGlobalKeysList  { get; protected set; }
        public HashSet<string> PrivateKeysList { get; protected set; }

        public readonly string[] BossKeys = new string[TOTAL_BOSSES] { "defeated_eikthyr", "defeated_gdking", "defeated_bonemass", "defeated_dragon", "defeated_goblinking" };
        public const int TOTAL_BOSSES = 5;

        private static string _filepath = "";
        private static bool _fileLoaded = false;

        private static int _cachedPublicBossKeys = -1;
        private static int _cachedPrivateBossKeys = -1;

        private static float _lastUpdateTime = 0f;
        private static readonly float _updateInterval = 5f;

        protected void ResetPlayer()
        {
            try
            {
                BlockedGlobalKeys = "";
                AllowedGlobalKeys = "";
                BlockedGlobalKeysList = new HashSet<string>();
                AllowedGlobalKeysList = new HashSet<string>();
                PrivateKeysList = new HashSet<string>();

                _filepath = "";
                _fileLoaded = false;
                _cachedPublicBossKeys = -1;
                _cachedPrivateBossKeys = -1;
            }
            catch (Exception e)
            {
                ProgressionPlugin.GetProgressionLogger().LogDebug("Exception in ResetPlayer...");
                ProgressionPlugin.GetProgressionLogger().LogDebug(e);
            }
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
            UpdateGlobalKeyConfiguration(ProgressionPlugin.Instance.GetBlockedGlobalKeys(), ProgressionPlugin.Instance.GetAllowedGlobalKeys());
            ProgressionPlugin.GetProgressionLogger().LogDebug($"Updating chached Key Information: {delta} time passed.");
        }

        /// <summary>
        /// Updates class data if chached values have expired.
        /// </summary>
        private float Update()
        {
            var time = Time.time;
            var delta = time - _lastUpdateTime;

            if (delta  > _updateInterval)
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

        private string GetNewFilePath(string original)
        {
            return original + ".newkeys";
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
        /// Returns weither the Player has unlocked the given key, or false if no keys were loaded.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasPrivateKey(string key)
        {
            return PrivateKeysList.Contains(key);
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

        protected virtual bool HasGlobalKey(string key)
        {
            return ProgressionAPI.Instance.GetGlobalKey(key);
        }

        /// <summary>
        /// Attempts to add the given key to the Player's private list.
        /// </summary>
        /// <param name="key"></param>
        public void AddPrivateKey(string key)
        {
            PrivateKeysList.Add(key);
        }

        /// <summary>
        /// Attempts to remove the given key from the Player's private list.
        /// </summary>
        /// <param name="key"></param>
        public void RemovePrivateKey(string key)
        {
            PrivateKeysList.Remove(key);
        }

        /// <summary>
        /// Reset the private key list for the Player.
        /// </summary>
        public void ResetPrivateKeys()
        {
            PrivateKeysList = new HashSet<string>();
        }

        /// <summary>
        /// Saves private player keys to a file.
        /// </summary>
        /// <param name="filesource"></param>
        /// <param name="keys"></param>
        private void SaveFile(FileHelpers.FileSource filesource, HashSet<string> keys)
        {
            if (Directory.Exists(_filepath) && !_fileLoaded)
            {
                // Do not override an existing file that has not been loaded yet.
                ProgressionPlugin.GetProgressionLogger().LogDebug("Skipping Saving Player Keys.");
                return;
            }

            // Create ZPackage
            ZPackage zPackage = new ZPackage();
            int length = PrivateKeysList.Count;
            zPackage.Write(length);

            foreach (string key in keys)
            {
                zPackage.Write(key);
            }

            // Save ZPackage
            FileWriter fileWriter = new FileWriter(GetNewFilePath(_filepath), FileHelpers.FileHelperType.Binary, filesource);
            byte[] zPackageHash = zPackage.GenerateHash();
            byte[] zPackageArray = zPackage.GetArray();
            fileWriter.m_binary.Write(zPackageArray.Length);
            fileWriter.m_binary.Write(zPackageArray);
            fileWriter.m_binary.Write(zPackageHash.Length);
            fileWriter.m_binary.Write(zPackageHash);
            fileWriter.Finish();
            FileHelpers.ReplaceOldFile(GetFilePath(_filepath), GetNewFilePath(_filepath), GetOldFilePath(_filepath), filesource);
        }

        /// <summary>
        /// Loads the saved file given the file path has been initialized.
        /// </summary>
        /// <param name="filesource"></param>
        /// <returns></returns>
        private HashSet<string> LoadFile(FileHelpers.FileSource filesource)
        {
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

                // Set the loaded flag for use when saving, this will help prevent data loss on error
                _fileLoaded = true;

                fileReader.Dispose();
            }
            catch (Exception e)
            {
                ProgressionPlugin.GetProgressionLogger().LogWarning($"Failed to load Source: {filesource}, Path: {GetFilePath(_filepath)}");
                ProgressionPlugin.GetProgressionLogger().LogWarning(e);
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
                if (Instance.BlockGlobalKey(ProgressionPlugin.Instance.GetBlockAllGlobalKeys(), name))
                {
                    ProgressionPlugin.GetProgressionLogger().LogDebug($"Skipping adding global key: {name}.");
                    return false; // Skip adding the global key
                }

                ProgressionPlugin.GetProgressionLogger().LogDebug($"Adding global key: {name}.");
                return true; // Continue adding the global key
            }

            private static void Postfix(string name)
            {
                ProgressionPlugin.GetProgressionLogger().LogDebug($"Adding private key: {name}.");
                Instance.AddPrivateKey(name);
            }
        }

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.SavePlayerData))]
        public static class Patch_PlayerProfile_SavePlayerData
        {
            private static void Postfix(PlayerProfile __instance)
            {
                try
                {
                    ProgressionPlugin.GetProgressionLogger().LogDebug("Patch_PlayerProfile_SavePlayerData postfix called. Saving Keys.");
                    if (Instance.SetFilePaths(__instance.GetPath()))
                    {
                        Instance.SaveFile(__instance.m_fileSource, Instance.PrivateKeysList);
                    }
                }
                catch (Exception e)
                {
                    ProgressionPlugin.GetProgressionLogger().LogError("Error saving key data from file.");
                    ProgressionPlugin.GetProgressionLogger().LogError(e);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.LoadPlayerData))]
        public static class Patch_PlayerProfile_LoadPlayerData
        {
            private static void Postfix(PlayerProfile __instance)
            {
                if (!ProgressionAPI.Instance.IsInTheMainScene() || _fileLoaded)
                {
                    return;
                }

                try
                {
                    ProgressionPlugin.GetProgressionLogger().LogDebug("Patch_PlayerProfile_LoadPlayerData postfix called. Loading keys.");
                    // TODO test
                    //var profile = Game.instance.GetPlayerProfile();
                    var profile = __instance;
                    if (Instance.SetFilePaths(profile.GetPath()))
                    {
                        Instance.PrivateKeysList = Instance.LoadFile(profile.m_fileSource);
                    }
                    else
                    {
                        ProgressionPlugin.GetProgressionLogger().LogWarning($"File paths could not be set for FileSource: {profile.m_fileSource}, keys data may not load or save correctly.");
                    }
                }
                catch (Exception e)
                {
                    ProgressionPlugin.GetProgressionLogger().LogError("Error loading key data from file.");
                    ProgressionPlugin.GetProgressionLogger().LogError(e);
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        public static class Patch_Player_Awake
        {
            private static void Postfix()
            {
                ProgressionPlugin.GetProgressionLogger().LogDebug("Resetting Player Key Manager.");
                Instance.ResetPlayer();
            }
        }

        /// <summary>
        /// Adds commands for managing private plyer keys
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

                ProgressionPlugin.GetProgressionLogger().LogInfo("Adding Terminal Commands for private key management.");

                new Terminal.ConsoleCommand("setprivatekey", "[name]", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length >= 2)
                    {
                        Instance.AddPrivateKey(args[1]);
                        args.Context.AddString("Setting private key " + args[1]);
                    }
                    else
                    {
                        args.Context.AddString("Syntax: setprivatekey [key]");
                    }
                }, isCheat: true, isNetwork: false, onlyServer: true);
                new Terminal.ConsoleCommand("removeprivatekey", "[name]", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length >= 2)
                    {
                        Instance.RemovePrivateKey(args[1]);
                        args.Context.AddString("Removing private key " + args[1]);
                    }
                    else
                    {
                        args.Context.AddString("Syntax: removeprivatekey [key]");
                    }
                }, isCheat: true, isNetwork: false, onlyServer: true);
                new Terminal.ConsoleCommand("resetprivatekeys", "[name]", delegate (Terminal.ConsoleEventArgs args)
                {
                    Instance.ResetPrivateKeys();
                    args.Context.AddString("Private keys cleared");
                }, isCheat: true, isNetwork: false, onlyServer: true);
                new Terminal.ConsoleCommand("listprivatekeys", "", delegate (Terminal.ConsoleEventArgs args)
                {
                    args.Context.AddString("Keys " + Instance.PrivateKeysList.Count);
                    foreach (string key in Instance.PrivateKeysList)
                    {
                        args.Context.AddString(key);
                    }
                }, isCheat: true, isNetwork: false, onlyServer: true);
            }
        }

        #endregion
    }
}