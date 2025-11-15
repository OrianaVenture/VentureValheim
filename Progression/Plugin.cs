using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Managers;

namespace VentureValheim.Progression;

[BepInDependency(Jotunn.Main.ModGuid)]
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
    private const string ModVersion = "0.3.9";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;
    private static string ConfigFileName = ModGUID + ".cfg";
    private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource VentureProgressionLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    #region ConfigurationEntries

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
    public static ConfigEntry<bool> CE_AdminBypass = null!;
    public static ConfigEntry<bool> CE_UseBlockedActionMessage = null!;
    public static ConfigEntry<string> CE_BlockedActionMessage = null!;
    public static ConfigEntry<bool> CE_UseBlockedActionEffect = null!;
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
    public static ConfigEntry<bool> CE_LockEating = null!;
    public static ConfigEntry<string> CE_LockPortalsKey = null!;

    // Locking Teleporting Metals & Others
    public static ConfigEntry<string> CE_UnlockPortalCopperTinKey = null!;
    public static ConfigEntry<string> CE_UnlockPortalIronKey = null!;
    public static ConfigEntry<string> CE_UnlockPortalSilverKey = null!;
    public static ConfigEntry<string> CE_UnlockPortalBlackMetalKey = null!;
    public static ConfigEntry<string> CE_UnlockPortalFlametalKey = null!;
    public static ConfigEntry<string> CE_UnlockPortalDragonEggKey = null!;
    public static ConfigEntry<string> CE_UnlockPortalMistlandsKey = null!;
    public static ConfigEntry<string> CE_UnlockPortalAshlandsKey = null!;
    public static ConfigEntry<string> CE_UnlockPortalHildirChestsKey = null!;

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

    // Trader (Haldor) Configuration
    public static ConfigEntry<bool> CE_UnlockAllHaldorItems = null!;
    public static ConfigEntry<string> CE_HelmetYuleKey = null!;
    public static ConfigEntry<string> CE_HelmetDvergerKey = null!;
    public static ConfigEntry<string> CE_BeltStrengthKey = null!;
    public static ConfigEntry<string> CE_YmirRemainsKey = null!;
    public static ConfigEntry<string> CE_FishingRodKey = null!;
    public static ConfigEntry<string> CE_FishingBaitKey = null!;
    public static ConfigEntry<string> CE_ThunderstoneKey = null!;
    public static ConfigEntry<string> CE_ChickenEggKey = null!;
    public static ConfigEntry<string> CE_BarrelRingsKey = null!;

    // Hildir Configuration
    public static ConfigEntry<bool> CE_UnlockAllHildirItems = null!;
    public static ConfigEntry<string> CE_CryptItemsKey = null!;
    public static ConfigEntry<string> CE_CaveItemsKey = null!;
    public static ConfigEntry<string> CE_TowerItemsKey = null!;

    // Bog Witch Configuration
    public static ConfigEntry<bool> CE_UnlockAllBogWitchItems = null!;
    public static ConfigEntry<string> CE_ScytheHandleKey = null!;

    // Raids Configuration
    public static ConfigEntry<bool> CE_UsePrivateRaids = null!;

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

        const string keys = "Keys";
        const string locking = "Locking";
        const string portal = "PortalUnlocking";
        const string skills = "Skills";
        const string trader = "Trader";
        const string hildir = "Hildir";
        const string witch = "BogWitch";
        const string raids = "Raids";

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
            true, true, ref CE_UsePrivateKeys);
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
        AddConfig("AdminBypass", locking,
            "True to allow admins to bypass locking settings (boolean)",
            true, false, ref CE_AdminBypass);
        AddConfig("UseBlockedActionMessage", locking,
            "True to enable the blocked display message used in this mod (string).",
            true, true, ref CE_UseBlockedActionMessage);
        AddConfig("BlockedActionMessage", locking,
            "Generic blocked display message used in this mod (string).",
            true, "The Gods Reject You", ref CE_BlockedActionMessage);
        AddConfig("UseBlockedActionEffect", locking,
            "True to enable the blocked display effect (fire) used in this mod (string).",
            true, true, ref CE_UseBlockedActionEffect);
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
        AddConfig("LockEating", locking,
            "True to lock the ability to eat biome food materials based on keys. Uses private key if enabled, global key if not (boolean).",
            true, true, ref CE_LockEating);
        AddConfig("LockPortalsKey", locking,
            "Use this key to control player ability to use portals (ex: defeated_eikthyr). Leave blank to allow vanilla portal behavior (string).",
            true, "", ref CE_LockPortalsKey);

        // Portal Unlocking
        // TODO: Move LockPortalsKey to this section
        AddConfig("UnlockPortalCopperTinKey", portal,
            "Use this key to control player ability to teleport copper and tin. Leave blank to allow vanilla portal behavior (string).",
            true, "", ref CE_UnlockPortalCopperTinKey);
        AddConfig("UnlockPortalIronKey", portal,
            "Use this key to control player ability to teleport iron. Leave blank to allow vanilla portal behavior (string).",
            true, "", ref CE_UnlockPortalIronKey);
        AddConfig("UnlockPortalSilverKey", portal,
            "Use this key to control player ability to teleport silver. Leave blank to allow vanilla portal behavior (string).",
            true, "", ref CE_UnlockPortalSilverKey);
        AddConfig("UnlockPortalBlackMetalKey", portal,
            "Use this key to control player ability to teleport black metal. Leave blank to allow vanilla portal behavior (string).",
            true, "", ref CE_UnlockPortalBlackMetalKey);
        AddConfig("UnlockPortalFlametalKey", portal,
            "Use this key to control player ability to teleport flametal. Leave blank to allow vanilla portal behavior (string).",
            true, "", ref CE_UnlockPortalFlametalKey);
        AddConfig("UnlockPortalDragonEggKey", portal,
            "Use this key to control player ability to teleport dragon eggs. Leave blank to allow vanilla portal behavior (string).",
            true, "", ref CE_UnlockPortalDragonEggKey);
        AddConfig("UnlockPortalMistlandsKey", portal,
            "Use this key to control player ability to teleport MechanicalSpring and DvergrNeedle. Leave blank to allow vanilla portal behavior (string).",
            true, "", ref CE_UnlockPortalMistlandsKey);
        AddConfig("UnlockPortalAshlandsKey", portal,
            "Use this key to control player ability to teleport CharredCogwheel. Leave blank to allow vanilla portal behavior (string).",
            true, "", ref CE_UnlockPortalAshlandsKey);
        AddConfig("UnlockPortalHildirChestsKey", portal,
            "Use this key to control player ability to teleport Hildir's stolen chests. Leave blank to allow vanilla portal behavior (string).",
            true, "", ref CE_UnlockPortalHildirChestsKey);

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
        AddConfig("BarrelRingsKey", trader,
            "Custom key for unlocking the Barrel Rings. Leave blank to use default (string).",
            true, "", ref CE_BarrelRingsKey);

        // Hildir
        AddConfig("UnlockAllHildirItems", hildir,
            "True to remove the key check from Hildir entirely and unlock all items (boolean).",
            true, false, ref CE_UnlockAllHildirItems);
        AddConfig("CryptItemsKey", hildir,
            "Custom key for unlocking the Crypt Dungeon items. Leave blank to use default (string).",
            true, "", ref CE_CryptItemsKey);
        AddConfig("CaveItemsKey", hildir,
            "Custom key for unlocking the Cave Dungeon items. Leave blank to use default (string).",
            true, "", ref CE_CaveItemsKey);
        AddConfig("TowerItemsKey", hildir,
            "Custom key for unlocking the Tower Dungeon items. Leave blank to use default (string).",
            true, "", ref CE_TowerItemsKey);

        // Bog Witch
        AddConfig("UnlockAllBogWitchItems", witch,
            "True to remove the key check from Bog Witch entirely and unlock all items (boolean).",
            true, false, ref CE_UnlockAllBogWitchItems);
        AddConfig("ScytheHandleKey", witch,
            "Custom key for unlocking the Scythe Handle. Leave blank to use default (string).",
            true, "", ref CE_ScytheHandleKey);

        // Raids
        AddConfig("UsePrivateRaids", raids, "True to use this mod's raids feature when using private keys (bool).",
            true, true, ref CE_UsePrivateRaids);

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
            VentureProgressionLogger.LogInfo("Attempting to reload configuration...");
            Config.Reload();
        }
        catch
        {
            VentureProgressionLogger.LogError($"There was an issue loading {ConfigFileName}");
            return;
        }

        _lastReloadTime = now;

        KeyManager.Instance.UpdateAllConfigurations();
    }
}

