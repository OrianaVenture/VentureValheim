using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace VentureValheim.NoUnlockSpam;

public class NoUnlockSpam
{
    public static bool HotkeyPressed = false;

    [HarmonyPatch(typeof(MessageHud), nameof(MessageHud.UpdateUnlockMsg))]
    public static class Patch_MessageHud_UpdateUnlockMsg
    {
        private static void Prefix(MessageHud __instance)
        {
            if (__instance.m_unlockMsgQueue.Count > 0 && (HotkeyPressed || !NoUnlockSpamPlugin.GetNotificationsEnabled()))
            {
                __instance.m_unlockMsgQueue.Clear();
                NoUnlockSpamPlugin.NoUnlockSpamLogger.LogDebug("Clearing messages!");
            }

            HotkeyPressed = false;
        }
    }

    // Thank you Redsekio for the template!
    [HarmonyPatch(typeof(Player))]
    static class PlayerPatch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Player), nameof(Player.UpdateHover))))
                .ThrowIfInvalid($"Could not patch Player.Update()!")
                .Advance(offset: 1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlayerPatch), nameof(UpdateInputDelegate))))
                .InstructionEnumeration();
        }

        static void UpdateInputDelegate(Player player, bool takeInput)
        {
            if (takeInput && ZInput.GetKeyDown(NoUnlockSpamPlugin.GetToggleKey()))
            {
                HotkeyPressed = true;
            }
        }
    }
}