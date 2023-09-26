using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace VentureValheim.MultiplayerTweaks
{
    public class MultiplayerTweaks
    {
        private MultiplayerTweaks() { }
        private static readonly MultiplayerTweaks _instance = new MultiplayerTweaks();

        public static MultiplayerTweaks Instance
        {
            get => _instance;
        }

        private bool _lastHitByPlayer = false;

        /// <summary>
        /// Checks if any connected player has the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsPlayer(ZDOID id)
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

        /// <summary>
        /// Removes any Haldor/Hildir locations from the icons list for a zone.
        /// This ensures they are not added to the player minimap.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetLocationIcons))]
        public static class Patch_ZoneSystem_GetLocationIcons
        {
            private static void Postfix(ref Dictionary<Vector3, string> icons)
            {
                if (MultiplayerTweaksPlugin.GetEnableHaldorMapPin() && 
                    MultiplayerTweaksPlugin.GetEnableHildirMapPin())
                {
                    return;
                }

                var list = new List<Vector3>();
                foreach (var item in icons)
                {
                    if (!MultiplayerTweaksPlugin.GetEnableHaldorMapPin() && item.Value.Equals("Vendor_BlackForest"))
                    {
                        list.Add(item.Key);
                    }
                    else if (!MultiplayerTweaksPlugin.GetEnableHildirMapPin() && item.Value.Equals("Hildir_camp"))
                    {
                        list.Add(item.Key);
                    }
                }

                foreach (var item in list)
                {
                    icons.Remove(item);
                }
            }
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
        /// Replaces the "I have arrived!" message call in Game.UpdateRespawn.
        /// </summary>
        [HarmonyPatch(typeof(Game), nameof(Game.UpdateRespawn))]
        public static class Patch_Game_UpdateRespawn
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var method = AccessTools.Method(typeof(Chat), nameof(Chat.SendText));
                for (var lcv = 5; lcv < codes.Count; lcv++)
                {
                    if (codes[lcv].opcode == OpCodes.Callvirt)
                    {
                        if (codes[lcv].operand?.Equals(method) ?? false)
                        {
                            codes[lcv - 5].opcode = OpCodes.Nop;
                            codes[lcv - 4].opcode = OpCodes.Nop;
                            codes[lcv - 3].opcode = OpCodes.Nop;
                            codes[lcv - 2].opcode = OpCodes.Nop;
                            codes[lcv - 1].opcode = OpCodes.Nop;
                            var methodCall = AccessTools.Method(typeof(MultiplayerTweaks), nameof(MultiplayerTweaks.SendArrivalMessage));
                            codes[lcv] = new CodeInstruction(OpCodes.Call, methodCall);
                            break;
                        }
                    }
                }

                return codes.AsEnumerable();
            }
        }

        /// <summary>
        /// Skips or sends the arrival message based off configurations.
        /// </summary>
        private static void SendArrivalMessage()
        {
            if (MultiplayerTweaksPlugin.GetEnableArrivalMessage())
            {
                Talker.Type talk = Talker.Type.Shout;
                if (!MultiplayerTweaksPlugin.GetEnableArrivalMessageShout())
                {
                    talk = Talker.Type.Normal;
                }

                string message = MultiplayerTweaksPlugin.GetOverrideArrivalMessage();
                if (message.IsNullOrWhiteSpace())
                {
                    message = Localization.instance.Localize("$text_player_arrived");
                }

                Chat.instance.SendText(talk, message);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        public static class Patch_Player_OnSpawned
        {
            /// <summary>
            /// Disables the Valkrie on first spawn.
            /// </summary>
            [HarmonyPriority(Priority.Last)]
            private static void Prefix(Player __instance)
            {
                if (!MultiplayerTweaksPlugin.GetEnableValkrie())
                {
                    __instance.m_firstSpawn = false;
                }
            }

            /// <summary>
            /// Set the Player position as public or private if overridden.
            /// Set the PVP option if overridden.
            /// </summary>
            private static void Postfix(Player __instance)
            {
                if (MultiplayerTweaksPlugin.GetOverridePlayerMapPins())
                {
                    ZNet.instance.SetPublicReferencePosition(MultiplayerTweaksPlugin.GetForcePlayerMapPinsOn());
                }

                if (MultiplayerTweaksPlugin.GetOverridePlayerPVP())
                {
                    __instance.m_nview?.GetZDO()?.Set(ZDOVars.s_pvp, MultiplayerTweaksPlugin.GetForcePlayerPVPOn());
                    __instance.m_pvp = MultiplayerTweaksPlugin.GetForcePlayerPVPOn();
                    InventoryGui.instance.m_pvp.isOn = MultiplayerTweaksPlugin.GetForcePlayerPVPOn();
                }
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
                            var methodCall = AccessTools.Method(typeof(MultiplayerTweaks), nameof(MultiplayerTweaks.GetMaximumPlayers));
                            codes[lcv] = new CodeInstruction(OpCodes.Call, methodCall);
                            break;
                        }
                    }
                }

                return codes.AsEnumerable();
            }
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
        /// Force the player's public position on or off based of configs.
        /// </summary>
        [HarmonyPatch(typeof(ZNet), nameof(ZNet.SetPublicReferencePosition))]
        public static class Patch_ZNet_SetPublicReferencePosition
        {
            private static void Prefix(ref bool pub)
            {
                if (MultiplayerTweaksPlugin.GetOverridePlayerMapPins())
                {
                    pub = MultiplayerTweaksPlugin.GetForcePlayerMapPinsOn();
                }
            }
        }

        /// <summary>
        /// Replace the ZoneSystem.GetLocationIcon method call with GetCustomSpawnPoint.
        /// </summary>
        [HarmonyPatch(typeof(Game), nameof(Game.FindSpawnPoint))]
        public static class Patch_Game_FindSpawnPoint
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var method = AccessTools.Method(typeof(ZoneSystem), nameof(ZoneSystem.GetLocationIcon));
                for (var lcv = 0; lcv < codes.Count; lcv++)
                {
                    if (codes[lcv].opcode == OpCodes.Callvirt)
                    {
                        if (codes[lcv].operand?.Equals(method) ?? false)
                        {
                            var methodCall = AccessTools.Method(typeof(MultiplayerTweaks), nameof(MultiplayerTweaks.GetCustomSpawnPoint));
                            codes[lcv] = new CodeInstruction(OpCodes.Callvirt, methodCall);
                            break;
                        }
                    }
                }

                return codes.AsEnumerable();
            }
        }

        /// <summary>
        /// Replacement method for ZoneSystem.GetLocationIcon
        /// </summary>
        /// <param name="iconname"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool GetCustomSpawnPoint(string iconname, out Vector3 position)
        {
            var point = MultiplayerTweaksPlugin.GetPlayerDefaultSpawnPoint();
            if (!point.IsNullOrWhiteSpace())
            {
                var coordinates = point.Split(',');
                if (coordinates.Length == 2)
                {
                    try
                    {
                        MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogWarning("Depreciated use of the x,z config format. Please use the new x,y,z format for best results!");

                        float x = float.Parse(coordinates[0]);
                        float z = float.Parse(coordinates[1]);

                        if (ZoneSystem.instance.GetGroundHeight(new Vector3(x, 0f, z), out var height))
                        {
                            position = new Vector3(x, height + 2f, z);
                            //MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogDebug($"Spawning at position: {x}, {height}, {z}.");
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogError("Error setting the new spawn point. Check your configuration for formatting issues.");
                        MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogError(e);
                    }
                }
                else if (coordinates.Length == 3)
                {
                    try
                    {
                        float x = float.Parse(coordinates[0]);
                        float y = float.Parse(coordinates[1]);
                        float z = float.Parse(coordinates[2]);

                        position = new Vector3(x, y, z);
                        //MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogDebug($"Spawning at position: {x}, {y}, {z}.");
                        return true;
                    }
                    catch (Exception e)
                    {
                        MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogError("Error setting the new spawn point. Check your configuration for formatting issues.");
                        MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogError(e);
                    }
                }
            }

            // Default to original behavior
            return ZoneSystem.instance.GetLocationIcon(iconname, out position);
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
                    if (Instance.IsPlayer(hit.m_attacker))
                    {
                        Instance._lastHitByPlayer = true;
                    }
                    else
                    {
                        Instance._lastHitByPlayer = false;
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
                    if (Instance._lastHitByPlayer)
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
                if (!MultiplayerTweaksPlugin.GetSkillLossOnPVPDeath() && Instance._lastHitByPlayer)
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
                Instance._lastHitByPlayer = false;
            }
        }
    }
}