public interface IProgressionConfiguration
{
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
    public bool GetAdminBypass();
    public bool GetUseBlockedActionMessage();
    public string GetBlockedActionMessage();
    public bool GetUseBlockedActionEffect();
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
    public bool GetLockEating();
    public string GetLockPortalsKey();

    // Portal Unlocking
    public string GetUnlockPortalCopperTinKey();
    public string GetUnlockPortalIronKey();
    public string GetUnlockPortalSilverKey();
    public string GetUnlockPortalBlackMetalKey();
    public string GetUnlockPortalFlametalKey();
    public string GetUnlockPortalDragonEggKey();
    public string GetUnlockPortalMistlandsKey();
    public string GetUnlockPortalAshlandsKey();
    public string GetUnlockPortalHildirChestsKey();

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
    public string GetBarrelRingsKey();

    // Hildir Configuration
    public bool GetUnlockAllHildirItems();
    public string GetCryptItemsKey();
    public string GetCaveItemsKey();
    public string GetTowerItemsKey();

    // Bog Witch Configuration
    public bool GetUnlockAllBogWitchItems();
    public string GetScytheHandleKey();

    // Raids Configuration
    public bool GetUsePrivateRaids();
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
    public bool GetAdminBypass() => ProgressionPlugin.CE_AdminBypass.Value;
    public bool GetUseBlockedActionMessage() => ProgressionPlugin.CE_UseBlockedActionMessage.Value;
    public string GetBlockedActionMessage() => ProgressionPlugin.CE_BlockedActionMessage.Value;
    public bool GetUseBlockedActionEffect() => ProgressionPlugin.CE_UseBlockedActionEffect.Value;
    public bool GetLockTaming()
    {
        if (Instance.GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
        {
            return false;
        }
        return ProgressionPlugin.CE_LockTaming.Value;
    }

    public string GetOverrideLockTamingDefaults() => ProgressionPlugin.CE_OverrideLockTamingDefaults.Value;

    public bool GetLockGuardianPower()
    {
        if (Instance.GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
        {
            return false;
        }
        return ProgressionPlugin.CE_LockGuardianPower.Value;
    }

    public bool GetLockBossSummons()
    {
        if (Instance.GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
        {
            return false;
        }
        return ProgressionPlugin.CE_LockBossSummons.Value;
    }

    public string GetOverrideLockBossSummonsDefaults() => ProgressionPlugin.CE_OverrideLockBossSummonsDefaults.Value;
    public bool GetUnlockBossSummonsOverTime() => ProgressionPlugin.CE_UnlockBossSummonsOverTime.Value;
    public int GetUnlockBossSummonsTime() => ProgressionPlugin.CE_UnlockBossSummonsTime.Value;

    public bool GetLockEquipment()
    {
        if (Instance.GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
        {
            return false;
        }

        return ProgressionPlugin.CE_LockEquipment.Value;
    }

    public bool GetLockCrafting()
    {
        if (Instance.GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
        {
            return false;
        }

        return ProgressionPlugin.CE_LockCrafting.Value;
    }

    public bool GetLockBuilding()
    {
        if (Instance.GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
        {
            return false;
        }

        return ProgressionPlugin.CE_LockBuilding.Value;
    }

    public bool GetLockCooking()
    {
        if (Instance.GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
        {
            return false;
        }

        return ProgressionPlugin.CE_LockCooking.Value;
    }

    public bool GetLockEating()
    {
        if (Instance.GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
        {
            return false;
        }

        return ProgressionPlugin.CE_LockEating.Value;
    }

    public string GetLockPortalsKey()
    {
        if (Instance.GetAdminBypass() && SynchronizationManager.Instance.PlayerIsAdmin)
        {
            return "";
        }

        return ProgressionPlugin.CE_LockPortalsKey.Value;
    }

    // Portal Unlocking
    public string GetUnlockPortalCopperTinKey() => ProgressionPlugin.CE_UnlockPortalCopperTinKey.Value;
    public string GetUnlockPortalIronKey() => ProgressionPlugin.CE_UnlockPortalIronKey.Value;
    public string GetUnlockPortalSilverKey() => ProgressionPlugin.CE_UnlockPortalSilverKey.Value;
    public string GetUnlockPortalBlackMetalKey() => ProgressionPlugin.CE_UnlockPortalBlackMetalKey.Value;
    public string GetUnlockPortalFlametalKey() => ProgressionPlugin.CE_UnlockPortalFlametalKey.Value;
    public string GetUnlockPortalDragonEggKey() => ProgressionPlugin.CE_UnlockPortalDragonEggKey.Value;
    public string GetUnlockPortalMistlandsKey() => ProgressionPlugin.CE_UnlockPortalMistlandsKey.Value;
    public string GetUnlockPortalAshlandsKey() => ProgressionPlugin.CE_UnlockPortalAshlandsKey.Value;
    public string GetUnlockPortalHildirChestsKey() => ProgressionPlugin.CE_UnlockPortalHildirChestsKey.Value;

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
    public string GetBarrelRingsKey() => ProgressionPlugin.CE_BarrelRingsKey.Value;

    // Hildir
    public bool GetUnlockAllHildirItems() => ProgressionPlugin.CE_UnlockAllHildirItems.Value;
    public string GetCryptItemsKey() => ProgressionPlugin.CE_CryptItemsKey.Value;
    public string GetCaveItemsKey() => ProgressionPlugin.CE_CaveItemsKey.Value;
    public string GetTowerItemsKey() => ProgressionPlugin.CE_TowerItemsKey.Value;

    // Bog Witch
    public bool GetUnlockAllBogWitchItems() => ProgressionPlugin.CE_UnlockAllBogWitchItems.Value;
    public string GetScytheHandleKey() => ProgressionPlugin.CE_ScytheHandleKey.Value;

    // Raids
    public bool GetUsePrivateRaids() => ProgressionPlugin.CE_UsePrivateRaids.Value;
}