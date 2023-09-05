using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;

namespace VentureValheim.NoGrass
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class NoGrassPlugin : BaseUnityPlugin
    {
        private const string ModName = "NoGrass";
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
        /// Increase slider options to include 0.
        /// </summary>
        [HarmonyPatch(typeof(Settings), nameof(Settings.Awake))]
        public static class Patch_Settings_Awake
        {
            private static void Prefix(Settings __instance)
            {
                __instance.m_vegetation.minValue = 0f;
            }
        }

        /// <summary>
        /// Remove the subtraction from the vegetation setting to fix localization.
        /// If there are any other instances of subtraction added within this method this may fail.
        /// </summary>
        [HarmonyPatch(typeof(Settings), nameof(Settings.Update))]
        public static class Patch_Settings_Update
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (var lcv = 1; lcv < codes.Count; lcv++)
                {
                    if (codes[lcv].opcode == OpCodes.Sub)
                    {
                        codes[lcv - 1].opcode = OpCodes.Nop; //ldc.i4.1
                        codes[lcv].opcode = OpCodes.Nop; //sub
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}