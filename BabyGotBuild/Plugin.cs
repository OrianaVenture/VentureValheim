using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.BabyGotBuild;

[BepInDependency(Jotunn.Main.ModGuid)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class BabyGotBuildPlugin : BaseUnityPlugin
{
    private const string ModName = "BabyGotBuild";
    private const string ModVersion = "1.0.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource BabyGotBuildLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public void Awake()
    {
        BabyGotBuildLogger.LogInfo("I like big builds and I cannot lie!");

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
    }
}