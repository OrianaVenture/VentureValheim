using HarmonyLib;
using System;
using UnityEngine.SceneManagement;

namespace VentureValheim.LocationReset;

public static class ResetCommands
{
    /// <summary>
    /// Attempts to reset all locations in range.
    /// </summary>
    /// <param name="range"></param>
    public static void ManualReset(int range)
    {
        var point = Player.m_localPlayer.transform.position;
        var list = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int lcv = 0; lcv < list.Length; lcv++)
        {
            var location = list[lcv].GetComponent<LocationProxy>();

            if (location != null && LocationReset.InBounds(point, location.transform.position, range))
            {
                int hash = 0;
                if (location.m_nview != null && location.m_nview.GetZDO() != null)
                {
                    hash = location.m_nview.GetZDO().GetInt(ZDOVars.s_location);
                }

                LocationReset.Instance.TryReset(location, hash, true);
            }
        }
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

            LocationResetPlugin.LocationResetLogger.LogInfo("Adding Terminal Commands for location management.");
            const int maxRange = 100;

            new Terminal.ConsoleCommand("resetlocations", "[name]", delegate (Terminal.ConsoleEventArgs args)
            {
                if (args.Length > 1)
                {
                    int.TryParse(args[1], out int range);
                    range = Math.Min(range, maxRange);
                    ManualReset(range);
                    args.Context.AddString($"Resetting all in range {range}...");
                }
                else
                {
                    ManualReset(20);
                    args.Context.AddString($"Resetting all in default range {20}...");
                }
            }, isCheat: true, isNetwork: false, onlyServer: true);
        }
    }
}