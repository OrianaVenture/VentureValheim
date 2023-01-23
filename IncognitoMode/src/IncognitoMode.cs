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

        private const string HELMET_HIDDEN = "VV_HelmetHidden";

        private string HelmetPrefabsString = "";
        private HashSet<string> HelmetPrefabs = new HashSet<string>();

        public void Update()
        {
            if (!IncognitoModePlugin.GetHiddenByItems().Equals(Instance.HelmetPrefabsString))
            {
                HelmetPrefabsString = IncognitoModePlugin.GetHiddenByItems();
                HelmetPrefabs = new HashSet<string>();

                if (!HelmetPrefabsString.IsNullOrWhiteSpace())
                {
                    var prefabs = HelmetPrefabsString.Split(',');
                    for (var lcv = 0; lcv < prefabs.Length; lcv++)
                    {
                        HelmetPrefabs.Add(prefabs[lcv].Trim());
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
                var hidden = __instance.m_nview?.GetZDO()?.GetBool(HELMET_HIDDEN) ?? false;

                if (hidden)
                {
                    __result = IncognitoModePlugin.GetHiddenDisplayName() ?? "???";
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
                if (__instance.m_nview?.GetZDO() != null && __instance.m_nview.IsOwner())
			    {
                    Instance.Update();

                    var helmet = __instance.m_helmetItem;
                    if (helmet != null && Instance.HelmetPrefabs.Contains(helmet.m_dropPrefab?.name))
                    {
                        __instance.m_nview.GetZDO().Set(HELMET_HIDDEN, true);
                    }
                    else
                    {
                        __instance.m_nview.GetZDO().Set(HELMET_HIDDEN, false);
                    }
			    }

            }
        }
    }
}