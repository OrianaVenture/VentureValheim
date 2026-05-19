using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Reflection;
using UnityEngine;

namespace VentureValheim.PiecesSmoothStone;

[BepInDependency(Jotunn.Main.ModGuid)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class PiecesSmoothStonePlugin : BaseUnityPlugin
{
    private const string ModName = "VenturePiecesSmoothStone";
    private const string ModVersion = "0.1.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource PiecesSmoothStoneLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public static GameObject Root;
    internal static AssetBundle StoneBundle;
    internal static Material StoneTexture;

    public void Awake()
    {
        PiecesSmoothStoneLogger.LogInfo("Man, it's a hot one.");

        // Create a dummy root object to reference
        Root = new GameObject("SmoothStoneRoot");
        Root.SetActive(false);
        DontDestroyOnLoad(Root);

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);

        PrefabManager.OnPrefabsRegistered += PiecesSmoothStone.Initialize;

        StoneBundle = AssetUtils.LoadAssetBundleFromResources("vv_smoothstone", Assembly.GetExecutingAssembly());
        StoneTexture = StoneBundle.LoadAsset<Material>("vv_smooth_stone");
    }
}