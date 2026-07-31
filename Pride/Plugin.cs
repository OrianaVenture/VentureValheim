using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Utils;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace VentureValheim.Pride;

[BepInDependency(Jotunn.Main.ModGuid)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class PridePlugin : BaseUnityPlugin
{
    private const string ModName = "Pride";
    private const string ModVersion = "1.0.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;
    private static string ConfigFileName = ModGUID + ".cfg";
    private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource PrideLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public static GameObject Root;
    internal static AssetBundle PrideBundle;
    public static Texture2D PrideTexture;
    public static Texture2D TransTexture;
    public static Texture2D RoundShieldTexture;
    public static Texture2D BannerTexture;

    #region ConfigurationEntries

    private static ConfigEntry<bool> CE_ReplaceSailsPride = null!;
    private static ConfigEntry<bool> CE_ReplaceSailsTrans = null!;
    private static ConfigEntry<bool> CE_ReplaceShields = null!;
    private static ConfigEntry<bool> CE_ReplaceBanner = null!;

    public static bool GetReplaceSailsPride() => CE_ReplaceSailsPride.Value;
    public static bool GetReplaceSailsTrans() => CE_ReplaceSailsTrans.Value;
    public static bool GetReplaceShields() => CE_ReplaceShields.Value;
    public static bool GetReplaceBanner() => CE_ReplaceBanner.Value;

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

    private void Awake()
    {
        #region Configuration

        const string general = "General";

        AddConfig("ReplaceSailsRainbow", general, "True to replace all sails with the rainbow pride flag (boolean).",
            false, true, ref CE_ReplaceSailsPride);
        AddConfig("ReplaceSailsTrans", general, "True to replace all sails with the trans flag when ReplaceSailsRainbow is false (boolean).",
            false, false, ref CE_ReplaceSailsTrans);
        AddConfig("ReplaceShields", general, "True to add inclusive pride flag all round shields (boolean).",
            false, true, ref CE_ReplaceShields);
        AddConfig("ReplaceBanner", general, "True to replace the red/white/blue banner (piece_banner06) with the trans flag colors (boolean).",
            false, true, ref CE_ReplaceBanner);

        #endregion

        PrideLogger.LogInfo("Happy Pride Month (and beyond)!");
        Root = new GameObject("PrideRoot");
        Root.SetActive(false);
        DontDestroyOnLoad(Root);

        PrideBundle = AssetUtils.LoadAssetBundleFromResources("vv_pride", Assembly.GetExecutingAssembly());
        PrideTexture = PrideBundle.LoadAsset<Texture2D>("vv_PrideFlagSquare");
        TransTexture = PrideBundle.LoadAsset<Texture2D>("vv_TransFlagSquare");
        RoundShieldTexture = PrideBundle.LoadAsset<Texture2D>("vv_ShieldwoodPrideUnderlay");
        BannerTexture = PrideBundle.LoadAsset<Texture2D>("vv_PrideBannerOverlay");

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
        DateTime now = DateTime.Now;
        long time = now.Ticks - _lastReloadTime.Ticks;
        if (!File.Exists(ConfigFileFullPath) || time < RELOAD_DELAY) return;

        try
        {
            PrideLogger.LogDebug("Attempting to reload configuration...");
            Config.Reload();
        }
        catch
        {
            PrideLogger.LogError($"There was an issue loading {ConfigFileName}");
            return;
        }

        _lastReloadTime = now;

        PrideShip.TryUpdateSails();
        PrideShield.TryUpdateShield();
        PrideBanner.TryUpdateBanner();
    }
}