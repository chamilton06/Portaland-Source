using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// draws ink forests on the map from the trees painted on the terrain
public class MapForests : MonoBehaviour
{
    public Terrain terrain;
    public RectTransform mapArea;
    public Sprite[] treeIcons;                                          // pack tree drawings, picked at random
    public Color treeColor = new Color(0.1f, 0.12f, 0.06f, 0.95f);      // dark ink
    public Color forestLight = new Color(0.62f, 0.68f, 0.35f, 0.75f);   // yellowy green speckle
    public Color forestDark = new Color(0.3f, 0.45f, 0.2f, 0.85f);      // deep green speckle
    public int cellsAcross = 32;          // more = smaller, denser icons
    public int minTreesPerCell = 4;       // fewer trees than this = maybe a small tree
    public float iconScale = 1.8f;        // big tree size compared to its cell
    public float iconChance = 0.6f;       // not every forest cell gets a big tree
    public float sparseChance = 0.35f;    // lone trees sometimes get a small drawing
    public int textureWidth = 256;        // green cloud detail
    public float greenSpread = 0.5f;      // how much green leaks past the trees (0 = none, 1 = a lot)
    public int seed = 7;                  // same layout every time

    private Texture2D greenTex;

    void Start()
    {
        Build();
    }

    // call again if the trees change (procedural worlds)
    public void Build()
    {
        Transform old = mapArea.Find("Forests");
        if (old != null) Destroy(old.gameObject);
        if (greenTex != null) Destroy(greenTex);
        if (terrain == null || treeIcons.Length == 0) return;

        // under everything else on the map
        RectTransform forests = MakeLayer("Forests", mapArea);
        forests.SetAsFirstSibling();

        // count trees per grid cell
        Vector3 size = terrain.terrainData.size;
        int cellsDown = Mathf.Max(1, Mathf.RoundToInt(cellsAcross * size.z / size.x));
        int[,] counts = new int[cellsAcross, cellsDown];
        foreach (TreeInstance tree in terrain.terrainData.treeInstances)
        {
            int x = Mathf.Min((int)(tree.position.x * cellsAcross), cellsAcross - 1);
            int y = Mathf.Min((int)(tree.position.z * cellsDown), cellsDown - 1);
            counts[x, y]++;
        }

        Random.InitState(seed);

        // green cloud as one picture under the trees
        RawImage green = new GameObject("Green", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
        StretchInto(green.rectTransform, forests);
        greenTex = MakeGreen(counts, cellsDown);
        green.texture = greenTex;
        green.raycastTarget = false;

        // one layer per icon so the ui can batch them
        Dictionary<Sprite, RectTransform> treeLayers = new Dictionary<Sprite, RectTransform>();
        foreach (Sprite icon in treeIcons)
        {
            if (icon != null && !treeLayers.ContainsKey(icon)) treeLayers[icon] = MakeLayer(icon.name, forests);
        }

        float cw = 1f / cellsAcross;
        float ch = 1f / cellsDown;
        for (int x = 0; x < cellsAcross; x++)
        {
            for (int y = 0; y < cellsDown; y++)
            {
                int count = counts[x, y];
                if (count == 0) continue;

                // big trees in forests, small ones dotted around the edges
                bool dense = count >= minTreesPerCell;
                if (Random.value > (dense ? iconChance : sparseChance)) continue;
                float scale = dense ? iconScale : iconScale * 0.55f;

                Sprite icon = treeIcons[Random.Range(0, treeIcons.Length)];
                if (icon == null) continue;
                Vector2 centre = new Vector2((x + 0.5f) * cw, (y + 0.5f) * ch);
                Vector2 jitter = new Vector2(Random.Range(-0.3f, 0.3f) * cw, Random.Range(-0.3f, 0.3f) * ch);
                MakeImage(treeLayers[icon], icon, centre + jitter, new Vector2(cw, ch) * scale);
            }
        }
    }

    // speckled green cloud wherever there are trees, fading at the edges
    Texture2D MakeGreen(int[,] counts, int cellsDown)
    {
        int w = textureWidth;
        int h = Mathf.Max(1, Mathf.RoundToInt(w * (float)cellsDown / cellsAcross));
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[w * h];
        for (int py = 0; py < h; py++)
        {
            for (int px = 0; px < w; px++)
            {
                float u = (px + 0.5f) / w * cellsAcross - 0.5f;
                float v = (py + 0.5f) / h * cellsDown - 0.5f;
                float density = SampleDensity(counts, cellsDown, u, v);

                // wobbly cloud edge
                float cloud = Mathf.PerlinNoise(px * 0.05f + seed, py * 0.05f + seed);
                float edge = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, 0.55f, density + (cloud - 0.5f) * 0.4f));

                // grainy dots + patchy colour
                float speck = Random.value;
                Color c = Color.Lerp(forestLight, forestDark, Mathf.PerlinNoise(px * 0.2f, py * 0.2f) * 0.6f + speck * 0.4f);
                c.a *= edge * (0.6f + 0.4f * speck);
                pixels[py * w + px] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    // smooth 0-1 amount of trees between cell centres
    float SampleDensity(int[,] counts, int cellsDown, float u, float v)
    {
        int x0 = Mathf.FloorToInt(u);
        int y0 = Mathf.FloorToInt(v);
        float tx = u - x0;
        float ty = v - y0;
        float bottom = Mathf.Lerp(Spread(counts, cellsDown, x0, y0), Spread(counts, cellsDown, x0 + 1, y0), tx);
        float top = Mathf.Lerp(Spread(counts, cellsDown, x0, y0 + 1), Spread(counts, cellsDown, x0 + 1, y0 + 1), tx);
        return Mathf.Lerp(bottom, top, ty);
    }

    float Cell(int[,] counts, int cellsDown, int x, int y)
    {
        if (x < 0 || y < 0 || x >= cellsAcross || y >= cellsDown) return 0f;
        return Mathf.Clamp01(counts[x, y] / (float)(minTreesPerCell * 2));
    }

    // leaks a bit of green into the cells around the trees for a softer fade
    float Spread(int[,] counts, int cellsDown, int x, int y)
    {
        float amount = Cell(counts, cellsDown, x, y);
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                amount = Mathf.Max(amount, Cell(counts, cellsDown, x + dx, y + dy) * greenSpread);
            }
        }
        return amount;
    }

    RectTransform MakeLayer(string layerName, Transform parent)
    {
        RectTransform rt = new GameObject(layerName, typeof(RectTransform)).GetComponent<RectTransform>();
        StretchInto(rt, parent);
        return rt;
    }

    void StretchInto(RectTransform rt, Transform parent)
    {
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // placed with anchors so it scales with the map
    void MakeImage(Transform parent, Sprite sprite, Vector2 centre, Vector2 size)
    {
        GameObject go = new GameObject(sprite.name, typeof(RectTransform), typeof(Image));
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = centre - size * 0.5f;
        rt.anchorMax = centre + size * 0.5f;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = treeColor;
        img.preserveAspect = true;
        img.raycastTarget = false;
    }
}