using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.LocationReset;

[BepInDependency(Jotunn.Main.ModGuid)]
[BepInDependency(ModCompatibility.DungeonSplitterName, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModCompatibility.MVBPName, BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class LocationResetPlugin : BaseUnityPlugin
{
    static LocationResetPlugin() { }
    private LocationResetPlugin() { }
    private static readonly LocationResetPlugin _instance = new LocationResetPlugin();

    public static LocationResetPlugin Instance
    {
        get => _instance;
    }

    private const string ModName = "LocationReset";
    private const string ModVersion = "0.10.7";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;
    private static string ConfigFileName = ModGUID + ".cfg";
    private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource LocationResetLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    #region ConfigurationEntries

    private static ConfigEntry<int> CE_ResetTime = null!;
    private static ConfigEntry<bool> CE_SkipPlayerGroundPieceCheck = null!;
    private static ConfigEntry<string> CE_IgnoreList = null!;
    private static ConfigEntry<string> CE_AllowList = null!;

    private static ConfigEntry<bool> CE_ResetGroundLocations = null!;
    private static ConfigEntry<bool> CE_OverrideResetTimes = null!;
    private static ConfigEntry<int> CE_FarmResetTime = null!;
    private static ConfigEntry<int> CE_VillageResetTime = null!;
    private static ConfigEntry<int> CE_TrollResetTime = null!;
    private static ConfigEntry<int> CE_BurialResetTime = null!;
    private static ConfigEntry<int> CE_CryptResetTime = null!;
    private static ConfigEntry<int> CE_CaveResetTime = null!;
    private static ConfigEntry<int> CE_CampResetTime = null!;
    private static ConfigEntry<int> CE_MineResetTime = null!;
    private static ConfigEntry<int> CE_QueenResetTime = null!;
    private static ConfigEntry<int> CE_HildirBurialResetTime = null!;
    private static ConfigEntry<int> CE_HildirCaveResetTime = null!;
    private static ConfigEntry<int> CE_HildirTowerResetTime = null!;

    private static ConfigEntry<int> CE_CharredFortress = null!;
    private static ConfigEntry<int> CE_LeviathanLava = null!;
    private static ConfigEntry<int> CE_MorgenHole = null!;
    private static ConfigEntry<int> CE_PlaceofMystery = null!;

    private static ConfigEntry<int> CE_CopperTinCaveResetTime = null!;
    private static ConfigEntry<int> CE_SilverCaveResetTime = null!;

    private static ConfigEntry<bool> CE_EnableLeviathanReset = null!;
    private static ConfigEntry<int> CE_LeviathanResetTime = null!;

    public static readonly int Hash_StartTemple = "StartTemple".GetStableHashCode();

    private static readonly int Hash_TrollCave02 = "TrollCave02".GetStableHashCode();
    private static readonly int Hash_Village = "WoodVillage1".GetStableHashCode();
    private static readonly int Hash_Farm = "WoodFarm1".GetStableHashCode();
    private static readonly int Hash_Burial2 = "Crypt2".GetStableHashCode();
    private static readonly int Hash_Burial3 = "Crypt3".GetStableHashCode();
    private static readonly int Hash_Burial4 = "Crypt4".GetStableHashCode();
    private static readonly int Hash_Crypt = "SunkenCrypt4".GetStableHashCode();
    private static readonly int Hash_Cave = "MountainCave02".GetStableHashCode();
    private static readonly int Hash_Camp = "GoblinCamp2".GetStableHashCode();
    private static readonly int Hash_Mine1 = "Mistlands_DvergrTownEntrance1".GetStableHashCode();
    private static readonly int Hash_Mine2 = "Mistlands_DvergrTownEntrance2".GetStableHashCode();
    private static readonly int Hash_Queen = "Mistlands_DvergrBossEntrance1".GetStableHashCode();
    private static readonly int Hash_HildirBurial = "Hildir_crypt".GetStableHashCode();
    private static readonly int Hash_HildirCave = "Hildir_cave".GetStableHashCode();
    public static readonly int Hash_HildirTower = "Hildir_plainsfortress".GetStableHashCode();

    public static readonly int Hash_CharredFortress = "CharredFortress".GetStableHashCode();
    public static readonly int Hash_LeviathanLava = "LeviathanLava".GetStableHashCode();
    public static readonly int Hash_MorgenHole1 = "MorgenHole1".GetStableHashCode();
    public static readonly int Hash_MorgenHole2 = "MorgenHole2".GetStableHashCode();
    public static readonly int Hash_MorgenHole3 = "MorgenHole3".GetStableHashCode();
    public static readonly int Hash_PlaceofMystery1 = "PlaceofMystery1".GetStableHashCode();
    public static readonly int Hash_PlaceofMystery2 = "PlaceofMystery2".GetStableHashCode();
    public static readonly int Hash_PlaceofMystery3 = "PlaceofMystery3".GetStableHashCode();

    private static readonly int Hash_CopperTinCave = "VV_CopperTinCave".GetStableHashCode();
    private static readonly int Hash_SilverCave = "VV_SilverCave".GetStableHashCode();

    public static int GetResetTime(int hash)
    {
        if (CE_OverrideResetTimes.Value)
        {
            if (hash == Hash_TrollCave02)
            {
                return CE_TrollResetTime.Value;
            }
            else if (hash == Hash_Village)
            {
                return CE_VillageResetTime.Value;
            }
            else if (hash == Hash_Farm)
            {
                return CE_FarmResetTime.Value;
            }
            else if (hash == Hash_Burial2 || hash == Hash_Burial3 || hash == Hash_Burial4)
            {
                return CE_BurialResetTime.Value;
            }
            else if (hash == Hash_Crypt)
            {
                return CE_CryptResetTime.Value;
            }
            else if (hash == Hash_Cave)
            {
                return CE_CaveResetTime.Value;
            }
            else if (hash == Hash_Camp)
            {
                return CE_CampResetTime.Value;
            }
            else if (hash == Hash_Mine1 || hash == Hash_Mine2)
            {
                return CE_MineResetTime.Value;
            }
            else if (hash == Hash_Queen)
            {
                return CE_QueenResetTime.Value;
            }
            else if (hash == Hash_HildirBurial)
            {
                return CE_HildirBurialResetTime.Value;
            }
            else if (hash == Hash_HildirCave)
            {
                return CE_HildirCaveResetTime.Value;
            }
            else if (hash == Hash_HildirTower)
            {
                return CE_HildirTowerResetTime.Value;
            }
            else if (hash == Hash_CopperTinCave)
            {
                return CE_CopperTinCaveResetTime.Value;
            }
            else if (hash == Hash_SilverCave)
            {
                return CE_SilverCaveResetTime.Value;
            }
            else if (hash == Hash_CharredFortress)
            {
                return CE_CharredFortress.Value;
            }
            else if (hash == Hash_LeviathanLava)
            {
                return CE_LeviathanLava.Value;
            }
            else if (hash == Hash_MorgenHole1 || hash == Hash_MorgenHole2 || hash == Hash_MorgenHole3)
            {
                return CE_MorgenHole.Value;
            }
            else if (hash == Hash_PlaceofMystery1 || hash == Hash_PlaceofMystery2 || hash == Hash_PlaceofMystery3)
            {
                return CE_PlaceofMystery.Value;
            }
        }

        return CE_ResetTime.Value;
    }

    public static bool GetSkipPlayerGroundPieceCheck() => CE_SkipPlayerGroundPieceCheck.Value;
    public static bool GetResetGroundLocations() => CE_ResetGroundLocations.Value;

    public static bool GetEnableLeviathanReset() => CE_EnableLeviathanReset.Value;
    public static int GetLeviathanResetTime() => CE_LeviathanResetTime.Value;

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
        const string advanced = "Advanced";
        const string leviathans = "Leviathans";

        Config.SaveOnConfigSet = false;

        AddConfig("ResetTime", general, "Default number of in-game days for reset, one day is about 30 minutes (int).",
            true, 30, ref CE_ResetTime);
        AddConfig("SkipPlayerGroundPieceCheck", general, "When True will reset locations even if player placed pieces " +
            "and tombstones are on the ground outside the entrance to sky locations (boolean).",
            true, false, ref CE_SkipPlayerGroundPieceCheck);
        AddConfig("IgnoreList", general, "List of locations to ignore and not reset (comma-separated string).",
            true, "", ref CE_IgnoreList);
        AddConfig("AllowList", general, "List of locations to always allow and reset, bypasses the " +
            "SkipPlayerGroundPieceCheck configuration (comma-separated string).",
            true, "", ref CE_AllowList);

        AddConfig("ResetGroundLocations", advanced, "True to reset all misc locations found on the ground (not including meadow farms/villages or fuling camps) (boolean).",
            true, true, ref CE_ResetGroundLocations);
        AddConfig("OverrideResetTimes", advanced, "True to use all the values below rather than the default when applicable (boolean).",
            true, false, ref CE_OverrideResetTimes);
        AddConfig("FarmResetTime", advanced, "Number of in-game days for resetting meadow farms (int).",
            true, 30, ref CE_FarmResetTime);
        AddConfig("VillageResetTime", advanced, "Number of in-game days for resetting meadow draugr villages (int).",
            true, 30, ref CE_VillageResetTime);
        AddConfig("TrollResetTime", advanced, "Number of in-game days for resetting black forest troll caves (int).",
            true, 30, ref CE_TrollResetTime);
        AddConfig("BurialResetTime", advanced, "Number of in-game days for resetting black forest burial chambers (int).",
            true, 30, ref CE_BurialResetTime);
        AddConfig("CryptResetTime", advanced, "Number of in-game days for resetting swamp crypts (int).",
            true, 30, ref CE_CryptResetTime);
        AddConfig("CaveResetTime", advanced, "Number of in-game days for resetting mountain caves (int).",
            true, 30, ref CE_CaveResetTime);
        AddConfig("CampResetTime", advanced, "Number of in-game days for resetting plain fuling camps (int).",
            true, 30, ref CE_CampResetTime);
        AddConfig("MineResetTime", advanced, "Number of in-game days for resetting mistland infested mines (int).",
            true, 30, ref CE_MineResetTime);
        AddConfig("QueenResetTime", advanced, "Number of in-game days for resetting mistland infested citadel (int).",
            true, 30, ref CE_QueenResetTime);
        AddConfig("HildirCryptResetTime", advanced, "Number of in-game days for resetting hildir's black forest crypts (int).",
            true, 30, ref CE_HildirBurialResetTime);
        AddConfig("HildirCaveResetTime", advanced, "Number of in-game days for resetting hildir's mountain caves (int).",
            true, 30, ref CE_HildirCaveResetTime);
        AddConfig("HildirTowerResetTime", advanced, "Number of in-game days for resetting hildir's plain towers (int).",
            true, 30, ref CE_HildirTowerResetTime);

        AddConfig("CharredFortressResetTime", advanced, "Number of in-game days for resetting the charred fortress (int).",
            true, 30, ref CE_CharredFortress);
        AddConfig("LeviathanLavaResetTime", advanced, "Number of in-game days for resetting flametal leviathans (int).",
            true, 30, ref CE_LeviathanLava);
        AddConfig("MorgenHoleResetTime", advanced, "Number of in-game days for resetting the different morgen holes (int).",
            true, 30, ref CE_MorgenHole);
        AddConfig("PlaceofMysteryResetTime", advanced, "Number of in-game days for resetting the different places of mystery (int).",
            true, 30, ref CE_PlaceofMystery);

        AddConfig("CopperTinCaveResetTime", advanced, "Number of in-game days for resetting copper & tin mining caves (int).",
            true, 30, ref CE_CopperTinCaveResetTime);
        AddConfig("SilverCaveResetTime", advanced, "Number of in-game days for resetting silver mining caves (int).",
            true, 30, ref CE_SilverCaveResetTime);

        AddConfig("EnableLeviathanReset", leviathans, "True to enable resetting Leviathans (boolean).",
            true, true, ref CE_EnableLeviathanReset);
        AddConfig("LeviathanResetTime", leviathans, "Default number of in-game days for reset, one day is about 30 minutes (int).",
            true, 30, ref CE_LeviathanResetTime);

        Config.Save();
        Config.SaveOnConfigSet = true;

        #endregion

        LocationResetLogger.LogInfo("LocationReset getting ready for mass destruction. Consider making backups before using this mod!");

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
        SetupWatcher();

        // Check for Dungeon Splitter
        if (ModCompatibility.DungeonSplitterInstalled)
        {
            LocationResetLogger.LogInfo("Detected Dungeon Splitter, this mod will NOT reset sky locations!");
        }

        // Check for More Vanilla Build Prefabs
        if (ModCompatibility.MVBPInstalled)
        {
            LocationResetLogger.LogInfo("Detected More Vanilla Build Prefabs, this mod will NOT reset the start temple!");
        }

        Jotunn.Managers.SynchronizationManager.OnConfigurationSynchronized += (obj, attr) =>
        {
            LocationResetLogger.LogDebug("Refreshing configuration, setting custom ignored and allowed lists.");
            ResetConfigurations();
        };
    }

    public void OnDestroy()
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
            LocationResetLogger.LogDebug("Attempting to reload configuration...");
            Config.Reload();
            ResetConfigurations();
        }
        catch
        {
            LocationResetLogger.LogError($"There was an issue loading {ConfigFileName}");
        }
    }

    /// <summary>
    /// First time config setup.
    /// </summary>
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Start))]
    public static class Patch_ZNet_Start
    {
        private static void Postfix()
        {
            ResetConfigurations();
        }
    }

    private static void ResetConfigurations()
    {
        LocationReset.SetCustomLocationHashes(
            ref LocationReset.Instance.CustomIgnoreLocationHashes, CE_IgnoreList.Value);
        LocationReset.SetCustomLocationHashes(
            ref LocationReset.Instance.CustomAllowLocationHashes, CE_AllowList.Value);
    }
}