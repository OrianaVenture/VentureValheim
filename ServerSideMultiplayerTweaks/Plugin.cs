using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;

namespace VentureValheim.ServerSideMultiplayerTweaks;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class ServerSideMultiplayerTweaksPlugin : BaseUnityPlugin
{
    private const string ModName = "ServerSideMultiplayerTweaks";
    private const string ModVersion = "0.1.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;
    private static string ConfigFileName = ModGUID + ".cfg";
    private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource ServerSideMultiplayerTweaksLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    #region ConfigurationEntries

    internal static ConfigEntry<bool> CE_OverridePlayerMapPins = null!;
    internal static ConfigEntry<bool> CE_ForcePlayerMapPinsOn = null!;
    
    public static bool GetOverridePlayerMapPins() => CE_OverridePlayerMapPins.Value;
    public static bool GetForcePlayerMapPinsOn() => CE_ForcePlayerMapPinsOn.Value;

    private void AddConfig<T>(string key, string section, string description, T value, ref ConfigEntry<T> configEntry)
    {
        configEntry = Config.Bind(section, key, value,
            new ConfigDescription(description));
    }

    #endregion

    public void Awake()
    {
        #region Configuration

        const string general = "General";

        AddConfig("OverridePlayerMapPositions", general,
            "Override Player map pin position behavior for Minimap (boolean).",
            true, ref CE_OverridePlayerMapPins);
        AddConfig("ForcePlayerMapPositionOn", general,
            "True to always show Player position on Minimap, False to hide, when OverridePlayerMapPositions is True (boolean).",
            true, ref CE_ForcePlayerMapPinsOn);

        #endregion

        ServerSideMultiplayerTweaksLogger.LogInfo("Always has been.");

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
            ServerSideMultiplayerTweaksLogger.LogInfo("Attempting to reload configuration...");
            Config.Reload();
        }
        catch
        {
            ServerSideMultiplayerTweaksLogger.LogError($"There was an issue loading {ConfigFileName}");
            return;
        }

        _lastReloadTime = now;
    }
}