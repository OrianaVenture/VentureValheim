using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.LocationReset
{
    public static class LeviathanExtension
    {
        private const string LEVIATHAN_TIME = "VV_LeviathanTime";

        public static int GetLastSubmerged(this Leviathan leviathan)
        {
            return leviathan?.m_nview?.GetZDO()?.GetInt(LEVIATHAN_TIME, -1) ?? -1;
        }

        public static void SetLastSubmerged(this Leviathan leviathan, int day)
        {
            if (leviathan.m_nview != null && leviathan.m_nview.IsOwner())
            {
                leviathan.m_nview.GetZDO()?.Set(LEVIATHAN_TIME, day);
            }
        }

        /// <summary>
        /// Moves the Leviathan underwater if reset is enabled, otherwise destroys it.
        /// </summary>
        /// <param name="leviathan"></param>
        private static void MoveUnderwater(this Leviathan leviathan)
        {
            if (LocationResetPlugin.GetEnableLeviathanReset())
            {
                if (leviathan.m_body.position.y > 1f)
                {
                    Vector3 position = leviathan.m_body.position;
                    position.y = 0f;
                    leviathan.m_body.MovePosition(position);
                }
            }
            else
            {
                leviathan.m_nview?.Destroy();
            }
        }

        /// <summary>
        /// Checks if a Leviathan has expired and needs to be deleted, if so deletes it.
        /// </summary>
        /// <param name="leviathan"></param>
        /// <returns>True if check succeeds, false if client is not the owner</returns>
        public static bool CheckDelete(this Leviathan leviathan, out bool deleted)
        {
            deleted = false;

            if (leviathan.m_nview != null && leviathan.m_nview.IsOwner())
            {
                var time = leviathan.GetLastSubmerged();

                if (time != -1 && (LocationReset.GetGameDay() - time) >= LocationResetPlugin.GetLeviathanResetTime())
                {
                    var zdo = leviathan.m_nview?.GetZDO();
                    if (zdo != null)
                    {
                        ZDOMan.instance.DestroyZDO(zdo);
                        deleted = true;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Change the Destroy function from the Leviathans so they remain in the world after sinking.
        /// </summary>
        [HarmonyPatch(typeof(Leviathan), nameof(Leviathan.FixedUpdate))]
        public static class Patch_Leviathan_FixedUpdate
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var method = AccessTools.Method(typeof(ZNetView), nameof(ZNetView.Destroy));
                for (var lcv = 1; lcv < codes.Count; lcv++)
                {
                    if (codes[lcv].opcode == OpCodes.Callvirt)
                    {
                        if (codes[lcv].operand?.Equals(method) ?? false)
                        {
                            codes[lcv - 1].opcode = OpCodes.Nop;
                            var methodCall = AccessTools.Method(typeof(LeviathanExtension), nameof(LeviathanExtension.MoveUnderwater));
                            codes[lcv] = new CodeInstruction(OpCodes.Call, methodCall);
                            break;
                        }
                    }
                }

                return codes.AsEnumerable();
            }
        }

        /// <summary>
        /// Sets the day the Leviathan was triggered to sink.
        /// </summary>
        [HarmonyPatch(typeof(Leviathan), nameof(Leviathan.Leave))]
        public static class Patch_Leviathan_Leave
        {
            private static void Postfix(Leviathan __instance)
            {
                if (LocationResetPlugin.GetEnableLeviathanReset())
                {
                    if (__instance != null && __instance.m_left)
                    {
                        var day = LocationReset.GetGameDay();
                        var time = __instance.m_nview?.GetZDO()?.GetInt(LEVIATHAN_TIME, -1) ?? -1;
                        if (time == -1 || day - time >= LocationResetPlugin.GetLeviathanResetTime())
                        {
                            __instance.SetLastSubmerged(day);
                        }
                    }
                }
            }
        }
    }
}