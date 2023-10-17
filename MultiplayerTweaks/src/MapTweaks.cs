using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace VentureValheim.MultiplayerTweaks
{
    public class MapTweaks
    {
        /// <summary>
        /// Removes any Haldor/Hildir locations from the icons list for a zone.
        /// This ensures they are not added to the player minimap.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetLocationIcons))]
        public static class Patch_ZoneSystem_GetLocationIcons
        {
            private static void Postfix(ref Dictionary<Vector3, string> icons)
            {
                if (MultiplayerTweaksPlugin.GetEnableHaldorMapPin() &&
                    MultiplayerTweaksPlugin.GetEnableHildirMapPin())
                {
                    return;
                }

                var list = new List<Vector3>();
                foreach (var item in icons)
                {
                    if (!MultiplayerTweaksPlugin.GetEnableHaldorMapPin() && item.Value.Equals("Vendor_BlackForest"))
                    {
                        list.Add(item.Key);
                    }
                    else if (!MultiplayerTweaksPlugin.GetEnableHildirMapPin() && item.Value.Equals("Hildir_camp"))
                    {
                        list.Add(item.Key);
                    }
                }

                foreach (var item in list)
                {
                    icons.Remove(item);
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        public static class Patch_Player_OnSpawned
        {
            /// <summary>
            /// Set the Player map position as public or private if overridden.
            /// </summary>
            private static void Postfix()
            {
                if (MultiplayerTweaksPlugin.GetOverridePlayerMapPins())
                {
                    ZNet.instance.SetPublicReferencePosition(MultiplayerTweaksPlugin.GetForcePlayerMapPinsOn());
                }
            }
        }

        /// <summary>
        /// Force the player's public position on or off based of configs.
        /// </summary>
        [HarmonyPatch(typeof(ZNet), nameof(ZNet.SetPublicReferencePosition))]
        public static class Patch_ZNet_SetPublicReferencePosition
        {
            private static void Prefix(ref bool pub)
            {
                if (MultiplayerTweaksPlugin.GetOverridePlayerMapPins())
                {
                    pub = MultiplayerTweaksPlugin.GetForcePlayerMapPinsOn();
                }
            }
        }
    }
}
