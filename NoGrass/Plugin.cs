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
        private const string ModVersion = "0.1.4";
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
        [HarmonyPatch(typeof(Valheim.SettingsGui.GraphicsSettings), nameof(Valheim.SettingsGui.GraphicsSettings.Awake))]
        public static class Patch_GraphicsSettings_Awake
        {
            private static void Prefix(Valheim.SettingsGui.GraphicsSettings __instance)
            {
                __instance.m_vegetationSlider.minValue = 0f;
                __instance.m_vegetationSlider.value = PlatformPrefs.GetInt("ClutterQuality");
            }
        }

        /// <summary>
        /// Remove the subtraction from the vegetation setting to fix localization.
        /// </summary>
        [HarmonyPatch(typeof(Valheim.SettingsGui.GraphicsSettings), nameof(Valheim.SettingsGui.GraphicsSettings.OnQualityChanged))]
        public static class Patch_GraphicsSettings_OnQualityChanged
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (var lcv = 0; lcv < codes.Count - 4; lcv++)
                {
                    if (codes[lcv].opcode == OpCodes.Ldfld &&
                        codes[lcv].operand.ToString() == "UnityEngine.UI.Slider m_vegetationSlider" &&
                        codes[lcv + 4].opcode == OpCodes.Sub)
                    {
                        codes[lcv + 3].opcode = OpCodes.Nop; //ldc.i4.1
                        codes[lcv + 4].opcode = OpCodes.Nop; //sub
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}