# Valheim Icon Overlay 101

In this guide I will show you how to add an overlay to existing icon sprites in the game without the need to copy and modify the original outside of code. Why would you need to use this? If you need to change how an icon (or other sprite/texture) looks in the game and want to make a simple replacement without writing complex code patches or duplicating and distributing content outside of copyright permissions.

## Part 1: Finding the icon through code

Grab the original icon when it is available on ObjectDB.Awake and assign to a variable. Here is a basic patch example to do so using the MeadBaseTasty prefab. I wrote a helper method that will find the item drop with a redundant backup search in case custom mod items are not added to the hashed item list at the time the code runs.

```csharp
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

[HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
public static class Patch_ObjectDB_Awake
{
    private static void Postfix()
    {
        if (SceneManager.GetActiveScene().name.Equals("main"))
        {
            try
            {
                Sprite baseSprite = null;
                Texture2D baseSpriteTexture = null;

                if (GetItemDrop("MeadBaseTasty", out var item))
                {
                    if (item.m_itemData.m_shared.m_icons.Length > 0)
                    {
                        baseSprite = item.m_itemData.m_shared.m_icons[0];
                    }
                }

                // Part 2 here
            }
            catch (Exception e)
            {
                // OH NO! Use your custom bepinex logger to log the exception here
            }
        }
    }
}
```

## Part 2: Create a readable texture

Many textures in the game might not be readable, this will throw exceptions if you try to use GetPixels on them. Test if your icon is readable with a simple check, if it is not then we will have to duplicate the icon using a RenderTexture.

```csharp
if (baseSprite != null)
{
    if (baseSprite.texture.isReadable)
    {
        baseSpriteTexture = baseSprite.texture;
    }
    else
    {
        baseSpriteTexture = DuplicateTexture(baseSprite);
    }

    // Part 3 here
}
```

### Get a readable copy:

```csharp
private static Texture2D DuplicateTexture(Sprite sprite)
{
    if (sprite == null)
    {
        return null;
    }

    // The resulting sprite dimensions
    int width = (int)sprite.textureRect.width;
    int height = (int)sprite.textureRect.height;

    // The location of the target icon in the texture
    int x = (int)sprite.textureRect.x;
    int y = sprite.texture.height - (int)sprite.textureRect.y - height; // Y is inverted for my example

    // The whole sprite atlas
    var texture = sprite.texture;

    RenderTexture previous = RenderTexture.active;

    // Our RenderTexture for displaying the whole sprite atlas.
    // Be sure to match format of your texture or else it may display strangely
    // such as a darker image than the original.
    RenderTexture renderTex = RenderTexture.GetTemporary(
        texture.width,
        texture.height,
        0,
        RenderTextureFormat.Default,
        RenderTextureReadWrite.sRGB);

    UnityEngine.Graphics.Blit(texture, renderTex);
    RenderTexture.active = renderTex;

    // Create a copy of the target icon texture that is readable
    Texture2D readableText = new Texture2D(width, height);
    readableText.ReadPixels(new Rect(x, y, width, height), 0, 0);
    readableText.Apply();
    RenderTexture.active = previous;
    RenderTexture.ReleaseTemporary(renderTex);

    return readableText;
}
```

## Part 3: Combine the original with the overlay

When you create your overlay make sure the background is transparent. Your overlay should just contain the pixels you want to overlay on the original sprite, and should be the same size as the original. These assets should be of texture type "Sprite (2D and UI)", and must have read/write enabled or you will have to use the DuplicateTexture code to read it (under advanced texture settings in unity). Now, follow your usual process of creating an asset bundle and load it in code, and replace the original sprite for your item:

```csharp

Sprite overlay = LoadAsset("mySprite");
if (overlay != null)
{
    var sprite = MergeTextures("MyNewSpriteName", baseSpriteTexture, overlay.texture);
    item.m_itemData.m_shared.m_icons = new Sprite[] { sprite };
}

```

### Merge textures:

```csharp
private static Sprite MergeTextures(string name, Texture2D baseTexture, Texture2D overlayTexture)
{
    int width = baseTexture.width;
    int height = baseTexture.height;
    var merged = new Texture2D(width, height);

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            // Your default background pixel, I use clear
            // If you do not set this value first you may get strange halo effects!
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

    var newSprite = Sprite.Create(merged, new Rect(0, 0, width, height), new Vector2(0, 0));
    newSprite.name = name;
    return newSprite;
}
```

## Summary

After piecing together these steps your final patch would look something like:

```csharp
[HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
public static class Patch_ObjectDB_Awake
{
    private static void Postfix()
    {
        if (SceneManager.GetActiveScene().name.Equals("main"))
        {
            try
            {
                Sprite baseSprite = null;
                Texture2D baseSpriteTexture = null;

                if (GetItemDrop("MeadBaseTasty", out var item))
                {
                    if (item.m_itemData.m_shared.m_icons.Length > 0)
                    {
                        baseSprite = item.m_itemData.m_shared.m_icons[0];
                    }
                }

                if (baseSprite != null)
                {
                    if (baseSprite.texture.isReadable)
                    {
                        baseSpriteTexture = baseSprite.texture;
                    }
                    else
                    {
                        baseSpriteTexture = DuplicateTexture(baseSprite);
                    }

                    var overlay = LoadAsset("mySprite");
                    if (overlay != null)
                    {
                        var sprite = MergeTextures("MyNewSpriteName", baseSpriteTexture, overlay.texture);
                        item.m_itemData.m_shared.m_icons = new Sprite[] { sprite };
                    }
                }
            }
            catch (Exception e)
            {
                // OH NO! Use your custom bepinex logger to log the exception here
            }
        }
    }
}
```

These concepts can be applied to more than just item icons. Feel free to use and modify this code for your own projects!

### Bonus DuplicateTexture Method

Another way you can load an unreadable sprite that involves reading the whole texture then cropping out needed parts. If you need to target multiple sprites in an atlas this is another approach you can modify and use:

```csharp
private static Texture2D DuplicateTexture(Sprite sprite)
{
    if (sprite == null)
    {
        return null;
    }

    // The resulting sprite dimensions
    int width = (int)sprite.textureRect.width;
    int height = (int)sprite.textureRect.height;

    // The whole sprite atlas
    var texture = sprite.texture;

    RenderTexture previous = RenderTexture.active;

    // Our RenderTexture for displaying the whole sprite atlas.
    // Be sure to match format of your texture or else it may display strangely
    // such as a darker image than the original.
    RenderTexture renderTex = RenderTexture.GetTemporary(
        texture.width,
        texture.height,
        0,
        RenderTextureFormat.Default,
        RenderTextureReadWrite.sRGB);

    UnityEngine.Graphics.Blit(texture, renderTex);
    RenderTexture.active = renderTex;

    // Create a copy of the texture that is readable
    Texture2D readableText = new Texture2D(texture.width, texture.height);
    readableText.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
    readableText.Apply();
    RenderTexture.active = previous;
    RenderTexture.ReleaseTemporary(renderTex);

    // Crop to the needed texture
    Texture2D smallTexture = new Texture2D(width, height);
    // Strangely did not need to invert Y when using this method
    var colors = readableText.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, width, height);
    smallTexture.SetPixels(colors);
    smallTexture.Apply();

    return smallTexture;
}
```
