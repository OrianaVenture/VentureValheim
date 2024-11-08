using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.FarmGrid
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class FarmGridPlugin : BaseUnityPlugin
    {
        private const string ModName = "VentureFarmGrid";
        private const string ModVersion = "0.1.1";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource FarmGridLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        private static ConfigEntry<int> CE_FarmGridFixedPartitions = null!;
        private static ConfigEntry<float> CE_PlantSpacing = null!;
        private static ConfigEntry<int> CE_FarmGridSections = null!;
        private static ConfigEntry<float> CE_FarmGridYOffset = null!;
        private static ConfigEntry<Color> CE_FarmGridColor = null!;
        private static ConfigEntry<string> CE_CustomPlants = null!;

        public static int GetFarmGridFixedPartitions() => CE_FarmGridFixedPartitions.Value;
        public static float GetExtraPlantSpacing() => CE_PlantSpacing.Value;

        public static int GetFarmGridSections()
        {
            return Math.Max(0, Math.Min(CE_FarmGridSections.Value, 10));
        }

        public static float GetFarmGridYOffset() => CE_FarmGridYOffset.Value;
        public static Color GetFarmGridColor() => CE_FarmGridColor.Value;
        public static string GetCustomPlants() => CE_CustomPlants.Value;

        private void AddConfig<T>(string key, string section, string description, bool synced, T value, ref ConfigEntry<T> configEntry)
        {
            string extendedDescription = GetExtendedDescription(description, synced);
            configEntry = Config.Bind(section, key, value, new ConfigDescription(extendedDescription, null));
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
            const string advanced = "Advanced";

            AddConfig("FarmGridFixedPartitions", general, "Fixed number partitions for initial snapping angles. " +
                "Vanilla uses 16 for build pieces, 0 allows free rotation (int).",
                false, 16, ref CE_FarmGridFixedPartitions);
            AddConfig("PlantSpacing", general, "Extra spacing to add to all plants in addition to the plant's needed growth space (float).",
                false, 0.01f, ref CE_PlantSpacing);
            AddConfig("FarmGridSections", general, "Amount of grid squares to draw from the center (limited to 10 maximum) (int).",
                false, 2, ref CE_FarmGridSections);
            AddConfig("FarmGridYOffset", general, "Grid offset from the ground (float).",
                false, 0.1f, ref CE_FarmGridYOffset);
            AddConfig("FarmGridColor", general, "Color of the grid (Color).",
                false, new Color(0.8f, 0.8f, 0.8f, 0.2f), ref CE_FarmGridColor);

            AddConfig("CustomPlants", advanced, "Additional non-Plant prefabs and default overrides " +
                "(Example: RaspberryBush: 0.5, BlueberryBush: 0.5) (string in format: Prefab:float,Prefab:float).",
                false, "", ref CE_CustomPlants);

            #endregion

            FarmGridLogger.LogInfo("Snap. Crackle. Pop.");
            FarmGrid.SetupConfigurations();

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
            _lastReloadTime = DateTime.Now;
            FileSystemWatcher watcher = new(BepInEx.Paths.ConfigPath, ConfigFileName);
            // Due to limitations of technology this can trigger twice in a row
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private DateTime _lastReloadTime;
        private const long RELOAD_DELAY = 10000000; // One second

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            var now = DateTime.Now;
            var time = now.Ticks - _lastReloadTime.Ticks;
            if (!File.Exists(ConfigFileFullPath) || time < RELOAD_DELAY) return;

            try
            {
                FarmGridLogger.LogInfo("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                FarmGridLogger.LogError($"There was an issue loading {ConfigFileName}");
                return;
            }

            _lastReloadTime = now;

            FarmGrid.SetupConfigurations();
        }
    }
}