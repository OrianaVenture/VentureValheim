using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.Template
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class TemplatePlugin : BaseUnityPlugin
    {
        private const string ModName = "Template";
        private const string ModVersion = "0.1.0";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource TemplateLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        private static readonly ConfigSync ConfigurationSync = new(ModGUID)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        internal static ConfigEntry<bool> CE_ServerConfigLocked = null!;

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

            #endregion

            TemplateLogger.LogInfo("Initializing Template configurations...");

            try
            {
                Template.Instance.Initialize();
            }
            catch (Exception e)
            {
                TemplateLogger.LogError("Error configuring Template, aborting...");
                TemplateLogger.LogError(e);
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
                TemplateLogger.LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                TemplateLogger.LogError($"There was an issue loading {ConfigFileName}");
            }
        }
    }
}