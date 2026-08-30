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

    private static Texture2D RenderPlaceholder(CardDefinition definition)
    {
        var texture = new Texture2D(PlaceholderSize, PlaceholderSize, TextureFormat.RGBA32, false);
        var baseColor = archetypeColors[(int)definition.Archetype];
        //Stable per-card seed so re-running the sync regenerates identical art
        int seed = 17;
        foreach (char c in definition.Id) seed = seed * 31 + c;
        var rng = new System.Random(seed);

        var background = Color.Lerp(baseColor, Color.black, 0.55f);
        var pixels = new Color[PlaceholderSize * PlaceholderSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = background;
        texture.SetPixels(pixels);

        //A few seeded translucent discs and bars in the archetype's palette
        int shapes = 4 + rng.Next(3);
        for (int n = 0; n < shapes; n++)
        {
            var tint = Color.Lerp(baseColor, Color.white, 0.15f + 0.55f * (float)rng.NextDouble());
            tint.a = 0.55f;
            if (rng.Next(2) == 0)
                BlendDisc(texture, rng.Next(PlaceholderSize), rng.Next(PlaceholderSize), 12 + rng.Next(34), tint);
            else
                BlendBar(texture, rng.Next(PlaceholderSize), 6 + rng.Next(14), tint, rng.Next(2) == 0);
        }

        //Cost pips along the bottom edge so the art hints at the card even tiny
        for (int pip = 0; pip < definition.Cost && pip < 8; pip++)
            BlendDisc(texture, 12 + pip * 15, 10, 5, Color.Lerp(baseColor, Color.white, 0.8f));

        texture.Apply();
        return texture;
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
