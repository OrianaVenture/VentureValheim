using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using VentureValheim.ServerSync;

namespace VentureValheim.Progression
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ProgressionPlugin : BaseUnityPlugin
    {
        private const string ModName = "WorldAdvancementProgression";
        private const string ModVersion = "0.0.3";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + "." + ModVersion + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource VentureProgressionLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

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
            
            private void AddConfig<T>(string key, string section, string description, bool synced, T value, ref ConfigEntry<T> configEntry)
            {
                string extendedDescription = ConfigSync.Instance.GetExtendedDescription(description, synced);
                configEntry = Config.Bind(section, key, value, extendedDescription);
                ConfigSync.Instance.AddConfigEntry(configEntry, synced);
            }

        #endregion

        public void Awake()
        {
            #region Configuration

                const string general = "General";
                const string skills = "Skills";

                AddConfig("Force Server Config", general,
                    ConfigSync.Instance.GetExtendedDescription("Force Server Config (boolean)",true),
                    true, true, ref CE_ServerConfigLocked);
                AddConfig("Enabled", general,
                    ConfigSync.Instance.GetExtendedDescription("Enable module (boolean).",true),
                    true, true, ref CE_ModEnabled);
                
                AddConfig("BlockAllGlobalKeys", general,
                    ConfigSync.Instance.GetExtendedDescription("Whether to stop all global keys from being added to the global list (boolean).",true),
                    true, true, ref CE_BlockAllGlobalKeys);
                AddConfig("BlockedGlobalKeys", general,
                    ConfigSync.Instance.GetExtendedDescription("Stop only these keys being added to the global list when BlockAllGlobalKeys is false (comma-separated).",true),
                    true, "", ref CE_BlockedGlobalKeys);
                AddConfig("AllowedGlobalKeys", general,
                    ConfigSync.Instance.GetExtendedDescription("Allow only these keys being added to the global list when BlockAllGlobalKeys is true (comma-separated).",true),
                    true, "", ref CE_AllowedGlobalKeys);
                
                
                AddConfig("AllowSkillDrain", skills,
                    ConfigSync.Instance.GetExtendedDescription("Enable skill drain (boolean).",true),
                    true, true, ref CE_AllowSkillDrain);
                AddConfig("UseAbsoluteSkillDrain", skills,
                    ConfigSync.Instance.GetExtendedDescription("Reduce skills by a set value (boolean).",true),
                    true, false, ref CE_UseAbsoluteSkillDrain);
                AddConfig("AbsoluteSkillDrain", skills,
                    ConfigSync.Instance.GetExtendedDescription("Reduce all skills by this value (on death) (int).",true),
                    true, 1, ref CE_AbsoluteSkillDrain);

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