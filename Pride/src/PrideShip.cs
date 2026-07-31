using HarmonyLib;
using MagicaCloth2;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.Pride;

public class PrideShip
{
    /// <summary>
    /// Finds all ships and changes sail materials.
    /// </summary>
    public static void UpdateSails()
    {
        foreach (GameObject obj in ZNetScene.instance.m_prefabs)
        {
            if (obj!= null && obj.TryGetComponent<Ship>(out Ship ship))
            {
                PridePlugin.PrideLogger.LogDebug($"Replacing sail texture for {obj.name}!");
                ModifyMaterial(ref ship);
            }
        }
    }

    public static void ModifyMaterial(ref Ship ship)
    {
        MagicaCloth sailCloth = ship.GetComponentInChildren<MagicaCloth>();

        if (sailCloth == null)
        {
            return;
        }

        SkinnedMeshRenderer sail = sailCloth.GetComponentInChildren<SkinnedMeshRenderer>();

        if (sail == null || sail.materials == null || sail.materials.Length < 1)
        {
            return;
        }

        Material originalMaterial = sail.materials[0];
        originalMaterial.mainTexture = PridePlugin.GetReplaceSailsPride() ? PridePlugin.PrideTexture : PridePlugin.TransTexture;
    }

    public static void TryUpdateSails()
    {
        if (!PridePlugin.GetReplaceSailsPride() && !PridePlugin.GetReplaceSailsTrans())
        {
            return;
        }

        if (SceneManager.GetActiveScene().name.Equals("main"))
        {
            UpdateSails();
        }
    }

    [HarmonyPriority(Priority.VeryLow)]
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    public static class Patch_ObjectDB_Awake
    {
        private static void Postfix()
        {
            TryUpdateSails();
        }
    }
}