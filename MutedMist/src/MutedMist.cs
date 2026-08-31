using HarmonyLib;
using UnityEngine;

namespace VentureValheim.MutedMist;

public class MutedMist
{
    [HarmonyPatch(typeof(ParticleMist), nameof(ParticleMist.Awake))]
    public static class Patch_ParticleMist_Awake
    {
        private static void Postfix(ParticleMist __instance)
        {
            UpdateParticleMist();
        }
    }

    public static void UpdateParticleMist()
    {
        if (!ParticleMist.instance)
        {
            return;
        }

        MutedMistPlugin.MutedMistLogger.LogDebug("Updating Mistlands Mist.");

        ParticleSystemRenderer mist = ParticleMist.instance.GetComponent<ParticleSystemRenderer>();

        if (mist != null)
        {
            Color color = mist.material.color;
            color.a = MutedMistPlugin.GetTransparencyIntensity();
            mist.material.color = color;
        }
    }
}