using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.Pride;

public class PrideBanner
{
    private static Texture OriginalTexture;
    private static Texture BannerCombinedTexture;

    public static void UpdateBanner()
    {
        GameObject banner = PrefabManager.Instance.GetPrefab("piece_banner06");
        PridePlugin.PrideLogger.LogDebug($"Replacing texture for piece_banner06!");
        ModifyMaterial(ref banner);
    }

    public static void ModifyMaterial(ref GameObject banner)
    {
        if (banner == null)
        {
            return;
        }

        Transform bannerTransform = Utils.FindChild(banner.transform, "default");
        MeshRenderer bannerMesh = bannerTransform?.GetComponent<MeshRenderer>();

        if (bannerMesh == null || bannerMesh.materials == null || bannerMesh.materials.Length < 1)
        {
            return;
        }

        if (!PridePlugin.GetReplaceBanner())
        {
            if (OriginalTexture != null)
            {
                bannerMesh.materials[0].mainTexture = OriginalTexture;
            }

            return;
        }

        if (OriginalTexture == null)
        {
            Texture originalTex = bannerMesh.materials[0].mainTexture;
            if (originalTex.isReadable)
            {
                OriginalTexture = originalTex;
            }
            else
            {
                OriginalTexture = IconMerge.DuplicateTexture(originalTex);
            }
        }

        if (BannerCombinedTexture == null)
        {
            BannerCombinedTexture = IconMerge.MergeTextures((Texture2D)OriginalTexture, PridePlugin.BannerTexture);
        }

        bannerMesh.materials[0].mainTexture = BannerCombinedTexture;
    }

    public static void TryUpdateBanner()
    {
        if (SceneManager.GetActiveScene().name.Equals("main"))
        {
            UpdateBanner();
        }
    }

    [HarmonyPriority(Priority.VeryLow)]
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    public static class Patch_ObjectDB_Awake
    {
        private static void Postfix()
        {
            TryUpdateBanner();
        }
    }
}