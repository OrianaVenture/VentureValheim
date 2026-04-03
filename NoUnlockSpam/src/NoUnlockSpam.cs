using HarmonyLib;

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

    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    public static class Patch_Player_Update
    {
        private static void Postfix(Player __instance)
        {
            if (__instance.TakeInput() && ZInput.GetKeyDown(NoUnlockSpamPlugin.GetToggleKey()))
            {
                HotkeyPressed = true;
            }
        }
    }
}