using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;

namespace VentureValheim.IncognitoMode
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class IncognitoModePlugin : BaseUnityPlugin
    {
        private const string ModName = "IncognitoMode";
        private const string ModVersion = "0.1.1";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource IncognitoModeLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        private static readonly ConfigSync ConfigurationSync = new(ModGUID)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        internal ConfigEntry<bool> CE_ServerConfigLocked = null!;

        internal static ConfigEntry<bool> CE_ModEnabled = null!;
        internal static ConfigEntry<string> CE_HiddenByItems = null!;
        internal static ConfigEntry<string> CE_HiddenDisplayName = null!;
        internal static ConfigEntry<bool> CE_HideNameInChat = null!;

        public static string GetHiddenByItems() => CE_HiddenByItems.Value;
        public static string GetHiddenDisplayName() => CE_HiddenDisplayName.Value;
        public static bool GetHideNameInChat() => CE_HideNameInChat.Value;

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

            AddConfig("Enabled", general,"Enable module (boolean).",
                true, true, ref CE_ModEnabled);
            AddConfig("HiddenByItems", general, "Prefab names of helmets that can hide a Player's identity/name (comma-separated string).",
                true, "HelmetRoot, HelmetFenring, HelmetPadded", ref CE_HiddenByItems);
            AddConfig("HiddenDisplayName", general, "The hidden Player's display name (string).",
                true, "???", ref CE_HiddenDisplayName);
            AddConfig("HideNameInChat", general, "When hidden also hides the name in chat (boolean).",
                true, true, ref CE_HideNameInChat);

            #endregion

            if (!CE_ModEnabled.Value)
                return;

            IncognitoModeLogger.LogInfo("I wear a mask, and that mask is not to hide who I am, but to create who I am.");

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
                IncognitoModeLogger.LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                IncognitoModeLogger.LogError($"There was an issue loading {ConfigFileName}");
            }
        }
    }
}