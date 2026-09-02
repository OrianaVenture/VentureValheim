using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.MultiplayerTweaks;

public class TraderMapTweaks
{
    private const string HALDOR_LOC = "Vendor_BlackForest";
    private const string HILDIR_LOC = "Hildir_camp";
    private const string BOGWITCH_LOC = "BogWitch_Camp";

    // Random numbers for saving custom pin data, if collides with another mod will need to alter
    private const int HALDOR_INT = 123;
    private const int HILDIR_INT = 124;
    private const int BOGWITCH_INT = 125;

    internal static int HaldorIndex = -1;
    internal static int HildirIndex = -1;
    internal static int BogWitchIndex = -1;

    private static void TransformPinIdsToSave(ref List<Minimap.PinData> pins)
    {
        foreach (Minimap.PinData pin in pins)
        {
            int type = (int)pin.m_type;
            if (type == HaldorIndex)
            {
                pin.m_type = (Minimap.PinType)HALDOR_INT;
            }
            else if (type == HildirIndex)
            {
                pin.m_type = (Minimap.PinType)HILDIR_INT;
            }
            else if (type == BogWitchIndex)
            {
                pin.m_type = (Minimap.PinType)BOGWITCH_INT;
            }
        }
    }

    private static void TransformPinIdsToPlay(ref List<Minimap.PinData> pins)
    {
        foreach (Minimap.PinData pin in pins)
        {
            int type = (int)pin.m_type;
            if (type == HALDOR_INT)
            {
                pin.m_type = (Minimap.PinType)HaldorIndex;
            }
            else if (type == HILDIR_INT)
            {
                pin.m_type = (Minimap.PinType)HildirIndex;
            }
            else if (type == BOGWITCH_INT)
            {
                pin.m_type = (Minimap.PinType)BogWitchIndex;
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
    /// Append three new entries to the end of the m_visibleIconTypes list.
    /// Set the index as the "play" type to work with Minimap.Update.
    /// </summary>
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.Start))]
    private static class Patch_Minimap_Start
    {
        [HarmonyPriority(Priority.VeryLow)]
        private static void Postfix(Minimap __instance)
        {
            bool[] visibleIconsNew = new bool[__instance.m_visibleIconTypes.Length + 3];

            for (int lcv = 0; lcv < __instance.m_visibleIconTypes.Length; lcv++)
            {
                visibleIconsNew[lcv] = __instance.m_visibleIconTypes[lcv];
            }

            HaldorIndex = __instance.m_visibleIconTypes.Length;
            HildirIndex = __instance.m_visibleIconTypes.Length + 1;
            BogWitchIndex = __instance.m_visibleIconTypes.Length + 2;

            visibleIconsNew[HaldorIndex] = true;
            visibleIconsNew[HildirIndex] = true;
            visibleIconsNew[BogWitchIndex] = true;

            __instance.m_visibleIconTypes = visibleIconsNew;
        }
    }

    /// <summary>
    /// Setup custom pins. Set to the index for the m_visibleIconTypes list for update function ("play" type).
    /// </summary>
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.AddPin))]
    private static class Patch_Minimap_AddPin
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(ref Minimap.PinType type, out int __state)
        {
            int typeInt = (int)type;
            if (typeInt == HALDOR_INT)
            {
                type = (Minimap.PinType)HaldorIndex;
            }
            else if (typeInt == HILDIR_INT)
            {
                type = (Minimap.PinType)HildirIndex;
            }
            else if (typeInt == BOGWITCH_INT)
            {
                type = (Minimap.PinType)BogWitchIndex;
            }

            if ((int)type >= Minimap.instance.m_visibleIconTypes.Length)
            {
                MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogWarning("Minimap conversion type out of range of visible icons.");
            }
            else if (type < Minimap.PinType.Icon0)
            {
                MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogWarning("Minimap conversion type less than icon 0.");
            }

            __state = (int)type;
        }
    }

    [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetSprite))]
    private static class Patch_Minimap_GetSprite
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(Minimap.PinType type, ref Sprite __result)
        {
            int typeInt = (int)type;
            if (typeInt == HaldorIndex)
            {
                __result = Minimap.instance.GetLocationIcon(HALDOR_LOC);
                return false;
            }
            else if (typeInt == HildirIndex)
            {
                __result = Minimap.instance.GetLocationIcon(HILDIR_LOC);
                return false;
            }
            else if (typeInt == BogWitchIndex)
            {
                __result = Minimap.instance.GetLocationIcon(BOGWITCH_LOC);
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Add a new map pin when interacting with traders.
    /// </summary>
    [HarmonyPatch(typeof(Trader), nameof(Trader.Interact))]
    private static class Patch_Trader_Interact
    {
        private static void Postfix(Trader __instance)
        {
            string name = Utils.GetPrefabName(__instance.name);

            int pinType = -1;
            switch (name)
            {
                case "Haldor":
                    pinType = HaldorIndex;
                    break;
                case "Hildir":
                    pinType = HildirIndex;
                    break;
                case "BogWitch":
                    pinType = BogWitchIndex;
                    break;
            }

            if (pinType != -1 && !Minimap.instance.HavePinInRange(__instance.transform.position, 1f))
            {
                Minimap.PinData pin = Minimap.instance.AddPin(__instance.transform.position, (Minimap.PinType)pinType, "", true, false);
            }
        }
    }

    /// <summary>
    /// Removes any Haldor/Hildir locations from the icons list for a zone.
    /// This ensures they are not added to the player minimap.
    /// </summary>
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetLocationIcons))]
    private static class Patch_ZoneSystem_GetLocationIcons
    {
        private static void Postfix(ref Dictionary<Vector3, string> icons)
        {
            bool fixSpawn = !MultiplayerTweaksPlugin.GetEnableTempleMapPin() &&
                MultiplayerTweaksPlugin.GetPlayerDefaultSpawnPoint().IsNullOrWhiteSpace();

            if (!fixSpawn &&
                MultiplayerTweaksPlugin.GetEnableTempleMapPin() &&
                MultiplayerTweaksPlugin.GetEnableHaldorMapPin() &&
                MultiplayerTweaksPlugin.GetEnableHildirMapPin() &&
                MultiplayerTweaksPlugin.GetEnableBogWitchMapPin())
            {
                return;
            }

            var list = new List<Vector3>();
            foreach (var item in icons)
            {
                switch (item.Value)
                {
                    case "StartTemple":
                        if (!MultiplayerTweaksPlugin.GetEnableTempleMapPin() && !fixSpawn)
                        {
                            list.Add(item.Key);
                        }
                        break;
                    case HALDOR_LOC:
                        if (!MultiplayerTweaksPlugin.GetEnableHaldorMapPin())
                        {
                            list.Add(item.Key);
                        }
                        break;
                    case HILDIR_LOC:
                        if (!MultiplayerTweaksPlugin.GetEnableHildirMapPin())
                        {
                            list.Add(item.Key);
                        }
                        break;
                    case BOGWITCH_LOC:
                        if (!MultiplayerTweaksPlugin.GetEnableBogWitchMapPin())
                        {
                            list.Add(item.Key);
                        }
                        break;
                    default:
                        break;
                }
            }

            foreach (var item in list)
            {
                icons.Remove(item);
            }
        }
    }
}
