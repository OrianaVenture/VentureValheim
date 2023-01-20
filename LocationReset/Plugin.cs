using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;

namespace VentureValheim.LocationReset
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class LocationResetPlugin : BaseUnityPlugin
    {
        static LocationResetPlugin() { }
        private LocationResetPlugin() { }
        private static readonly LocationResetPlugin _instance = new LocationResetPlugin();

        public static LocationResetPlugin Instance
        {
            get => _instance;
        }

        private const string ModName = "LocationReset";
        private const string ModVersion = "0.1.0";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource LocationResetLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        private static readonly ConfigSync ConfigurationSync = new(ModGUID)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        private static ConfigEntry<bool> CE_ServerConfigLocked = null!;

        private static ConfigEntry<bool> CE_ModEnabled = null!;
        private static ConfigEntry<int> CE_ResetTime = null!;
        private static ConfigEntry<bool> CE_SkipPlayerGroundPieceCheck = null!;

        public static int GetResetTime() => CE_ResetTime.Value;
        public static bool GetSkipPlayerGroundPieceCheck() => CE_SkipPlayerGroundPieceCheck.Value;

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

            AddConfig("ResetTime", general, "Number of in-game days for reset, one day is about 30 minutes (int).",
                true, 30, ref CE_ResetTime);
            AddConfig("SkipPlayerGroundPieceCheck", general, "When True will reset locations even if player placed pieces " +
                "and tombstones are on the ground outside the entrance (bool).",
                true, false, ref CE_SkipPlayerGroundPieceCheck);

            #endregion

            if (!CE_ModEnabled.Value)
                return;

            LocationResetLogger.LogInfo("LocationReset getting ready for mass destruction. Consider making backups before using this mod!");

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
                LocationResetLogger.LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                LocationResetLogger.LogError($"There was an issue loading {ConfigFileName}");
            }
        }
    }
}