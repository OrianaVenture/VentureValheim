using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace VentureValheim.MultiplayerTweaks
{
    public class ArrivalTweaks
    {
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

        /// <summary>
        /// Sends the message to all players in range of the target.
        /// </summary>
        /// <param name="baseAI"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        private static void SendMessageInRange(BaseAI baseAI, MessageHud.MessageType type, string message)
        {
            Player.MessageAllInRange(baseAI.transform.position, 100, type, message);
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
                            var methodCall = AccessTools.Method(typeof(ArrivalTweaks), nameof(ArrivalTweaks.SendArrivalMessage));
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
            /// </summary>
            private static void Postfix()
            {
                if (MultiplayerTweaksPlugin.GetOverridePlayerMapPins())
                {
                    ZNet.instance.SetPublicReferencePosition(MultiplayerTweaksPlugin.GetForcePlayerMapPinsOn());
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
                            var methodCall = AccessTools.Method(typeof(ArrivalTweaks), nameof(ArrivalTweaks.GetCustomSpawnPoint));
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
        /// Replaces the "MessageAll" calls for the BaseAI class with custom SendMessageInRange.
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns></returns>
        private static IEnumerable<CodeInstruction> ReplaceMessageAll(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var method = AccessTools.Method(typeof(MessageHud), nameof(MessageHud.MessageAll));
            for (var lcv = 4; lcv < codes.Count; lcv++)
            {
                if (codes[lcv].opcode == OpCodes.Callvirt)
                {
                    if (codes[lcv].operand?.Equals(method) ?? false)
                    {
                        codes[lcv - 4].opcode = OpCodes.Ldarg_0; // this
                        var methodCall = AccessTools.Method(typeof(ArrivalTweaks), nameof(ArrivalTweaks.SendMessageInRange));
                        codes[lcv] = new CodeInstruction(OpCodes.Call, methodCall);
                        break;
                    }
                }
            }

            return codes.AsEnumerable();
        }

        [HarmonyPatch(typeof(BaseAI), nameof(BaseAI.Awake))]
        public static class Patch_BaseAI_Awake
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return ReplaceMessageAll(instructions);
            }
        }

        [HarmonyPatch(typeof(BaseAI), nameof(BaseAI.OnDeath))]
        public static class Patch_BaseAI_OnDeath
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return ReplaceMessageAll(instructions);
            }
        }

        [HarmonyPatch(typeof(BaseAI), nameof(BaseAI.SetAlerted))]
        public static class Patch_BaseAI_SetAlerted
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return ReplaceMessageAll(instructions);
            }
        }
    }
}
