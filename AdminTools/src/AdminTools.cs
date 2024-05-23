using System;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.AdminTools
{
    public class AdminTools
    {
        // TODO easy character setup swap for RP
        private static string BedName = "Ragnar";

        /// <summary>
        /// Adds commands.
        /// </summary>
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

                AdminToolsPlugin.AdminToolsLogger.LogInfo("Adding Terminal Commands.");

                try
                {
                    new Terminal.ConsoleCommand("setbedname", "[name]", delegate (Terminal.ConsoleEventArgs args)
                    {
                        if (args.Length < 2)
                        {
                            args.Context.AddString("Syntax: setbedname [name]");
                            return;
                        }

                        BedName = args[1];

                    }, isCheat: false, isNetwork: false, onlyServer: false);
                }
                catch (Exception e)
                {
                    AdminToolsPlugin.AdminToolsLogger.LogWarning("Error, could not add terminal command. " +
                        "This can happen when two mods add the same command. " +
                        "The rest of this mod should work as expected.");
                    AdminToolsPlugin.AdminToolsLogger.LogWarning(e);
                }
            }
        }

        [HarmonyPatch(typeof(Bed), nameof(Bed.Interact))]
        private static class Patch_Bed_Interact
        {
            private static bool Prefix(Bed __instance)
            {
                if (ZInput.GetKey(KeyCode.RightAlt))
                {
                    // Set bed name
                    __instance.SetOwner(0L, BedName);

                    return false;
                }

                return true;
            }
        }
    }
}