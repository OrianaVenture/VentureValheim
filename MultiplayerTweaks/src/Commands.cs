using HarmonyLib;
using System;

namespace VentureValheim.MultiplayerTweaks;

public static class ResetCommands
{

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    private static class Patch_Terminal_InitTerminal
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(out bool __state)
        {
            __state = Terminal.m_terminalInitialized;
        }

        private static void Postfix(bool __state)
        {
            if (__state)
            {
                return;
            }

            MultiplayerTweaksPlugin.MultiplayerTweaksLogger.LogInfo("Adding Terminal Commands.");

            new Terminal.ConsoleCommand("surrender", "", delegate (Terminal.ConsoleEventArgs args)
            {
                args.Context.AddString("The Gods Accept Your Sacrifice.");

                if (Player.m_localPlayer == null || Game.instance.m_playerProfile == null)
                {
                    return;
                }

                // Clear the custom logout point set by this mod
                GeneralTweaks.OverrideRespawn();

                HitData hit = new HitData
                {
                    m_damage =
                    {
                        m_damage = 99999f
                    },
                    m_hitType = HitData.HitType.Self
                };

                Player.m_localPlayer.Damage(hit);
            }, isCheat: false, isNetwork: false, onlyServer: false);
        }
    }
}