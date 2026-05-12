using HarmonyLib;
using UnityEngine;

namespace VentureValheim.NoVegvisirHints;

public class NoVegvisirHints
{
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
        private static bool Prefix(Minimap.PinType type, Vector3 pos)
        {
            Minimap.PinType invalid = (Minimap.PinType)(-1);

            if (type == invalid)
            {
                NoVegvisirHintsPlugin.NoVegvisirHintsLogger.LogDebug("Map pin blocked!");

                if (Player.m_localPlayer != null && NoVegvisirHintsPlugin.GetSetLookDirection())
                {
                    Player.m_localPlayer.SetLookDir(pos - Player.m_localPlayer.transform.position, 3.5f);
                }

                return false;
            }

            return true;
        }
    }
}
