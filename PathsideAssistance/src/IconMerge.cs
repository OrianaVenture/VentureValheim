using System;
using System.Reflection;
using Jotunn.Utils;
using UnityEngine;

namespace VentureValheim.PathsideAssistance
{
    public class IconMerge
    {
        static IconMerge() { }
        private IconMerge() { }
        private static readonly IconMerge _instance = new IconMerge();

        public static IconMerge Instance
        {
            get => _instance;
        }

        private static AssetBundle _iconBundle = null;
        private static Sprite _overlay = null;

        /// <summary>
        /// Duplicates a Texture2D of a previously unreadable sprite texture
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        private static Texture2D DuplicateTexture(Sprite sprite)
        {
            if (sprite == null)
            {
                return null;
            }

            // Target icon
            int width = (int)sprite.textureRect.width;
            int height = (int)sprite.textureRect.height;
            int x = (int)sprite.textureRect.x;
            int y = sprite.texture.height - (int)sprite.textureRect.y - height; // Inverted

            RenderTexture previous = RenderTexture.active;
            RenderTexture atlas = RenderTexture.GetTemporary(
                sprite.texture.width,
                sprite.texture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.sRGB);

            Graphics.Blit(sprite.texture, atlas);
            RenderTexture.active = atlas;
            Texture2D readableTexture = new Texture2D(width, height);
            readableTexture.ReadPixels(new Rect(x, y, width, height), 0, 0);
            readableTexture.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(atlas);

            return readableTexture;
        }

        /// <summary>
        /// Merges a Texture2D with an overlay
        /// </summary>
        /// <param name="name"></param>
        /// <param name="baseTexture"></param>
        /// <param name="overlayTexture"></param>
        /// <returns></returns>
        private static Sprite MergeTextures(string name, Texture2D baseTexture, Texture2D overlayTexture)
        {
            int width = baseTexture.width;
            int height = baseTexture.height;
            var merged = new Texture2D(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    merged.SetPixel(x, y, UnityEngine.Color.clear);

                    var overlayPixel = overlayTexture.GetPixel(x, y);
                    if (overlayPixel.a != 0)
                    {
                        merged.SetPixel(x, y, overlayPixel);
                        continue;
                    }

                    var basePixel = baseTexture.GetPixel(x, y);
                    if (basePixel.a != 0)
                    {
                        merged.SetPixel(x, y, basePixel);
                        continue;
                    }
                }
            }

            merged.Apply();
            var newSprite = Sprite.Create(merged, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
            newSprite.name = name;
            return newSprite;
        }

        public static void AddSpriteOverlay(ref GameObject item)
        {
            if (_iconBundle == null)
            {
                try
                {
                    _iconBundle = AssetUtils.LoadAssetBundleFromResources("pathside_icon", Assembly.GetExecutingAssembly());
                    _overlay = _iconBundle.LoadAsset<Sprite>("VV_PA_Icon");
                }
                catch (Exception e)
                {
                    PathsideAssistancePlugin.PathsideAssistanceLogger.LogError("Exception Caught! This mod might not behave as expected.");
                    PathsideAssistancePlugin.PathsideAssistanceLogger.LogInfo(e);
                    return;
                }
            }

            if (_overlay == null)
            {
                return;
            }

            var piece = item.GetComponent<Piece>();
            if (piece != null && piece.m_icon != null)
            {
                Texture2D baseSpriteTexture;
                Sprite baseSprite = piece.m_icon;
                
                if (baseSprite.texture.isReadable)
                {
                    baseSpriteTexture = baseSprite.texture;
                }
                else
                {
                    baseSpriteTexture = DuplicateTexture(baseSprite);
                }

                var sprite = MergeTextures($"VV_PA_{item.name}", baseSpriteTexture, _overlay.texture);

                piece.m_icon = sprite;
            }
        }
    }
}