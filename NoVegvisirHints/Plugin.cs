using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace VentureValheim.NoVegvisirHints;

[BepInDependency(Jotunn.Main.ModGuid)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class NoVegvisirHintsPlugin : BaseUnityPlugin
{
    private const string ModName = "NoVegvisirHints";
    private const string ModVersion = "0.1.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource NoVegvisirHintsLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public void Awake()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
    }
}

[HarmonyPatch(typeof(Game), nameof(Game.DiscoverClosestLocation))]
public static class Patch_Game_DiscoverClosestLocation
{
    private static bool Prefix(int pinType)
    {
        if (pinType == (int)Minimap.PinType.Boss ||
            pinType == (int)Minimap.PinType.Hildir1 ||
            pinType == (int)Minimap.PinType.Hildir2 ||
            pinType == (int)Minimap.PinType.Hildir3)
        {
            return false; // Skip method
        }

        return true;
    }
}