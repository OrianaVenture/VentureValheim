using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace VentureValheim.MultiplayerTweaks
{
    public class GeneralTweaks
    {
        private static bool _lastHitByPlayer = false;

        /// <summary>
        /// Checks if any connected player has the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsPlayer(ZDOID id)
        {
            if (id != null && !id.IsNone() && Player.s_players != null)
            {
                var players = Player.s_players;
                for (int lcv = 0; lcv < players.Count; lcv++)
                {
                    if (players[lcv].GetZDOID().Equals(id))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static int GetMaximumPlayers()
        {
            var number = MultiplayerTweaksPlugin.GetMaximumPlayers();

            if (number < 1)
            {
                number = 1;
            }

            return number;
        }

        /// <summary>
        /// When Tutorials disabled adds the tutorial to the seen list.
        /// This enables tutorial tracking even when Hugin is disabled.
        /// </summary>
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch(typeof(Player), nameof(Player.ShowTutorial))]
        public static class Patch_Player_ShowTutorial
        {
            private static bool Prefix(Player __instance, string name)
            {
                if (!Raven.m_tutorialsEnabled)
                {
                    __instance.SetSeenTutorial(name);
                    return false; // Skip original
                }

                return true; // Continue
            }
        }

        /// <summary>
        /// Patch the maximum player number for SteamGameServer
        /// </summary>
        [HarmonyPatch(typeof(SteamGameServer), nameof(SteamGameServer.SetMaxPlayerCount))]
        public static class Patch_SteamGameServer_SetMaxPlayerCount
        {
            private static void Prefix(ref int cPlayersMax)
            {
                try
                {
                    cPlayersMax = GetMaximumPlayers();
                    MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogInfo($"Steam Maximum Server Player Count set to {cPlayersMax}.");
                }
                catch (Exception e)
                {
                    MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogError("Error patching SteamGameServer.SetMaxPlayerCount with maximum player count.");
                    MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogError(e);
                }
            }
        }

        /// <summary>
        /// Patch the maximum player number for ZNet
        /// </summary>
        [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo))]
        public static class Patch_ZNet_RPC_PeerInfo
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var method = AccessTools.Method(typeof(ZNet), nameof(ZNet.GetNrOfPlayers));
                for (var lcv = 1; lcv < codes.Count; lcv++)
                {
                    if (codes[lcv].opcode == OpCodes.Ldc_I4_S)
                    {
                        if (codes[lcv - 1].operand?.Equals(method) ?? false)
                        {
                            var methodCall = AccessTools.Method(typeof(GeneralTweaks), nameof(GeneralTweaks.GetMaximumPlayers));
                            codes[lcv] = new CodeInstruction(OpCodes.Call, methodCall);
                            break;
                        }
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        public static class Patch_Player_OnSpawned
        {
            /// <summary>
            /// Set the PVP option if overridden.
            /// </summary>
            private static void Postfix(Player __instance)
            {
                if (MultiplayerTweaksPlugin.GetOverridePlayerPVP())
                {
                    __instance.m_nview?.GetZDO()?.Set(ZDOVars.s_pvp, MultiplayerTweaksPlugin.GetForcePlayerPVPOn());
                    __instance.m_pvp = MultiplayerTweaksPlugin.GetForcePlayerPVPOn();
                    InventoryGui.instance.m_pvp.isOn = MultiplayerTweaksPlugin.GetForcePlayerPVPOn();
                }
            }
        }

        /// <summary>
        /// Method used to set the interactable part on the PVP UI toggle.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.CanSwitchPVP))]
        public static class Patch_Player_CanSwitchPVP
        {
            private static void Postfix(ref bool __result)
            {
                if (MultiplayerTweaksPlugin.GetOverridePlayerPVP())
                {
                    __result = false;
                }
            }
        }

        /// <summary>
        /// Records if the last hit done to the local player was by another player.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.OnDamaged))]
        public static class Patch_Player_OnDamaged
        {
            private static void Postfix(HitData hit)
            {
                if (!MultiplayerTweaksPlugin.GetTeleportOnPVPDeath())
                {
                    if (IsPlayer(hit.m_attacker))
                    {
                        _lastHitByPlayer = true;
                    }
                    else
                    {
                        _lastHitByPlayer = false;
                    }
                }
            }
        }

        /// <summary>
        /// The game checks for a logout point first when respawning, set this value here.
        /// </summary>
        [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.SetDeathPoint))]
        public static class Patch_PlayerProfile_SetDeathPoint
        {
            private static void Postfix(PlayerProfile __instance, Vector3 point)
            {
                if (!MultiplayerTweaksPlugin.GetTeleportOnPVPDeath())
                {
                    if (_lastHitByPlayer)
                    {
                        __instance.SetLogoutPoint(point);
                    }
                }
            }
        }

        /// <summary>
        /// Prevents skill loss on PvP death when enabled.
        /// Higher than normal priority to skip other mod skill patches
        /// including World Advancement and Progression.
        /// </summary>
        [HarmonyPatch(typeof(Skills), nameof(Skills.LowerAllSkills))]
        public static class Patch_Skills_LowerAllSkills
        {
            [HarmonyPriority(Priority.HigherThanNormal)]
            private static bool Prefix()
            {
                if (!MultiplayerTweaksPlugin.GetSkillLossOnPVPDeath() && _lastHitByPlayer)
                {
                    return false; // Skip original method
                }

                return true; // Continue
            }
        }

        /// <summary>
        /// Reset the "last hit by player" tracker.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
        public static class Patch_Player_OnDeath
        {
            private static void Postfix()
            {
                _lastHitByPlayer = false;
            }
        }
    }
}
