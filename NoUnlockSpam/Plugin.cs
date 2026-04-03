using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace VentureValheim.NoUnlockSpam;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class NoUnlockSpamPlugin : BaseUnityPlugin
{
    private const string ModName = "NoUnlockSpam";
    private const string ModVersion = "0.1.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;
    private static string ConfigFileName = ModGUID + ".cfg";
    private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource NoUnlockSpamLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    #region ConfigurationEntries

    public static ConfigEntry<bool> CE_NotificationsEnabled = null!;
    private static ConfigEntry<KeyCode> CE_ToggleKey = null!;

    public static bool GetNotificationsEnabled() => CE_NotificationsEnabled.Value;
    public static KeyCode GetToggleKey() => CE_ToggleKey.Value;

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

    private void Awake()
    {
        NoUnlockSpamLogger.LogInfo("Okay, we get it.");

        const string general = "General";

        AddConfig("NotificationsEnabled", general, "False to disable all unlock notifications (boolean).",
            false, true, ref CE_NotificationsEnabled);
        AddConfig("ToggleKey", general, "Keycode to dismiss the queued unlock notifications (Unity.KeyCode).",
            false, KeyCode.U, ref CE_ToggleKey);

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
            NoUnlockSpamLogger.LogInfo("Attempting to reload configuration...");
            Config.Reload();
        }
        catch
        {
            NoUnlockSpamLogger.LogError($"There was an issue loading {ConfigFileName}");
            return;
        }

        _lastReloadTime = now;
    }
}