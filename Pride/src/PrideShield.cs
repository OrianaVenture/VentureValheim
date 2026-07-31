using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.Pride;

public class PrideShield
{
    private static Texture OriginalTexture;
    private static Texture RoundShieldCombinedTexture;

    public static void UpdateShield()
    {
        GameObject roundShield = PrefabManager.Instance.GetPrefab("ShieldWood");
        PridePlugin.PrideLogger.LogDebug($"Replacing texture for ShieldWood!");
        ModifyMaterial(ref roundShield);
    }

    public static void ModifyMaterial(ref GameObject shield)
    {
        if (shield == null)
        {
            return;
        }

        MeshRenderer shieldMesh = shield.GetComponentInChildren<MeshRenderer>();

        if (shieldMesh == null || shieldMesh.materials == null || shieldMesh.materials.Length < 1)
        {
            return;
        }

        if (!PridePlugin.GetReplaceShields())
        {
            if (OriginalTexture != null)
            {
                shieldMesh.materials[0].mainTexture = OriginalTexture;
            }

            return;
        }

        if (OriginalTexture == null)
        {
            Texture originalTex = shieldMesh.materials[0].mainTexture;
            if (originalTex.isReadable)
            {
                OriginalTexture = originalTex;
            }
            else
            {
                OriginalTexture = IconMerge.DuplicateTexture(originalTex);
            }
        }

        if (RoundShieldCombinedTexture == null)
        {
            RoundShieldCombinedTexture = IconMerge.MergeTextures(PridePlugin.RoundShieldTexture, (Texture2D)OriginalTexture);
        }

        shieldMesh.materials[0].mainTexture = RoundShieldCombinedTexture;
    }

    public static void TryUpdateShield()
    {
        if (SceneManager.GetActiveScene().name.Equals("main"))
        {
            UpdateShield();
        }
    }

    [HarmonyPriority(Priority.VeryLow)]
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    public static class Patch_ObjectDB_Awake
    {
        private static void Postfix()
        {
            TryUpdateShield();
        }
    }
}