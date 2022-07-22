using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;

namespace VentureValheim.Progression
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ProgressionPlugin : BaseUnityPlugin
    {
        private const string ModName = "VentureProgression";
        private const string ModVersion = "0.0.1";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource VentureProgressionLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigurationSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
            
        private ConfigEntry<bool> CE_ServerConfigLocked = null!;
        
        private static ConfigEntry<bool> CE_ModEnabled = null!;
    
        private static ConfigEntry<bool> CE_BlockAllGlobalKeys = null!;
        private static ConfigEntry<string> CE_BlockedGlobalKeys = null!;
        private static ConfigEntry<string> CE_AllowedGlobalKeys = null!;

        private bool GetBlockAllGlobalKeys() => CE_BlockAllGlobalKeys.Value;
        private string GetBlockedGlobalKeys() => CE_BlockedGlobalKeys.Value;
        private string GetAllowedGlobalKeys() => CE_AllowedGlobalKeys.Value;

        internal ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigurationSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }
        
        internal ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        public void Awake()
        {
            const string general = "General";
            
            CE_ServerConfigLocked = config(general, "Force Server Config", true, "Force Server Config (boolean)");
            ConfigurationSync.AddLockingConfigEntry(CE_ServerConfigLocked);
            CE_ModEnabled = config(general, "Enabled", true, "Enable module (boolean).");
            CE_BlockAllGlobalKeys = config(general, "BlockAllGlobalKeys", true, 
                "Whether to stop all global keys from being added to the global list (boolean).");
            CE_BlockedGlobalKeys = config(general, "BlockedGlobalKeys", "", 
                "Stop only these keys being added to the global list when blockAllGlobalKeys is false (comma-separated).");
            CE_AllowedGlobalKeys = config(general, "AllowedGlobalKeys", "", 
                "Allow only these keys being added to the global list when blockAllGlobalKeys is true (comma-separated).");
            
            if (!CE_ModEnabled.Value)
                return;
            
            try
            {
                VentureProgressionLogger.LogInfo("Initializing ProgressionManager configuration...");
                ProgressionManager.Instance.Initialize(GetBlockAllGlobalKeys(), GetBlockedGlobalKeys(), GetAllowedGlobalKeys());
                VentureProgressionLogger.LogInfo(ProgressionManager.BlockAllGlobalKeys
                    ? $"Blocking all keys except {ProgressionManager.AllowedGlobalKeysList?.Count ?? 0} globally allowed keys.."
                    : $"Allowing all keys except {ProgressionManager.BlockedGlobalKeysList?.Count ?? 0} globally blocked keys..");
            }
            catch (Exception e)
            {
                VentureProgressionLogger.LogError("Error configuring ProgressionManager, aborting...");
                VentureProgressionLogger.LogError(e);
                return;
            }
            
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
                VentureProgressionLogger.LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                VentureProgressionLogger.LogError($"There was an issue loading {ConfigFileName}");
            }
        }
    }
}