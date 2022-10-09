using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.Progression
{
    public class KeyManager
    {
        public string BlockedGlobalKeys { get; private set; }
        public string AllowedGlobalKeys { get; private set; }
        public HashSet<string> BlockedGlobalKeysList  { get; private set; }
        public HashSet<string> AllowedGlobalKeysList  { get; private set; }
        public HashSet<string> PrivateKeysList { get; private set; }

        public readonly string[] BossKeys = new string[]
        { "defeated_eikthyr", "defeated_gdking", "defeated_bonemass", "defeated_dragon", "defeated_goblinking" };

        private string _filepath = "";
        private bool _fileLoaded = false;

        private int _cachedPublicBossKeys = -1;
        private int _cachedPrivateBossKeys = -1;

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

        private float _timer = 0f;
        private readonly float _update = 5f;

        private KeyManager()
        {
            ResetPlayer();
        }

        private static readonly KeyManager _instance = new KeyManager();

        public static KeyManager Instance
        {
            get => _instance;
        }

        private void ResetPlayer()
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

        /// <summary>
        /// Updates class data if chached values have expired.
        /// </summary>
        public static void Update()
        {
            var time = Time.time;
            var delta = time - Instance._timer;

            if (delta - Instance._timer > Instance._update)
            {
                ProgressionPlugin.GetProgressionLogger().LogDebug($"Updating chached Key Information: {delta} time passed.");
                UpdateGlobalKeyConfiguration(ProgressionPlugin.Instance.GetBlockedGlobalKeys(),
                    ProgressionPlugin.Instance.GetAllowedGlobalKeys());
                Instance._cachedPublicBossKeys = CountPublicBossKeys();
                Instance._cachedPrivateBossKeys = CountPrivateBossKeys();

                Instance._timer = time;
            }
        }

        /// <summary>
        /// Set the values for BlockedGlobalKeysList and AllowedGlobalKeysList if changed.
        /// </summary>
        /// <param name="blockedGlobalKeys"></param>
        /// <param name="allowedGlobalKeys"></param>
        private static void UpdateGlobalKeyConfiguration(string blockedGlobalKeys, string allowedGlobalKeys)
        {
            if (!Instance.BlockedGlobalKeys.Equals(blockedGlobalKeys))
            {
                Instance.BlockedGlobalKeysList = new HashSet<string>();

                if (!blockedGlobalKeys.IsNullOrWhiteSpace())
                {
                    var keys = blockedGlobalKeys.Split(',').ToList();
                    for (var lcv = 0; lcv < Instance.BlockedGlobalKeysList.Count; lcv++)
                    {
                        Instance.BlockedGlobalKeysList.Add(keys[lcv].Trim());
                    }
                }
            }

            if (!Instance.AllowedGlobalKeys.Equals(allowedGlobalKeys))
            {
                Instance.AllowedGlobalKeysList = new HashSet<string>();

                if (!allowedGlobalKeys.IsNullOrWhiteSpace())
                {
                    var keys = allowedGlobalKeys.Split(',').ToList();
                    for (var lcv = 0; lcv < Instance.AllowedGlobalKeysList.Count; lcv++)
                    {
                        Instance.AllowedGlobalKeysList.Add(keys[lcv].Trim());
                    }
                }
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
        /// Skips the original ZoneSystem.SetGlobalKey method if a key is blocked.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetGlobalKey))]
        public static class Patch_ZoneSystem_SetGlobalKey
        {
            [HarmonyPriority(Priority.Last)]
            private static bool Prefix(string name)
            {
                Update();
                if (Instance.BlockGlobalKey(name))
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
                AddPrivateKey(name);
            }
        }

        /// <summary>
        /// Whether to block a Global Key based on configuration settings.
        /// </summary>
        /// <param name="globalKey"></param>
        /// <returns>True when default blocked and does not exist in the allowed list,
        /// or when default unblocked and key is in the blocked list.</returns>
        public bool BlockGlobalKey(string globalKey)
        {
            if (ProgressionPlugin.Instance.GetBlockAllGlobalKeys())
            {
                return !AllowedGlobalKeysList?.Contains(globalKey) ?? true;
            }

            return BlockedGlobalKeysList?.Contains(globalKey) ?? false;
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
        private static int CountPrivateBossKeys()
        {
            int count = 0;

            foreach (string key in Instance.BossKeys)
            {
                if (Instance.PrivateKeysList.Contains(key))
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
        private static int CountPublicBossKeys()
        {
            int count = 0;
            foreach (string key in Instance.BossKeys)
            {
                if (ZoneSystem.instance.GetGlobalKey(key))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Attempts to add the given key to the Player's private list.
        /// </summary>
        /// <param name="key"></param>
        public static void AddPrivateKey(string key)
        {
            Instance.PrivateKeysList.Add(key);
        }

        /// <summary>
        /// Attempts to remove the given key from the Player's private list.
        /// </summary>
        /// <param name="key"></param>
        public static void RemovePrivateKey(string key)
        {
            Instance.PrivateKeysList.Remove(key);
        }

        /// <summary>
        /// Reset the private key list for the Player.
        /// </summary>
        public static void ResetPrivateKeys()
        {
            Instance.PrivateKeysList = new HashSet<string>();
        }

        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.SavePlayerData))]
        public static class Patch_PlayerProfile_SavePlayerData
        {
            private static void Postfix(Player player, PlayerProfile __instance)
            {
                try
                {
                    ProgressionPlugin.GetProgressionLogger().LogDebug("Patch_PlayerProfile_SavePlayerData postfix called.");
                    if (Instance.SetFilePaths(__instance.m_fileSource, __instance.GetPath()))
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
            private static void Postfix(Player player)
            {
                if (!ProgressionAPI.IsInTheMainScene() || Instance._fileLoaded)
                {
                    return;
                }

                try
                {
                    ProgressionPlugin.GetProgressionLogger().LogDebug("Patch_PlayerProfile_LoadPlayerData postfix called.");
                    var profile = Game.instance.GetPlayerProfile();
                    if (Instance.SetFilePaths(profile.m_fileSource, profile.GetPath()))
                    {
                        Instance.PrivateKeysList = Instance.LoadFile(profile.m_fileSource);
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
                ProgressionPlugin.GetProgressionLogger().LogDebug("Patch_Player_Awake postfix called.");
                Instance.ResetPlayer();
            }
        }

        /// <summary>
        /// Saves private player keys to a file.
        /// </summary>
        /// <param name="filesource"></param>
        /// <param name="keys"></param>
        private void SaveFile(FileHelpers.FileSource filesource, HashSet<string> keys)
        {
            if (Directory.Exists(_filepath) && !Instance._fileLoaded)
            {
                // Do not override an existing file that has not been loaded yet.
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
            FileReader? fileReader = null;
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
                Instance._fileLoaded = true;

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
        /// Set the file path if defined.
        /// </summary>
        /// <param name="filesource"></param>
        /// <param name="path"></param>
        /// <returns>Returns true if the file path is defined.</returns>
        private bool SetFilePaths(FileHelpers.FileSource filesource, string path)
        {
            if (Instance._filepath.IsNullOrWhiteSpace())
            {
                Instance._filepath = path;
            }

            if (!Instance._filepath.IsNullOrWhiteSpace()) return true;

            ProgressionPlugin.GetProgressionLogger().LogWarning($"File paths could not be set for FileSource: {filesource}, keys data may not load or save correctly.");
            return false;
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
                        AddPrivateKey(args[1]);
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
                        RemovePrivateKey(args[1]);
                        args.Context.AddString("Removing private key " + args[1]);
                    }
                    else
                    {
                        args.Context.AddString("Syntax: removeprivatekey [key]");
                    }
                }, isCheat: true, isNetwork: false, onlyServer: true);
                new Terminal.ConsoleCommand("resetprivatekeys", "[name]", delegate (Terminal.ConsoleEventArgs args)
                {
                    ResetPrivateKeys();
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
    }
}