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
        private const string ModVersion = "0.4.5";
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
        internal static ConfigEntry<bool> CE_EnableHaldorMapPin = null!;
        internal static ConfigEntry<bool> CE_EnableArrivalMessage = null!;
        internal static ConfigEntry<bool> CE_EnableArrivalMessageShout = null!;
        internal static ConfigEntry<string> CE_OverrideArrivalMessage = null!;
        internal static ConfigEntry<bool> CE_OverridePlayerMapPins = null!;
        internal static ConfigEntry<bool> CE_ForcePlayerMapPinsOn = null!;
        internal static ConfigEntry<string> CE_PlayerDefaultSpawnPoint = null!;
        internal static ConfigEntry<bool> CE_OverridePlayerPVP = null!;
        internal static ConfigEntry<bool> CE_ForcePlayerPVPOn = null!;
        internal static ConfigEntry<bool> CE_TeleportOnPVPDeath = null!;

        public static int GetMaximumPlayers() => CE_MaximumPlayers.Value;
        public static bool GetEnableValkrie() => CE_EnableValkrie.Value;
        public static bool GetEnableHaldorMapPin() => CE_EnableHaldorMapPin.Value;
        public static bool GetEnableArrivalMessage() => CE_EnableArrivalMessage.Value;
        public static bool GetEnableArrivalMessageShout() => CE_EnableArrivalMessageShout.Value;
        public static string GetOverrideArrivalMessage() => CE_OverrideArrivalMessage.Value;
        public static bool GetOverridePlayerMapPins() => CE_OverridePlayerMapPins.Value;
        public static bool GetForcePlayerMapPinsOn() => CE_ForcePlayerMapPinsOn.Value;
        public static string GetPlayerDefaultSpawnPoint() => CE_PlayerDefaultSpawnPoint.Value;
        public static bool GetOverridePlayerPVP() => CE_OverridePlayerPVP.Value;
        public static bool GetForcePlayerPVPOn() => CE_ForcePlayerPVPOn.Value;
        public static bool GetTeleportOnPVPDeath() => CE_TeleportOnPVPDeath.Value;

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
            const string map = "Map";
            const string arrival = "Arrival";

            AddConfig("Force Server Config", general, "Force Server Config (boolean).",
                true, true, ref CE_ServerConfigLocked);
            ConfigurationSync.AddLockingConfigEntry(CE_ServerConfigLocked);

            AddConfig("Enabled", general,"Enable module (boolean).",
                true, true, ref CE_ModEnabled);
            AddConfig("MaximumPlayers", general, "Maximum Players for the Server (integer).",
                true, 10, ref CE_MaximumPlayers);
            AddConfig("OverridePlayerPVP", general, "Override Player pvp behavior (boolean).",
                true, false, ref CE_OverridePlayerPVP);
            AddConfig("ForcePlayerPVPOn", general, "True to set pvp always on when OverridePlayerPVP is True (boolean).",
                true, true, ref CE_ForcePlayerPVPOn);
            AddConfig("TeleportOnPVPDeath", general, "False to respawn players at their graves on a PVP death (boolean).",
                true, true, ref CE_TeleportOnPVPDeath);

            AddConfig("PlayerDefaultSpawnPoint", arrival, "Coordinates for the default player spawn point (x,z) no parentheses, leave empty to use game default (comma-separated floats).",
                true, "", ref CE_PlayerDefaultSpawnPoint);
            AddConfig("EnableValkrie", arrival, "True to enable Valkrie Intro (boolean).",
                true, true, ref CE_EnableValkrie);
            AddConfig("EnableArrivalMessage", arrival, "True to enable Arrival Message on player login (boolean).",
                true, true, ref CE_EnableArrivalMessage);
            AddConfig("UseArrivalShout", arrival, "False to use a Normal message when EnableArrivalMessage is True (boolean).",
                true, true, ref CE_EnableArrivalMessageShout);
            AddConfig("OverrideArrivalMessage", arrival, "Set a new arrival message, leave blank to use default (string).",
                true, "", ref CE_OverrideArrivalMessage);

            AddConfig("EnableHaldorMapPin", map, "True to allow Haldor map pin on Minimap (boolean).",
                true, true, ref CE_EnableHaldorMapPin);
            AddConfig("OverridePlayerMapPositions", map, "Override Player map pin position behavior for Minimap (boolean).",
                true, false, ref CE_OverridePlayerMapPins);
            AddConfig("ForcePlayerMapPositionOn", map, "True to always show Player position on Minimap when OverridePlayerMapPositions is True (boolean).",
                true, true, ref CE_ForcePlayerMapPinsOn);

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