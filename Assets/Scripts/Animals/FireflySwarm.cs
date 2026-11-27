using System.Collections.Generic;
using UnityEngine;

// little glowy bugs that come out at night, some fly high so you gotta jump for them
public class FireflySwarm : MonoBehaviour
{
    public GameObject fireflyPrefab;    // glowing ball with a halo + a child point light
    public Item fireflyItem;            // goes in the bag when caught
    public int count = 20;
    public float radius = 10f;          // how spread out they fly
    public float minHeight = 2f;        // above the ground, player head is about 2
    public float maxHeight = 3.8f;      // jump reach is about 3.5
    public float bob = 0.25f;           // how much each one floats up n down
    public float driftSpeed = 0.1f;     // how fast they wander
    public float catchRadius = 0.6f;    // how close you gotta get
    public float respawnTime = 60f;     // caught ones come back after this
    public float lightDistance = 20f;   // only near ones light the ground, saves fps
    public bool alwaysOn = false;       // tick to test in daytime

    // blinking
    public float blinkSpeed = 1.2f;
    public float dimGlow = 0.15f;       // how bright between flashes
    public float haloOnAbove = 0.45f;   // halo shows when glow is brighter than this

    class Fly
    {
        public Transform t;
        public Light light;
        public Behaviour halo;
        public Vector3 seed;
        public float height;
        public float blinkRate;
        public float caughtUntil;
    }

    readonly List<Fly> flies = new List<Fly>();
    Vector3 baseScale;
    float baseIntensity = 2.5f;
    float baseRange = 6f;
    Transform player;
    Inventory inventory;
    Terrain terrain;

    void Start()
    {
        PlayerStats p = FindFirstObjectByType<PlayerStats>();
        if (p != null) player = p.transform;
        inventory = FindFirstObjectByType<Inventory>();
        terrain = Terrain.activeTerrain;
        if (fireflyPrefab == null) return;

        baseScale = fireflyPrefab.transform.localScale;
        Light prefabLight = fireflyPrefab.GetComponentInChildren<Light>();
        if (prefabLight != null)
        {
            baseIntensity = prefabLight.intensity;
            baseRange = prefabLight.range;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(fireflyPrefab, transform.position, Quaternion.identity, transform);
            flies.Add(new Fly
            {
                t = go.transform,
                light = go.GetComponentInChildren<Light>(),
                halo = go.GetComponent("Halo") as Behaviour,   // halo has no proper class, this grabs it
                seed = new Vector3(Random.value * 100f, Random.value * 100f, Random.value * 100f),
                height = Random.Range(minHeight, maxHeight),   // each one picks its own height
                blinkRate = blinkSpeed * Random.Range(0.6f, 1.4f),
                caughtUntil = 0f
            });
        }
    }

    void Update()
    {
        bool night = alwaysOn || DayNight.IsNight;
        float time = Time.time;

        foreach (Fly f in flies)
        {
            bool show = night && time >= f.caughtUntil;
            if (f.t.gameObject.activeSelf != show) f.t.gameObject.SetActive(show);
            if (!show) continue;

            // lazy wobbly flying around the swarm
            float x = (Mathf.PerlinNoise(f.seed.x + time * driftSpeed, 0f) - 0.5f) * 2f * radius;
            float z = (Mathf.PerlinNoise(f.seed.z + time * driftSpeed, 1f) - 0.5f) * 2f * radius;
            float y = f.height + (Mathf.PerlinNoise(f.seed.y + time * driftSpeed * 2f, 2f) - 0.5f) * 2f * bob;

            Vector3 pos = transform.position + new Vector3(x, 0f, z);
            pos.y = GroundAt(pos) + y;
            f.t.position = pos;

            // real firefly blink: mostly dim then a quick bright flash
            float wave = 0.5f + 0.5f * Mathf.Sin(time * f.blinkRate + f.seed.x);
            float glow = Mathf.Lerp(dimGlow, 1f, Mathf.Pow(wave, 4f));
            f.t.localScale = baseScale * Mathf.Lerp(0.5f, 1f, glow);
            if (f.halo != null) f.halo.enabled = glow > haloOnAbove;

            if (f.light != null)
            {
                bool near = player == null || Vector3.Distance(player.position, pos) < lightDistance;
                f.light.enabled = near;
                f.light.intensity = baseIntensity * glow;
                f.light.range = baseRange * Mathf.Lerp(0.7f, 1f, glow);   // light circle on the ground pulses
            }

            if (player != null && Vector3.Distance(player.position + Vector3.up, pos) < catchRadius)
            {
                Catch(f);
            }
        }
    }

    // terrain height under them, but never below this objects height (keeps them above the lake)
    float GroundAt(Vector3 pos)
    {
        float floor = transform.position.y;
        if (terrain == null) return floor;
        float ground = terrain.SampleHeight(pos) + terrain.transform.position.y;
        return Mathf.Max(ground, floor);
    }

    void Catch(Fly f)
    {
        if (inventory == null || fireflyItem == null) return;
        inventory.AddItem(fireflyItem, 1);
        f.caughtUntil = Time.time + respawnTime;
        f.t.gameObject.SetActive(false);
    }

    // yellow circle = where they fly
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.85f, 1f, 0.35f, 0.8f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * (minHeight + maxHeight) * 0.5f, radius);
    }
}