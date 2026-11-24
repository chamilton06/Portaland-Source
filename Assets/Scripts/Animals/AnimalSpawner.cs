using System.Collections.Generic;
using UnityEngine;

// keeps an area stocked with random animals, refills after they die
public class AnimalSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public GameObject prefab;
        public float weight = 1f;   // higher = shows up more often
    }

    public List<Entry> animals = new List<Entry>();
    public float radius = 25f;              // area size
    public int maxAlive = 5;                // never more than this at once
    public float fillInterval = 3f;         // secs between spawns while filling up
    public float respawnDelay = 30f;        // extra wait after one dies
    public float minPlayerDistance = 15f;   // dont pop in right next to the player
    public float activeRange = 120f;        // only spawns when player is this close to the edge, 0 = always
    public bool onlyOnTerrain = true;       // no animals on roofs
    public bool fillOnStart = false;        // spawn everything instantly at start

    readonly List<GameObject> alive = new List<GameObject>();
    float nextSpawnAt;
    int lastCount;
    Transform player;

    void Start()
    {
        PlayerStats p = FindFirstObjectByType<PlayerStats>();
        if (p != null) player = p.transform;

        if (fillOnStart)
        {
            for (int i = 0; i < maxAlive; i++) TrySpawn();
        }
        lastCount = alive.Count;
        nextSpawnAt = Time.time;   // start filling straight away
    }

    void Update()
    {
        alive.RemoveAll(a => a == null);   // dead ones get destroyed by AnimalAI

        // something died, wait a bit before replacing it
        if (alive.Count < lastCount) nextSpawnAt = Mathf.Max(nextSpawnAt, Time.time + respawnDelay);
        lastCount = alive.Count;

        if (alive.Count >= maxAlive) return;
        if (Time.time < nextSpawnAt) return;
        if (activeRange > 0f && player != null && Vector3.Distance(player.position, transform.position) > activeRange + radius) return;

        TrySpawn();
        lastCount = alive.Count;
        nextSpawnAt = Time.time + fillInterval;
    }

    bool TrySpawn()
    {
        GameObject prefab = PickAnimal();
        if (prefab == null) return false;

        // a few goes at finding a decent spot
        for (int tries = 0; tries < 10; tries++)
        {
            Vector2 r = Random.insideUnitCircle * radius;
            Vector3 top = transform.position + new Vector3(r.x, 100f, r.y);
            if (!Physics.Raycast(top, Vector3.down, out RaycastHit hit, 300f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) continue;
            if (onlyOnTerrain && !(hit.collider is TerrainCollider)) continue;
            if (player != null && Vector3.Distance(hit.point, player.position) < minPlayerDistance) continue;

            Quaternion facing = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            GameObject animal = Instantiate(prefab, hit.point, facing);
            alive.Add(animal);
            return true;
        }
        return false;
    }

    // weighted random pick
    GameObject PickAnimal()
    {
        float total = 0f;
        foreach (Entry e in animals) if (e.prefab != null) total += e.weight;
        if (total <= 0f) return null;

        float roll = Random.value * total;
        foreach (Entry e in animals)
        {
            if (e.prefab == null) continue;
            roll -= e.weight;
            if (roll <= 0f) return e.prefab;
        }
        return animals[animals.Count - 1].prefab;
    }

    // green circle = spawn area
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}