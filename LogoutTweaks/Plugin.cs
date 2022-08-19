using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;

namespace VentureValheim.LogoutTweaks
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class LogoutTweaksPlugin : BaseUnityPlugin
    {
        private const string ModName = "LogoutTweaks";
        private const string ModVersion = "0.0.3";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ModVersion + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource LogoutTweaksLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

            private static readonly ConfigSync ConfigurationSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

            internal ConfigEntry<bool> CE_ServerConfigLocked = null!;

            internal static ConfigEntry<bool> CE_ModEnabled = null!;

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
                AddConfig("Enabled", general,"Enable module (boolean).",
                    true, true, ref CE_ModEnabled);

            #endregion

            if (!CE_ModEnabled.Value)
                return;

            LogoutTweaksLogger.LogInfo("Initializing LogoutTweaks configurations...");

            try
            {
                LogoutTweaks.Instance.Initialize();
            }
            catch (Exception e)
            {
                LogoutTweaksLogger.LogError("Error configuring LogoutTweaks, aborting...");
                LogoutTweaksLogger.LogError(e);
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
                LogoutTweaksLogger.LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                LogoutTweaksLogger.LogError($"There was an issue loading {ConfigFileName}");
            }
        }
    }
}