using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace VentureValheim.MultiplayerTweaks;

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
                        codes[lcv - 5].opcode = OpCodes.Nop; // ldarg.0
                        codes[lcv - 4].opcode = OpCodes.Nop; // ldc.i4.0
                        codes[lcv - 3].opcode = OpCodes.Nop; // call
                        codes[lcv - 2].opcode = OpCodes.Nop; // ldstr
                        codes[lcv - 1].opcode = OpCodes.Nop; // callvirt
                        var methodCall = AccessTools.Method(typeof(ArrivalTweaks), nameof(SendArrivalMessage));
                        codes[lcv] = new CodeInstruction(OpCodes.Call, methodCall);
                        break;
                    }
                }
            }

            return codes.AsEnumerable();
        }
    }

    /// <summary>
    /// Disables the Valkrie on first spawn.
    /// Set the Player position as public or private if overridden.
    /// </summary>
    [HarmonyPatch(typeof(Game), nameof(Game.SpawnPlayer))]
    public static class Patch_Game_SpawnPlayer
    {
        private static void Prefix(Game __instance, ref Vector3 spawnPoint, ref bool spawnValkyrie)
        {
            if (spawnValkyrie && !MultiplayerTweaksPlugin.GetEnableValkrie())
            {
                spawnValkyrie = false;
                if (__instance.m_inIntro)
                {
                    // Redundant skip intro in case of ShowIntro patch failure
                    __instance.SkipIntro();
                }
            }

            if (MultiplayerTweaksPlugin.GetOverridePlayerMapPins())
            {
                ZNet.instance.SetPublicReferencePosition(MultiplayerTweaksPlugin.GetForcePlayerMapPinsOn());
            }
        }
    }


    /// <summary>
    /// Disables the intro text on first spawn.
    /// </summary>
    [HarmonyPatch(typeof(Game), nameof(Game.ShowIntro))]
    public static class Patch_Game_ShowIntro
    {
        private static bool Prefix(Game __instance)
        {
            if (!MultiplayerTweaksPlugin.GetEnableValkrie())
            {
                __instance.m_inIntro = false;
                return false; // Skip original
            }

            return true; //continue
        }
    }

    /// <summary>
    /// The game checks for a logout point first when respawning, set this value here
    /// when a custom spawn point is used and existing values for spawn points do not exist.
    /// </summary>
    [HarmonyPatch(typeof(Game), nameof(Game.FindSpawnPoint))]
    public static class Patch_Game_FindSpawnPoint
    {
        private static void Prefix(Game __instance)
        {
            if (!__instance.m_playerProfile.HaveLogoutPoint() &&
                !__instance.m_playerProfile.HaveCustomSpawnPoint() &&
                GetCustomSpawnPoint(out var point))
            {
                __instance.m_respawnAfterDeath = false;
                __instance.m_playerProfile.SetLogoutPoint(point);
            }
        }
    }

    /// <summary>
    /// Replacement method for ZoneSystem.GetLocationIcon
    /// </summary>
    /// <param name="iconname"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public static bool GetCustomSpawnPoint(out Vector3 position)
    {
        var point = MultiplayerTweaksPlugin.GetPlayerDefaultSpawnPoint();
        if (!point.IsNullOrWhiteSpace())
        {
            var coordinates = point.Split(',');
            if (coordinates.Length >= 3)
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
                    MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogError(
                        "Error setting the new spawn point. Check your configuration for formatting issues.");
                    MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogWarning(e);
                }
            }
            else
            {
                MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogError(
                    "Error setting the new spawn point. Check your configuration for formatting issues.");
            }
        }

        position = Vector3.zero;
        return false;
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
                    var methodCall = AccessTools.Method(typeof(ArrivalTweaks), nameof(SendMessageInRange));
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
