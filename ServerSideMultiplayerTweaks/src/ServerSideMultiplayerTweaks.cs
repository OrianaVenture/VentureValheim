using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace VentureValheim.ServerSideMultiplayerTweaks;

public class ServerSideMultiplayerTweaks
{
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_ServerSyncedPlayerData))]
    private static class Patch_ZNet_RPC_ServerSyncedPlayerData
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> RPCServerSyncedPlayerDataTranspiler(
            IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                .Start()
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(OpCodes.Ldarg_2),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZPackage), nameof(ZPackage.ReadBool))),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(ZNetPeer), nameof(ZNetPeer.m_publicRefPos))))
                .ThrowIfInvalid($"Could not patch ZNet.RPC_ServerSyncedPlayerData!")
                .Advance(offset: 3)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ServerSideMultiplayerTweaks), nameof(GetPositionVisibility))))
                .InstructionEnumeration();
        }
    }

    public static bool GetPositionVisibility(bool publicPosition)
    {
        if (ServerSideMultiplayerTweaksPlugin.GetOverridePlayerMapPins())
        {
            return ServerSideMultiplayerTweaksPlugin.GetForcePlayerMapPinsOn();
        }

        return publicPosition;
    }
}