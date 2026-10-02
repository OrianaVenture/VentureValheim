using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Reflection;
using UnityEngine;

namespace VentureValheim.VentureSigns;

[BepInDependency(Jotunn.Main.ModGuid)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class VentureSignsPlugin : BaseUnityPlugin
{
    private const string ModName = "VentureSigns";
    private const string ModVersion = "1.0.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource VentureSignsLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public static GameObject Root;
    internal static AssetBundle SignBundle;

    public void Awake()
    {
        VentureSignsLogger.LogInfo("That trail that we blaze!");

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);

        SignBundle = AssetUtils.LoadAssetBundleFromResources("vv_signs", Assembly.GetExecutingAssembly());

        PrefabManager.OnPrefabsRegistered += PiecesSigns.Initialize;
    }
}