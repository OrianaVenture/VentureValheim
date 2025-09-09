using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.ReelyGoodRod;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class ReelyGoodRodPlugin : BaseUnityPlugin
{
    private const string ModName = "ReelyGoodRod";
    private const string ModVersion = "0.1.1";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource ReelyGoodRodLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public void Awake()
    {
        ReelyGoodRodLogger.LogInfo("This is a roddery, hands up!");

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
    }

    
    /// <summary>
    /// Patch stamina drain for reeling in fishies.
    /// </summary>
    [HarmonyPatch(typeof(Fish), nameof(Fish.GetStaminaUse))]
    public static class Patch_Fish_GetStaminaUse
    {
        private static void Postfix(ref float __result)
        {
            __result *= 0.5f;
        }
    }
}