using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;

namespace VentureValheim.MiningCaves
{
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public class MiningCavesPlugin : BaseUnityPlugin
    {
        private const string ModName = "MiningCaves";
        private const string ModVersion = "0.1.1";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony HarmonyInstance = new(ModGUID);
        public static readonly ManualLogSource MiningCavesLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        #region ConfigurationEntries
        public static ConfigEntry<bool> CE_AdminBypass = null!;
        public static ConfigEntry<bool> CE_LockTerrain = null!;
        public static ConfigEntry<string> CE_LockTerrainIgnoreItems = null!;

        public static bool GetLockTerrain()
        {
            if (CE_AdminBypass.Value && SynchronizationManager.Instance.PlayerIsAdmin)
            {
                return false;
            }

            return CE_LockTerrain.Value;
        }

        public static string GetLockTerrainIgnoreItems() => CE_LockTerrainIgnoreItems.Value;

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
            const string general = "General";

            AddConfig("AdminBypass", general,
                "True to allow admins to bypass settings (boolean)",
                true, false, ref CE_AdminBypass);
            AddConfig("LockTerrain", general,
                "True to restrict actions that would alter the terrain such as mining or flattening (boolean)",
                true, false, ref CE_LockTerrain);
            AddConfig("LockTerrainIgnoreItems", general,
                "Prefab names of items to ignore for the terrain locking feature (comma-separated)",
                true, "", ref CE_LockTerrainIgnoreItems);

            ZoneManager.OnVanillaLocationsAvailable += CaveManager.AddMiningCaves;

            MiningCavesLogger.LogInfo("So you're mining stuff to craft with and crafting stuff to mine with?");
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
                MiningCavesLogger.LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                MiningCavesLogger.LogError($"There was an issue loading {ConfigFileName}");
            }
        }
    }
}