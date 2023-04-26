using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;

namespace VentureValheim.FloatingItems
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class FloatingItemsPlugin : BaseUnityPlugin
    {
        private const string ModName = "VentureFloatingItems";
        private const string ModVersion = "0.1.1";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource FloatingItemsLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        private static readonly ConfigSync ConfigurationSync = new(ModGUID)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        internal static ConfigEntry<bool> CE_ServerConfigLocked = null!;

        internal static ConfigEntry<string> CE_FloatingItems = null!;
        internal static ConfigEntry<string> CE_SinkingItems = null!;
        internal static ConfigEntry<bool> CE_FloatTrophies = null!;
        internal static ConfigEntry<bool> CE_FloatMeat = null!;
        internal static ConfigEntry<bool> CE_FloatHides = null!;
        internal static ConfigEntry<bool> CE_FloatGearAndCraftable = null!;

        public static string GetFloatingItems() => CE_FloatingItems.Value;
        public static string GetSinkingItems() => CE_SinkingItems.Value;
        public static bool GetFloatTrophies() => CE_FloatTrophies.Value;
        public static bool GetFloatMeat() => CE_FloatMeat.Value;
        public static bool GetFloatHides() => CE_FloatHides.Value;
        public static bool GetFloatGearAndCraftable() => CE_FloatGearAndCraftable.Value;

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

            AddConfig("FloatingItems", general, "Additional prefab names of the items you want to float (comma-separated string).",
                true, "SerpentScale", ref CE_FloatingItems);
            AddConfig("SinkingItems", general, "Additional prefab names of the items you want to always sink (comma-separated string).",
                true, "BronzeNails, IronNails", ref CE_SinkingItems);
            AddConfig("FloatTrophies", general, "Apply floating to all trophies (boolean).",
                true, true, ref CE_FloatTrophies);
            AddConfig("FloatMeat", general, "Apply floating to all types of meat (boolean).",
                true, true, ref CE_FloatMeat);
            AddConfig("FloatHides", general, "Apply floating to all leathers and jute fabrics (boolean).",
                true, true, ref CE_FloatHides);
            AddConfig("FloatGearAndCraftable", general, "Apply floating to all craftable items and other gear (boolean).",
                true, true, ref CE_FloatGearAndCraftable);

            #endregion

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
                FloatingItemsLogger.LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                FloatingItemsLogger.LogError($"There was an issue loading {ConfigFileName}");
            }
        }
    }
}