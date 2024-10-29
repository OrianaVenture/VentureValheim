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

        private static AssetBundle _iconBundle = null;

        private static readonly HashSet<string> _meadList = new HashSet<string>()
        {
            "MeadBaseHealthMinor",
            "MeadBaseHealthMedium",
            "MeadBaseHealthMajor",
            "MeadBaseHealthLingering",
            "MeadBaseStaminaMinor",
            "MeadBaseStaminaMedium",
            "MeadBaseStaminaLingering",
            "MeadBaseEitrMinor",
            "MeadBaseEitrLingering",
            "MeadBaseFrostResist",
            "MeadBasePoisonResist",
            "BarleyWineBase",
            "MeadBaseBugRepellent",
            "MeadBaseBzerker",
            "MeadBaseHasty",
            "MeadBaseLightFoot",
            "MeadBaseStrength",
            "MeadBaseSwimmer",
            "MeadBaseTamer"
        };

        private static bool _objectDBReady = false;

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
                // Try hash code
                var prefab = ObjectDB.instance.GetItemPrefab(name.GetStableHashCode());
                if (prefab == null)
                {
                    // Failed, try slow search
                    prefab = ObjectDB.instance.GetItemPrefab(name);
                }

                if (prefab != null)
                {
                    item = prefab.GetComponent<ItemDrop>();
                    if (item != null)
                    {
                        return true;
                    }
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
            int y = (int)sprite.textureRect.y;
            //int y = sprite.texture.height - (int)sprite.textureRect.y - height; // Inverted (Legacy code after Bog Witch)

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

        /// <summary>
        /// Creates and applies new mead base icons if applicable. Sets the stack size of all mead bases.
        /// </summary>
        public static void ApplyMeadingfulChanges()
        {
            if (!_objectDBReady)
            {
                return;
            }

            if (MeadingfulIconsPlugin.GetReplaceIcons() && _iconBundle == null)
            {
                try
                {
                    _iconBundle = AssetUtils.LoadAssetBundleFromResources("meadingful_icons", Assembly.GetExecutingAssembly());
                }
                catch (Exception e)
                {
                    MeadingfulIconsPlugin.MeadingfulIconsLogger.LogError("Exception Caught! This mod might not behave as expected.");
                    MeadingfulIconsPlugin.MeadingfulIconsLogger.LogInfo(e);
                }
            }

            Texture2D baseSpriteTexture = null;

            if (GetItemDrop("MeadBaseTasty", out var tasty))
            {
                if (MeadingfulIconsPlugin.GetReplaceIcons() && _iconBundle != null && tasty.m_itemData.m_shared.m_icons.Length > 0)
                {
                    Sprite baseSprite = tasty.m_itemData.m_shared.m_icons[0];
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
                    if (MeadingfulIconsPlugin.GetReplaceIcons() && _iconBundle != null)
                    {
                        Texture2D originalSpriteTexture = null;
                        if (item.m_itemData.m_shared.m_icons.Length > 0)
                        {
                            Sprite baseSprite = item.m_itemData.m_shared.m_icons[0];
                            if (baseSprite.texture.isReadable)
                            {
                                originalSpriteTexture = baseSprite.texture;
                            }
                            else
                            {
                                originalSpriteTexture = DuplicateTexture(baseSprite);
                            }
                        }
                        else
                        {
                            originalSpriteTexture = baseSpriteTexture;
                        }

                        Sprite overlay = null;
                        try
                        {
                            overlay = _iconBundle.LoadAsset<Sprite>($"VV_{mead}");
                        }
                        catch (Exception e)
                        {
                            MeadingfulIconsPlugin.MeadingfulIconsLogger.LogError("Exception Caught! This mod might not behave as expected.");
                            MeadingfulIconsPlugin.MeadingfulIconsLogger.LogInfo(e);
                        }

                        if (overlay != null && overlay.texture.isReadable)
                        {
                            var sprite = MergeTextures($"VV_{mead}", originalSpriteTexture, overlay.texture);
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

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        public static class Patch_ObjectDB_Awake
        {
            private static void Postfix()
            {
                if (SceneManager.GetActiveScene().name.Equals("main"))
                {
                    _objectDBReady = true;
                    ApplyMeadingfulChanges();
                }
                else
                {
                    _objectDBReady = false;
                }
            }
        }
    }
}