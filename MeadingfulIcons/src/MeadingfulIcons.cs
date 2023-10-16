using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Jotunn.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VentureValheim.MeadingfulIcons
{
    public class MeadingfulIcons
    {
        static MeadingfulIcons() { }
        private MeadingfulIcons() { }
        private static readonly MeadingfulIcons _instance = new MeadingfulIcons();

        public static MeadingfulIcons Instance
        {
            get => _instance;
        }

        private static readonly HashSet<string> _meadList = new HashSet<string>()
        {
            "MeadBaseHealthMinor",
            "MeadBaseHealthMedium",
            "MeadBaseHealthMajor",
            "MeadBaseStaminaMinor",
            "MeadBaseStaminaMedium",
            "MeadBaseStaminaLingering",
            "MeadBaseEitrMinor",
            "MeadBaseFrostResist",
            "MeadBasePoisonResist",
            "BarleyWineBase"
        };

        /// <summary>
        /// Attempts to get the ItemDrop by the given name's hashcode, if not found searches by string.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="item"></param>
        /// <returns>True on sucessful find</returns>
        public static bool GetItemDrop(string name, out ItemDrop item)
        {
            item = null;

            if (!name.IsNullOrWhiteSpace())
            {
                try
                {
                    // Try hash code
                    item = ObjectDB.instance.GetItemPrefab(name.GetStableHashCode())?.GetComponent<ItemDrop>();
                }
                catch
                {
                    // Failed, try slow search
                    item = ObjectDB.instance.GetItemPrefab(name)?.GetComponent<ItemDrop>();
                }

                if (item != null)
                {
                    return true;
                }
            }

            return false;
        }

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

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        public static class Patch_ObjectDB_Awake
        {
            private static void Postfix()
            {
                if (SceneManager.GetActiveScene().name.Equals("main"))
                {
                    try
                    {
                        AssetBundle bundle = AssetUtils.LoadAssetBundleFromResources("meadingful_icons", Assembly.GetExecutingAssembly());
                        Sprite baseSprite = null;
                        Texture2D baseSpriteTexture = null;

                        if (GetItemDrop("MeadBaseTasty", out var tasty))
                        {
                            if (tasty.m_itemData.m_shared.m_icons.Length > 0)
                            {
                                baseSprite = tasty.m_itemData.m_shared.m_icons[0];
                                if (baseSprite.texture.isReadable)
                                {
                                    baseSpriteTexture = baseSprite.texture;
                                }
                                else
                                {
                                    baseSpriteTexture = DuplicateTexture(baseSprite);
                                }
                            }
                            tasty.m_itemData.m_shared.m_maxStackSize = MeadingfulIconsPlugin.GetStackSize();
                        }

                        foreach (var mead in _meadList)
                        {
                            if (GetItemDrop(mead, out var item))
                            {
                                if (MeadingfulIconsPlugin.GetReplaceIcons() && baseSpriteTexture != null)
                                {
                                    var overlay = bundle.LoadAsset<Sprite>($"VV_{mead}");
                                    if (overlay != null && overlay.texture.isReadable)
                                    {
                                        var sprite = MergeTextures($"VV_{mead}", baseSpriteTexture, overlay.texture);
                                        item.m_itemData.m_shared.m_icons = new Sprite[] { sprite };
                                    }
                                    else
                                    {
                                        MeadingfulIconsPlugin.MeadingfulIconsLogger.LogWarning($"Could not load {mead} icon! Skipping.");
                                    }
                                }

                                item.m_itemData.m_shared.m_maxStackSize = MeadingfulIconsPlugin.GetStackSize();
                            }
                            else
                            {
                                MeadingfulIconsPlugin.MeadingfulIconsLogger.LogWarning($"Could not find {mead}! Skipping.");
                            }
                        }

                        MeadingfulIconsPlugin.MeadingfulIconsLogger.LogInfo("Done applying mead configurations.");
                    }
                    catch (Exception e)
                    {
                        MeadingfulIconsPlugin.MeadingfulIconsLogger.LogError("Exception Caught! This mod might not behave as expected.");
                        MeadingfulIconsPlugin.MeadingfulIconsLogger.LogInfo(e);
                    }
                }
            }
        }
    }
}