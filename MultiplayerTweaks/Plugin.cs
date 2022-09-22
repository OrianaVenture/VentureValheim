using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;

namespace VentureValheim.MultiplayerTweaks
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class MultiplayerTweaksPlugin : BaseUnityPlugin
    {
        private const string ModName = "MultiplayerTweaks";
        private const string ModVersion = "0.1.2";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource MultiplayerTweaksLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        private static readonly ConfigSync ConfigurationSync = new(ModGUID)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        internal ConfigEntry<bool> CE_ServerConfigLocked = null!;
        internal static ConfigEntry<bool> CE_ModEnabled = null!;
        internal static ConfigEntry<int> CE_MaximumPlayers = null!;
        internal static ConfigEntry<bool> CE_EnableValkrie = null!;
        internal static ConfigEntry<bool> CE_EnableHuginTutorials = null!;
        internal static ConfigEntry<bool> CE_EnableHaldorMapPin = null!;
        internal static ConfigEntry<bool> CE_EnableArrivalMessage = null!;
        internal static ConfigEntry<bool> CE_EnableArrivalMessageShout = null!;
        internal static ConfigEntry<string> CE_OverrideArrivalMessage = null!;

        public static int GetMaximumPlayers() => CE_MaximumPlayers.Value;
        public static bool GetEnableValkrie() => CE_EnableValkrie.Value;
        public static bool GetEnableHuginTutorials() => CE_EnableHuginTutorials.Value;
        public static bool GetEnableHaldorMapPin() => CE_EnableHaldorMapPin.Value;
        public static bool GetEnableArrivalMessage() => CE_EnableArrivalMessage.Value;
        public static bool GetEnableArrivalMessageShout() => CE_EnableArrivalMessageShout.Value;
        public static string GetOverrideArrivalMessage() => CE_OverrideArrivalMessage.Value;

        private void AddConfig<T>(string key, string section, string description, bool synced, T value, ref ConfigEntry<T> configEntry)
        {
            string extendedDescription = GetExtendedDescription(description, synced);
            configEntry = Config.Bind(section, key, value, extendedDescription);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigurationSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synced;
        }

        public string GetExtendedDescription(string description, bool synchronizedSetting)
        {
            return description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]");
        }

        #endregion

        public void Awake()
        {
            #region Configuration

            const string general = "General";

            AddConfig("Force Server Config", general, "Force Server Config (boolean).",
                true, true, ref CE_ServerConfigLocked);
            ConfigurationSync.AddLockingConfigEntry(CE_ServerConfigLocked);

            AddConfig("Enabled", general,"Enable module (boolean).",
                true, true, ref CE_ModEnabled);
            AddConfig("MaximumPlayers", general, "Maximum Players for Server (integer).",
                true, 10, ref CE_MaximumPlayers);
            AddConfig("EnableValkrie", general, "Enable Valkrie Intro (boolean).",
                true, false, ref CE_EnableValkrie);
            AddConfig("EnableHugin", general, "Enable Hugin tutorials (boolean).",
                false, true, ref CE_EnableHuginTutorials);
            AddConfig("EnableHaldorMapPin", general, "Enable Haldor Map Pin on Minimap (boolean).",
                true, false, ref CE_EnableHaldorMapPin);
            AddConfig("EnableArrivalMessage", general, "Enable Arrival Message on new server connection (boolean).",
                true, false, ref CE_EnableArrivalMessage);
            AddConfig("UseArrivalShout", general, "True to Shout arrival message, False to use Normal message (boolean).",
                true, false, ref CE_EnableArrivalMessageShout);
            AddConfig("OverrideArrivalMessage", general, "Override arrival message, leave blank to use default (string).",
                true, "", ref CE_OverrideArrivalMessage);

            #endregion

            if (!CE_ModEnabled.Value)
                return;

            MultiplayerTweaksLogger.LogInfo("Initializing MultiplayerTweaks!");

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
            HarmonyInstance.UnpatchSelf();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                MultiplayerTweaksLogger.LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                MultiplayerTweaksLogger.LogError($"There was an issue loading {ConfigFileName}");
            }
        }
    }
}