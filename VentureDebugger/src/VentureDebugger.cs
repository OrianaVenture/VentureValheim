using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace VentureValheim.VentureDebugger;

public class VentureDebugger
{
    /// <summary>
    /// Catches the exception of converting an invalid datetime and sets the pickable time to
    /// the current game time to prevent the error from occurring again.
    /// </summary>
    [HarmonyPatch(typeof(Pickable), nameof(Pickable.UpdateRespawn))]
    public static class Patch_Pickable_UpdateRespawn
    {
        private static Exception Finalizer(Pickable __instance, Exception __exception)
        {
            if (__instance != null && __exception != null)
            {
                VentureDebuggerPlugin.VentureDebuggerLogger.LogDebug("Catching exception and fixing:");
                VentureDebuggerPlugin.VentureDebuggerLogger.LogDebug(__exception);
                var timeNow = ZNet.instance.GetTime();
                __instance.m_nview.GetZDO().Set(ZDOVars.s_pickedTime, timeNow.Ticks);
            }

            return null;
        }
    }

    /// <summary>
    /// Fix adding GlobalKeys.PlayerEvents to the list multiple times
    /// </summary>
    [HarmonyPatch(typeof(ZPlayFabMatchmaking), "CreateLobby")]
    public static class Patch_ZPlayFabMatchmaking_CreateLobby
    {
        private static void Prefix()
        {
            RemoveDuplicates(ref ZPlayFabMatchmaking.m_instance.m_serverData.modifiers);
        }
    }

    /// <summary>
    /// Fix adding GlobalKeys.PlayerEvents to the list multiple times (server patch)
    /// </summary>
    [HarmonyPatch(typeof(ZSteamMatchmaking), "RegisterServer")]
    public static class Patch_ZSteamMatchmaking_RegisterServer
    {
        private static void Prefix(ref string[] modifiers)
        {
            RemoveDuplicates(ref modifiers);
        }
    }

    private static void RemoveDuplicates(ref string[] keys)
    {
        if (keys != null)
        {
            var fixedKeys = new HashSet<string>();
            foreach (string key in keys)
            {
                if (!fixedKeys.Contains(key))
                {
                    fixedKeys.Add(key);
                }
                else
                {
                    VentureDebuggerPlugin.VentureDebuggerLogger.LogWarning($"Found duplicate world modifier key {key}, fixing.");
                }
            }

            keys = fixedKeys.ToArray();
        }
    }
}