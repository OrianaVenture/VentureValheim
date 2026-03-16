using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;

namespace VentureValheim.AsocialCartography;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class AsocialCartographyPlugin : BaseUnityPlugin
{
    private const string ModName = "AsocialCartography";
    private const string ModVersion = "0.3.1";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;
    private static string ConfigFileName = ModGUID + ".cfg";
    private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource AsocialCartographyLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    #region ConfigurationEntries

    private static ConfigEntry<bool> CE_AddPins = null!;
    private static ConfigEntry<bool> CE_ReceivePins = null!;
    private static ConfigEntry<bool> CE_IgnoreBossPins = null!;
    private static ConfigEntry<bool> CE_IgnoreHildirPins = null!;
    private static ConfigEntry<string> CE_IgnoredCustomPins = null!;
    private static ConfigEntry<float> CE_ReceivePinRadius = null!;

    public static bool GetAddPins() => CE_AddPins.Value;
    public static bool GetIgnoreBossPins() => CE_IgnoreBossPins.Value;
    public static bool GetIgnoreHildirPins() => CE_IgnoreHildirPins.Value;
    public static bool GetReceivePins() => CE_ReceivePins.Value;
    public static string GetIgnoredCustomPins() => CE_IgnoredCustomPins.Value;
    public static float GetReceivePinRadius() => CE_ReceivePinRadius.Value;

    private void AddConfig<T>(string key, string section, string description, bool synced, T value, ref ConfigEntry<T> configEntry)
    {
        string extendedDescription = GetExtendedDescription(description, synced);
        configEntry = Config.Bind(section, key, value, extendedDescription);
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

        AddConfig("AddPins", general, "False to disable adding player-placed map pins when adding to a map table (boolean).",
            false, false, ref CE_AddPins);
        AddConfig("ReceivePins", general, "False to disable taking player-placed map pins when reading a map table (boolean).",
            false, true, ref CE_ReceivePins);
        AddConfig("IgnoreBossPins", general, "False to include boss map pins in the above configs (boolean).",
            false, true, ref CE_IgnoreBossPins);
        AddConfig("IgnoreHildirPins", general, "False to include hildir map pins in the above configs (boolean).",
            false, true, ref CE_IgnoreHildirPins);
        AddConfig("IgnoredCustomPins", general, "List of map pins by integer id to include in the above configs (comma-separated list of integers).",
            false, "", ref CE_IgnoredCustomPins);
        AddConfig("ReceivePinRadius", general, "Overlap radius that must be exceeded to receive a pin from the map table. Default in vanilla is 1 (float).",
            false, 50f, ref CE_ReceivePinRadius);

        #endregion

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
        SetupWatcher();
        AsocialCartography.UpdateConfigurations();
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
            AsocialCartographyLogger.LogInfo("Attempting to reload configuration...");
            Config.Reload();
        }
        catch
        {
            AsocialCartographyLogger.LogError($"There was an issue loading {ConfigFileName}");
            return;
        }

        _lastReloadTime = now;

        AsocialCartography.UpdateConfigurations();
    }
}