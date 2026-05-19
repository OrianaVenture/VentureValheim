using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Reflection;
using UnityEngine;

namespace VentureValheim.PiecesRoughStone;

[BepInDependency(Jotunn.Main.ModGuid)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class PiecesRoughStonePlugin : BaseUnityPlugin
{
    private const string ModName = "VenturePiecesRoughStone";
    private const string ModVersion = "0.1.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource PiecesRoughStoneLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public static GameObject Root;
    internal static AssetBundle StoneBundle;
    internal static Material StoneTexture;

    public void Awake()
    {
        PiecesRoughStoneLogger.LogInfo("My girlfriend turned into the moon.");

        // Create a dummy root object to reference
        Root = new GameObject("RoughStoneRoot");
        Root.SetActive(false);
        DontDestroyOnLoad(Root);

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);

        PrefabManager.OnPrefabsRegistered += PiecesRoughStone.Initialize;

        StoneBundle = AssetUtils.LoadAssetBundleFromResources("vv_roughstone", Assembly.GetExecutingAssembly());
        StoneTexture = StoneBundle.LoadAsset<Material>("vv_rough_stone");
    }
}