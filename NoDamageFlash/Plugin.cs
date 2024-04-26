using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace VentureValheim.NoDamageFlash
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class NoDamageFlashPlugin : BaseUnityPlugin
    {
        private const string ModName = "NoDamageFlash";
        private const string ModVersion = "0.1.0";
        private const string Author = "com.orianaventure.mod";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);
        }

        /// <summary>
        /// Disable damage flash by never applying it.
        /// </summary>
        [HarmonyPatch(typeof(Hud), nameof(Hud.DamageFlash))]
        public static class Patch_Hud_DamageFlash
        {
            private static bool Prefix()
            {
                return false;
            }
        }
    }
}