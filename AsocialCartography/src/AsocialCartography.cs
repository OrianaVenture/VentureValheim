using HarmonyLib;
using Splatform;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace VentureValheim.AsocialCartography;

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
    /// Prevent player pins from being added to the table when config enabled.
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
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Minimap), nameof(Minimap.HavePinInRange))))
                .ThrowIfInvalid($"Could not patch Minimap.AddSharedMapData! (HavePinInRange not found)")
                .SetInstruction(Transpilers.EmitDelegate(AsocialCartographyPlugin.GetReceivePinRadius))
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Minimap), nameof(Minimap.GetClosestPin))))
                .ThrowIfInvalid($"Could not patch Minimap.AddSharedMapData! (GetClosestPin not found)")
                .SetInstruction(Transpilers.EmitDelegate(AsocialCartographyPlugin.GetReceivePinRadius))
                .MatchStartForward(
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Minimap), nameof(Minimap.AddPin))))
                .ThrowIfInvalid("Could not patch Minimap.AddSharedMapData! (AddPin not found)")
                .ExtractLabels(out List<Label> lables)
                .SetInstruction(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(AsocialCartography), nameof(AddPinReplacement))))
                .AddLabels(lables)
                .InstructionEnumeration();
        }
    }

    /// <summary>
    /// Minimap.AddPin replacement: Skip add pins when config disabled.
    /// </summary>
    public Minimap.PinData AddPinReplacement(Vector3 pos, Minimap.PinType type, string name, bool save,
        bool isChecked, long ownerID, PlatformUserID author)
    {
        if (AsocialCartographyPlugin.GetReceivePins() || AllowedPin(type))
        {
            return Minimap.instance.AddPin(pos, type, name, save, isChecked, ownerID, author);
        }

        return new Minimap.PinData();
    }
}