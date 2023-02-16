using HarmonyLib;
using UnityEngine;
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

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        public static class Patch_ObjectDB_Awake
        {
            private static void Postfix()
            {
                if (SceneManager.GetActiveScene().name.Equals("main"))
                {
                    var se = ObjectDB.instance.GetStatusEffect("Puke");
                    if (se != null)
                    {
                        se.m_startEffects = new EffectList();
                        NoPukePlugin.NoPukeLogger.LogInfo("Done disabling puke animations and effects.");
                    }
                    else
                    {
                        NoPukePlugin.NoPukeLogger.LogInfo("Could not disable puke animations and effects.");
                    }
                }
            }
        }
    }
}