using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace VentureValheim.NoDamageFlash;

public class NoDamageFlash
{
    /// <summary>
    /// Disable damage flash by never applying it.
    /// </summary>
    [HarmonyPatch(typeof(Hud), nameof(Hud.DamageFlash))]
    public static class Patch_Hud_DamageFlash
    {
        private static bool Prefix()
        {
            return !NoDamageFlashPlugin.GetRemoveAllFlash();
        }
    }

    /// <summary>
    /// Remove flash from using blood magic weapons based off config.
    /// </summary>
    [HarmonyPatch(typeof(Character), nameof(Character.UseHealth))]
    public static class Patch_Character_UseHealth
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    useEnd: false,
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Character), nameof(Character.IsPlayer))))
                .ThrowIfInvalid($"Could not patch Player.UseHealth")
                .Advance(offset: 1)
                .InsertAndAdvance(Transpilers.EmitDelegate(FlashEnabledDelegate))
                .InstructionEnumeration();
        }

        private static bool FlashEnabledDelegate(bool isPlayerResult)
        {
            if (NoDamageFlashPlugin.GetRemoveUseHealthFlash())
            {
                return false;
            }

            return isPlayerResult;
        }
    }

    /// <summary>
    /// Remove flash from taking damage based off config.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.OnDamaged))]
    public static class Patch_Player_OnDamaged
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    useEnd: false,
                    new CodeMatch(OpCodes.Ldarg_1),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(HitData), nameof(HitData.GetTotalDamage))),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Character), nameof(Character.GetMaxHealth))),
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(OpCodes.Div))
                .ThrowIfInvalid($"Could not patch Player.OnDamaged")
                .Advance(offset: 6)
                .InsertAndAdvance(Transpilers.EmitDelegate(FlashEnabledDelegate))
                .SetOpcodeAndAdvance(OpCodes.Brfalse_S)
                .InstructionEnumeration();
        }

        private static bool FlashEnabledDelegate(float totalDamage, float maxHealthTenth)
        {
            return !NoDamageFlashPlugin.GetRemoveDamageFlash() && totalDamage > maxHealthTenth;
        }
    }

    /// <summary>
    /// Remove flash from puking based off config.
    /// </summary>
    [HarmonyPatch(typeof(SE_Puke), nameof(SE_Puke.UpdateStatusEffect))]
    public static class Patch_SE_Puke_UpdateStatusEffect
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    useEnd: false,
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Player), nameof(Player.RemoveOneFood))))
                .ThrowIfInvalid($"Could not patch SE_Puke.UpdateStatusEffect")
                .Advance(offset: 1)
                .InsertAndAdvance(Transpilers.EmitDelegate(FlashEnabledDelegate))
                .InstructionEnumeration();
        }

        private static bool FlashEnabledDelegate(bool consumedFood)
        {
            if (NoDamageFlashPlugin.GetRemovePukeFlash())
            {
                return false;
            }

            return consumedFood;
        }
    }
}
