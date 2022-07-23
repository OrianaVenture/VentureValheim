using System;
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
        private const string ModName = "WorldAdvancementProgression";
        private const string ModVersion = "0.0.2";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + "." + ModVersion + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource VentureProgressionLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigurationSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
            
        #region ConfigurationEntries
        
            private ConfigEntry<bool> CE_ServerConfigLocked = null!;
            
            private static ConfigEntry<bool> CE_ModEnabled = null!;
        
            // Progression Manager
            private static ConfigEntry<bool> CE_BlockAllGlobalKeys = null!;
            private static ConfigEntry<string> CE_BlockedGlobalKeys = null!;
            private static ConfigEntry<string> CE_AllowedGlobalKeys = null!;
            
            private bool GetBlockAllGlobalKeys() => CE_BlockAllGlobalKeys.Value;
            private string GetBlockedGlobalKeys() => CE_BlockedGlobalKeys.Value;
            private string GetAllowedGlobalKeys() => CE_AllowedGlobalKeys.Value;
            
            // Skills Manager
            private static ConfigEntry<bool> CE_AllowSkillDrain = null!;
            private static ConfigEntry<bool> CE_UseAbsoluteSkillDrain = null!;
            private static ConfigEntry<int> CE_AbsoluteSkillDrain = null!;
            
            private bool GetAllowSkillDrain() => CE_AllowSkillDrain.Value;
            private bool GetUseAbsoluteSkillDrain() => CE_UseAbsoluteSkillDrain.Value;
            private int GetAbsoluteSkillDrain() => CE_AbsoluteSkillDrain.Value;


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
        
        #endregion

        public void Awake()
        {
            #region Configuration
            
                const string general = "General";
                const string skills = "Skills";
                
                CE_ServerConfigLocked = config(general, "Force Server Config", true, "Force Server Config (boolean)");
                ConfigurationSync.AddLockingConfigEntry(CE_ServerConfigLocked);
                CE_ModEnabled = config(general, "Enabled", true, "Enable module (boolean).");
                
                CE_BlockAllGlobalKeys = config(general, "BlockAllGlobalKeys", true, 
                    "Whether to stop all global keys from being added to the global list (boolean).");
                CE_BlockedGlobalKeys = config(general, "BlockedGlobalKeys", "", 
                    "Stop only these keys being added to the global list when blockAllGlobalKeys is false (comma-separated).");
                CE_AllowedGlobalKeys = config(general, "AllowedGlobalKeys", "", 
                    "Allow only these keys being added to the global list when blockAllGlobalKeys is true (comma-separated).");
                
                CE_AllowSkillDrain = config(skills, "AllowSkillDrain", true, "Enable skill drain (boolean).");
                CE_UseAbsoluteSkillDrain = config(skills, "UseAbsoluteSkillDrain", false, "Reduce skills by a set value (boolean).");
                CE_AbsoluteSkillDrain = config(skills, "AbsoluteSkillDrain", 1, "Reduce all skills by this value (on death) (int).");
            
            #endregion

            if (!CE_ModEnabled.Value)
                return;
            
            VentureProgressionLogger.LogInfo("Initializing Progression configurations...");
            
            try
            {
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
            
            try
            {
                SkillsManager.Instance.Initialize(GetAllowSkillDrain(), GetUseAbsoluteSkillDrain(), GetAbsoluteSkillDrain());
                VentureProgressionLogger.LogDebug($"Skill gain: {GetAllowSkillDrain()}. Using custom skill drain: " +
                    $"{GetUseAbsoluteSkillDrain()} with a value of {GetAbsoluteSkillDrain()}");
            }
            catch (Exception e)
            {
                VentureProgressionLogger.LogError("Error configuring SkillsManager, aborting...");
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