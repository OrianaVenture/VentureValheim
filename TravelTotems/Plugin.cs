using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.TravelTotems;

[BepInDependency(VVMTGUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(Jotunn.Main.ModGuid)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class TravelTotemsPlugin : BaseUnityPlugin
{
    private const string ModName = "TravelTotems";
    private const string ModVersion = "0.1.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;
    private static string ConfigFileName = ModGUID + ".cfg";
    private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

    private const string VVMTGUID = "com.orianaventure.mod.MultiplayerTweaks";
    private const string VVMTAPI = "VentureValheim.MultiplayerTweaks.API";
    public static bool MultiplayerTweaksInstalled { get; private set; }

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource TravelTotemsLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    internal static AssetBundle TotemBundle { get; private set; }

    #region ConfigurationEntries

    private static ConfigEntry<bool> CE_AdminBypass = null!;
    private static ConfigEntry<bool> CE_ShowTotemID = null!;
    private static ConfigEntry<bool> CE_TotemNamingAccess = null!;
    private static ConfigEntry<bool> CE_ShowMapPins = null!;
    private static ConfigEntry<string> CE_TotemAccessDefault = null!;
    private static ConfigEntry<string> CE_TotemUnlockRequirementsString = null!;
    private static ConfigEntry<bool> CE_AllowSelectDefaultSpawn = null!;
    private static ConfigEntry<bool> CE_AllowSelectTraders = null!;
    private static ConfigEntry<bool> CE_AllowSelectBed = null!;
    private static ConfigEntry<bool> CE_AllowTotemClaiming = null!;

    private static ConfigEntry<bool> CE_TradersSellTotemBomb = null!;
    private static ConfigEntry<int> CE_TradersSellTotemBombCost = null!;

    private static ConfigEntry<int> CE_LocationAmountDefault = null!;
    private static ConfigEntry<int> CE_LocationAmountMeadows = null!;
    private static ConfigEntry<int> CE_LocationAmountBlackForest = null!;
    private static ConfigEntry<int> CE_LocationAmountSwamp = null!;
    private static ConfigEntry<int> CE_LocationAmountMountain = null!;
    private static ConfigEntry<int> CE_LocationAmountPlains = null!;
    private static ConfigEntry<int> CE_LocationAmountMistlands = null!;
    private static ConfigEntry<int> CE_LocationAmountAshlands = null!;
    private static ConfigEntry<int> CE_LocationSpacing = null!;

    private static ConfigEntry<bool> CE_PieceRecipesEnabled = null!;
    private static ConfigEntry<string> CE_TravelTotemPieceCost = null!;
    private static ConfigEntry<string> CE_TravelTotemPieceMistlandsCost = null!;
    private static ConfigEntry<string> CE_TravelTotemPieceAshlandsCost = null!;

    private static ConfigEntry<bool> CE_ItemRecipesEnabled = null!;
    private static ConfigEntry<string> CE_TotemBombCraftCost = null!;

    // General & Client

    private static bool AdminAccess()
    {
        return CE_AdminBypass.Value && SynchronizationManager.Instance.PlayerIsAdmin;
    }

    public static bool GetOverrideTotemAccess(bool isCreator)
    {
        if (isCreator || AdminAccess())
        {
            return true;
        }

        return false;
    }

    public static bool GetTotemNamingAccess(bool isCreator, bool isPublic)
    {
        if (isCreator || AdminAccess() || (CE_TotemNamingAccess.Value && isPublic))
        {
            return true;
        }

        return false;
    }

    public static bool GetShowMapPins() => CE_ShowMapPins.Value;
    public static bool GetShowTotemID() => CE_ShowTotemID.Value;

    public static TravelTotemType GetTravelTotemType()
    {
        string type = CE_TotemAccessDefault.Value.ToLower();
        switch (type)
        {
            case "alwaysunlock":
                return TravelTotemType.AlwaysUnlock;
            case "alwayslock":
                return TravelTotemType.AlwaysLock;
            case "findunlock":
                return TravelTotemType.FindUnlock;
            default:
                return TravelTotemType.Undefined;
        }
    }

    public static string GetTotemUnlockRequirementsString() => CE_TotemUnlockRequirementsString.Value;
    public static bool GetAllowSelectDefaultSpawn() => CE_AllowSelectDefaultSpawn.Value;
    public static bool GetAllowSelectTraders() => CE_AllowSelectTraders.Value;
    public static bool GetAllowSelectBed() => CE_AllowSelectBed.Value;
    public static bool GetAllowTotemClaiming() => CE_AllowTotemClaiming.Value;

    // Trader
    public static bool GetTradersSellTotemBomb() => CE_TradersSellTotemBomb.Value;
    public static int GetTradersSellTotemBombCost() => CE_TradersSellTotemBombCost.Value;

    // Locations
    public static int GetLocationAmountDefault() => CE_LocationAmountDefault.Value;
    public static int GetLocationAmountMeadows() => CE_LocationAmountMeadows.Value;
    public static int GetLocationAmountBlackForest() => CE_LocationAmountBlackForest.Value;
    public static int GetLocationAmountSwamp() => CE_LocationAmountSwamp.Value;
    public static int GetLocationAmountMountain() => CE_LocationAmountMountain.Value;
    public static int GetLocationAmountPlains() => CE_LocationAmountPlains.Value;
    public static int GetLocationAmountMistlands() => CE_LocationAmountMistlands.Value;
    public static int GetLocationAmountAshlands() => CE_LocationAmountAshlands.Value;
    public static int GetLocationSpacing() => CE_LocationSpacing.Value;

    // Pieces
    public static bool GetTravelTotemPieceRecipesEnabled() => CE_PieceRecipesEnabled.Value;
    public static string GetTravelTotemPieceCost() => CE_TravelTotemPieceCost.Value;
    public static string GetTravelTotemPieceMistlandsCost() => CE_TravelTotemPieceMistlandsCost.Value;
    public static string GetTravelTotemPieceAshlandsCost() => CE_TravelTotemPieceAshlandsCost.Value;


    public static bool GetTotemItemRecipesEnabled() => CE_ItemRecipesEnabled.Value;
    public static string GetTotemBombItemCost() => CE_TotemBombCraftCost.Value;

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
        const string client = "Client";
        const string trader = "Trader";
        const string location = "Location";
        const string pieces = "Pieces";
        const string items = "Items";

        AddConfig("TotemNamingAccess", general, "When false allow only admins (using AdminBypass) to rename public travel totems (boolean).",
            true, false, ref CE_TotemNamingAccess);
        AddConfig("TotemAccessDefault", general, "Default access type for new totems: AlwaysUnlock, FindUnlock, AlwaysLock (string).",
            true, "FindUnlock", ref CE_TotemAccessDefault);
        AddConfig("TotemUnlockRequirements", general, "Prefab amount pairs of required items to unlock a portal when access is set to TouchUnlock. " +
            "For example: Wood:2,SurtlingCore:2 (A comma-separated list of ITEM:QUANTITY pairs).",
            true, "", ref CE_TotemUnlockRequirementsString);
        AddConfig("AllowSelectDefaultSpawn", general, "When true allow teleporting to the starting stones icon when using totems (boolean).",
            true, true, ref CE_AllowSelectDefaultSpawn);
        AddConfig("AllowSelectTraders", general, "When true allow teleporting to discovered trader icons when using totems (boolean).",
            true, true, ref CE_AllowSelectTraders);
        AddConfig("AllowSelectBed", general, "When true allow teleporting to bed icons when using totems (boolean).",
            true, true, ref CE_AllowSelectBed);
        AddConfig("AllowTotemClaiming", general, "When true allow players to claim public totems they unlock as if they built them (boolean).",
            true, false, ref CE_AllowTotemClaiming);

        AddConfig("AdminBypass", client, "Allow local admins to access and change all travel totems at all times (boolean).",
            false, false, ref CE_AdminBypass);
        AddConfig("ShowMapPins", client, "Always show map pins of available Totems on the Player minimap (boolean).",
            false, true, ref CE_ShowMapPins);
        AddConfig("ShowTotemID", client, "Display the Totem ID when looking at a Totem (boolean).",
            false, false, ref CE_ShowTotemID);

        AddConfig("TradersSellTotemBomb", trader, "Adds Totem Bombs for sale by the traders (boolean).",
            false, true, ref CE_TradersSellTotemBomb);
        AddConfig("TradersSellTotemBombCost", trader, "Coins cost of the Totem Bomb (boolean).",
            false, 100, ref CE_TradersSellTotemBombCost);

        AddConfig("LocationAmountDefault", location, "Maximum number of undecorated, biome universal locations to add during world generation. " +
            "Set to 0 to disable (int).",
            true, 0, ref CE_LocationAmountDefault);
        AddConfig("LocationAmountMeadows", location, "Maximum number of Meadows locations to add during world generation. " +
            "Set to 0 to disable (int).",
            true, 20, ref CE_LocationAmountMeadows);
        AddConfig("LocationAmountBlackForest", location, "Maximum number of BlackForest locations to add during world generation. " +
            "Set to 0 to disable (int).",
            true, 20, ref CE_LocationAmountBlackForest);
        AddConfig("LocationAmountSwamp", location, "Maximum number of Swamp locations to add during world generation. " +
            "Set to 0 to disable (int).",
            true, 20, ref CE_LocationAmountSwamp);
        AddConfig("LocationAmountMountain", location, "Maximum number of Mountain locations to add during world generation. " +
            "Set to 0 to disable (int).",
            true, 20, ref CE_LocationAmountMountain);
        AddConfig("LocationAmountPlains", location, "Maximum number of Plains locations to add during world generation. " +
            "Set to 0 to disable (int).",
            true, 20, ref CE_LocationAmountPlains);
        AddConfig("LocationAmountMistlands", location, "Maximum number of Mistlands locations to add during world generation. " +
            "Set to 0 to disable (int).",
            true, 20, ref CE_LocationAmountMistlands);
        AddConfig("LocationAmountAshlands", location, "Maximum number of Ashlands locations to add during world generation. " +
            "Set to 0 to disable (int).",
            true, 10, ref CE_LocationAmountAshlands);

        AddConfig("LocationSpacing", location, "Minimum world distance between similar totems. " +
            "Setting also commonly known as \"MinDistanceFromSimilar\" (int).",
            true, 1500, ref CE_LocationSpacing);

        AddConfig("PieceRecipeOverridesEnabled", pieces, "True to use these cost configurations. " +
            "Set to false if using another mod to customize this (boolean).",
            true, true, ref CE_PieceRecipesEnabled);
        AddConfig("PieceCost", pieces, "Default Totem build cost, if left blank will be disabled " +
            "(A comma-separated list of ITEM:QUANTITY pairs).",
            true, AssetManager.TravelTotemPiece_Cost, ref CE_TravelTotemPieceCost);
        AddConfig("PieceMistlandsCost", pieces, "Mistlands Totem build cost, if left blank will be disabled " +
            "(A comma-separated list of ITEM:QUANTITY pairs).",
            true, AssetManager.TravelTotemPieceMistlands_Cost, ref CE_TravelTotemPieceMistlandsCost);
        AddConfig("PieceAshlandsCost", pieces, "Ashlands Totem build cost, if left blank will be disabled " +
            "(A comma-separated list of ITEM:QUANTITY pairs).",
            true, AssetManager.TravelTotemPieceAshlands_Cost, ref CE_TravelTotemPieceAshlandsCost);

        AddConfig("ItemRecipeOverridesEnabled", items, "True to use these cost configurations. " +
            "Set to false if using another mod to customize this (boolean).",
            true, true, ref CE_ItemRecipesEnabled);
        AddConfig("TotemBombCraftCost", items, "Totem Bomb crafting cost, if left blank will be disabled " +
            "(A comma-separated list of ITEM:QUANTITY pairs).",
            true, AssetManager.TotemBombCraft_Cost, ref CE_TotemBombCraftCost);

        #endregion

        TravelTotemsLogger.LogInfo("Praise be to the gods.");

        CheckForMultiplayerTweaks();

        ZoneManager.OnVanillaLocationsAvailable += AssetManager.AddTotemLocations;
        PrefabManager.OnVanillaPrefabsAvailable += AssetManager.AddPrefabs;

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
        SetupWatcher();

        TotemBundle = AssetUtils.LoadAssetBundleFromResources("vv_traveltotems", Assembly.GetExecutingAssembly());

        // Ensure configurations apply in singleplayer games
        ItemManager.OnItemsRegistered += AssetManager.UpdateItemConfigurations;
        PieceManager.OnPiecesRegistered += AssetManager.UpdatePieceConfigurations;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void CheckForMultiplayerTweaks()
    {
        try
        {
            MultiplayerTweaksInstalled = Chainloader.PluginInfos.TryGetValue(VVMTGUID, out PluginInfo mtPlugin);

            if (MultiplayerTweaksInstalled)
            {
                Type mtType = Assembly.GetAssembly(mtPlugin.Instance.GetType()).GetType(VVMTAPI);

                if (mtType != null)
                {
                    MethodInfo method = AccessTools.Method(mtType, "GetHaldorPinIndex");
                    if (method != null)
                    {
                        TravelTotemsLogger.LogInfo("Multiplayer Tweaks installed. Personal Trader pins can be tracked for teleportation!");
                        return;
                    }
                }

                MultiplayerTweaksInstalled = false;
                TravelTotemsLogger.LogWarning("Multiplayer Tweaks version mismatch! Please update Multiplayer Tweaks.");
            }
        }
        catch
        {
            MultiplayerTweaksInstalled = false;
        }

        if (!MultiplayerTweaksInstalled)
        {
            TravelTotemsLogger.LogDebug("Multiplayer Tweaks is not installed. Will not check pins for compatibility.");
        }
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
            TravelTotemsLogger.LogInfo("Attempting to reload configuration...");
            Config.Reload();
        }
        catch
        {
            TravelTotemsLogger.LogError($"There was an issue loading {ConfigFileName}");
            return;
        }

        _lastReloadTime = now;

        if (ZNet.instance != null && !ZNet.instance.IsDedicated())
        {
            TravelTotems.UpdateSettings();
            AssetManager.UpdateConfigurations();
            TravelTotemMap.RefreshMapPins();

            if (ZoneSystem.instance)
            {
                TravelTotemMap.RefreshLocationIcons();
            }
        }
    }

    public static bool IsInTheMainScene()
    {
        return SceneManager.GetActiveScene().name.Equals("main");
    }
}