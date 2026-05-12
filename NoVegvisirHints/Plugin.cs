using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;

namespace VentureValheim.NoVegvisirHints;

[BepInDependency(Jotunn.Main.ModGuid)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class NoVegvisirHintsPlugin : BaseUnityPlugin
{
    private const string ModName = "NoVegvisirHints";
    private const string ModVersion = "0.2.1";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;
    private static string ConfigFileName = ModGUID + ".cfg";
    private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource NoVegvisirHintsLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    #region ConfigurationEntries

    public static ConfigEntry<bool> CE_SetLookDirection = null!;

    public static bool GetSetLookDirection() => CE_SetLookDirection.Value;

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

        AddConfig("SetLookDirection", general,
            "True to turn the camera in the direction of the location when pins are blocked (boolean).",
            true, true, ref CE_SetLookDirection);

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
            NoVegvisirHintsLogger.LogInfo("Attempting to reload configuration...");
            Config.Reload();
        }
        catch
        {
            NoVegvisirHintsLogger.LogError($"There was an issue loading {ConfigFileName}");
            return;
        }

        _lastReloadTime = now;
    }
}