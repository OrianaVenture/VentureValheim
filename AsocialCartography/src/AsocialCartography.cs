using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Splatform;
using UnityEngine;

namespace VentureValheim.AsocialCartography
{
    public class AsocialCartography
    {
        private static bool AllowedPin(Minimap.PinType pin)
        {
            if (pin == Minimap.PinType.Icon0 ||
                pin == Minimap.PinType.Icon1 ||
                pin == Minimap.PinType.Icon2 ||
                pin == Minimap.PinType.Icon3 ||
                pin == Minimap.PinType.Icon4)
            {
                return false;
            }

            if (!AsocialCartographyPlugin.GetIgnoreBossPins() &&
                pin == Minimap.PinType.Boss)
            {
                return false;
            }

            if (!AsocialCartographyPlugin.GetIgnoreHildirPins() && 
                (pin == Minimap.PinType.Hildir1 ||
                 pin == Minimap.PinType.Hildir2 ||
                 pin == Minimap.PinType.Hildir3))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets all the existing pins from the map data that are not already present
        /// on the player's current minimap instance.
        /// </summary>
        /// <param name="minimap">Player minimap instance</param>
        /// <param name="mapData">Cartography table data</param>
        /// <returns></returns>
        private static List<Minimap.PinData> GetMapPins(Minimap minimap, byte[] mapData)
        {
            List<Minimap.PinData> existingPins = new List<Minimap.PinData>();
            if (mapData != null)
            {
                try
                {
                    ZPackage zPackage = new ZPackage(mapData);
                    int version = zPackage.ReadInt();

                    // Advance the ZPackage pointer to map pin data
                    // Could write custom code for this but more prone to breaking
                    if (minimap.ReadExploredArray(zPackage, version) != null)
                    {
                        // Map version 2 last suppported in 217.14
                        // Map version 3 implemented in 217.22
                        if (version >= 2)
                        {
                            int total = zPackage.ReadInt();
                            for (int lcv = 0; lcv < total; lcv++)
                            {
                                long playerID = zPackage.ReadLong();
                                string name = zPackage.ReadString();
                                Vector3 pos = zPackage.ReadVector3();
                                Minimap.PinType type = (Minimap.PinType)zPackage.ReadInt();
                                bool isChecked = zPackage.ReadBool();
                                string author = ((version >= 3) ? zPackage.ReadString() : "");

                                if (!minimap.HavePinInRange(pos, 1f))
                                {
                                    var pin = new Minimap.PinData();
                                    pin.m_type = type;
                                    pin.m_name = name;
                                    pin.m_pos = pos;
                                    pin.m_save = true;
                                    pin.m_checked = isChecked;
                                    pin.m_ownerID = playerID;
                                    pin.m_author = new PlatformUserID(author);
                                    existingPins.Add(pin);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    AsocialCartographyPlugin.AsocialCartographyLogger.LogError("Caught an exception while parsing map data:");
                    AsocialCartographyPlugin.AsocialCartographyLogger.LogWarning(e);
                }
            }

            return existingPins;
        }

        /// <summary>
        /// Prevent player pins from being added to the table when config enabled.
        /// Merge existing pins from the table to the map data.
        /// </summary>
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetSharedMapData))]
        public static class Patch_Minimap_GetSharedMapData
        {
            private static void Prefix(Minimap __instance, byte[] oldMapData, out List<Minimap.PinData> __state)
            {
                // Preserve a copy of the player pins
                __state = __instance.m_pins.ToList();

                // Clean player pins list
                if (!AsocialCartographyPlugin.GetAddPins())
                {
                    __instance.m_pins = new List<Minimap.PinData>();
                    for (int lcv = 0; lcv < __state.Count; lcv++)
                    {
                        if (AllowedPin(__state[lcv].m_type))
                        {
                            __instance.m_pins.Add(__state[lcv]);
                        }
                    }
                }

                // Add existing pins after player pins are cleaned
                var pins = GetMapPins(__instance, oldMapData);
                __instance.m_pins.AddRange(pins);
            }

            private static void Postfix(Minimap __instance, List<Minimap.PinData> __state)
            {
                if (__state != null)
                {
                    __instance.m_pins = __state;
                }
            }
        }

        /// <summary>
        /// Prevent pins from being added to the player map when config enabled.
        /// </summary>
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.AddSharedMapData))]
        public static class Patch_Minimap_AddSharedMapData
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var method = AccessTools.Method(typeof(Minimap), nameof(Minimap.AddPin));
                for (var lcv = 0; lcv < codes.Count; lcv++)
                {
                    if (codes[lcv].opcode == OpCodes.Call)
                    {
                        if (codes[lcv].operand?.Equals(method) ?? false)
                        {
                            var methodCall = AccessTools.Method(typeof(AsocialCartography), nameof(AddPinReplacement));
                            codes[lcv] = new CodeInstruction(OpCodes.Call, methodCall);
                            break;
                        }
                    }
                }

                return codes.AsEnumerable();
            }
        }

        /// <summary>
        /// Minimap.AddPin replacement: Skip add pins when config disabled.
        /// </summary>
        public Minimap.PinData AddPinReplacement(Vector3 pos, Minimap.PinType type, string name, bool save, bool isChecked, long ownerID, PlatformUserID author)
        {
            if (AsocialCartographyPlugin.GetReceivePins() || AllowedPin(type))
            {
                return Minimap.instance.AddPin(pos, type, name, save, isChecked, ownerID, author);
            }

            return new Minimap.PinData();
        }
    }
}