//Editor-only tool living in a runtime assembly; guarded so player builds compile
#if UNITY_EDITOR
using System.IO;
using System.Linq;
using LightCard.Core;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Mirrors the engine catalog (CardCatalogV1, the current source of truth for
/// implemented cards) into library Card assets so the deck builder and match
/// view stay in sync: creates missing Card assets, refreshes drifted stats and
/// text, registers everything in the library's CardCollection, and generates a
/// deterministic placeholder sprite for any card without art.
/// Stopgap until the Sheets importer is revived with fresh credentials.
/// </summary>
public static class CardAssetSync
{
    private const string CardFolder = "Assets/Data/Cards";
    private const string PlaceholderFolder = "Assets/Art/CardPlaceholders";
    private const string LibraryPath = "Assets/Data/Dev Card Library.asset";
    private const int PlaceholderSize = 128;

    [MenuItem("LightCard/Sync Card Assets From Catalog")]
    public static void Sync()
    {
        var library = AssetDatabase.LoadAssetAtPath<CardLibrary>(LibraryPath);
        if (library == null || library.cardCollection == null)
        {
            Debug.LogError($"CardAssetSync: no CardLibrary with a collection at {LibraryPath}.");
            return;
        }

        Directory.CreateDirectory(PlaceholderFolder);

        int created = 0, updated = 0, sprites = 0;
        foreach (var definition in CardCatalogV1.Cards.Values.OrderBy(c => c.Id))
        {
            var card = library.cardCollection.cards.FirstOrDefault(c => c != null && c.name == definition.Id);
            if (card == null)
            {
                card = ScriptableObject.CreateInstance<Card>();
                AssetDatabase.CreateAsset(card, $"{CardFolder}/{definition.Id}.asset");
                library.cardCollection.cards.Add(card);
                created++;
            }
            else if (StatsDiffer(card, definition))
            {
                updated++;
            }

            card.name = definition.Id;
            card.archetype = (Card.Archetype)(int)definition.Archetype;
            card.type = (Card.Type)(int)definition.Type;
            card.cost = definition.Cost;
            card.power = definition.Power;
            card.life = definition.Life;
            card.description = definition.Text;

            //Art pipeline: a PNG dropped at CardPlaceholders/<Id>.png always wins
            //(the Midjourney workflow); otherwise generate procedural art if the
            //card has none at all.
            string artPath = $"{PlaceholderFolder}/{definition.Id}.png";
            if (File.Exists(artPath))
            {
                var dropIn = ImportSprite(artPath);
                if (card.sprite != dropIn) { card.sprite = dropIn; sprites++; }
            }
            else if (card.sprite == null)
            {
                File.WriteAllBytes(artPath, RenderPlaceholder(definition).EncodeToPNG());
                card.sprite = ImportSprite(artPath);
                sprites++;
            }

            EditorUtility.SetDirty(card);
        }

        EditorUtility.SetDirty(library.cardCollection);
        AssetDatabase.SaveAssets();
        Debug.Log($"CardAssetSync: {CardCatalogV1.Cards.Count} catalog cards - {created} created, {updated} stat-synced, {sprites} placeholder sprites generated.");
    }

    private static bool StatsDiffer(Card card, CardDefinition definition) =>
        card.cost != definition.Cost || card.power != definition.Power || card.life != definition.Life ||
        (int)card.archetype != (int)definition.Archetype || (int)card.type != (int)definition.Type ||
        card.description != definition.Text;

    //---- Placeholder art: a deterministic abstract pattern per card ----

    private static readonly Color[] archetypeColors =
    {
        new Color(0.30f, 0.62f, 0.32f), //Garden - green
        new Color(0.28f, 0.44f, 0.78f), //Atelier - blue
        new Color(0.80f, 0.30f, 0.24f), //Heart - red
        new Color(0.48f, 0.34f, 0.72f), //Ocean - purple
        new Color(0.28f, 0.26f, 0.34f), //Tower - black
        new Color(0.85f, 0.70f, 0.28f)  //Expedition - yellow
    };

    /// <summary>Import (or re-import) a PNG with sprite settings and return its Sprite.</summary>
    private static Sprite ImportSprite(string path)
    {
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer.textureType != TextureImporterType.Sprite || !importer.alphaIsTransparency)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    [MenuItem("LightCard/Regenerate ALL Procedural Card Art")]
    public static void RegenerateAllProceduralArt()
    {
        Directory.CreateDirectory(PlaceholderFolder);
        var library = AssetDatabase.LoadAssetAtPath<CardLibrary>(LibraryPath);
        int regenerated = 0;
        foreach (var definition in CardCatalogV1.Cards.Values)
        {
            //Cards whose art lives outside the placeholder folder (2021 sketches,
            //hand-assigned art) are never touched - writing a placeholder PNG
            //would override them through the drop-in rule on the next sync
            var card = library != null ? library.cardCollection.cards.FirstOrDefault(c => c != null && c.name == definition.Id) : null;
            if (card != null && card.sprite != null && !AssetDatabase.GetAssetPath(card.sprite).StartsWith(PlaceholderFolder)) continue;

            string path = $"{PlaceholderFolder}/{definition.Id}.png";
            //Only (re)write files that are missing or themselves procedural
            //(128x128) - never clobber Midjourney drop-ins or other real art
            if (File.Exists(path))
            {
                var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (existing != null && (existing.width != PlaceholderSize || existing.height != PlaceholderSize)) continue;
            }
            File.WriteAllBytes(path, RenderPlaceholder(definition).EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            regenerated++;
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"CardAssetSync: regenerated {regenerated} procedural placeholder images. Run Sync to assign any new ones.");
        Sync();
    }

    private static Texture2D RenderPlaceholder(CardDefinition definition)
    {
        var texture = new Texture2D(PlaceholderSize, PlaceholderSize, TextureFormat.RGBA32, false);
        var baseColor = archetypeColors[(int)definition.Archetype];
        //Stable per-card seed so re-running the sync regenerates identical art
        int seed = 17;
        foreach (char c in definition.Id) seed = seed * 31 + c;
        var rng = new System.Random(seed);

        //Transparent background: figures stand directly on the field
        var pixels = new Color[PlaceholderSize * PlaceholderSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
        texture.SetPixels(pixels);

        var bodyColor = Color.Lerp(baseColor, Color.black, 0.25f + 0.15f * (float)rng.NextDouble());
        var accent = Color.Lerp(baseColor, Color.white, 0.45f + 0.3f * (float)rng.NextDouble());

        //Card-type silhouette motif so units, charms, and abilities read apart at a glance
        switch (definition.Type)
        {
            case CardType.Unit:
            {
                //Blocky figure: legs, torso, head, seeded stance and build
                int stance = 10 + rng.Next(10);
                int torsoW = 26 + rng.Next(16);
                int headR = 11 + rng.Next(7);
                FillRect(texture, 64 - stance - 6, 14, 12, 34, bodyColor);           //left leg
                FillRect(texture, 64 + stance - 6, 14, 12, 34, bodyColor);           //right leg
                FillRect(texture, 64 - torsoW / 2, 44, torsoW, 40, bodyColor);       //torso
                BlendDisc(texture, 64, 96, headR, bodyColor);                        //head
                BlendDisc(texture, 64 - torsoW / 4, 64 + rng.Next(12), 5, accent);   //emblem
                if (rng.Next(2) == 0) FillRect(texture, 64 + torsoW / 2, 40 + rng.Next(20), 6, 40, accent); //weapon
                break;
            }
            case CardType.Charm:
            {
                //Totem: plinth + tapering obelisk + floating gem
                FillRect(texture, 34, 12, 60, 12, bodyColor);
                FillRect(texture, 46, 24, 36, 46 + rng.Next(16), bodyColor);
                FillRect(texture, 54, 70, 20, 22, bodyColor);
                BlendDisc(texture, 64, 100 + rng.Next(8), 9 + rng.Next(5), accent);
                break;
            }
            default:
            {
                //Ability: radiating burst
                int spokes = 6 + rng.Next(5);
                for (int n = 0; n < spokes; n++)
                {
                    double angle = (System.Math.PI * 2 * n) / spokes + rng.NextDouble() * 0.3;
                    int ex = 64 + (int)(System.Math.Cos(angle) * (34 + rng.Next(16)));
                    int ey = 64 + (int)(System.Math.Sin(angle) * (34 + rng.Next(16)));
                    DrawThickLine(texture, 64, 64, ex, ey, 4, bodyColor);
                }
                BlendDisc(texture, 64, 64, 15 + rng.Next(6), accent);
                break;
            }
        }

        //Cost pips along the bottom edge so the art hints at the card even tiny
        for (int pip = 0; pip < definition.Cost && pip < 8; pip++)
            BlendDisc(texture, 12 + pip * 15, 6, 5, accent);

        texture.Apply();
        return texture;
    }

    private static void FillRect(Texture2D texture, int x0, int y0, int width, int height, Color color)
    {
        for (int y = Mathf.Max(0, y0); y < Mathf.Min(PlaceholderSize, y0 + height); y++)
            for (int x = Mathf.Max(0, x0); x < Mathf.Min(PlaceholderSize, x0 + width); x++)
                texture.SetPixel(x, y, color);
    }

    private static void DrawThickLine(Texture2D texture, int x0, int y0, int x1, int y1, int thickness, Color color)
    {
        int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
        for (int n = 0; n <= steps; n++)
        {
            float t = steps == 0 ? 0f : (float)n / steps;
            BlendDisc(texture, Mathf.RoundToInt(Mathf.Lerp(x0, x1, t)), Mathf.RoundToInt(Mathf.Lerp(y0, y1, t)), thickness / 2 + 1, color);
        }
    }

    private static void BlendDisc(Texture2D texture, int cx, int cy, int radius, Color color)
    {
        for (int y = Mathf.Max(0, cy - radius); y <= Mathf.Min(PlaceholderSize - 1, cy + radius); y++)
        {
            for (int x = Mathf.Max(0, cx - radius); x <= Mathf.Min(PlaceholderSize - 1, cx + radius); x++)
            {
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) > radius * radius) continue;
                texture.SetPixel(x, y, Color.Lerp(texture.GetPixel(x, y), color, color.a));
            }
        }
    }

    private static void BlendBar(Texture2D texture, int position, int thickness, Color color, bool vertical)
    {
        for (int a = 0; a < PlaceholderSize; a++)
        {
            for (int b = position; b < Mathf.Min(PlaceholderSize, position + thickness); b++)
            {
                int x = vertical ? b : a, y = vertical ? a : b;
                texture.SetPixel(x, y, Color.Lerp(texture.GetPixel(x, y), color, color.a));
            }
        }
    }
}
#endif
