using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;

namespace VentureValheim.Scaling;

[BepInDependency(Jotunn.Main.ModGuid)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class ScalingPlugin : BaseUnityPlugin
{
    static ScalingPlugin() { }
    private ScalingPlugin() { }
    private static readonly ScalingPlugin _instance = new ScalingPlugin();

    public static ScalingPlugin Instance
    {
        get => _instance;
    }

    private const string ModName = "WorldScaling";
    private const string ModVersion = "0.3.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;
    private static string ConfigFileName = ModGUID + ".cfg";
    private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource VentureScalingLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    #region ConfigurationEntries

    public static ConfigEntry<bool> CE_GenerateGameData = null!;

    // Auto-Scaling Configuration
    public static ConfigEntry<string> CE_AutoScaleType = null!;
    public static ConfigEntry<float> CE_AutoScaleFactor = null!;
    public static ConfigEntry<bool> CE_AutoScaleIgnoreOverrides = null!;
    public static ConfigEntry<bool> CE_AutoScaleCreatures = null!;
    public static ConfigEntry<string> CE_AutoScaleCreatureHealth = null!;
    public static ConfigEntry<string> CE_AutoScaleCreatureDamage = null!;
    public static ConfigEntry<bool> CE_AutoScaleItems = null!;

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
        const string creatures = "Creatures";
        const string items = "Items";

        AddConfig("GenerateGameDataFiles", general, "Finds all items and creatures and creates data files in your config path for viewing only (boolean).",
            false, false, ref CE_GenerateGameData);
        AddConfig("AutoScaleType", general,
            "Auto-scaling type: Vanilla, Linear, Exponential, or Custom (string).",
            true, "Linear", ref CE_AutoScaleType);
        AddConfig("AutoScaleFactor", general,
            "Auto-scaling factor used for the Auto-scaling type algorithm (float).",
            true, 0.75f, ref CE_AutoScaleFactor);
        AddConfig("AutoScaleIgnoreOverrides", general,
            "When true ignores the overrides specified in the yaml files (boolean).",
            true, false, ref CE_AutoScaleIgnoreOverrides);

        AddConfig("ScaleCreatures", creatures,
            "Enable the scaling of creatures (boolean).",
            true, true, ref CE_AutoScaleCreatures);
        AddConfig("CreaturesHealth", creatures,
            "Override the Base Health distribution for Creatures (comma-separated list of 6 integers) (string).",
            true, "", ref CE_AutoScaleCreatureHealth);
        AddConfig("CreaturesDamage", creatures,
            "Override the Base Damage distribution for Creatures (comma-separated list of 6 integers) (string).",
            true, "", ref CE_AutoScaleCreatureDamage);

        AddConfig("AutoScaleItems", items,
            "Enable the scaling of Items (boolean).",
            true, true, ref CE_AutoScaleItems);

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
            VentureScalingLogger.LogInfo("Attempting to reload configuration...");
            Config.Reload();
        }
        catch
        {
            VentureScalingLogger.LogError($"There was an issue loading {ConfigFileName}");
            return;
        }

        _lastReloadTime = now;

        WorldConfiguration.Instance.SetupScaling();
    }
}

public interface IScalingConfiguration
{
    public bool GetGenerateGameData();
    public string GetAutoScaleType();
    public float GetAutoScaleFactor();
    public bool GetAutoScaleIgnoreOverrides();
    public bool GetAutoScaleCreatures();
    public string GetAutoScaleCreatureHealth();
    public string GetAutoScaleCreatureDamage();
    public bool GetAutoScaleItems();
}

public class ScalingConfiguration : IScalingConfiguration
{
    static ScalingConfiguration() { }

    public ScalingConfiguration() { }
    public  ScalingConfiguration(IScalingConfiguration ScalingConfiguration)
    {
        _instance = ScalingConfiguration;
    }
    private static IScalingConfiguration _instance = new ScalingConfiguration();

    public static IScalingConfiguration Instance
    {
        get => _instance;
    }

    public bool GetGenerateGameData() => ScalingPlugin.CE_GenerateGameData.Value;
    public string GetAutoScaleType() => ScalingPlugin.CE_AutoScaleType.Value;
    public float GetAutoScaleFactor() => ScalingPlugin.CE_AutoScaleFactor.Value;
    public bool GetAutoScaleIgnoreOverrides() => ScalingPlugin.CE_AutoScaleIgnoreOverrides.Value;
    public bool GetAutoScaleCreatures() => ScalingPlugin.CE_AutoScaleCreatures.Value;
    public string GetAutoScaleCreatureHealth() => ScalingPlugin.CE_AutoScaleCreatureHealth.Value;
    public string GetAutoScaleCreatureDamage() => ScalingPlugin.CE_AutoScaleCreatureDamage.Value;
    public bool GetAutoScaleItems() => ScalingPlugin.CE_AutoScaleItems.Value;
}