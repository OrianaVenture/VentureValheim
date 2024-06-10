using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;

namespace VentureValheim.IncognitoMode
{
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class IncognitoModePlugin : BaseUnityPlugin
    {
        private const string ModName = "IncognitoMode";
        private const string ModVersion = "0.4.0";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource IncognitoModeLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        internal static ConfigEntry<string> CE_HiddenByItems = null!;
        internal static ConfigEntry<string> CE_HiddenDisplayName = null!;
        internal static ConfigEntry<bool> CE_HideNameInChat = null!;
        internal static ConfigEntry<bool> CE_HidePlatformTag = null!;

        public static string GetHiddenByItems() => CE_HiddenByItems.Value;
        public static string GetHiddenDisplayName() => CE_HiddenDisplayName.Value;
        public static bool GetHideNameInChat() => CE_HideNameInChat.Value;
        public static bool GetHidePlatformTag() => CE_HidePlatformTag.Value;

        private readonly ConfigurationManagerAttributes AdminConfig = new ConfigurationManagerAttributes { IsAdminOnly = true };
        private readonly ConfigurationManagerAttributes ClientConfig = new ConfigurationManagerAttributes { IsAdminOnly = false };

        private void AddConfig<T>(string key, string section, string description, bool synced, T value, ref ConfigEntry<T> configEntry)
        {
            string extendedDescription = GetExtendedDescription(description, synced);
            configEntry = Config.Bind(section, key, value,
                new ConfigDescription(extendedDescription, null, synced ? AdminConfig : ClientConfig));
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

            AddConfig("HiddenByItems", general, "Prefab names of helmet/shoulder items that can hide a Player's name (comma-separated string).",
                true, "HelmetRoot, HelmetFenring, HelmetPadded, HelmetMage_Ashlands, HelmetFlametal", ref CE_HiddenByItems);
            AddConfig("HiddenDisplayName", general, "The hidden Player's display name (string).",
                true, "???", ref CE_HiddenDisplayName);
            AddConfig("HideNameInChat", general, "When hidden also hides the name in chat (boolean).",
                true, true, ref CE_HideNameInChat);
            AddConfig("HidePlatformTag", general, "When hidden also hides steam/xbox platform tags (boolean).",
                true, false, ref CE_HidePlatformTag);

            #endregion

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
            FileSystemWatcher watcher = new(BepInEx.Paths.ConfigPath, ConfigFileName);
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