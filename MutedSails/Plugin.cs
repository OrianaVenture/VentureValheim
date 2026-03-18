using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.MutedSails;

[BepInDependency(Jotunn.Main.ModGuid)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class MutedSailsPlugin : BaseUnityPlugin
{
    private const string ModName = "MutedSails";
    private const string ModVersion = "0.1.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource MutedSailsLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public void Awake()
    {
        MutedSailsLogger.LogInfo("This one took like way longer to figure out than I wanted. Bees!");

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
    }
}