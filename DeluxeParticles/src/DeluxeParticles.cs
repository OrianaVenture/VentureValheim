using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.DeluxeParticles;

public class DeluxeParticles
{
    private DeluxeParticles() {}
    private static readonly DeluxeParticles _instance = new DeluxeParticles();

    public static DeluxeParticles Instance
    {
        get => _instance;
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    public static class Patch_ObjectDB_Awake
    {
        [HarmonyPriority(Priority.Last)]
        private static void Postfix()
        {
            if (SceneManager.GetActiveScene().name.Equals("main"))
            {
                Deluxify();
                DeluxeParticlesPlugin.DeluxeParticlesLogger.LogInfo("Done deluxifying particles.");
            }
        }
    }

    private static void Deluxify()
    {
        List<GameObject> items = ObjectDB.m_instance.m_items;

        for (int lcv = 0; lcv < items.Count; lcv++)
        {
            ParticleSystem item = items[lcv].GetComponent<ParticleSystem>();
            if (item != null)
            {
                ParticleSystem.MainModule itemMain = item.main;

                itemMain.startLifetime = new ParticleSystem.MinMaxCurve(3f);
                itemMain.startSize = new ParticleSystem.MinMaxCurve(0.5f, 0.7f);
                itemMain.maxParticles = 3;

                ParticleSystem.EmissionModule emission = item.emission;
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(1f);
            }
        }
    }
}