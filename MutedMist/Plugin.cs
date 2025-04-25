using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace VentureValheim.MutedMist;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class MutedMistPlugin : BaseUnityPlugin
{
    private const string ModName = "MutedMist";
    private const string ModVersion = "0.1.0";
    private const string Author = "com.orianaventure.mod";
    private const string ModGUID = Author + "." + ModName;

    private readonly Harmony HarmonyInstance = new(ModGUID);

    public static readonly ManualLogSource MutedMistLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

    public void Awake()
    {
        MutedMistLogger.LogInfo("We will rule over all this land, and we will call it... This Land....");

        Assembly assembly = Assembly.GetExecutingAssembly();
        HarmonyInstance.PatchAll(assembly);
    }

    [HarmonyPatch(typeof(ParticleMist), nameof(ParticleMist.Awake))]
    public static class Patch_ParticleMist_Awake
    {
        private static void Postfix(ParticleMist __instance)
        {
            var mist = __instance.GetComponent<ParticleSystemRenderer>();

            if (mist != null)
            {
                var color = mist.material.color;
                color.a = 0.2f;
                mist.material.color = color;
            }
        }
    }
}