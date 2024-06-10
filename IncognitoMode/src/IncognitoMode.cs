using System.Collections.Generic;
using BepInEx;
using HarmonyLib;

namespace VentureValheim.IncognitoMode
{
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

        public void Update()
        {
            if (!IncognitoModePlugin.GetHiddenByItems().Equals(Instance.ItemPrefabsString))
            {
                ItemPrefabsString = IncognitoModePlugin.GetHiddenByItems();
                ItemPrefabs = new HashSet<string>();

                if (!ItemPrefabsString.IsNullOrWhiteSpace())
                {
                    var prefabs = ItemPrefabsString.Split(',');
                    for (var lcv = 0; lcv < prefabs.Length; lcv++)
                    {
                        ItemPrefabs.Add(prefabs[lcv].Trim());
                    }
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
                if (__instance.m_nview == null)
                {
                    return;
                }

                var zdo = __instance.m_nview.GetZDO();
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

                var zdo = __instance.m_nview.GetZDO();
                if (zdo != null)
			    {
                    Instance.Update();

                    bool hidden = false;
                    var helmet = __instance.m_helmetItem;
                    var back = __instance.m_shoulderItem;
                    var utility = __instance.m_utilityItem;

                    if (helmet != null && helmet.m_dropPrefab != null && Instance.ItemPrefabs.Contains(helmet.m_dropPrefab.name))
                    {
                        hidden = true;
                    }
                    else if (back != null && back.m_dropPrefab != null && Instance.ItemPrefabs.Contains(back.m_dropPrefab.name))
                    {
                        hidden = true;
                    }
                    else if (utility != null && utility.m_dropPrefab != null && Instance.ItemPrefabs.Contains(utility.m_dropPrefab.name))
                    {
                        hidden = true;
                    }

                    zdo.Set(NAME_HIDDEN, hidden);
                }
            }
        }

        /// <summary>
        /// Change the chat display name, patch higher so the change happens before other mods.
        /// </summary>
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPatch(typeof(UserInfo), nameof(UserInfo.GetDisplayName))]
        public static class Patch_UserInfo_GetDisplayName
        {
            private static void Prefix(ref UserInfo __instance)
            {
                if (IncognitoModePlugin.GetHideNameInChat())
                {
                    Player speaker = null;
                    var players = Player.s_players;

                    for (int lcv = 0; lcv < players.Count; lcv++)
                    {
                        var player = players[lcv].GetPlayerName();
                        if (player.Equals(__instance.Name))
                        {
                            speaker = players[lcv];
                            break;
                        }
                    }

                    bool hidden = false;

                    if (speaker != null && speaker.m_nview != null)
                    {
                        var zdo = speaker.m_nview.GetZDO();
                        if (zdo != null)
                        {
                            hidden = zdo.GetBool(NAME_HIDDEN);
                        }
                    }

                    if (hidden)
                    {
                        __instance.Name = GetDisplayName();
                        if (IncognitoModePlugin.GetHidePlatformTag())
                        {
                            __instance.Gamertag = "";
                        }
                    }
                }
            }
        }
    }
}