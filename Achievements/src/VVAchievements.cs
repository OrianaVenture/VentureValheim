using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace VentureValheim.VVAchievements;

public class VVAchievements
{
    /// <summary>
    /// Remove the IsModded check from evaluating cheated.
    /// </summary>
    [HarmonyPatch(typeof(Achievements))]
    private static class AchievementsTranspiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Achievements.IsCheatedAtAll))]
        private static IEnumerable<CodeInstruction> IsCheatedAtAllTranspiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(Game), nameof(Game.isModded))))
                .ThrowIfInvalid("Could not patch Achievements.IsCheatedAtAll()!")
                .RemoveInstruction()
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AchievementsTranspiler), nameof(ReturnFalse))))
                .InstructionEnumeration();
        }

        private static bool ReturnFalse()
        {
            return false;
        }
    }

    /// <summary>
    /// Clean up the bypass cheats flag from the player.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.Load))]
    public static class Patch_Player_Load
    {
        private static void Postfix(Player __instance)
        {
            if (!SceneManager.GetActiveScene().name.Equals("main"))
            {
                return;
            }

            __instance.AddUniqueKeyValue("bypasscheatchecks", "0");
        }
    }

    /// <summary>
    /// Treat the world as not cheated.
    /// </summary>
    [HarmonyPatch(typeof(Achievements), nameof(Achievements.IsWorldCheated))]
    public static class Patch_Achievements_IsWorldCheated
    {
        private static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}