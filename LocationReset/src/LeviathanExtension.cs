using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.LocationReset;

public static class LeviathanExtension
{
    private const string LEVIATHAN_TIME = "VV_LeviathanTime";

    public static int GetLastSubmerged(this Leviathan leviathan)
    {
        if (leviathan.m_nview != null && leviathan.m_nview.GetZDO() != null)
        {
            return leviathan.m_nview.GetZDO().GetInt(LEVIATHAN_TIME, -1);
        }

        return -1;
    }

    public static void SetLastSubmerged(this Leviathan leviathan, int day)
    {
        if (leviathan.m_nview != null && leviathan.m_nview.GetZDO() != null && leviathan.m_nview.IsOwner())
        {
            leviathan.m_nview.GetZDO().Set(LEVIATHAN_TIME, day);
        }
    }

    /// <summary>
    /// Moves the Leviathan underwater if reset is enabled, otherwise destroys it.
    /// It should have already submerged completely when this code is reached.
    /// </summary>
    /// <param name="leviathan"></param>
    private static void MoveUnderwater(this Leviathan leviathan)
    {
        if (leviathan == null)
        {
            return;
        }

        if (LocationResetPlugin.GetEnableLeviathanReset())
        {
            if (leviathan.transform.position.y > 0f)
            {
                Vector3 position = leviathan.transform.position;
                position.y = 0f;
                leviathan.transform.position = position;
            }
        }
        else if (leviathan.m_nview != null)
        {
            leviathan.m_nview.Destroy();
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
            int time = leviathan.GetLastSubmerged();

            if (time != -1 && (LocationReset.GetGameDay() - time) >= LocationResetPlugin.GetLeviathanResetTime())
            {
                ZDO zdo = leviathan.m_nview.GetZDO();
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
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            System.Reflection.MethodInfo method = AccessTools.Method(typeof(ZNetView), nameof(ZNetView.Destroy));
            for (int lcv = 1; lcv < codes.Count; lcv++)
            {
                if (codes[lcv].opcode == OpCodes.Callvirt)
                {
                    if (codes[lcv].operand?.Equals(method) ?? false)
                    {
                        codes[lcv - 1].opcode = OpCodes.Nop;
                        System.Reflection.MethodInfo methodCall = AccessTools.Method(typeof(LeviathanExtension), nameof(LeviathanExtension.MoveUnderwater));
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
                    int day = LocationReset.GetGameDay();
                    int time = __instance.GetLastSubmerged();
                    if (time == -1 || day - time >= LocationResetPlugin.GetLeviathanResetTime())
                    {
                        __instance.SetLastSubmerged(day);
                    }
                }
            }
        }
    }
}