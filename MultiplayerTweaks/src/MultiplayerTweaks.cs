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
        private MultiplayerTweaks()
        {
        }
        private static readonly MultiplayerTweaks _instance = new MultiplayerTweaks();

        public static MultiplayerTweaks Instance
        {
            get => _instance;
        }

        public void Initialize()
        {
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
        [HarmonyPatch(typeof(Player), nameof(Player.ShowTutorial))]
        public static class Patch_Player_ShowTutorial
        {
            private static bool Prefix(Player __instance, string name)
            {
                if (!MultiplayerTweaksPlugin.GetEnableHuginTutorials())
                {
                    __instance.SetSeenTutorial(name);
                    return true; // Skip original
                }

                return false; // Continue
            }
        }

        /// <summary>
        /// When Tutorials disabled ensures the Raven prefab defaults tutorials to disabled.
        /// This will skip the Hugin tips at the beginning of the game.
        /// </summary>
        [HarmonyPatch(typeof(Raven), nameof(Raven.Awake))]
        public static class Patch_Game_Awake
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
                        if (codes[lcv].operand.Equals(method))
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

        /// <summary>
        /// Disables the Valkrie on first spawn.
        /// </summary>
        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        public static class Patch_Player_OnSpawned
        {
            private static void Prefix(Player __instance)
            {
                if (!MultiplayerTweaksPlugin.GetEnableValkrie())
                {
                    __instance.m_firstSpawn = false;
                }
            }
        }

        /// <summary>
        /// Patch the maximum player number for accepting new connections
        /// </summary>
        [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake))]
        public static class Patch_ZNet_Awake
        {
            private static void Postfix(ZNet __instance)
            {
                try
                {
                    int number = MultiplayerTweaksPlugin.GetMaximumPlayers();
                    if (number < 1)
                    {
                        number = 1;
                    }

                    __instance.m_serverPlayerLimit = number;
                }
                catch (Exception e)
                {
                    MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogError("Error patching ZNet.Awake with maximum player count.");
                    MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogError(e);
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
                    int number = MultiplayerTweaksPlugin.GetMaximumPlayers();
                    if (number < 1)
                    {
                        number = 1;
                    }

                    cPlayersMax = number;
                }
                catch (Exception e)
                {
                    MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogError("Error patching SteamGameServer.SetMaxPlayerCount with maximum player count.");
                    MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogError(e);
                }
            }
        }
    }
}