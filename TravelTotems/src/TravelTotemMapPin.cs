using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.TravelTotems;

public class TravelTotemMapPin
{
    // Random numbers for saving custom pin data, if collides with another mod will need to alter
    private const int TOTEM_INT = 126;

    internal static int TotemIndex = -1;

    public static Minimap.PinType GetTotemPinType()
    {
        return (Minimap.PinType)TotemIndex;
    }

    private static void TransformPinIdsToSave(ref List<Minimap.PinData> pins)
    {
        foreach (Minimap.PinData pin in pins)
        {
            int type = (int)pin.m_type;
            if (type == TotemIndex)
            {
                pin.m_type = (Minimap.PinType)TOTEM_INT;
            }
        }
    }

    private static void TransformPinIdsToPlay(ref List<Minimap.PinData> pins)
    {
        foreach (Minimap.PinData pin in pins)
        {
            int type = (int)pin.m_type;
            if (type == TOTEM_INT)
            {
                pin.m_type = (Minimap.PinType)TotemIndex;
            }
        }
    }

    [HarmonyPatch(typeof(Minimap))]
    private static class Patch_Minimap_GetMapData
    {
        /// <summary>
        /// Set pins types to "save" type integers for identification.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPatch(nameof(Minimap.GetSharedMapData))]
        [HarmonyPatch(nameof(Minimap.GetMapData))]
        private static void Prefix(ref Minimap __instance)
        {
            TransformPinIdsToSave(ref __instance.m_pins);
        }

        /// <summary>
        /// Set pin types to "play" type integers dynamically set for the m_visibleIconTypes list.
        /// This prevents errors from being thrown in Minimap.Update.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPriority(Priority.VeryLow)]
        [HarmonyPatch(nameof(Minimap.GetSharedMapData))]
        [HarmonyPatch(nameof(Minimap.GetMapData))]
        private static void Postfix(ref Minimap __instance)
        {
            TransformPinIdsToPlay(ref __instance.m_pins);
        }
    }

    /// <summary>
    /// Append one new entry to the end of the m_visibleIconTypes list.
    /// Set the index as the "play" type to work with Minimap.Update.
    /// </summary>
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Start))]
    private static class Patch_Minimap_Start
    {
        [HarmonyPriority(Priority.VeryLow)]
        private static void Postfix(Minimap __instance)
        {
            bool[] visibleIconsNew = new bool[__instance.m_visibleIconTypes.Length + 1];

            for (int lcv = 0; lcv < __instance.m_visibleIconTypes.Length; lcv++)
            {
                visibleIconsNew[lcv] = __instance.m_visibleIconTypes[lcv];
            }

            TotemIndex = __instance.m_visibleIconTypes.Length;
            visibleIconsNew[TotemIndex] = true;

            __instance.m_visibleIconTypes = visibleIconsNew;
        }
    }

    /// <summary>
    /// Setup custom pin. Set to the index for the m_visibleIconTypes list for update function ("play" type).
    /// </summary>
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.AddPin))]
    private static class Patch_Minimap_AddPin
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(ref Minimap.PinType type)
        {
            int typeInt = (int)type;
            if (typeInt == TOTEM_INT)
            {
                type = (Minimap.PinType)TotemIndex;
            }

            if ((int)type >= Minimap.instance.m_visibleIconTypes.Length)
            {
                TravelTotemsPlugin.TravelTotemsLogger.LogWarning("Minimap conversion type out of range of visible icons.");
            }
            else if (type < Minimap.PinType.Icon0)
            {
                TravelTotemsPlugin.TravelTotemsLogger.LogWarning("Minimap conversion type less than icon 0.");
            }
        }
    }

    /// <summary>
    /// Intercept the display sprite type of the Totems.
    /// </summary>
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetSprite))]
    private static class Patch_Minimap_GetSprite
    {
        [HarmonyPriority(Priority.First)]
        private static bool Prefix(ref Minimap.PinType type, ref Sprite __result)
        {
            if (type == (Minimap.PinType)TotemIndex)
            {
                __result = AssetManager.TotemMapSprite;
                return false;
            }

            __result = null;
            return true;
        }
    }
}
