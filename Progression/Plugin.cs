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
        private const string ModVersion = "0.0.29";
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
        // TODO public static ConfigEntry<bool> CE_AdminBypass = null!;

        // Key Manager
        public static ConfigEntry<bool> CE_BlockAllGlobalKeys = null!;
        public static ConfigEntry<string> CE_BlockedGlobalKeys = null!;
        public static ConfigEntry<string> CE_AllowedGlobalKeys = null!;
        public static ConfigEntry<string> CE_EnforcedGlobalKeys = null!;
        public static ConfigEntry<bool> CE_UsePrivateKeys = null!;
        public static ConfigEntry<string> CE_BlockedPrivateKeys = null!;
        public static ConfigEntry<string> CE_AllowedPrivateKeys = null!;
        public static ConfigEntry<string> CE_EnforcedPrivateKeys = null!;

        // Locking
        public static ConfigEntry<bool> CE_UseBlockedActionMessage = null!;
        public static ConfigEntry<string> CE_BlockedActionMessage = null!;
        public static ConfigEntry<bool> CE_LockTaming = null!;
        public static ConfigEntry<string> CE_OverrideLockTamingDefaults = null!;
        public static ConfigEntry<bool> CE_LockGuardianPower = null!;
        public static ConfigEntry<bool> CE_LockBossSummons = null!;
        public static ConfigEntry<string> CE_OverrideLockBossSummonsDefaults = null!;
        public static ConfigEntry<bool> CE_UnlockBossSummonsOverTime = null!;
        public static ConfigEntry<int> CE_UnlockBossSummonsTime = null!;
        public static ConfigEntry<bool> CE_LockEquipment = null!;
        public static ConfigEntry<bool> CE_LockCrafting = null!;
        public static ConfigEntry<bool> CE_LockBuilding = null!;
        public static ConfigEntry<bool> CE_LockCooking = null!;

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
        public static ConfigEntry<bool> CE_UseBossKeysForSkillLevel = null!;
        public static ConfigEntry<int> CE_BossKeysSkillPerKey = null!;

        // Trader Configuration
        public static ConfigEntry<bool> CE_UnlockAllHaldorItems = null!;
        public static ConfigEntry<string> CE_HelmetYuleKey = null!;
        public static ConfigEntry<string> CE_HelmetDvergerKey = null!;
        public static ConfigEntry<string> CE_BeltStrengthKey = null!;
        public static ConfigEntry<string> CE_YmirRemainsKey = null!;
        public static ConfigEntry<string> CE_FishingRodKey = null!;
        public static ConfigEntry<string> CE_FishingBaitKey = null!;
        public static ConfigEntry<string> CE_ThunderstoneKey = null!;
        public static ConfigEntry<string> CE_ChickenEggKey = null!;

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
            const string locking = "Locking";
            const string skills = "Skills";
            const string trader = "Trader";

            AddConfig("Force Server Config", general, "Force Server Config (boolean)",
                true, true, ref CE_ServerConfigLocked);
            ConfigurationSync.AddLockingConfigEntry(CE_ServerConfigLocked);
            // TODO AddConfig("AdminBypass", general,
                //"True to allow admins to bypass locking settings (boolean)",
                //false, true, ref CE_AdminBypass);

            // Keys
            AddConfig("BlockAllGlobalKeys", keys,
                "True to stop all global keys from being added to the global list (boolean).",
                true, true, ref CE_BlockAllGlobalKeys);
            AddConfig("BlockedGlobalKeys", keys,
                "Stop only these keys being added to the global list when BlockAllGlobalKeys is false (comma-separated).",
                true, "", ref CE_BlockedGlobalKeys);
            AddConfig("AllowedGlobalKeys", keys,
                "Allow only these keys being added to the global list when BlockAllGlobalKeys is true (comma-separated).",
                true, "", ref CE_AllowedGlobalKeys);
            AddConfig("EnforcedGlobalKeys", keys,
                "Always add these keys to the global list on startup (comma-separated).",
                true, "", ref CE_EnforcedGlobalKeys);
            AddConfig("UsePrivateKeys", keys,
                "True to use private player keys to control game behavior (boolean).",
                true, false, ref CE_UsePrivateKeys);
            AddConfig("BlockedPrivateKeys", keys,
                "Stop only these keys being added to the player's key list when UsePrivateKeys is true (comma-separated).",
                true, "", ref CE_BlockedPrivateKeys);
            AddConfig("AllowedPrivateKeys", keys,
                "Allow only these keys being added to the player's key list when UsePrivateKeys is true (comma-separated).",
                true, "", ref CE_AllowedPrivateKeys);
            AddConfig("EnforcedPrivateKeys", keys,
                "Always add these keys to the player's private list on startup (comma-separated).",
                true, "", ref CE_EnforcedPrivateKeys);

            // Locking
            AddConfig("UseBlockedActionMessage", locking,
                "True to enable the blocked display message used in this mod (string).",
                true, true, ref CE_UseBlockedActionMessage);
            AddConfig("BlockedActionMessage", locking,
                "Generic blocked display message used in this mod (string).",
                true, "The Gods Reject You", ref CE_BlockedActionMessage);
            AddConfig("LockTaming", locking,
                "True to lock the ability to tame creatures based on keys. Uses private key if enabled, global key if not (boolean).",
                true, false, ref CE_LockTaming);
            AddConfig("OverrideLockTamingDefaults", locking,
                "Override keys needed to Tame creatures. Leave blank to use defaults (comma-separated prefab,key pairs).",
                true, "", ref CE_OverrideLockTamingDefaults);
            AddConfig("LockGuardianPower", locking,
                "True to lock the ability to get and use guardian powers based on keys. Uses private key if enabled, global key if not (boolean).",
                true, true, ref CE_LockGuardianPower);
            AddConfig("LockBossSummons", locking,
                "True to lock the ability to spawn bosses based on keys. Uses private key if enabled, global key if not (boolean).",
                true, true, ref CE_LockBossSummons);
            AddConfig("OverrideLockBossSummonsDefaults", locking,
                "Override keys needed to summon bosses. Leave blank to use defaults (comma-separated prefab,key pairs).",
                true, "", ref CE_OverrideLockBossSummonsDefaults);
            AddConfig("UnlockBossSummonsOverTime", locking,
                "True to unlock the ability to spawn bosses based on in-game days passed when LockBossSummons is True (boolean).",
                true, false, ref CE_UnlockBossSummonsOverTime);
            AddConfig("UnlockBossSummonsTime", locking,
                "Number of in-game days required to unlock the next boss in the sequence when UnlockBossSummonsOverTime is True (int).",
                true, 100, ref CE_UnlockBossSummonsTime);
            AddConfig("LockEquipment", locking,
                "True to lock the ability to equip or use boss items or items made from biome metals/materials based on keys. Uses private key if enabled, global key if not (boolean).",
                true, true, ref CE_LockEquipment);
            AddConfig("LockCrafting", locking,
                "True to lock the ability to craft items based on boss items and biome metals/materials and keys. Uses private key if enabled, global key if not (boolean).",
                true, true, ref CE_LockCrafting);
            AddConfig("LockBuilding", locking,
                "True to lock the ability to build based on boss items and biome metals/materials and keys. Uses private key if enabled, global key if not (boolean).",
                true, true, ref CE_LockBuilding);
            AddConfig("LockCooking", locking,
                "True to lock the ability to cook with biome food materials based on keys. Uses private key if enabled, global key if not (boolean).",
                true, true, ref CE_LockCooking);

            // Skills
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
            AddConfig("UseBossKeysForSkillLevel", skills,
                "True to use unlocked boss keys to control skill floor/ceiling values (boolean).",
                true, false, ref CE_UseBossKeysForSkillLevel);
            AddConfig("BossKeysSkillPerKey", skills,
                "Skill drain floor and skill gain ceiling increased this amount per boss defeated (boolean).",
                true, 10, ref CE_BossKeysSkillPerKey);

            // Trader
            AddConfig("UnlockAllHaldorItems", trader,
                "True to remove the key check from Haldor entirely and unlock all items (boolean).",
                true, false, ref CE_UnlockAllHaldorItems);
            AddConfig("HelmetYuleKey", trader,
                "Custom key for unlocking the Yule Hat. Leave blank to use default (string).",
                true, "", ref CE_HelmetYuleKey);
            AddConfig("HelmetDvergerKey", trader,
                "Custom key for unlocking the Dverger Circlet. Leave blank to use default (string).",
                true, "", ref CE_HelmetDvergerKey);
            AddConfig("BeltStrengthKey", trader,
                "Custom key for unlocking the Megingjord. Leave blank to use default (string).",
                true, "", ref CE_BeltStrengthKey);
            AddConfig("YmirRemainsKey", trader,
                "Custom key for unlocking Ymir Flesh. Leave blank to use default (string).",
                true, "", ref CE_YmirRemainsKey);
            AddConfig("FishingRodKey", trader,
                "Custom key for unlocking the Fishing Rod. Leave blank to use default (string).",
                true, "", ref CE_FishingRodKey);
            AddConfig("FishingBaitKey", trader,
                "Custom key for unlocking Fishing Bait. Leave blank to use default (string).",
                true, "", ref CE_FishingBaitKey);
            AddConfig("ThunderstoneKey", trader,
                "Custom key for unlocking the Thunder Stone. Leave blank to use default (string).",
                true, "", ref CE_ThunderstoneKey);
            AddConfig("ChickenEggKey", trader,
                "Custom key for unlocking the Egg. Leave blank to use default (string).",
                true, "", ref CE_ChickenEggKey);

            #endregion

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
        // TODO public bool GetAdminBypass();

        // Key Manager
        public bool GetBlockAllGlobalKeys();
        public string GetBlockedGlobalKeys();
        public string GetAllowedGlobalKeys();
        public string GetEnforcedGlobalKeys();
        public bool GetUsePrivateKeys();
        public string GetBlockedPrivateKeys();
        public string GetAllowedPrivateKeys();
        public string GetEnforcedPrivateKeys();

        // Locking
        public bool GetUseBlockedActionMessage();
        public string GetBlockedActionMessage();
        public bool GetLockTaming();
        public string GetOverrideLockTamingDefaults();
        public bool GetLockGuardianPower();
        public bool GetLockBossSummons();
        public string GetOverrideLockBossSummonsDefaults();
        public bool GetUnlockBossSummonsOverTime();
        public int GetUnlockBossSummonsTime();
        public bool GetLockEquipment();
        public bool GetLockCrafting();
        public bool GetLockBuilding();
        public bool GetLockCooking();

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
        public bool GetUseBossKeysForSkillLevel();
        public int GetBossKeysSkillPerKey();

        // Trader Configuration
        public bool GetUnlockAllHaldorItems();
        public string GetHelmetYuleKey();
        public string GetHelmetDvergerKey();
        public string GetBeltStrengthKey();
        public string GetYmirRemainsKey();
        public string GetFishingRodKey();
        public string GetFishingBaitKey();
        public string GetThunderstoneKey();
        public string GetChickenEggKey();
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

        // TODO public bool GetAdminBypass() => ProgressionPlugin.CE_AdminBypass.Value;

        // Keys
        public bool GetBlockAllGlobalKeys() => ProgressionPlugin.CE_BlockAllGlobalKeys.Value;
        public string GetBlockedGlobalKeys() => ProgressionPlugin.CE_BlockedGlobalKeys.Value;
        public string GetAllowedGlobalKeys() => ProgressionPlugin.CE_AllowedGlobalKeys.Value;
        public string GetEnforcedGlobalKeys() => ProgressionPlugin.CE_EnforcedGlobalKeys.Value;
        public bool GetUsePrivateKeys() => ProgressionPlugin.CE_UsePrivateKeys.Value;
        public string GetBlockedPrivateKeys() => ProgressionPlugin.CE_BlockedPrivateKeys.Value;
        public string GetAllowedPrivateKeys() => ProgressionPlugin.CE_AllowedPrivateKeys.Value;
        public string GetEnforcedPrivateKeys() => ProgressionPlugin.CE_EnforcedPrivateKeys.Value;

        // Locking
        public bool GetUseBlockedActionMessage() => ProgressionPlugin.CE_UseBlockedActionMessage.Value;
        public string GetBlockedActionMessage() => ProgressionPlugin.CE_BlockedActionMessage.Value;
        public bool GetLockTaming() => ProgressionPlugin.CE_LockTaming.Value;
        public string GetOverrideLockTamingDefaults() => ProgressionPlugin.CE_OverrideLockTamingDefaults.Value;
        public bool GetLockGuardianPower() => ProgressionPlugin.CE_LockGuardianPower.Value;
        public bool GetLockBossSummons() => ProgressionPlugin.CE_LockBossSummons.Value;
        public string GetOverrideLockBossSummonsDefaults() => ProgressionPlugin.CE_OverrideLockBossSummonsDefaults.Value;
        public bool GetUnlockBossSummonsOverTime() => ProgressionPlugin.CE_UnlockBossSummonsOverTime.Value;
        public int GetUnlockBossSummonsTime() => ProgressionPlugin.CE_UnlockBossSummonsTime.Value;
        public bool GetLockEquipment() => ProgressionPlugin.CE_LockEquipment.Value;
        public bool GetLockCrafting() => ProgressionPlugin.CE_LockCrafting.Value;
        public bool GetLockBuilding() => ProgressionPlugin.CE_LockBuilding.Value;
        public bool GetLockCooking() => ProgressionPlugin.CE_LockCooking.Value;

        // Skills
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
        public bool GetUseBossKeysForSkillLevel() => ProgressionPlugin.CE_UseBossKeysForSkillLevel.Value;
        public int GetBossKeysSkillPerKey() => ProgressionPlugin.CE_BossKeysSkillPerKey.Value;

        // Trader
        public bool GetUnlockAllHaldorItems() => ProgressionPlugin.CE_UnlockAllHaldorItems.Value;
        public string GetHelmetYuleKey() => ProgressionPlugin.CE_HelmetYuleKey.Value;
        public string GetHelmetDvergerKey() => ProgressionPlugin.CE_HelmetDvergerKey.Value;
        public string GetBeltStrengthKey() => ProgressionPlugin.CE_BeltStrengthKey.Value;
        public string GetYmirRemainsKey() => ProgressionPlugin.CE_YmirRemainsKey.Value;
        public string GetFishingRodKey() => ProgressionPlugin.CE_FishingRodKey.Value;
        public string GetFishingBaitKey() => ProgressionPlugin.CE_FishingBaitKey.Value;
        public string GetThunderstoneKey() => ProgressionPlugin.CE_ThunderstoneKey.Value;
        public string GetChickenEggKey() => ProgressionPlugin.CE_ChickenEggKey.Value;
    }
}