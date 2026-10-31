using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Managers;

namespace VentureValheim.SlimChicks;

[BepInDependency(Jotunn.Main.ModGuid)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class SlimChicksPlugin : BaseUnityPlugin
{
    private const string ModName = "SlimChicks";
    private const string ModVersion = "1.0.1";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource SlimChicksLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public void Awake()
    {
        SlimChicksLogger.LogInfo("Let's get together and feel alright.");
        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
        
        PrefabManager.OnPrefabsRegistered += SlimChicks.SlimThoseChicks;
    }
}