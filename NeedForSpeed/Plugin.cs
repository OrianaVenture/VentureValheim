using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;

namespace VentureValheim.NeedForSpeed
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class NeedForSpeedPlugin : BaseUnityPlugin
    {
        static NeedForSpeedPlugin() { }
        private NeedForSpeedPlugin() { }
        private static readonly NeedForSpeedPlugin _instance = new NeedForSpeedPlugin();

        public static NeedForSpeedPlugin Instance
        {
            get => _instance;
        }

        private const string ModName = "NeedForSpeed";
        private const string ModVersion = "0.1.0";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource NeedForSpeedLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        private static readonly ConfigSync ConfigurationSync = new(ModGUID)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        internal ConfigEntry<bool> CE_ServerConfigLocked = null!;

        internal static ConfigEntry<bool> CE_ModEnabled = null!;
        internal static ConfigEntry<float> CE_JogSpeedMultiplier = null!;
        internal static ConfigEntry<float> CE_RunSpeedMultiplier = null!;

        public float GetJogSpeedMultiplier() => CE_JogSpeedMultiplier.Value;
        public float GetRunSpeedMultiplier() => CE_RunSpeedMultiplier.Value;

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
            AddConfig("JogSpeedMultiplier", general, "Jog Speed Multiplier, 1.3 is 30% faster (float).",
                true, 1.3f, ref CE_JogSpeedMultiplier);
            AddConfig("RunSpeedMultiplier", general, "Run Speed Multiplier, 1.3 is 30% faster (float).",
                true, 1.3f, ref CE_RunSpeedMultiplier);

            #endregion

            if (!CE_ModEnabled.Value)
                return;

            NeedForSpeedLogger.LogInfo("Somebody got the zoomies? Get ready to go fast!");

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
                NeedForSpeedLogger.LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                NeedForSpeedLogger.LogError($"There was an issue loading {ConfigFileName}");
            }
        }
    }
}