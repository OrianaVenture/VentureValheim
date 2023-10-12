using System;
using HarmonyLib;

namespace VentureValheim.VentureDebugger
{
    public class VentureDebugger
    {
        /// <summary>
        /// Catches the exception of converting an invalid datetime and sets the pickable time to
        /// the current game time to prevent the error from occurring again.
        /// </summary>
        [HarmonyPatch(typeof(Pickable), nameof(Pickable.UpdateRespawn))]
        public static class Patch_Pickable_UpdateRespawn
        {
            private static void Finalizer(Pickable __instance, Exception __exception)
            {
                if (__instance != null && __exception != null)
                {
                    VentureDebuggerPlugin.VentureDebuggerLogger.LogDebug("Catching exception and fixing:");
                    VentureDebuggerPlugin.VentureDebuggerLogger.LogDebug(__exception);
                    var timeNow = ZNet.instance.GetTime();
                    __instance.m_nview.GetZDO().Set(ZDOVars.s_pickedTime, timeNow.Ticks);
                }
            }
        }
    }
}