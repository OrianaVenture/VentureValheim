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
        private const string ModVersion = "0.2.3";
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

        private static ConfigEntry<int> CE_ResetTime = null!;
        private static ConfigEntry<bool> CE_SkipPlayerGroundPieceCheck = null!;

        private static ConfigEntry<bool> CE_ResetGroundLocations = null!;
        private static ConfigEntry<bool> CE_OverrideResetTimes = null!;
        private static ConfigEntry<int> CE_FarmResetTime = null!;
        private static ConfigEntry<int> CE_VillageResetTime = null!;
        private static ConfigEntry<int> CE_TrollResetTime = null!;
        private static ConfigEntry<int> CE_BurialResetTime = null!;
        private static ConfigEntry<int> CE_CryptResetTime = null!;
        private static ConfigEntry<int> CE_CaveResetTime = null!;
        private static ConfigEntry<int> CE_CampResetTime = null!;
        private static ConfigEntry<int> CE_MineResetTime = null!;
        private static ConfigEntry<int> CE_QueenResetTime = null!;

        private static readonly int TrollCave02 = "TrollCave02".GetStableHashCode();
        private static readonly string DGVillage = "DG_MeadowsVillage";
        private static readonly string DGFarm = "DG_MeadowsFarm";
        private static readonly string DGBurial = "DG_ForestCrypt";
        private static readonly string DGCrypt = "DG_SunkenCrypt";
        private static readonly string DGCave = "DG_Cave";
        private static readonly string DGCamp = "DG_GoblinCamp";
        private static readonly string DGMine = "DG_DvergrTown";
        private static readonly string DGQueen = "DG_DvergrBoss";

        public static int GetResetTime(int hash)
        {
            if (CE_OverrideResetTimes.Value)
            {
                if (hash == TrollCave02)
                {
                    return CE_TrollResetTime.Value;
                }
            }

            return CE_ResetTime.Value;
        }

        public static int GetResetTime(string prefab)
        {
            if (CE_OverrideResetTimes.Value)
            {
                if (prefab.Contains(DGVillage))
                {
                    return CE_VillageResetTime.Value;
                }
                else if (prefab.Contains(DGFarm))
                {
                    return CE_FarmResetTime.Value;
                }
                else if (prefab.Contains(DGBurial))
                {
                    return CE_BurialResetTime.Value;
                }
                else if (prefab.Contains(DGCrypt))
                {
                    return CE_CryptResetTime.Value;
                }
                else if (prefab.Contains(DGCave))
                {
                    return CE_CaveResetTime.Value;
                }
                else if (prefab.Contains(DGCamp))
                {
                    return CE_CampResetTime.Value;
                }
                else if (prefab.Contains(DGMine))
                {
                    return CE_MineResetTime.Value;
                }
                else if (prefab.Contains(DGQueen))
                {
                    return CE_QueenResetTime.Value;
                }
            }

            return CE_ResetTime.Value;
        }

        public static bool GetSkipPlayerGroundPieceCheck() => CE_SkipPlayerGroundPieceCheck.Value;
        public static bool GetResetGroundLocations() => CE_ResetGroundLocations.Value;

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
            const string advanced = "Advanced";

            AddConfig("Force Server Config", general, "Force Server Config (boolean).",
                true, true, ref CE_ServerConfigLocked);
            ConfigurationSync.AddLockingConfigEntry(CE_ServerConfigLocked);

            AddConfig("ResetTime", general, "Default number of in-game days for reset, one day is about 30 minutes (int).",
                true, 30, ref CE_ResetTime);
            AddConfig("SkipPlayerGroundPieceCheck", general, "When True will reset locations even if player placed pieces " +
                "and tombstones are on the ground outside the entrance to sky locations (boolean).",
                true, false, ref CE_SkipPlayerGroundPieceCheck);

            AddConfig("ResetGroundLocations", advanced, "True to reset all misc locations found on the ground (not including meadow farms/villages or fuling camps) (boolean).",
                true, true, ref CE_ResetGroundLocations);
            AddConfig("OverrideResetTimes", advanced, "True to use all the values below rather than the default when applicable (boolean).",
                true, false, ref CE_OverrideResetTimes);
            AddConfig("FarmResetTime", advanced, "Number of in-game days for resetting meadow farms (int).",
                true, 30, ref CE_FarmResetTime);
            AddConfig("VillageResetTime", advanced, "Number of in-game days for resetting meadow draugr villages (int).",
                true, 30, ref CE_VillageResetTime);
            AddConfig("TrollResetTime", advanced, "Number of in-game days for resetting black forest troll caves (int).",
                true, 30, ref CE_TrollResetTime);
            AddConfig("BurialResetTime", advanced, "Number of in-game days for resetting black forest burial chambers (int).",
                true, 30, ref CE_BurialResetTime);
            AddConfig("CryptResetTime", advanced, "Number of in-game days for resetting swamp crypts (int).",
                true, 30, ref CE_CryptResetTime);
            AddConfig("CaveResetTime", advanced, "Number of in-game days for resetting mountain caves (int).",
                true, 30, ref CE_CaveResetTime);
            AddConfig("CampResetTime", advanced, "Number of in-game days for resetting plain fuling camps (int).",
                true, 30, ref CE_CampResetTime);
            AddConfig("MineResetTime", advanced, "Number of in-game days for resetting mistland infested mines (int).",
                true, 30, ref CE_MineResetTime);
            AddConfig("QueenResetTime", advanced, "Number of in-game days for resetting mistland infested citadel (int).",
                true, 30, ref CE_QueenResetTime);

            #endregion

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