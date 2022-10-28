using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.NoSeasonalRestrictions
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class NoSeasonalRestrictionsPlugin : BaseUnityPlugin
    {
        private const string ModName = "NoSeasonalRestrictions";
        private const string ModVersion = "0.1.3";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ModVersion + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource NoSeasonalRestrictionsLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        internal static ConfigEntry<bool> CE_ModEnabled = null!;

        private void AddConfig<T>(string key, string section, string description, T value, ref ConfigEntry<T> configEntry)
        {
            configEntry = Config.Bind(section, key, value, description);
        }

        #endregion

        public void Awake()
        {
            AddConfig("Enabled", "General", "Enable module (boolean).",
                true, ref CE_ModEnabled);

            if (!CE_ModEnabled.Value)
                return;

            NoSeasonalRestrictionsLogger.LogInfo("Initializing NoSeasonalRestrictions, the weather is hot and snowy!");

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
            HarmonyInstance.UnpatchSelf();
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
                NoSeasonalRestrictionsLogger.LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                NoSeasonalRestrictionsLogger.LogError($"There was an issue loading {ConfigFileName}");
            }
        }
    }
}