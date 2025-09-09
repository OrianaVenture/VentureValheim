using HarmonyLib;
using System.Collections.Generic;

namespace VentureValheim.Progression;

public partial class KeyManager
{
    private const string Haldor = "Haldor";
    private const string Hildir = "Hildir";
    private const string BogWitch = "BogWitch";

    /// <summary>
    /// Enables all of Haldor's items by bypassing key checking.
    /// </summary>
    [HarmonyPatch(typeof(Trader), nameof(Trader.GetAvailableItems))]
    public static class Patch_Trader_GetAvailableItems
    {
        private static bool Prefix(Trader __instance, ref List<Trader.TradeItem> __result)
        {
            var name = Utils.GetPrefabName(__instance.gameObject);

            if ((name.Equals(Haldor) && ProgressionConfiguration.Instance.GetUnlockAllHaldorItems()) ||
                (name.Equals(Hildir) && ProgressionConfiguration.Instance.GetUnlockAllHildirItems()) ||
                (name.Equals(BogWitch) && ProgressionConfiguration.Instance.GetUnlockAllBogWitchItems()))
            {
                __result = new List<Trader.TradeItem>(__instance.m_items);
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Fix error thrown when index is 0 and no items exist.
    /// </summary>
    [HarmonyPatch(typeof(StoreGui), nameof(StoreGui.SelectItem))]
    public static class Patch_StoreGui_SelectItem
    {
        private static void Prefix(StoreGui __instance, ref int index)
        {
            if (__instance.m_itemList.Count == 0)
            {
                index = -1;
            }
        }
    }

    /// <summary>
    /// Set up custom keys for Trader items.
    /// </summary>
    [HarmonyPatch(typeof(Trader), nameof(Trader.Start))]
    public static class Patch_Trader_Start
    {
        [HarmonyPriority(Priority.First)]
        private static void Postfix(Trader __instance)
        {
            var traderName = Utils.GetPrefabName(__instance.gameObject);

            Dictionary<string, string> items = null;

            if (traderName.Equals(Haldor))
            {
                items = Instance.GetTraderConfiguration(__instance.m_items);
            }
            else if (traderName.Equals(Hildir))
            {
                items = Instance.GetHildirConfiguration(__instance.m_items);
            }
            else if (traderName.Equals(BogWitch))
            {
                items = Instance.GetBogWitchConfiguration(__instance.m_items);
            }

            if (items == null)
            {
                return;
            }

            foreach (var item in __instance.m_items)
            {
                if (item.m_prefab != null)
                {
                    var name = Utils.GetPrefabName(item.m_prefab.gameObject);

                    if (items.ContainsKey(name))
                    {
                        item.m_requiredGlobalKey = items[name];
                    }
                }
            }
        }
    }
}
