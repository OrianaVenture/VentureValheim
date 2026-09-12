using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;
using System;
using System.IO;
using System.Reflection;

namespace VentureValheim.Cornerstone;

[BepInDependency(Jotunn.Main.ModGuid)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class CornerstonePlugin : BaseUnityPlugin
{
    private const string ModName = "Cornerstone";
    private const string ModVersion = "1.0.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;
    private static string ConfigFileName = ModGUID + ".cfg";
    private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource CornerstoneLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    #region ConfigurationEntries

    // Wood
    private static ConfigEntry<bool> CE_Wood_Enabled = null!;
    private static ConfigEntry<float> CE_Wood_MaxStability = null!;
    private static ConfigEntry<float> CE_Wood_MinStability = null!;
    private static ConfigEntry<float> CE_Wood_VerticalLoss = null!;
    private static ConfigEntry<float> CE_Wood_HorizontalLoss = null!;
    public static bool GetWoodEnabled() => CE_Wood_Enabled.Value;
    public static float GetWoodMaxStability() => CE_Wood_MaxStability.Value;
    public static float GetWoodMinStability() => CE_Wood_MinStability.Value;
    public static float GetWoodVerticalLoss() => CE_Wood_VerticalLoss.Value;
    public static float GetWoodHorizontalLoss() => CE_Wood_HorizontalLoss.Value;

    // HardWood
    private static ConfigEntry<bool> CE_HardWood_Enabled = null!;
    private static ConfigEntry<float> CE_HardWood_MaxStability = null!;
    private static ConfigEntry<float> CE_HardWood_MinStability = null!;
    private static ConfigEntry<float> CE_HardWood_VerticalLoss = null!;
    private static ConfigEntry<float> CE_HardWood_HorizontalLoss = null!;
    public static bool GetHardWoodEnabled() => CE_HardWood_Enabled.Value;
    public static float GetHardWoodMaxStability() => CE_HardWood_MaxStability.Value;
    public static float GetHardWoodMinStability() => CE_HardWood_MinStability.Value;
    public static float GetHardWoodVerticalLoss() => CE_HardWood_VerticalLoss.Value;
    public static float GetHardWoodHorizontalLoss() => CE_HardWood_HorizontalLoss.Value;

    // Stone
    private static ConfigEntry<bool> CE_Stone_Enabled = null!;
    private static ConfigEntry<float> CE_Stone_MaxStability = null!;
    private static ConfigEntry<float> CE_Stone_MinStability = null!;
    private static ConfigEntry<float> CE_Stone_VerticalLoss = null!;
    private static ConfigEntry<float> CE_Stone_HorizontalLoss = null!;

    public static bool GetStoneEnabled() => CE_Stone_Enabled.Value;
    public static float GetStoneMaxStability() => CE_Stone_MaxStability.Value;
    public static float GetStoneMinStability() => CE_Stone_MinStability.Value;
    public static float GetStoneVerticalLoss() => CE_Stone_VerticalLoss.Value;
    public static float GetStoneHorizontalLoss() => CE_Stone_HorizontalLoss.Value;

    // Iron
    private static ConfigEntry<bool> CE_Iron_Enabled = null!;
    private static ConfigEntry<float> CE_Iron_MaxStability = null!;
    private static ConfigEntry<float> CE_Iron_MinStability = null!;
    private static ConfigEntry<float> CE_Iron_VerticalLoss = null!;
    private static ConfigEntry<float> CE_Iron_HorizontalLoss = null!;
    public static bool GetIronEnabled() => CE_Iron_Enabled.Value;
    public static float GetIronMaxStability() => CE_Iron_MaxStability.Value;
    public static float GetIronMinStability() => CE_Iron_MinStability.Value;
    public static float GetIronVerticalLoss() => CE_Iron_VerticalLoss.Value;
    public static float GetIronHorizontalLoss() => CE_Iron_HorizontalLoss.Value;

    // Marble
    private static ConfigEntry<bool> CE_Marble_Enabled = null!;
    private static ConfigEntry<float> CE_Marble_MaxStability = null!;
    private static ConfigEntry<float> CE_Marble_MinStability = null!;
    private static ConfigEntry<float> CE_Marble_VerticalLoss = null!;
    private static ConfigEntry<float> CE_Marble_HorizontalLoss = null!;
    public static bool GetMarbleEnabled() => CE_Marble_Enabled.Value;
    public static float GetMarbleMaxStability() => CE_Marble_MaxStability.Value;
    public static float GetMarbleMinStability() => CE_Marble_MinStability.Value;
    public static float GetMarbleVerticalLoss() => CE_Marble_VerticalLoss.Value;
    public static float GetMarbleHorizontalLoss() => CE_Marble_HorizontalLoss.Value;

    // Ashstone
    private static ConfigEntry<bool> CE_Ashstone_Enabled = null!;
    private static ConfigEntry<float> CE_Ashstone_MaxStability = null!;
    private static ConfigEntry<float> CE_Ashstone_MinStability = null!;
    private static ConfigEntry<float> CE_Ashstone_VerticalLoss = null!;
    private static ConfigEntry<float> CE_Ashstone_HorizontalLoss = null!;
    public static bool GetAshstoneEnabled() => CE_Ashstone_Enabled.Value;
    public static float GetAshstoneMaxStability() => CE_Ashstone_MaxStability.Value;
    public static float GetAshstoneMinStability() => CE_Ashstone_MinStability.Value;
    public static float GetAshstoneVerticalLoss() => CE_Ashstone_VerticalLoss.Value;
    public static float GetAshstoneHorizontalLoss() => CE_Ashstone_HorizontalLoss.Value;

    // Ancient
    private static ConfigEntry<bool> CE_Ancient_Enabled = null!;
    private static ConfigEntry<float> CE_Ancient_MaxStability = null!;
    private static ConfigEntry<float> CE_Ancient_MinStability = null!;
    private static ConfigEntry<float> CE_Ancient_VerticalLoss = null!;
    private static ConfigEntry<float> CE_Ancient_HorizontalLoss = null!;
    public static bool GetAncientEnabled() => CE_Ancient_Enabled.Value;
    public static float GetAncientMaxStability() => CE_Ancient_MaxStability.Value;
    public static float GetAncientMinStability() => CE_Ancient_MinStability.Value;
    public static float GetAncientVerticalLoss() => CE_Ancient_VerticalLoss.Value;
    public static float GetAncientHorizontalLoss() => CE_Ancient_HorizontalLoss.Value;

    // Ice
    private static ConfigEntry<bool> CE_Ice_Enabled = null!;
    private static ConfigEntry<float> CE_Ice_MaxStability = null!;
    private static ConfigEntry<float> CE_Ice_MinStability = null!;
    private static ConfigEntry<float> CE_Ice_VerticalLoss = null!;
    private static ConfigEntry<float> CE_Ice_HorizontalLoss = null!;
    public static bool GetIceEnabled() => CE_Ice_Enabled.Value;
    public static float GetIceMaxStability() => CE_Ice_MaxStability.Value;
    public static float GetIceMinStability() => CE_Ice_MinStability.Value;
    public static float GetIceVerticalLoss() => CE_Ice_VerticalLoss.Value;
    public static float GetIceHorizontalLoss() => CE_Ice_HorizontalLoss.Value;

    // Timberwood
    private static ConfigEntry<bool> CE_Timberwood_Enabled = null!;
    private static ConfigEntry<float> CE_Timberwood_MaxStability = null!;
    private static ConfigEntry<float> CE_Timberwood_MinStability = null!;
    private static ConfigEntry<float> CE_Timberwood_VerticalLoss = null!;
    private static ConfigEntry<float> CE_Timberwood_HorizontalLoss = null!;
    public static bool GetTimberwoodEnabled() => CE_Timberwood_Enabled.Value;
    public static float GetTimberwoodMaxStability() => CE_Timberwood_MaxStability.Value;
    public static float GetTimberwoodMinStability() => CE_Timberwood_MinStability.Value;
    public static float GetTimberwoodVerticalLoss() => CE_Timberwood_VerticalLoss.Value;
    public static float GetTimberwoodHorizontalLoss() => CE_Timberwood_HorizontalLoss.Value;


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

        const string wood = "Wood";
        const string hardWood = "HardWood";
        const string stone = "Stone";
        const string iron = "Iron";
        const string marble = "Marble";
        const string ashstone = "Ashstone";
        const string ancient = "Ancient";
        const string ice = "Ice";
        const string timberwood = "Timberwood";

        // Wood
        AddConfig("EnableSection", wood, "True to use the settings in this section, set to False to use vanilla behavior (bool).",
            true, true, ref CE_Wood_Enabled);
        AddConfig("MaximumStability", wood, "How much stability this material provides on solid ground (float).",
            true, 100f, ref CE_Wood_MaxStability);
        AddConfig("MinimumStability", wood, "How much stability this material needs to remain standing (float).",
            true, 10f, ref CE_Wood_MinStability);
        AddConfig("VerticalLoss", wood, "Multiplier to vertical or upwards stability (float).",
            true, 0.125f, ref CE_Wood_VerticalLoss);
        AddConfig("HorizontalLoss", wood, "Multiplier to horizontal or outwards stability (float).",
            true, 0.2f, ref CE_Wood_HorizontalLoss);

        // HardWood
        AddConfig("EnableSection", hardWood, "True to use the settings in this section, set to False to use vanilla behavior (bool).",
            true, true, ref CE_HardWood_Enabled);
        AddConfig("MaximumStability", hardWood, "How much stability this material provides on solid ground (float).",
            true, 140, ref CE_HardWood_MaxStability);
        AddConfig("MinimumStability", hardWood, "How much stability this material needs to remain standing (float).",
            true, 10f, ref CE_HardWood_MinStability);
        AddConfig("VerticalLoss", hardWood, "Multiplier to vertical or upwards stability (float).",
            true, 0.1f, ref CE_HardWood_VerticalLoss);
        AddConfig("HorizontalLoss", hardWood, "Multiplier to horizontal or outwards stability (float).",
            true, 1f / 6f, ref CE_HardWood_HorizontalLoss);

        // Stone
        AddConfig("EnableSection", stone, "True to use the settings in this section, set to False to use vanilla behavior (bool).",
            true, true, ref CE_Stone_Enabled);
        AddConfig("MaximumStability", stone, "How much stability this material provides on solid ground (float).",
            true, 1000f, ref CE_Stone_MaxStability);
        AddConfig("MinimumStability", stone, "How much stability this material needs to remain standing (float).",
            true, 100f, ref CE_Stone_MinStability);
        AddConfig("VerticalLoss", stone, "Multiplier to vertical or upwards stability (float).",
            true, 0.125f, ref CE_Stone_VerticalLoss);
        AddConfig("HorizontalLoss", stone, "Multiplier to horizontal or outwards stability (float).",
            true, 0.5f, ref CE_Stone_HorizontalLoss);

        // Iron
        AddConfig("EnableSection", iron, "True to use the settings in this section, set to False to use vanilla behavior (bool).",
            true, true, ref CE_Iron_Enabled);
        AddConfig("MaximumStability", iron, "How much stability this material provides on solid ground (float).",
            true, 1500f, ref CE_Iron_MaxStability);
        AddConfig("MinimumStability", iron, "How much stability this material needs to remain standing (float).",
            true, 20f, ref CE_Iron_MinStability);
        AddConfig("VerticalLoss", iron, "Multiplier to vertical or upwards stability (float).",
            true, 1f / 13f, ref CE_Iron_VerticalLoss);
        AddConfig("HorizontalLoss", iron, "Multiplier to horizontal or outwards stability (float).",
            true, 1f / 13f, ref CE_Iron_HorizontalLoss);

        // Marble
        AddConfig("EnableSection", marble, "True to use the settings in this section, set to False to use vanilla behavior (bool).",
            true, true, ref CE_Marble_Enabled);
        AddConfig("MaximumStability", marble, "How much stability this material provides on solid ground (float).",
            true, 1500f, ref CE_Marble_MaxStability);
        AddConfig("MinimumStability", marble, "How much stability this material needs to remain standing (float).",
            true, 100f, ref CE_Marble_MinStability);
        AddConfig("VerticalLoss", marble, "Multiplier to vertical or upwards stability (float).",
            true, 0.125f, ref CE_Marble_VerticalLoss);
        AddConfig("HorizontalLoss", marble, "Multiplier to horizontal or outwards stability (float).",
            true, 0.5f, ref CE_Marble_HorizontalLoss);

        // Ashstone
        AddConfig("EnableSection", ashstone, "True to use the settings in this section, set to False to use vanilla behavior (bool).",
            true, true, ref CE_Ashstone_Enabled);
        AddConfig("MaximumStability", ashstone, "How much stability this material provides on solid ground (float).",
            true, 2000f, ref CE_Ashstone_MaxStability);
        AddConfig("MinimumStability", ashstone, "How much stability this material needs to remain standing (float).",
            true, 100f, ref CE_Ashstone_MinStability);
        AddConfig("VerticalLoss", ashstone, "Multiplier to vertical or upwards stability (float).",
            true, 0.1f, ref CE_Ashstone_VerticalLoss);
        AddConfig("HorizontalLoss", ashstone, "Multiplier to horizontal or outwards stability (float).",
            true, 1f / 3f, ref CE_Ashstone_HorizontalLoss);

        // Ancient
        AddConfig("EnableSection", ancient, "True to use the settings in this section, set to False to use vanilla behavior (bool).",
            true, true, ref CE_Ancient_Enabled);
        AddConfig("MaximumStability", ancient, "How much stability this material provides on solid ground (float).",
            true, 5000f, ref CE_Ancient_MaxStability);
        AddConfig("MinimumStability", ancient, "How much stability this material needs to remain standing (float).",
            true, 100f, ref CE_Ancient_MinStability);
        AddConfig("VerticalLoss", ancient, "Multiplier to vertical or upwards stability (float).",
            true, 1 / 15f, ref CE_Ancient_VerticalLoss);
        AddConfig("HorizontalLoss", ancient, "Multiplier to horizontal or outwards stability (float).",
            true, 0.25f, ref CE_Ancient_HorizontalLoss);

        // Ice
        AddConfig("EnableSection", ice, "True to use the settings in this section, set to False to use vanilla behavior (bool).",
            true, true, ref CE_Ice_Enabled);
        AddConfig("MaximumStability", ice, "How much stability this material provides on solid ground (float).",
            true, 1000f, ref CE_Ice_MaxStability);
        AddConfig("MinimumStability", ice, "How much stability this material needs to remain standing (float).",
            true, 100f, ref CE_Ice_MinStability);
        AddConfig("VerticalLoss", ice, "Multiplier to vertical or upwards stability (float).",
            true, 0.125f, ref CE_Ice_VerticalLoss);
        AddConfig("HorizontalLoss", ice, "Multiplier to horizontal or outwards stability (float).",
            true, 1f / 3f, ref CE_Ice_HorizontalLoss);

        // Timberwood
        AddConfig("EnableSection", timberwood, "True to use the settings in this section, set to False to use vanilla behavior (bool).",
            true, true, ref CE_Timberwood_Enabled);
        AddConfig("MaximumStability", timberwood, "How much stability this material provides on solid ground (float).",
            true, 200f, ref CE_Timberwood_MaxStability);
        AddConfig("MinimumStability", timberwood, "How much stability this material needs to remain standing (float).",
            true, 10f, ref CE_Timberwood_MinStability);
        AddConfig("VerticalLoss", timberwood, "Multiplier to vertical or upwards stability (float).",
            true, 1f / 13f, ref CE_Timberwood_VerticalLoss);
        AddConfig("HorizontalLoss", timberwood, "Multiplier to horizontal or outwards stability (float).",
            true, 0.2f, ref CE_Timberwood_HorizontalLoss);

        #endregion

        CornerstoneLogger.LogInfo("I want to Rock and Roll all niiiiiight!");

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
            CornerstoneLogger.LogInfo("Attempting to reload configuration...");
            Config.Reload();
        }
        catch
        {
            CornerstoneLogger.LogError($"There was an issue loading {ConfigFileName}");
            return;
        }

        _lastReloadTime = now;
    }
}