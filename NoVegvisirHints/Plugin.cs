using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace VentureValheim.NoVegvisirHints;

[BepInDependency(Jotunn.Main.ModGuid)]
[BepInPlugin(ModGUID, ModName, ModVersion)]
public class NoVegvisirHintsPlugin : BaseUnityPlugin
{
    private const string ModName = "NoVegvisirHints";
    private const string ModVersion = "0.2.0";
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

/// <summary>
/// Set any Vegvisir location to an invalid pin to prevent map pin from being discovered.
/// </summary>
[HarmonyPatch(typeof(Vegvisir), nameof(Vegvisir.Interact))]
public static class Patch_Vegvisir_Interact
{
    private static void Prefix(Vegvisir __instance)
    {
        if (__instance.m_locations.Count > 1)
        {
            // Do not block Hildir map pins
            return;
        }

        foreach (Vegvisir.VegvisrLocation location in __instance.m_locations)
        {
            location.m_pinType = (Minimap.PinType)(-1);
        }
    }
}

/// <summary>
/// Intercept and skip method for any invalid pins.
/// </summary>
[HarmonyPatch(typeof(Minimap), nameof(Minimap.DiscoverLocation))]
public static class Patch_Minimap_DiscoverLocation
{
    private static bool Prefix(Minimap.PinType type)
    {
        Minimap.PinType invalid = (Minimap.PinType)(-1);

        if (type == invalid)
        {
            NoVegvisirHintsPlugin.NoVegvisirHintsLogger.LogInfo("Map pin blocked!");
            return false;
        }

        return true;
    }
}