using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Managers;

namespace VentureValheim.MultiplayerTweaks
{
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class MultiplayerTweaksPlugin : BaseUnityPlugin
    {
        private const string ModName = "MultiplayerTweaks";
        private const string ModVersion = "0.11.2";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        
        private readonly Harmony HarmonyInstance = new(ModGUID);
        
        public static readonly ManualLogSource MultiplayerTweaksLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        // General
        internal static ConfigEntry<bool> CE_AdminBypass = null!;
        internal static ConfigEntry<int> CE_GameDayOffset = null!;
        internal static ConfigEntry<bool> CE_OverridePlayerPVP = null!;
        internal static ConfigEntry<bool> CE_ForcePlayerPVPOn = null!;
        internal static ConfigEntry<bool> CE_TeleportOnAnyDeath = null!;
        internal static ConfigEntry<bool> CE_TeleportOnPVPDeath = null!;
        internal static ConfigEntry<bool> CE_SkillLossOnAnyDeath = null!;
        internal static ConfigEntry<bool> CE_SkillLossOnPVPDeath = null!;
        internal static ConfigEntry<bool> CE_HidePlatformTag = null!;
        public static bool GetAdminBypass() => CE_AdminBypass.Value;
        public static int GetGameDayOffset() => CE_GameDayOffset.Value;
        public static bool GetOverridePlayerPVP()
        {
            if (GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
            {
                return false;
            }

            return CE_OverridePlayerPVP.Value;
        }
        public static bool GetForcePlayerPVPOn() => CE_ForcePlayerPVPOn.Value;
        public static bool GetTeleportOnAnyDeath() => CE_TeleportOnAnyDeath.Value;
        public static bool GetTeleportOnPVPDeath() => CE_TeleportOnPVPDeath.Value;
        public static bool GetSkillLossOnAnyDeath() => CE_SkillLossOnAnyDeath.Value;
        public static bool GetSkillLossOnPVPDeath() => CE_SkillLossOnPVPDeath.Value;
        public static bool GetHidePlatformTag() => CE_HidePlatformTag.Value;

        // Arrival
        internal static ConfigEntry<string> CE_PlayerDefaultSpawnPoint = null!;
        internal static ConfigEntry<bool> CE_EnableValkrie = null!;
        internal static ConfigEntry<bool> CE_EnableArrivalMessage = null!;
        internal static ConfigEntry<bool> CE_EnableArrivalMessageShout = null!;
        internal static ConfigEntry<string> CE_OverrideArrivalMessage = null!;
        public static string GetPlayerDefaultSpawnPoint() => CE_PlayerDefaultSpawnPoint.Value;
        public static bool GetEnableValkrie() => CE_EnableValkrie.Value;
        public static bool GetEnableArrivalMessage() => CE_EnableArrivalMessage.Value;
        public static bool GetEnableArrivalMessageShout() => CE_EnableArrivalMessageShout.Value;
        public static string GetOverrideArrivalMessage() => CE_OverrideArrivalMessage.Value;

        // Map
        internal static ConfigEntry<bool> CE_EnableTempleMapPin = null!;
        internal static ConfigEntry<bool> CE_EnableHaldorMapPin = null!;
        internal static ConfigEntry<bool> CE_EnableHildirMapPin = null!;
        internal static ConfigEntry<bool> CE_OverridePlayerMapPins = null!;
        internal static ConfigEntry<bool> CE_ForcePlayerMapPinsOn = null!;
        internal static ConfigEntry<bool> CE_AllowMapPings = null!;
        internal static ConfigEntry<bool> CE_AllowShoutPings = null!;
        public static bool GetEnableTempleMapPin()
        {
            if (GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
            {
                return true;
            }

            return CE_EnableTempleMapPin.Value;
        }
        public static bool GetEnableHaldorMapPin()
        {
            if (GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
            {
                return true;
            }

            return CE_EnableHaldorMapPin.Value;
        }
        public static bool GetEnableHildirMapPin()
        {
            if (GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
            {
                return true;
            }

            return CE_EnableHildirMapPin.Value;
        }
        public static bool GetOverridePlayerMapPins()
        {
            if (GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
            {
                return false;
            }

            return CE_OverridePlayerMapPins.Value;
        }
        public static bool GetForcePlayerMapPinsOn() => CE_ForcePlayerMapPinsOn.Value;
        public static bool GetAllowMapPings()
        {
            if (GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
            {
                return true;
            }

            return CE_AllowMapPings.Value;
        }
        public static bool GetAllowShoutPings()
        {
            if (GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
            {
                return true;
            }

            return CE_AllowShoutPings.Value;
        }

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
            const string map = "Map";
            const string arrival = "Arrival";

            AddConfig("AdminBypass", general, "True to allow admins to bypass some setting restrictions (boolean).",
                true, false, ref CE_AdminBypass);
            AddConfig("GameDayOffset", general, "Number to offset the game day display (int).",
                true, 0, ref CE_GameDayOffset);
            AddConfig("OverridePlayerPVP", general, "Override Player pvp behavior (boolean).",
                true, false, ref CE_OverridePlayerPVP);
            AddConfig("ForcePlayerPVPOn", general, "True to set pvp always on when OverridePlayerPVP is True (boolean).",
                true, true, ref CE_ForcePlayerPVPOn);
            AddConfig("TeleportOnAnyDeath", general, "False to respawn players at their graves on any death (boolean).",
                true, true, ref CE_TeleportOnAnyDeath);
            AddConfig("TeleportOnPVPDeath", general, "False to respawn players at their graves on a PVP death (boolean).",
                true, true, ref CE_TeleportOnPVPDeath);
            AddConfig("SkillLossOnAnyDeath", general, "False to prevent skill loss on any death (boolean).",
                true, true, ref CE_SkillLossOnAnyDeath);
            AddConfig("SkillLossOnPVPDeath", general, "False to prevent skill loss on a PVP death (boolean).",
                true, true, ref CE_SkillLossOnPVPDeath);
            AddConfig("HidePlatformTag", general, "When true hides steam/xbox platform tags from pings and chat messages (boolean).",
                true, false, ref CE_HidePlatformTag);

            AddConfig("PlayerDefaultSpawnPoint", arrival, "Coordinates for the default player spawn point (x,y,z) no parentheses, leave empty to use game default (comma-separated floats).",
                true, "", ref CE_PlayerDefaultSpawnPoint);
            AddConfig("EnableValkrie", arrival, "True to enable Valkrie Intro (boolean).",
                true, true, ref CE_EnableValkrie);
            AddConfig("EnableArrivalMessage", arrival, "True to enable Arrival Message on player login (boolean).",
                true, true, ref CE_EnableArrivalMessage);
            AddConfig("UseArrivalShout", arrival, "False to use a Normal message when EnableArrivalMessage is True (boolean).",
                true, true, ref CE_EnableArrivalMessageShout);
            AddConfig("OverrideArrivalMessage", arrival, "Set a new arrival message, leave blank to use default (string).",
                true, "", ref CE_OverrideArrivalMessage);

            AddConfig("EnableTempleMapPin", map, "False to hide Starting Temple map pin on Minimap (boolean).",
                true, true, ref CE_EnableTempleMapPin);
            AddConfig("EnableHaldorMapPin", map, "False to hide Haldor map pin on Minimap (boolean).",
                true, true, ref CE_EnableHaldorMapPin);
            AddConfig("EnableHildirMapPin", map, "False to hide Hildir map pin on Minimap (boolean).",
                true, true, ref CE_EnableHildirMapPin);
            AddConfig("OverridePlayerMapPositions", map, "Override Player map pin position behavior for Minimap (boolean).",
                true, false, ref CE_OverridePlayerMapPins);
            AddConfig("ForcePlayerMapPositionOn", map, "True to always show Player position on Minimap when OverridePlayerMapPositions is True (boolean).",
                true, true, ref CE_ForcePlayerMapPinsOn);
            AddConfig("AllowMapPings", map, "False to disable pings on the map from players (boolean).",
                true, true, ref CE_AllowMapPings);
            AddConfig("AllowShoutPings", map, "False to disable pings on the map when players shout messages (boolean).",
                true, true, ref CE_AllowShoutPings);

            #endregion

            MultiplayerTweaksLogger.LogInfo("Watch me Tweak, now watch me Neigh Neigh.");

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
                MultiplayerTweaksLogger.LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                MultiplayerTweaksLogger.LogError($"There was an issue loading {ConfigFileName}");
            }
        }
    }
}