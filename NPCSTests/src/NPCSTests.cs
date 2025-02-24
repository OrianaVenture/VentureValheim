using System;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.NPCS
{
    public class NPCSTests
    {
        public static void SpawnTests()
        {
            ZoneSystem.instance.RemoveGlobalKey("ragnarbrave");
            ZoneSystem.instance.RemoveGlobalKey("ragnarbold");
            ZoneSystem.instance.RemoveGlobalKey("gave_pickaxe");
            ZoneSystem.instance.RemoveGlobalKey("liv");
            ZoneSystem.instance.RemoveGlobalKey("vivica1");
            ZoneSystem.instance.RemoveGlobalKey("vivica2");
            ZoneSystem.instance.RemoveGlobalKey("rahshahs1");
            ZoneSystem.instance.RemoveGlobalKey("rahshahs2");
            ZoneSystem.instance.RemoveGlobalKey("thaneofvalheim");

            Player.m_localPlayer.RemoveUniqueKey("ragnarbrave");
            Player.m_localPlayer.RemoveUniqueKey("ragnarbold");
            Player.m_localPlayer.RemoveUniqueKey("gave_pickaxe");
            Player.m_localPlayer.RemoveUniqueKey("liv");
            Player.m_localPlayer.RemoveUniqueKey("vivica1");
            Player.m_localPlayer.RemoveUniqueKey("vivica2");
            Player.m_localPlayer.RemoveUniqueKey("rahshahs1");
            Player.m_localPlayer.RemoveUniqueKey("rahshahs2");
            Player.m_localPlayer.RemoveUniqueKey("thaneofvalheim");

            var playerPosition = Player.m_localPlayer.gameObject.transform.position;
            var playerRotation = Player.m_localPlayer.gameObject.transform.rotation;
            var position = playerPosition + (playerRotation * Vector3.forward);
            NPCSPlugin.NPCSLogger.LogInfo("Trying to spawn NPCS!");
            NPCFactory.SpawnSavedNPC(position, Quaternion.identity, "Ragnar1");
            NPCFactory.SpawnSavedNPC(position + Vector3.right * 1, Quaternion.identity, "Ragnar2");
            NPCFactory.SpawnSavedNPC(position + Vector3.right * 2, Quaternion.identity, "Vivica");
            NPCFactory.SpawnSavedNPC(position + Vector3.right * 3, Quaternion.identity, "Liv");
            NPCFactory.SpawnSavedNPC(position + Vector3.right * 4, Quaternion.identity, "MrHamHands");
            NPCFactory.SpawnSavedNPC(position + Vector3.right * 5, Quaternion.identity, "Jarl");

            NPCFactory.SpawnSavedNPC(position + Vector3.left * 1, Quaternion.identity, "Boar");
            NPCFactory.SpawnSavedNPC(position + Vector3.left * 2, Quaternion.identity, "Wildir1");
            NPCFactory.SpawnSavedNPC(position + Vector3.left * 3, Quaternion.identity, "Cain1");
            NPCFactory.SpawnSavedNPC(position + Vector3.left * 4, Quaternion.identity, "Clown");
            NPCFactory.SpawnSavedNPC(position + Vector3.left * 5, Quaternion.identity, "Skelly");
        }

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

                NPCSPlugin.NPCSLogger.LogInfo("Adding Terminal Commands for npc spawning.");

                new Terminal.ConsoleCommand("spawntest", "", delegate (Terminal.ConsoleEventArgs args)
                {
                    SpawnTests();
                }, isCheat: true, isNetwork: false, onlyServer: false);
            }
        }
    }
}