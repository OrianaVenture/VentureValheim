using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.TerrainReset;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class TerrainResetPlugin : BaseUnityPlugin
{
    private const string ModName = "TerrainReset";
    private const string ModVersion = "0.1.1";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;
    private static string ConfigFileName = ModGUID + ".cfg";
    private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource TerrainResetLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    private TerrainResetPlugin()
    {
    }
    private static readonly TerrainResetPlugin _instance = new TerrainResetPlugin();

    public static TerrainResetPlugin Instance
    {
        get => _instance;
    }

    #region ConfigurationEntries

    private static ConfigEntry<bool> CE_ModEnabled = null!;
    
    public static bool GetModEnabled() => CE_ModEnabled.Value;

    private static ConfigEntry<KeyCode> CE_HotKey = null!;
    private static ConfigEntry<float> CE_HotKeyRadius = null!;
    private static ConfigEntry<KeyCode> CE_ToolModKey = null!;
    private static ConfigEntry<float> CE_ToolRadius = null!;

    public static KeyCode GetHotKey() => CE_HotKey.Value;
    public static float GetHotKeyRadius() => CE_HotKeyRadius.Value;
    public static KeyCode GetToolModKey() => CE_ToolModKey.Value;
    public static float GetToolRadius() => CE_ToolRadius.Value;

    private void AddConfig<T>(string key, string section, string description, bool synced, T value, ref ConfigEntry<T> configEntry)
    {
        string extendedDescription = GetExtendedDescription(description, synced);
        configEntry = Config.Bind(section, key, value, new ConfigDescription(extendedDescription));
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

        AddConfig("Enabled", general, "Enable this mod (boolean).",
            false, true, ref CE_ModEnabled);

        AddConfig("HotKey", general, "Hotkey to reset terrain (KeyCode).",
            false, KeyCode.None, ref CE_HotKey);
        AddConfig("HotKeyRadius", general, "Reset radius for hotkey (float).",
            false, 50f, ref CE_HotKeyRadius);
        AddConfig("ToolModKey", general, "Modifier key to reset terrain when using tools (KeyCode).",
            false, KeyCode.LeftAlt, ref CE_ToolModKey);
        AddConfig("ToolRadius", general, "Reset radius for tools. Set to 0 to use the tool's original radius (float).",
            false, 0, ref CE_ToolRadius);

        #endregion

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
        SetupWatcher();
    }

    /// <summary>
    /// Apply terrain reset when hot key is pressed.
    /// </summary>
    private void Update()
    {
        if (!GetModEnabled() || TerrainReset.IgnoreKeyPresses() ||
            !TerrainReset.CheckKeyDown(GetHotKey()) )
        {
            return;
        }

        int resets = TerrainReset.ResetTerrain(Player.m_localPlayer.transform.position, GetHotKeyRadius());
        Player.m_localPlayer.Message(MessageHud.MessageType.Center, 
            string.Format("{0} edits reset.", resets));
    }

    private void OnDestroy()
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
            TerrainResetLogger.LogInfo("Attempting to reload configuration...");
            Config.Reload();
        }
        catch
        {
            TerrainResetLogger.LogError($"There was an issue loading {ConfigFileName}");
            return;
        }
    }
}