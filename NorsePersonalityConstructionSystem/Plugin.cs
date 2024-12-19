using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace VentureValheim.NPCS;

[BepInDependency(Jotunn.Main.ModGuid)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)] //todo: update this when mod is finished
public class NPCSPlugin : BaseUnityPlugin
{
    private const string ModName = "NorsePersonalityConstructionSystem";
    private const string ModVersion = "0.0.8";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource NPCSLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public static GameObject Root;
    public static AssetBundle Assets;

    public const string MOD_PREFIX = "vvnpcs_";

    public void Awake()
    {

        NPCSLogger.LogInfo("You're finally awake!");

        // Create a dummy root object to reference
        Root = new GameObject("NPCSRoot");
        Root.SetActive(false);
        DontDestroyOnLoad(Root);

        PrefabManager.OnPrefabsRegistered += NPCFactory.AddNPCS;

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
    }
}