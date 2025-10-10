using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.NoDamageFlash;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class NoDamageFlashPlugin : BaseUnityPlugin
{
    private const string ModName = "NoDamageFlash";
    private const string ModVersion = "0.2.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private static string ConfigFileName = ModGUID + ".cfg";
    private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource NoDamageFlashLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    #region ConfigurationEntries

    private static ConfigEntry<bool> CE_RemoveAllFlash = null!;
    private static ConfigEntry<bool> CE_RemoveDamageFlash = null!;
    private static ConfigEntry<bool> CE_RemovePukeFlash = null!;
    private static ConfigEntry<bool> CE_RemoveUseHealthFlash = null!;

    public static bool GetRemoveAllFlash() => CE_RemoveAllFlash.Value;
    public static bool GetRemoveDamageFlash() => CE_RemoveDamageFlash.Value;
    public static bool GetRemovePukeFlash() => CE_RemovePukeFlash.Value;
    public static bool GetRemoveUseHealthFlash() => CE_RemoveUseHealthFlash.Value;

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

        AddConfig("RemoveAllFlash", general, "True to disable all damage flash, set to false to use other configs (boolean).",
            false, true, ref CE_RemoveAllFlash);
        AddConfig("RemoveDamageFlash", general, "True to disable damage flash from recieving damage (boolean).",
            false, false, ref CE_RemoveDamageFlash);
        AddConfig("RemovePukeFlash", general, "True to disable damage flash from puking (boolean).",
            false, false, ref CE_RemovePukeFlash);
        AddConfig("RemoveUseHealthFlash", general, "True to disable damage flash from using blood magic weapons (boolean).",
            false, false, ref CE_RemoveUseHealthFlash);

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
            NoDamageFlashLogger.LogDebug("Attempting to reload configuration...");
            Config.Reload();
        }
        catch
        {
            NoDamageFlashLogger.LogError($"There was an issue loading {ConfigFileName}");
        }
    }
}