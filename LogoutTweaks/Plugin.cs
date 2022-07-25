using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using VentureValheim.ServerSync;

namespace VentureValheim.LogoutTweaks
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class LogoutTweaksPlugin : BaseUnityPlugin
    {
        private const string ModName = "LogoutTweaks";
        private const string ModVersion = "0.0.1";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ModVersion + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource LogoutTweaksLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries
        
        internal ConfigEntry<bool> CE_ServerConfigLocked = null!;
            
        internal static ConfigEntry<bool> CE_ModEnabled = null!;
            
        #endregion

        public void Awake()
        {
            #region Configuration
            
                const string general = "General";
                const string serverConfigLocked = "Force Server Config";
                const string enabled = "Enabled";

                string serverConfigLockedDescription = ConfigSync.Instance.GetExtendedDescription("Force Server Config (boolean)", true);
                CE_ServerConfigLocked = Config.Bind(general, serverConfigLocked, true, serverConfigLockedDescription);
                ConfigSync.Instance.AddConfigEntry(CE_ServerConfigLocked, true);

                string enabledDescription = ConfigSync.Instance.GetExtendedDescription("Enable module (boolean).", true);
                CE_ModEnabled = Config.Bind(general, enabled, true, enabledDescription);
                ConfigSync.Instance.AddConfigEntry(CE_ModEnabled, true);
            
            #endregion

            if (!CE_ModEnabled.Value)
                return;
            
            LogoutTweaksLogger.LogInfo("Initializing LogoutTweaks configurations...");
            
            try
            {
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