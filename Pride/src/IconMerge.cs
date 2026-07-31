using UnityEngine;

namespace VentureValheim.Pride;

internal class IconMerge
{
    /// <summary>
    /// Duplicates a Texture2D of a previously unreadable sprite texture
    /// </summary>
    /// <param name="sprite"></param>
    /// <returns></returns>
    internal static Texture2D DuplicateTexture(Texture texture)
    {
        if (texture == null)
        {
            return null;
        }

        int width = texture.width;
        int height = texture.height;

        RenderTexture previous = RenderTexture.active;
        RenderTexture atlas = RenderTexture.GetTemporary(
            width,
            height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.sRGB);

        Graphics.Blit(texture, atlas);
        RenderTexture.active = atlas;
        Texture2D readableTexture = new Texture2D(width, height);
        readableTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        readableTexture.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(atlas);

        return readableTexture;
    }

    /// <summary>
    /// Merges a Texture2D with an overlay
    /// </summary>
    /// <returns></returns>
    internal static Texture2D MergeTextures(Texture2D baseTexture, Texture2D overlayTexture, float alpha = 0f)
    {
        int width = baseTexture.width;
        int height = baseTexture.height;
        Texture2D merged = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                merged.SetPixel(x, y, UnityEngine.Color.clear);
                ApplyPixel(ref merged, baseTexture, overlayTexture, x, y, alpha);
            }
        }

        merged.Apply();
        return merged;
    }

    private static void ApplyPixel(ref Texture2D merged, Texture2D baseTex, Texture2D overlayTex, int x, int y, float alphaOverride)
    {
        Color basePixel = baseTex.GetPixel(x, y);
        Color overlayPixel = overlayTex.GetPixel(x, y);
        Color combined = Color.clear;
        if (overlayPixel.a > 0.9f)
        {
            combined = overlayPixel;
        }
        else if (overlayPixel.a > 0.1f)
        {
            float alpha = alphaOverride != 0f ? alphaOverride : overlayPixel.a;
            combined = Color.Lerp(basePixel, overlayPixel, alpha);
        }
        else
        {
            combined = basePixel;
        }

            //Color combined = basePixel * overlayPixel;
            //combined.a = Mathf.Max(basePixel.a, overlayPixel.a);

        merged.SetPixel(x, y, combined);
    }
}
