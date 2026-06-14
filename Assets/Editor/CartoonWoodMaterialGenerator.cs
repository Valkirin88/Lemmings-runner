using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CartoonWoodMaterialGenerator
{
    private const int DefaultTextureSize = 512;
    private const int DefaultPlankCount = 6;

    [MenuItem("Tools/Art/Generate Cartoon Wood Material")]
    public static void Generate()
    {
        const string outputDir = "Assets/Generated/CartoonWood";
        EnsureDirectory(outputDir);

        int size = DefaultTextureSize;
        int planks = DefaultPlankCount;

        // Generate texture data (tileable).
        float[,] height = GenerateHeight(size, planks, seed: 1337);
        Texture2D albedo = GenerateAlbedo(size, planks, height, seed: 1337);
        Texture2D heightTex = ToGrayscaleTexture(height);
        Texture2D normalTex = GenerateNormalFromHeight(height, strength: 6.5f);

        string albedoPath = $"{outputDir}/Wood_Albedo.png";
        string heightPath = $"{outputDir}/Wood_Height.png";
        string normalPath = $"{outputDir}/Wood_Normal.png";
        string matPath = $"{outputDir}/Wood_Cartoon.mat";

        WritePngAndImport(albedoPath, albedo);
        WritePngAndImport(heightPath, heightTex);
        WritePngAndImport(normalPath, normalTex);

        ConfigureTextureImport(albedoPath, isNormalMap: false, sRGB: true);
        ConfigureTextureImport(heightPath, isNormalMap: false, sRGB: false);
        ConfigureTextureImport(normalPath, isNormalMap: true, sRGB: false);

        // Load imported textures so Material references the assets, not in-memory Texture2D objects.
        var albedoAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
        var normalAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);

        var mat = CreateMaterial(albedoAsset, normalAsset);
        if (mat == null)
        {
            Debug.LogError("Could not find a suitable shader (URP/HDRP/Standard).");
            return;
        }

        // Replace existing material if it exists.
        var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (existing != null)
        {
            EditorUtility.CopySerialized(mat, existing);
            UnityEngine.Object.DestroyImmediate(mat);
            mat = existing;
        }
        else
        {
            AssetDatabase.CreateAsset(mat, matPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = mat;
        EditorGUIUtility.PingObject(mat);

        Debug.Log($"Generated cartoon wood material at: {matPath}");
    }

    private static void EnsureDirectory(string assetPath)
    {
        // assetPath is like "Assets/Generated/CartoonWood"
        string full = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
        Directory.CreateDirectory(full);
    }

    private static void WritePngAndImport(string assetPath, Texture2D tex)
    {
        byte[] png = tex.EncodeToPNG();
        string full = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
        File.WriteAllBytes(full, png);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }

    private static void ConfigureTextureImport(string assetPath, bool isNormalMap, bool sRGB)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        importer.wrapMode = TextureWrapMode.Repeat;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.sRGBTexture = sRGB;

        if (isNormalMap)
        {
            importer.textureType = TextureImporterType.NormalMap;
        }

        importer.SaveAndReimport();
    }

    private static Material CreateMaterial(Texture2D albedo, Texture2D normal)
    {
        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit") ??
            Shader.Find("HDRP/Lit") ??
            Shader.Find("Standard");

        if (shader == null) return null;

        var mat = new Material(shader);

        if (albedo != null)
        {
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", albedo);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", albedo);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
        }

        if (normal != null)
        {
            if (mat.HasProperty("_BumpMap")) mat.SetTexture("_BumpMap", normal);
            if (mat.HasProperty("_NormalMap")) mat.SetTexture("_NormalMap", normal);

            // Common normal keywords.
            mat.EnableKeyword("_NORMALMAP");
        }

        // Make it non-metallic and fairly rough (cartoony).
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.2f); // Standard
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.2f); // URP/HDRP variants

        return mat;
    }

    private static float[,] GenerateHeight(int size, int planks, int seed)
    {
        var h = new float[size, size];
        float plankGap = 0.06f;          // fraction of plank height
        float bevelWidth = 0.18f;        // how wide the edge bevel is (within plank)
        float grainStrength = 0.07f;
        float noiseStrength = 0.05f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = (float)x / size;
                float v = (float)y / size;

                float plankPos = v * planks;
                int plankIndex = FloorToInt(plankPos);
                float t = Frac(plankPos); // 0..1 within plank

                float edge = Mathf.Min(t, 1f - t); // 0 at edges
                float gap = 1f - SmoothStep(0f, plankGap, edge); // 1 at seam, 0 inside

                float bevel = SmoothStep(0f, bevelWidth, edge); // 0 at edge, 1 towards center

                // Per-plank offsets (tileable because plankIndex wraps by planks).
                int wrappedPlank = Mod(plankIndex, planks);
                float plankRnd = Hash01(wrappedPlank, 0, seed);
                float grainPhase = plankRnd * 10f;

                // Tileable noise for subtle variation.
                float n1 = TileableValueNoise01(u, v, period: 16, seed: seed);
                float n2 = TileableValueNoise01(u + 0.37f, v + 0.11f, period: 8, seed: seed + 17);

                // Simple stylized grain (periodic).
                float grain = Mathf.Sin((u * (6f + plankRnd * 4f) + grainPhase) * (Mathf.PI * 2f));
                grain = grain * 0.5f + 0.5f; // 0..1

                float baseHeight = 0.60f;
                float seamCut = gap * 0.35f;
                float bevelLift = (1f - gap) * (1f - bevel) * 0.06f;
                float grainLift = (grain - 0.5f) * grainStrength;
                float noise = ((n1 * 0.7f + n2 * 0.3f) - 0.5f) * noiseStrength;

                float height = baseHeight - seamCut + bevelLift + grainLift + noise;
                h[x, y] = Mathf.Clamp01(height);
            }
        }

        return h;
    }

    private static Texture2D GenerateAlbedo(int size, int planks, float[,] height, int seed)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: true);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;

        Color baseA = new Color(0.78f, 0.55f, 0.28f, 1f);
        Color baseB = new Color(0.68f, 0.45f, 0.22f, 1f);
        Color seam = new Color(0.30f, 0.20f, 0.12f, 1f);
        Color outline = new Color(0.18f, 0.12f, 0.08f, 1f);

        float plankGap = 0.06f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = (float)x / size;
                float v = (float)y / size;

                float plankPos = v * planks;
                int plankIndex = FloorToInt(plankPos);
                float t = Frac(plankPos);
                float edge = Mathf.Min(t, 1f - t);
                float gap = 1f - SmoothStep(0f, plankGap, edge);

                int wrappedPlank = Mod(plankIndex, planks);
                float plankRnd = Hash01(wrappedPlank, 0, seed);

                // Mix two base colors per plank for cartoony variation.
                Color plankBase = Color.Lerp(baseA, baseB, plankRnd);

                // Height-based subtle shading (still flat-ish).
                float h = height[x, y];
                float shade = Mathf.Lerp(0.92f, 1.08f, h);

                // Grain tint (very subtle).
                float grain = TileableValueNoise01(u * 1.0f, v * 1.0f, period: 16, seed: seed + 99);
                float grainTint = Mathf.Lerp(0.97f, 1.03f, grain);

                Color c = plankBase * (shade * grainTint);

                if (gap > 0.02f)
                {
                    // Seam darkening
                    c = Color.Lerp(c, seam, Mathf.Clamp01(gap * 1.2f));

                    // Thin darker outline right at the edge for a clean toon separation.
                    if (gap > 0.55f)
                        c = Color.Lerp(c, outline, Mathf.Clamp01((gap - 0.55f) * 1.8f));
                }

                // Occasional simple knot-ish accent (very stylized), tileable.
                float knot = TileableValueNoise01(u + wrappedPlank * 0.17f, v + 0.13f, period: 8, seed: seed + 7);
                if (knot > 0.92f && gap < 0.1f)
                {
                    c *= 0.92f;
                }

                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(updateMipmaps: true, makeNoLongerReadable: false);
        return tex;
    }

    private static Texture2D ToGrayscaleTexture(float[,] height)
    {
        int size = height.GetLength(0);
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: true);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float h = height[x, y];
                tex.SetPixel(x, y, new Color(h, h, h, 1f));
            }
        }

        tex.Apply(updateMipmaps: true, makeNoLongerReadable: false);
        return tex;
    }

    private static Texture2D GenerateNormalFromHeight(float[,] height, float strength)
    {
        int size = height.GetLength(0);
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: true);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            int y1 = (y + 1) % size;
            int y0 = (y - 1 + size) % size;

            for (int x = 0; x < size; x++)
            {
                int x1 = (x + 1) % size;
                int x0 = (x - 1 + size) % size;

                float hL = height[x0, y];
                float hR = height[x1, y];
                float hD = height[x, y0];
                float hU = height[x, y1];

                float dx = (hR - hL) * strength;
                float dy = (hU - hD) * strength;

                Vector3 n = new Vector3(-dx, -dy, 1f).normalized;
                Color c = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply(updateMipmaps: true, makeNoLongerReadable: false);
        return tex;
    }

    // --- Tileable value noise helpers (periodic on [0..1)) ---

    private static float TileableValueNoise01(float u, float v, int period, int seed)
    {
        // Scale to lattice.
        float x = u * period;
        float y = v * period;

        int x0 = FloorToInt(x);
        int y0 = FloorToInt(y);
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        float tx = x - x0;
        float ty = y - y0;

        float v00 = Hash01(Mod(x0, period), Mod(y0, period), seed);
        float v10 = Hash01(Mod(x1, period), Mod(y0, period), seed);
        float v01 = Hash01(Mod(x0, period), Mod(y1, period), seed);
        float v11 = Hash01(Mod(x1, period), Mod(y1, period), seed);

        float sx = tx * tx * (3f - 2f * tx);
        float sy = ty * ty * (3f - 2f * ty);

        float a = Mathf.Lerp(v00, v10, sx);
        float b = Mathf.Lerp(v01, v11, sx);
        return Mathf.Lerp(a, b, sy);
    }

    private static float Hash01(int x, int y, int seed)
    {
        unchecked
        {
            int h = seed;
            h = (h * 397) ^ x;
            h = (h * 397) ^ y;
            h ^= (h << 13);
            h ^= (h >> 17);
            h ^= (h << 5);
            // Map int to [0,1)
            uint u = (uint)h;
            return (u & 0x00FFFFFFu) / 16777216f;
        }
    }

    private static int Mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }

    private static int FloorToInt(float f) => (int)Math.Floor(f);

    private static float Frac(float f) => f - Mathf.Floor(f);

    private static float SmoothStep(float a, float b, float t)
    {
        if (Mathf.Abs(b - a) < 1e-6f) return 0f;
        t = Mathf.Clamp01((t - a) / (b - a));
        return t * t * (3f - 2f * t);
    }
}
