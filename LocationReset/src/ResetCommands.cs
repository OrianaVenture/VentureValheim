using HarmonyLib;
using System;
using UnityEngine.SceneManagement;

namespace VentureValheim.LocationReset;

public static class ResetCommands
{
    /// <summary>
    /// Attempts to reset all locations in range.
    /// </summary>
    public static void ManualReset(int range)
    {
        UnityEngine.Vector3 point = Player.m_localPlayer.transform.position;
        UnityEngine.GameObject[] list = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int lcv = 0; lcv < list.Length; lcv++)
        {
            LocationProxy location = list[lcv].GetComponent<LocationProxy>();

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

    /// <summary>
    /// Attempts to reset all items stands in range.
    /// </summary>
    public static void ManualResetItemStands(int range)
    {
        UnityEngine.Vector3 point = Player.m_localPlayer.transform.position;
        UnityEngine.GameObject[] list = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int lcv = 0; lcv < list.Length; lcv++)
        {
            ItemStand stand = list[lcv].GetComponentInChildren<ItemStand>();

            if (stand != null && stand.m_nview != null && LocationReset.InBounds(point, stand.transform.position, range))
            {
                if (!stand.m_nview.IsOwner())
                {
                    stand.m_nview.ClaimOwnership();
                }

                stand.DropItem();
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

            new Terminal.ConsoleCommand("resetlocations", "[range]", delegate (Terminal.ConsoleEventArgs args)
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
            new Terminal.ConsoleCommand("resetitemstands", "[range]", delegate (Terminal.ConsoleEventArgs args)
            {
                if (args.Length > 1)
                {
                    int.TryParse(args[1], out int range);
                    range = Math.Min(range, maxRange);
                    ManualResetItemStands(range);
                    args.Context.AddString($"Resetting all item stands in range {range}...");
                }
                else
                {
                    ManualResetItemStands(20);
                    args.Context.AddString($"Resetting all item stands in default range {20}...");
                }
            }, isCheat: true, isNetwork: false, onlyServer: true);
        }
    }
}