using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.AreaRepair;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class AreaRepairPlugin : BaseUnityPlugin
{
    private const string ModName = "VentureAreaRepair";
    private const string ModVersion = "1.0.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource AreaRepairLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public void Awake()
    {
        AreaRepairLogger.LogInfo("Click, click, click, click.");

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
    }
}