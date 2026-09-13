using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Reflection;
using UnityEngine;

namespace VentureValheim.TorchEmbers;

[BepInDependency(Jotunn.Main.ModGuid)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class TorchEmbersPlugin : BaseUnityPlugin
{
    private const string ModName = "TorchEmbers";
    private const string ModVersion = "1.0.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource TorchEmbersLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    internal static AssetBundle EmberBundle = null;

    public void Awake()
    {
        TorchEmbersLogger.LogInfo("I want to be 24 again and able to see in the dark lulz");

        EmberBundle = AssetUtils.LoadAssetBundleFromResources("vv_torchembers", Assembly.GetExecutingAssembly());

        CustomPrefab prefab = new CustomPrefab(EmberBundle, TorchEmbers.EMBER_PREFAB, true);
        PrefabManager.Instance.AddPrefab(prefab);
        PrefabManager.OnVanillaPrefabsAvailable += TorchEmbers.ApplyChanges;

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
    }
}