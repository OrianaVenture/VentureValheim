using System;
using HarmonyLib;

namespace VentureValheim.TravelTotems;

public class TerminalCommands
{
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

            TravelTotemsPlugin.TravelTotemsLogger.LogInfo("Adding Terminal Commands for totem management.");

            try
            {
                new Terminal.ConsoleCommand("clearknowntraveltotems", "[optional: ID]", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length > 1)
                    {
                        TravelTotemPlayer.TryRemoveTotem(args[1]);
                    }
                    else
                    {
                        TravelTotemPlayer.ClearKnownTotems();
                    }
                }, isCheat: false, isNetwork: false, onlyServer: false);
                new Terminal.ConsoleCommand("addknowntraveltotem", "[ID]", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length > 1)
                    {
                        TravelTotemPlayer.TryAddTotem(args[1]);
                    }
                    else
                    {
                        args.Context.AddString("Syntax: addknowntraveltotem [ID such as 0:4]");
                    }
                }, isCheat: true, isNetwork: false, onlyServer: false);
            }
            catch (Exception e)
            {
                TravelTotemsPlugin.TravelTotemsLogger.LogWarning("Error, could not add terminal command. " +
                    "This can happen when two mods add the same command. " +
                    "The rest of this mod should work as expected.");
                TravelTotemsPlugin.TravelTotemsLogger.LogWarning(e);
            }
        }
    }
}