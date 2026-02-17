using BepInEx;
using HarmonyLib;
using Splatform;
using System.Collections.Generic;

namespace VentureValheim.IncognitoMode;

public class IncognitoMode
{
    private IncognitoMode() { }
    private static readonly IncognitoMode _instance = new IncognitoMode();

    public static IncognitoMode Instance
    {
        get => _instance;
    }

    private const string NAME_HIDDEN = "VV_NameHidden";

    private string ItemPrefabsString = "";
    private HashSet<string> ItemPrefabs = new HashSet<string>();

    public static string GetDisplayName()
    {
        return IncognitoModePlugin.GetHiddenDisplayName() ?? "???";
    }

    public static bool ItemSetsHidden(ItemDrop.ItemData item)
    {
        return item != null && item.m_dropPrefab != null && Instance.ItemPrefabs.Contains(item.m_dropPrefab.name);
    }

    public void Update()
    {
        if (!IncognitoModePlugin.GetHiddenByItems().Equals(Instance.ItemPrefabsString))
        {
            ItemPrefabsString = IncognitoModePlugin.GetHiddenByItems();
            ItemPrefabs = new HashSet<string>();

            if (!ItemPrefabsString.IsNullOrWhiteSpace())
            {
                string[] prefabs = ItemPrefabsString.Split(',');
                for (int lcv = 0; lcv < prefabs.Length; lcv++)
                {
                    ItemPrefabs.Add(prefabs[lcv].Trim());
                }
            }
        }
    }

    /// <summary>
    /// Check if the display needs to be completely hidden.
    /// </summary>
    [HarmonyPatch(typeof(EnemyHud), nameof(EnemyHud.TestShow))]
    public static class Patch_EnemyHud_TestShow
    {
        private static void Postfix(EnemyHud __instance, Character c, ref bool __result)
        {
            if (__result == false || !IncognitoModePlugin.GetHideHud() || !c.IsPlayer() || c.m_nview == null)
            {
                return;
            }

            ZDO zdo = c.m_nview.GetZDO();
            if (zdo != null && zdo.GetBool(NAME_HIDDEN))
            {
                __result = false;
            }
        }
    }

    /// <summary>
    /// Check if the display name needs to be hidden.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.GetHoverName))]
    public static class Patch_Player_GetHoverName
    {
        private static void Postfix(Player __instance, ref string __result)
        {
            if (IncognitoModePlugin.GetHideHud() || __instance.m_nview == null)
            {
                return;
            }

            ZDO zdo = __instance.m_nview.GetZDO();
            if (zdo != null && zdo.GetBool(NAME_HIDDEN))
            {
                __result = GetDisplayName();
            }
        }
    }

    /// <summary>
    /// Set the name to hidden/shown whenever the equipment changes.
    /// </summary>
    [HarmonyPatch(typeof(Player), nameof(Player.SetupVisEquipment))]
    public static class Patch_Player_SetupVisEquipment
    {
        private static void Postfix(Player __instance)
        {
            if (__instance.m_nview == null || !__instance.m_nview.IsOwner())
            {
                return;
            }

            ZDO zdo = __instance.m_nview.GetZDO();
            if (zdo != null)
            {
                Instance.Update();

                bool hidden = ItemSetsHidden(__instance.m_helmetItem) ||
                    ItemSetsHidden(__instance.m_shoulderItem) ||
                    ItemSetsHidden(__instance.m_utilityItem) ||
                    ItemSetsHidden(__instance.m_trinketItem);

                zdo.Set(NAME_HIDDEN, hidden);
            }
        }
    }

    /// <summary>
    /// Change the chat display name, patch higher so the change happens before other mods.
    /// </summary>
    [HarmonyPriority(Priority.HigherThanNormal)]
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.TryGetPlayerByPlatformUserID))]
    public static class Patch_ZNet_TryGetPlayerByPlatformUserID
    {
        private static void Postfix(Terminal __instance, PlatformUserID platformUserID, ref ZNet.PlayerInfo playerInfo, bool __result)
        {
            if (!__result || !IncognitoModePlugin.GetHideNameInChat())
            {
                return;
            }

            Player speaker = null;
            List<Player> players = Player.s_players;

            for (int lcv = 0; lcv < players.Count; lcv++)
            {
                uint playerID = players[lcv].m_nview.GetZDO().m_uid.ID;

                if (playerID.Equals(playerInfo.m_characterID.ID))
                {
                    speaker = players[lcv];
                    break;
                }
            }

            bool hidden = false;

            if (speaker != null && speaker.m_nview != null)
            {
                ZDO zdo = speaker.m_nview.GetZDO();
                if (zdo != null)
                {
                    hidden = zdo.GetBool(NAME_HIDDEN);
                }
            }

            if (hidden)
            {
                playerInfo.m_name = GetDisplayName();
            }
        }
    }
}