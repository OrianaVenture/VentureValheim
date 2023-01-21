using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;

namespace VentureValheim.Progression
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ProgressionPlugin : BaseUnityPlugin
    {
        static ProgressionPlugin() { }
        private ProgressionPlugin() { }
        private static readonly ProgressionPlugin _instance = new ProgressionPlugin();

        public static ProgressionPlugin Instance
        {
            get => _instance;
        }

        private const string ModName = "WorldAdvancementProgression";
        private const string ModVersion = "0.0.20";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource VentureProgressionLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        private static readonly ConfigSync ConfigurationSync = new(ModGUID)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        private static ConfigEntry<bool> CE_ServerConfigLocked = null!;
        private static ConfigEntry<bool> CE_ModEnabled = null!;
        public static ConfigEntry<bool> CE_GenerateGameData = null!;

        // Progression Manager
        public static ConfigEntry<bool> CE_BlockAllGlobalKeys = null!;
        public static ConfigEntry<string> CE_BlockedGlobalKeys = null!;
        public static ConfigEntry<string> CE_AllowedGlobalKeys = null!;
        public static ConfigEntry<bool> CE_UseBossKeysForSkillLevel = null!;
        public static ConfigEntry<int> CE_BossKeysSkillPerKey = null!;
        public static ConfigEntry<bool> CE_UsePrivateKeys = null!;
        public static ConfigEntry<string> CE_BlockedPrivateKeys = null!;
        public static ConfigEntry<string> CE_AllowedPrivateKeys = null!;
        public static ConfigEntry<bool> CE_UnlockAllHaldorItems = null!;

        // Skills Manager
        public static ConfigEntry<bool> CE_EnableSkillManager = null!;
        public static ConfigEntry<bool> CE_AllowSkillDrain = null!;
        public static ConfigEntry<bool> CE_UseAbsoluteSkillDrain = null!;
        public static ConfigEntry<int> CE_AbsoluteSkillDrain = null!;
        public static ConfigEntry<bool> CE_CompareAndSelectDrain = null!;
        public static ConfigEntry<bool> CE_CompareUseMinimumDrain = null!;
        public static ConfigEntry<bool> CE_OverrideMaximumSkillLevel = null!;
        public static ConfigEntry<int> CE_MaximumSkillLevel = null!;
        public static ConfigEntry<bool> CE_OverrideMinimumSkillLevel = null!;
        public static ConfigEntry<int> CE_MinimumSkillLevel = null!;

        // Auto-Scaling Configuration
        public static ConfigEntry<bool> CE_AutoScaling = null!;
        public static ConfigEntry<string> CE_AutoScaleType = null!;
        public static ConfigEntry<float> CE_AutoScaleFactor = null!;
        public static ConfigEntry<bool> CE_AutoScaleCreatures = null!;
        public static ConfigEntry<string> CE_AutoScaleCreatureHealth = null!;
        public static ConfigEntry<string> CE_AutoScaleCreatureDamage = null!;
        public static ConfigEntry<bool> CE_AutoScaleCreaturesIgnoreDefaults = null!;
        public static ConfigEntry<bool> CE_AutoScaleItems = null!;
        public static ConfigEntry<bool> CE_AutoScaleItemsIgnoreDefaults = null!;

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
            const string keys = "Keys";
            const string skills = "Skills";
            const string autoScaling = "Auto-Scaling";

            AddConfig("Force Server Config", general, "Force Server Config (boolean)",
                true, true, ref CE_ServerConfigLocked);
            ConfigurationSync.AddLockingConfigEntry(CE_ServerConfigLocked);

            AddConfig("Enabled", general, "Enable module (boolean).",
                true, true, ref CE_ModEnabled);
            AddConfig("GenerateGameDataFiles", general, "Finds all items and creatures and creates data files in your config path for viewing only (boolean).",
                false, false, ref CE_GenerateGameData);

            AddConfig("BlockAllGlobalKeys", keys,
                "True to stop all global keys from being added to the global list (boolean).",
                true, true, ref CE_BlockAllGlobalKeys);
            AddConfig("BlockedGlobalKeys", keys,
                "Stop only these keys being added to the global list when BlockAllGlobalKeys is false (comma-separated).",
                true, "", ref CE_BlockedGlobalKeys);
            AddConfig("AllowedGlobalKeys", keys,
                "Allow only these keys being added to the global list when BlockAllGlobalKeys is true (comma-separated).",
                true, "", ref CE_AllowedGlobalKeys);
            AddConfig("UseBossKeysForSkillLevel", keys,
                "True to use private player boss keys to control skill floor/ceiling values (boolean).",
                true, false, ref CE_UseBossKeysForSkillLevel);
            AddConfig("BossKeysSkillPerKey", keys,
                "Skill drain floor and skill gain ceiling increased this amount per boss defeated (boolean).",
                true, 10, ref CE_BossKeysSkillPerKey);
            AddConfig("UsePrivateKeys", keys,
                "True to use private player keys to control game behavior (boolean).",
                true, false, ref CE_UsePrivateKeys);
            AddConfig("BlockedPrivateKeys", keys,
                "Stop only these keys being added to the player's key list when UsePrivateKeys is true (comma-separated).",
                true, "", ref CE_BlockedPrivateKeys);
            AddConfig("AllowedPrivateKeys", keys,
                "Allow only these keys being added to the player's key list when UsePrivateKeys is true (comma-separated).",
                true, "", ref CE_AllowedPrivateKeys);
            AddConfig("UnlockAllHaldorItems", keys,
                "True to remove the key check from Haldor entirely and unlock all items (boolean).",
                true, false, ref CE_UnlockAllHaldorItems);

            AddConfig("EnableSkillManager", skills,
                "Enable the Skill Manager feature (boolean).",
                true, true, ref CE_EnableSkillManager);
            AddConfig("AllowSkillDrain", skills,
                "Enable skill drain on death (boolean).",
                true, true, ref CE_AllowSkillDrain);
            AddConfig("UseAbsoluteSkillDrain", skills,
                "Reduce skills by a set number of levels (boolean).",
                true, false, ref CE_UseAbsoluteSkillDrain);
            AddConfig("AbsoluteSkillDrain", skills,
                "The number of levels (When UseAbsoluteSkillDrain is true) (int).",
                true, 1, ref CE_AbsoluteSkillDrain);
            AddConfig("CompareAndSelectDrain", skills,
                "Enable comparing skill drain original vs absolute value (When UseAbsoluteSkillDrain is true) (boolean).",
                true, false, ref CE_CompareAndSelectDrain);
            AddConfig("CompareUseMinimumDrain", skills,
                "True to use the smaller value (When CompareAndSelectDrain is true) (boolean).",
                true, true, ref CE_CompareUseMinimumDrain);
            AddConfig("OverrideMaximumSkillLevel", skills,
                "Override the maximum (ceiling) skill level for all skill gain (boolean).",
                true, false, ref CE_OverrideMaximumSkillLevel);
            AddConfig("MaximumSkillLevel", skills,
                "If overridden, the maximum (ceiling) skill level for all skill gain (int).",
                true, (int)SkillsManager.SKILL_MAXIMUM, ref CE_MaximumSkillLevel);
            AddConfig("OverrideMinimumSkillLevel", skills,
                "Override the minimum (floor) skill level for all skill loss (boolean).",
                true, false, ref CE_OverrideMinimumSkillLevel);
            AddConfig("MinimumSkillLevel", skills,
                "If overridden, the minimum (floor) skill level for all skill loss (int).",
                true, (int)SkillsManager.SKILL_MINIMUM, ref CE_MinimumSkillLevel);

            AddConfig("EnableAutoScaling", autoScaling,
                "Enabled the Auto-scaling feature (boolean).",
                true, false, ref CE_AutoScaling);
            AddConfig("AutoScaleType", autoScaling,
                "Auto-scaling type: Vanilla, Linear, or Exponential (string).",
                true, "Vanilla", ref CE_AutoScaleType);
            AddConfig("AutoScaleFactor", autoScaling,
                "Auto-scaling factor, 0.75 = 75% growth per biome \"difficulty order\" (float).",
                true, 0.75f, ref CE_AutoScaleFactor);
            AddConfig("AutoScaleCreatures", autoScaling,
                "Auto-scale Creatures (boolean).",
                true, true, ref CE_AutoScaleCreatures);
            AddConfig("AutoScaleCreaturesHealth", autoScaling,
                "Override the Base Health distribution for Creatures (comma-separated list of 6 integers) (string).",
                true, "", ref CE_AutoScaleCreatureHealth);
            AddConfig("AutoScaleCreaturesDamage", autoScaling,
                "Override the Base Damage distribution for Creatures (comma-separated list of 6 integers) (string).",
                true, "", ref CE_AutoScaleCreatureDamage);
            AddConfig("AutoScaleCreaturesIgnoreDefaults", autoScaling,
                "When True ignores ALL default classifications assigned by the mod, use to keep vanilla values unless specifically overridden in the yaml file (boolean).",
                true, false, ref CE_AutoScaleCreaturesIgnoreDefaults);
            AddConfig("AutoScaleItems", autoScaling,
                "Auto-scale Items (boolean).",
                true, true, ref CE_AutoScaleItems);
            AddConfig("AutoScaleItemsIgnoreDefaults", autoScaling,
                "When True ignores ALL default classifications assigned by the mod, use to keep vanilla values unless specifically overridden in the yaml file (boolean).",
                true, false, ref CE_AutoScaleItemsIgnoreDefaults);

            #endregion

            if (!CE_ModEnabled.Value)
                return;

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
                VentureProgressionLogger.LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                VentureProgressionLogger.LogError($"There was an issue loading {ConfigFileName}");
            }
        }
    }

    public interface IProgressionConfiguration
    {
        // General
        public bool GetGenerateGameData();

        // Key Manager
        public bool GetBlockAllGlobalKeys();
        public string GetBlockedGlobalKeys();
        public string GetAllowedGlobalKeys();
        public bool GetUseBossKeysForSkillLevel();
        public int GetBossKeysSkillPerKey();
        public bool GetUsePrivateKeys();
        public string GetBlockedPrivateKeys();
        public string GetAllowedPrivateKeys();
        public bool GetUnlockAllHaldorItems();

        // Skills Manager
        public bool GetEnableSkillManager();
        public bool GetAllowSkillDrain();
        public bool GetUseAbsoluteSkillDrain();
        public int GetAbsoluteSkillDrain();
        public bool GetCompareAndSelectDrain();
        public bool GetCompareUseMinimumDrain();
        public bool GetOverrideMaximumSkillLevel();
        public int GetMaximumSkillLevel();
        public bool GetOverrideMinimumSkillLevel();
        public int GetMinimumSkillLevel();

        // Auto-Scaling Configuration
        public bool GetUseAutoScaling();
        public string GetAutoScaleType();
        public float GetAutoScaleFactor();
        public bool GetAutoScaleCreatures();
        public string GetAutoScaleCreatureHealth();
        public string GetAutoScaleCreatureDamage();
        public bool GetAutoScaleCreaturesIgnoreDefaults();
        public bool GetAutoScaleItems();
        public bool GetAutoScaleItemsIgnoreDefaults();
    }

    public class ProgressionConfiguration : IProgressionConfiguration
    {
        static ProgressionConfiguration() { }

        public ProgressionConfiguration() { }
        public  ProgressionConfiguration(IProgressionConfiguration progressionConfiguration)
        {
            _instance = progressionConfiguration;
        }
        private static IProgressionConfiguration _instance = new ProgressionConfiguration();

        public static IProgressionConfiguration Instance
        {
            get => _instance;
        }

        // General
        public bool GetGenerateGameData() => ProgressionPlugin.CE_GenerateGameData.Value;

        // Key Manager
        public bool GetBlockAllGlobalKeys() => ProgressionPlugin.CE_BlockAllGlobalKeys.Value;
        public string GetBlockedGlobalKeys() => ProgressionPlugin.CE_BlockedGlobalKeys.Value;
        public string GetAllowedGlobalKeys() => ProgressionPlugin.CE_AllowedGlobalKeys.Value;
        public bool GetUseBossKeysForSkillLevel() => ProgressionPlugin.CE_UseBossKeysForSkillLevel.Value;
        public int GetBossKeysSkillPerKey() => ProgressionPlugin.CE_BossKeysSkillPerKey.Value;
        public bool GetUsePrivateKeys() => ProgressionPlugin.CE_UsePrivateKeys.Value;
        public string GetBlockedPrivateKeys() => ProgressionPlugin.CE_BlockedPrivateKeys.Value;
        public string GetAllowedPrivateKeys() => ProgressionPlugin.CE_AllowedPrivateKeys.Value;
        public bool GetUnlockAllHaldorItems() => ProgressionPlugin.CE_UnlockAllHaldorItems.Value;

        // Skills Manager
        public bool GetEnableSkillManager() => ProgressionPlugin.CE_EnableSkillManager.Value;
        public bool GetAllowSkillDrain() => ProgressionPlugin.CE_AllowSkillDrain.Value;
        public bool GetUseAbsoluteSkillDrain() => ProgressionPlugin.CE_UseAbsoluteSkillDrain.Value;
        public int GetAbsoluteSkillDrain() => ProgressionPlugin.CE_AbsoluteSkillDrain.Value;
        public bool GetCompareAndSelectDrain() => ProgressionPlugin.CE_CompareAndSelectDrain.Value;
        public bool GetCompareUseMinimumDrain() => ProgressionPlugin.CE_CompareUseMinimumDrain.Value;
        public bool GetOverrideMaximumSkillLevel() => ProgressionPlugin.CE_OverrideMaximumSkillLevel.Value;
        public int GetMaximumSkillLevel() => ProgressionPlugin.CE_MaximumSkillLevel.Value;
        public bool GetOverrideMinimumSkillLevel() => ProgressionPlugin.CE_OverrideMinimumSkillLevel.Value;
        public int GetMinimumSkillLevel() => ProgressionPlugin.CE_MinimumSkillLevel.Value;

        // Auto-Scaling Configuration
        public bool GetUseAutoScaling() => ProgressionPlugin.CE_AutoScaling.Value;
        public string GetAutoScaleType() => ProgressionPlugin.CE_AutoScaleType.Value;
        public float GetAutoScaleFactor() => ProgressionPlugin.CE_AutoScaleFactor.Value;
        public bool GetAutoScaleCreatures() => ProgressionPlugin.CE_AutoScaleCreatures.Value;
        public string GetAutoScaleCreatureHealth() => ProgressionPlugin.CE_AutoScaleCreatureHealth.Value;
        public string GetAutoScaleCreatureDamage() => ProgressionPlugin.CE_AutoScaleCreatureDamage.Value;
        public bool GetAutoScaleCreaturesIgnoreDefaults() => ProgressionPlugin.CE_AutoScaleCreaturesIgnoreDefaults.Value;
        public bool GetAutoScaleItems() => ProgressionPlugin.CE_AutoScaleItems.Value;
        public bool GetAutoScaleItemsIgnoreDefaults() => ProgressionPlugin.CE_AutoScaleItemsIgnoreDefaults.Value;
    }
}