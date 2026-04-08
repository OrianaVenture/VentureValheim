using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace VentureValheim.MiningCaves;

public class WaterVolumePatches
{
    [HarmonyPatch(typeof(WaterVolume))]
    private static class WaterVolumeTranspilers
    {
        /// <summary>
        /// Set the liquid type to the custom component value for use in other methods.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(WaterVolume.GetLiquidType))]
        private static void GetLiquidTypePostfix(WaterVolume __instance, ref LiquidType __result)
        {
            if (__instance != null && __instance is VV_VariableLiquidVolume)
            {
                __result = (__instance as VV_VariableLiquidVolume).VariableLiquidType;
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(WaterVolume.OnTriggerEnter))]
        private static IEnumerable<CodeInstruction> OnTriggerEnterTranspiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldc_I4_0)) // LiquidType.Water (0)
                .ThrowIfInvalid("Could not patch WaterVolume.OnTriggerEnter()!")
                .RemoveInstruction()
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0), // this
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(WaterVolume), nameof(WaterVolume.GetLiquidType))))
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(WaterVolume.UpdateFloaters))]
        private static IEnumerable<CodeInstruction> UpdateFloatersTranspiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldc_I4_0), // LiquidType.Water (0)
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(IWaterInteractable), nameof(IWaterInteractable.SetLiquidLevel))))
                .ThrowIfInvalid("Could not patch WaterVolume.UpdateFloaters()!")
                .RemoveInstruction()
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0), // this
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(WaterVolume), nameof(WaterVolume.GetLiquidType))))
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(WaterVolume.OnTriggerExit))]
        private static IEnumerable<CodeInstruction> OnTriggerExitTranspiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldc_I4_0), // LiquidType.Water (0)
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(IWaterInteractable), nameof(IWaterInteractable.Decrement))))
                .ThrowIfInvalid("Could not patch WaterVolume.OnTriggerExit()!")
                .RemoveInstruction()
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0), // this
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(WaterVolume), nameof(WaterVolume.GetLiquidType))))
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldc_I4_0), // LiquidType.Water (0)
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(IWaterInteractable), nameof(IWaterInteractable.SetLiquidLevel))))
                .ThrowIfInvalid("Could not patch WaterVolume.OnTriggerExit()!")
                .RemoveInstruction()
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0), // this
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(WaterVolume), nameof(WaterVolume.GetLiquidType))))
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(WaterVolume.OnDestroy))]
        private static IEnumerable<CodeInstruction> OnDestroyTranspiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldc_I4_0), // LiquidType.Water (0)
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(IWaterInteractable), nameof(IWaterInteractable.Decrement))))
                .ThrowIfInvalid("Could not patch WaterVolume.OnDestroy()!")
                .RemoveInstruction()
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0), // this
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(WaterVolume), nameof(WaterVolume.GetLiquidType))))
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldc_I4_0), // LiquidType.Water (0)
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(IWaterInteractable), nameof(IWaterInteractable.SetLiquidLevel))))
                .ThrowIfInvalid("Could not patch WaterVolume.OnDestroy()!")
                .RemoveInstruction()
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0), // this
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(WaterVolume), nameof(WaterVolume.GetLiquidType))))
                .InstructionEnumeration();
        }
    }
}