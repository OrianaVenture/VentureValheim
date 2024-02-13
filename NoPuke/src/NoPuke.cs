using BepInEx;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace VentureValheim.NoPuke
{
    public class NoPuke
    {
        private NoPuke() {}
        private static readonly NoPuke _instance = new NoPuke();

        public static NoPuke Instance
        {
            get => _instance;
        }

        /// <summary>
        /// Attempts to get the ItemDrop by the given name's hashcode, if not found searches by string.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <returns>True on sucessful find</returns>
        public static bool GetItemDrop(string name, out ItemDrop item)
        {
            item = null;

            if (!name.IsNullOrWhiteSpace())
            {
                // Try hash code
                var prefab = ObjectDB.instance.GetItemPrefab(name.GetStableHashCode());
                if (prefab == null)
                {
                    // Failed, try slow search
                    prefab = ObjectDB.instance.GetItemPrefab(name);
                }

                if (prefab != null)
                {
                    item = prefab.GetComponent<ItemDrop>();
                    if (item != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        public static class Patch_ObjectDB_Awake
        {
            private static void Postfix()
            {
                if (SceneManager.GetActiveScene().name.Equals("main"))
                {
                    var se = ObjectDB.instance.GetStatusEffect("Puke".GetStableHashCode());
                    if (se != null)
                    {
                        se.m_startEffects = new EffectList();
                    }
                    else
                    {
                        NoPukePlugin.NoPukeLogger.LogWarning("Could not disable puke animations and effects.");
                    }

                    if (GetItemDrop("bonemass_attack_aoe", out var bonemass))
                    {
                        bonemass.m_itemData.m_shared.m_startEffect = new EffectList();
                    }
                    else
                    {
                        NoPukePlugin.NoPukeLogger.LogWarning("Could not disable bonemass puke animations and effects.");
                    }

                    NoPukePlugin.NoPukeLogger.LogInfo("Done disabling puke animations and effects.");
                }
            }
        }
    }
}