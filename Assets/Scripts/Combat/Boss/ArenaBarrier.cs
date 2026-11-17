using UnityEngine;

// A wall of swirling blood mist that seals a boss arena: put it in a doorway and drag it into
// the boss's Boss Encounter > Barrier. It's switched on when the fight starts and off when the
// boss dies. The Box Collider is the wall: size it to fill the doorway (it shows as a red box in
// the Scene view). The mist is built when the game runs, to the collider's size.
[RequireComponent(typeof(BoxCollider))]
public class ArenaBarrier : MonoBehaviour
{
    [Tooltip("Colour of the mist and its glow.")]
    public Color colour = new Color(0.75f, 0.04f, 0.06f, 1f);
    [Tooltip("How much the dark fog layer hides what's behind the door. 1 = almost solid.")]
    [Range(0f, 1f)] public float opacity = 0.85f;
    [Tooltip("How bright the swirling red layers and the glow are.")]
    [Range(0f, 4f)] public float brightness = 1.8f;
    [Tooltip("How fast the mist swirls.")]
    [Min(0f)] public float swirlSpeed = 0.15f;
    [Tooltip("Optional. Played when the barrier appears.")]
    public AudioClip appearSound;
    [Range(0f, 1f)] public float appearVolume = 0.8f;
    [Min(0.05f)] public float fadeInTime = 0.8f;
    [Tooltip("Seconds it takes to fade away once the boss is beaten.")]
    [Min(0.05f)] public float fadeOutTime = 1.5f;

    private static Texture2D mistTexture;
    private Material fog;          // Dark, mostly see-through-proof layer.
    private Material[] swirls;     // Bright red layers drifting in different directions.
    private Light glow;
    private float shownAt;
    private float clearingAt = -1f;   // When it started fading away, or -1.

    private void Awake()
    {
        BoxCollider wall = GetComponent<BoxCollider>();
        wall.isTrigger = false; // Solid: nothing walks through.
        BuildMist(wall);
    }

    private void OnEnable()
    {
        shownAt = Time.time;
        clearingAt = -1f;
        GetComponent<BoxCollider>().enabled = true;
        // Not when the scene first loads, only when a fight switches it on.
        if (appearSound != null && Time.timeSinceLevelLoad > 0.5f)
            AudioSource.PlayClipAtPoint(appearSound, transform.position, appearVolume);
    }

    // Lets the player through straight away and fades the mist out, then switches itself off.
    public void Clear()
    {
        if (!isActiveAndEnabled) return;
        GetComponent<BoxCollider>().enabled = false;
        clearingAt = Time.time;
    }

    // A dark fog that covers the doorway, with two bright swirling layers over it on each side.
    private void BuildMist(BoxCollider wall)
    {
        if (mistTexture == null) mistTexture = MakeMistTexture();
        // Both shaders are double-sided, so the mist shows from inside and outside.
        Shader blended = Shader.Find("Sprites/Default");
        Shader additive = Shader.Find("Legacy Shaders/Particles/Additive") ?? blended;
        Vector2 tiling = new Vector2(Mathf.Max(1f, wall.size.x / 2f), Mathf.Max(1f, wall.size.y / 2f));

        fog = MakeLayer(wall, blended, tiling, 0f);
        swirls = new[]
        {
            MakeLayer(wall, additive, tiling, -0.03f),
            MakeLayer(wall, additive, tiling * 1.6f, 0.03f),
        };

        glow = new GameObject("Glow").AddComponent<Light>();
        glow.transform.SetParent(transform, false);
        glow.transform.localPosition = wall.center;
        glow.color = colour;
        glow.range = Mathf.Max(wall.size.x, wall.size.y) * 2.2f;
        glow.shadows = LightShadows.None;
    }

    private Material MakeLayer(BoxCollider wall, Shader shader, Vector2 tiling, float offset)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(quad.GetComponent<Collider>());
        quad.name = "Mist";
        quad.transform.SetParent(transform, false);
        quad.transform.localPosition = wall.center + Vector3.forward * offset;
        quad.transform.localScale = new Vector3(wall.size.x, wall.size.y, 1f);
        Material material = new Material(shader) { mainTexture = mistTexture, mainTextureScale = tiling };
        Renderer renderer = quad.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return material;
    }

    private void Update()
    {
        float fade = Mathf.Clamp01((Time.time - shownAt) / fadeInTime);
        if (clearingAt >= 0f)
        {
            fade *= 1f - Mathf.Clamp01((Time.time - clearingAt) / fadeOutTime);
            if (fade <= 0f) { gameObject.SetActive(false); return; }
        }
        float pulse = 0.8f + 0.2f * Mathf.Sin(Time.time * 2.2f);
        float t = Time.time * swirlSpeed;

        fog.mainTextureOffset = new Vector2(t * 0.3f, -t * 0.5f);
        fog.color = new Color(colour.r * 0.35f, colour.g * 0.2f, colour.b * 0.25f, opacity * fade);

        for (int i = 0; i < swirls.Length; i++)
        {
            float direction = i == 0 ? 1f : -1.3f;
            swirls[i].mainTextureOffset = new Vector2(t * direction, t * 0.7f);
            Color tint = colour * (brightness * pulse * fade * (i == 0 ? 0.5f : 0.35f));
            tint.a = 1f;
            if (swirls[i].HasProperty("_TintColor")) swirls[i].SetColor("_TintColor", tint);
            else swirls[i].color = tint;
        }
        glow.intensity = brightness * 1.8f * fade * pulse;
    }

    // Soft, seamless cloudy noise for the mist.
    private static Texture2D MakeMistTexture()
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat, name = "Mist" };
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = (float)x / size, v = (float)y / size, value = 0f, amplitude = 0.6f;
                for (int octave = 0; octave < 4; octave++)
                {
                    float period = 4f * (1 << octave);
                    value += SeamlessNoise(u * period, v * period, period) * amplitude;
                    amplitude *= 0.5f;
                }
                value = Mathf.Clamp01(value);
                pixels[y * size + x] = new Color(value, value, value, 0.45f + 0.55f * value);
            }
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    // Perlin noise that repeats every `period` units, by blending the four wrapped corners.
    private static float SeamlessNoise(float x, float y, float period)
    {
        float u = x / period, v = y / period;
        return Mathf.PerlinNoise(x, y) * (1f - u) * (1f - v)
             + Mathf.PerlinNoise(x - period, y) * u * (1f - v)
             + Mathf.PerlinNoise(x, y - period) * (1f - u) * v
             + Mathf.PerlinNoise(x - period, y - period) * u * v;
    }

    // Shows the wall in the Scene view while you place it.
    private void OnDrawGizmos()
    {
        BoxCollider wall = GetComponent<BoxCollider>();
        if (wall == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.8f, 0.05f, 0.05f, 0.35f);
        Gizmos.DrawCube(wall.center, wall.size);
        Gizmos.color = new Color(1f, 0.1f, 0.1f, 0.9f);
        Gizmos.DrawWireCube(wall.center, wall.size);
    }
}
