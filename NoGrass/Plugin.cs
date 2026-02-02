using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;

namespace VentureValheim.NoGrass;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class NoGrassPlugin : BaseUnityPlugin
{
    private const string ModName = "NoGrass";
    private const string ModVersion = "0.1.5";
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
    [HarmonyPatch(typeof(Valheim.SettingsGui.GraphicsSettings), nameof(Valheim.SettingsGui.GraphicsSettings.Initialize))]
    public static class Patch_GraphicsSettings_Initialize
    {
        private static void Postfix(Valheim.SettingsGui.GraphicsSettings __instance)
        {
            QualitySliderData slider = __instance.m_qualitySliders.Where(x => x.m_setting == GraphicsSettingInt.Vegetation).FirstOrDefault();

            if (slider.m_slider != null)
            {
                slider.m_slider.minValue = 0f;
                slider.m_slider.value = PlatformPrefs.GetInt("ClutterQuality");
            }
        }
    }

    /// <summary>
    /// Remove the subtraction from the vegetation setting to fix localization.
    /// </summary>
    [HarmonyPatch(typeof(Valheim.SettingsGui.GraphicsSettings), nameof(Valheim.SettingsGui.GraphicsSettings.GetDisplayValue))]
    public static class Patch_GraphicsSettings_GetDisplayValue
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    useEnd: false,
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Sub))
                .ThrowIfInvalid($"Could not patch GraphicsSettings.GetDisplayValue")
                .RemoveInstructions(2)
                .InstructionEnumeration();
        }
    }
}