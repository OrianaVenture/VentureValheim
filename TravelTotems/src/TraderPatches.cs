using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

namespace VentureValheim.TravelTotems;

public class TraderPatches
{
    [HarmonyPatch(typeof(Trader), nameof(Trader.Start))]
    public static class Patch_Trader_Start
    {
        private static void Postfix(Trader __instance)
        {
            if (TravelTotemsPlugin.GetTradersSellTotemBomb())
            {
                GameObject bombGO = PrefabManager.Cache.GetPrefab<GameObject>(AssetManager.TravelTotemBomb_PrefabName);
                if (!bombGO)
                {
                    TravelTotemsPlugin.TravelTotemsLogger.LogWarning("Could not add Totem Bomb to trader: Prefab not found.");
                    return;
                }

                ItemDrop itemDrop = bombGO.GetComponent<ItemDrop>();
                if (!itemDrop)
                {
                    TravelTotemsPlugin.TravelTotemsLogger.LogWarning("Could not add Totem Bomb to trader: ItemDrop undefined.");
                    return;
                }

                Trader.TradeItem bombTrade = new Trader.TradeItem
                {
                    m_prefab = itemDrop,
                    m_price = TravelTotemsPlugin.GetTradersSellTotemBombCost()
                };

                __instance.m_items.Add(bombTrade);
            }
        }
    }
}
