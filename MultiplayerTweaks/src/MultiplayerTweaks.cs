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
            if (id != null && !id.IsNone() && Player.m_players != null)
            {
                var players = Player.m_players;
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
        /// Removes any Haldor locations from the icons list for a zone.
        /// This ensures they are not added to the player minimap.
        /// </summary>
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetLocationIcons))]
        public static class Patch_ZoneSystem_GetLocationIcons
        {
            private static void Postfix(ref Dictionary<Vector3, string> icons)
            {
                if (!MultiplayerTweaksPlugin.GetEnableHaldorMapPin())
                {
                    var list = new List<Vector3>();
                    foreach (var item in icons)
                    {
                        if (item.Value.Equals("Vendor_BlackForest"))
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
                if (!MultiplayerTweaksPlugin.GetEnableHuginTutorials())
                {
                    __instance.SetSeenTutorial(name);
                    return false; // Skip original
                }

                return true; // Continue
            }
        }

        /// <summary>
        /// When Tutorials disabled ensures the Raven prefab defaults tutorials to disabled.
        /// This will skip the Hugin tips at the beginning of the game.
        /// </summary>
        [HarmonyPatch(typeof(Raven), nameof(Raven.Awake))]
        public static class Patch_Raven_Awake
        {
            private static void Prefix()
            {
                if (!MultiplayerTweaksPlugin.GetEnableHuginTutorials())
                {
                    Raven.m_tutorialsEnabled = false;
                }
            }
        }

        /// <summary>
        /// Removes sending the "I have arrived!" message from Game.UpdateRespawn.
        /// If the first spawn message is enabled it is sent in the postfix with the chosen configuration options.
        /// </summary>
        [HarmonyPatch(typeof(Game), nameof(Game.UpdateRespawn))]
        public static class Patch_Game_UpdateRespawn
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (var lcv = 3; lcv < codes.Count; lcv++)
                {
                    if (codes[lcv].opcode == OpCodes.Callvirt)
                    {
                        var method = AccessTools.Method(typeof(Chat), nameof(Chat.SendText));
                        if (codes[lcv].operand?.Equals(method) ?? false)
                        {
                            codes[lcv - 3].opcode = OpCodes.Nop;
                            codes[lcv - 2].opcode = OpCodes.Nop;
                            codes[lcv - 1].opcode = OpCodes.Nop;
                            codes[lcv].opcode = OpCodes.Nop;
                            break;
                        }
                    }
                }

                return codes.AsEnumerable();
            }

            [HarmonyPriority(Priority.First)]
            private static void Prefix(Game __instance, out bool __state)
            {
                __state = __instance.m_firstSpawn;
            }

            private static void Postfix(bool __state)
            {
                if (__state && MultiplayerTweaksPlugin.GetEnableArrivalMessage())
                {
                    Talker.Type talk = Talker.Type.Shout;
                    if (!MultiplayerTweaksPlugin.GetEnableArrivalMessageShout())
                    {
                        talk = Talker.Type.Normal;
                    }

                    string message = MultiplayerTweaksPlugin.GetOverrideArrivalMessage();
                    if (message.IsNullOrWhiteSpace())
                    {
                        message = "I have arrived!";
                    }

                    Chat.instance.SendText(talk, message);
                }
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
                    __instance.m_nview?.GetZDO()?.Set("pvp", MultiplayerTweaksPlugin.GetForcePlayerPVPOn());
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
                for (var lcv = 1; lcv < codes.Count; lcv++)
                {
                    if (codes[lcv].opcode == OpCodes.Ldc_I4_S)
                    {
                        var method = AccessTools.Method(typeof(ZNet), nameof(ZNet.GetNrOfPlayers));
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

        private static sbyte GetMaximumPlayers()
        {
            sbyte number = (sbyte)MultiplayerTweaksPlugin.GetMaximumPlayers();

            if (number < 1)
            {
                number = 1;
            }

            return number;
        }

        /// <summary>
        /// Skips Player map pins updates if overridden and configured always off.
        /// </summary>
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdatePlayerPins))]
        public static class Patch_Minimap_UpdatePlayerPins
        {
            private static bool Prefix()
            {
                if (MultiplayerTweaksPlugin.GetOverridePlayerMapPins() && !MultiplayerTweaksPlugin.GetForcePlayerMapPinsOn())
                {
                    return false; // Skip original
                }

                return true; // Continue
            }
        }

        /// <summary>
        /// Disable the Player map pin toggle if overridden.
        /// </summary>
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnTogglePublicPosition))]
        public static class Patch_Minimap_OnTogglePublicPosition
        {
            private static bool Prefix()
            {
                if (MultiplayerTweaksPlugin.GetOverridePlayerMapPins())
                {
                    return false; // Skip original
                }

                return true; // Continue
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
                for (var lcv = 0; lcv < codes.Count; lcv++)
                {
                    if (codes[lcv].opcode == OpCodes.Callvirt)
                    {
                        var method = AccessTools.Method(typeof(ZoneSystem), nameof(ZoneSystem.GetLocationIcon));
                        if (codes[lcv].operand?.Equals(method) ?? false)
                        {
                            var methodCall = AccessTools.Method(typeof(MultiplayerTweaks), nameof(MultiplayerTweaks.GetCustomSpawnPoint));
                            codes[lcv] = new CodeInstruction(OpCodes.Call, methodCall);
                            break;
                        }
                    }
                }

                return codes.AsEnumerable();
            }
        }

        private static void GetCustomSpawnPoint(string iconname, out Vector3 position)
        {
            var point = MultiplayerTweaksPlugin.GetPlayerDefaultSpawnPoint();
            if (!point.IsNullOrWhiteSpace())
            {
                var coordinates = point.Split(',');
                if (coordinates.Length == 2)
                {
                    try
                    {
                        float x = float.Parse(coordinates[0]);
                        float z = float.Parse(coordinates[1]);
                        if (ZoneSystem.instance.GetGroundHeight(new Vector3(x, 0, z), out var height))
                        {
                            position = new Vector3(x, height + 2f, z);
                            MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogDebug($"Spawning at position: {x}, {height}, {z}.");
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogError("Error setting the new spawn point. Check your configuration for formatting issues.");
                        MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogError(e);
                    }
                }
            }

            position = Vector3.zero;
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
    }
}