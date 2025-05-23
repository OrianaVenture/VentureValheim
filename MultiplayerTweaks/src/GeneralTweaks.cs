﻿using HarmonyLib;
using UnityEngine;

namespace VentureValheim.MultiplayerTweaks;

public class GeneralTweaks
{
    private static bool _overrideRespawn = false;
    private static bool _lastHitByPlayer = false;
    private static double _lastSpawnTime = 0d;

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

    public static void OverrideRespawn()
    {
        Game.instance.m_playerProfile.ClearLoguoutPoint();
        _overrideRespawn = true;
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
    /// Set the PVP option if overridden.
    /// Set local player tracking data.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
    public static class Patch_Player_OnSpawned
    {
        private static void Postfix(Player __instance)
        {
            if (MultiplayerTweaksPlugin.GetOverridePlayerPVP())
            {
                __instance.m_nview?.GetZDO()?.Set(ZDOVars.s_pvp, MultiplayerTweaksPlugin.GetForcePlayerPVPOn());
                __instance.m_pvp = MultiplayerTweaksPlugin.GetForcePlayerPVPOn();
                InventoryGui.instance.m_pvp.isOn = MultiplayerTweaksPlugin.GetForcePlayerPVPOn();
            }

            _lastSpawnTime = ZNet.instance.GetTimeSeconds();
            _lastHitByPlayer = false;
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

    /// <summary>
    /// The game checks for a logout point first when respawning, set this value here
    /// when teleporting on death is disabled.
    /// </summary>
    [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.SetDeathPoint))]
    public static class Patch_PlayerProfile_SetDeathPoint
    {
        private static void Postfix(PlayerProfile __instance, Vector3 point)
        {
            if (_overrideRespawn)
            {
                _overrideRespawn = false;
                return;
            }

            if (!MultiplayerTweaksPlugin.GetTeleportOnAnyDeath() ||
                (!MultiplayerTweaksPlugin.GetTeleportOnPVPDeath() && _lastHitByPlayer))
            {
                __instance.SetLogoutPoint(point);
            }
        }
    }

    /// <summary>
    /// Have to trick the game into using the logout point on death.
    /// </summary>
    [HarmonyPatch(typeof(Game), nameof(Game.RequestRespawn))]
    public static class Patch_Game_RequestRespawn
    {
        private static void Prefix(ref bool afterDeath)
        {
            if (!MultiplayerTweaksPlugin.GetTeleportOnAnyDeath() ||
                (!MultiplayerTweaksPlugin.GetTeleportOnPVPDeath() && _lastHitByPlayer))
            {
                afterDeath = false;
            }
        }
    }

    /// <summary>
    /// Prevents skill loss on death when enabled.
    /// Higher than normal priority to allow checking in other mod 
    /// skill patches including World Advancement and Progression.
    /// </summary>
    [HarmonyPatch(typeof(Skills), nameof(Skills.LowerAllSkills))]
    public static class Patch_Skills_LowerAllSkills
    {
        [HarmonyPriority(Priority.HigherThanNormal)]
        private static bool Prefix()
        {
            if (!MultiplayerTweaksPlugin.GetSkillLossOnAnyDeath() ||
                (!MultiplayerTweaksPlugin.GetSkillLossOnPVPDeath() && _lastHitByPlayer))
            {
                return false; // Skip original method
            }

            return true; // Continue
        }
    }

    /// <summary>
    /// Prevents skill loss on death when enabled and using the
    /// DeathSkillsReset global key (hardcore death modifier).
    /// </summary>
    [HarmonyPatch(typeof(Skills), nameof(Skills.Clear))]
    public static class Patch_Skills_Clear
    {
        [HarmonyPriority(Priority.HigherThanNormal)]
        private static bool Prefix()
        {
            if (!MultiplayerTweaksPlugin.GetSkillLossOnAnyDeath() ||
                (!MultiplayerTweaksPlugin.GetSkillLossOnPVPDeath() && _lastHitByPlayer))
            {
                return false; // Skip original method
            }

            return true; // Continue
        }
    }

    /// <summary>
    /// Add grace window to respawned players to prevent death loops.
    /// </summary>
    [HarmonyPatch(typeof(Character), nameof(Character.CheckDeath))]
    public static class Patch_Character_CheckDeath
    {
        private static bool Prefix(Character __instance)
        {
            if (__instance.IsPlayer() && __instance == Player.m_localPlayer)
            {
                // 15 seconds of immunity
                var time = ZNet.instance.GetTimeSeconds() - _lastSpawnTime;
                if (time < 15d)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Changes the display day for the new day message and other mods calling this method.
    /// </summary>
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.GetCurrentDay))]
    public static class Patch_EnvMan_GetCurrentDay
    {
        private static void Postfix(ref int __result)
        {
            var offset = MultiplayerTweaksPlugin.GetGameDayOffset();
            __result -= offset;
            if (__result < 0)
            {
                __result = 0;
            }
        }
    }

    /// <summary>
    /// Change display day of death pins.
    /// </summary>
    [HarmonyPatch(typeof(Minimap), nameof(Minimap.AddPin))]
    public static class Patch_Minimap_AddPin
    {
        private static void Prefix(Minimap.PinType type, ref string name)
        {
            if (type == Minimap.PinType.Death)
            {
                name = $"$hud_mapday {EnvMan.instance.GetCurrentDay()}";
            }
        }
    }

    /// <summary>
    /// Always hide the platform id when disabled.
    /// </summary>
    /*[HarmonyPatch(typeof(UserInfo), nameof(UserInfo.GamertagSuffix))]
    public static class Patch_UserInfo_GamertagSuffix
    {
        private static void Postfix(ref string __result)
        {
            if (MultiplayerTweaksPlugin.GetHidePlatformTag())
            {
                __result = "";
            }
        }
    }*/ // TODO fix this
}
