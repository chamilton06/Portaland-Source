// ============================================================================
// TerrainResources.cs
// Gives each painted asset (Tree, rock) a health, and when its health reaches
// 0, it disappears and the player gets its resources.
//
// To use, put this script on the Terrain
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

public enum ToolType { None, Axe, Pickaxe }

[RequireComponent(typeof(Terrain))]
public class TerrainResources : MonoBehaviour
{
    [System.Serializable]
    public class ResourceType
    {
        public string name = "Oak";
        public int prototypeIndex;
        public ToolType requiredTool = ToolType.Axe;
        public Item resource;                           // naturally collected Items
        public int health = 5;
        public int amount = 3;
    }

    public ResourceType[] types;

    Terrain terrain;
    TerrainData data;
    TreeInstance[] original;

    // Remaining health per tree/rock, keyed by its position (array indices shift when one is removed).
    readonly Dictionary<Vector3, int> health = new Dictionary<Vector3, int>();

    void Awake()
    {
        terrain = GetComponent<Terrain>();
        data = terrain.terrainData;
        original = data.treeInstances;
    }

    void OnDisable()
    {
        data.treeInstances = original;
        RefreshColliders();
    }

    // Hits the nearest harvestable tree/rock within range of 'point'.
    // Returns its type (null if nothing in range). 'broken' is true when it was destroyed.
    // If the returned type's requiredTool doesn't match 'tool', nothing happened (wrong tool).
    public ResourceType Hit(Vector3 point, float range, ToolType tool, int damage, out bool broken)
    {
        broken = false;
        TreeInstance[] trees = data.treeInstances;
        int index = FindNearest(trees, point, range, out ResourceType type);
        // break if index < 0 or using the wrong tool
        if (index < 0 || type.requiredTool != tool) return type;

        Vector3 key = trees[index].position;
        int hp = health.TryGetValue(key, out int h) ? h : type.health;  // get the current health
        hp -= damage;   // deal damage

        // if not destoryed, update health and leave
        if (hp > 0)
        {
            health[key] = hp;
            return type;
        }

        // remove asset if destroyed, ang give its resources
        health.Remove(key);
        var list = new List<TreeInstance>(trees);
        list.RemoveAt(index);
        data.treeInstances = list.ToArray();
        RefreshColliders();
        broken = true;
        return type;
    }

    // Finds the nearest asset to the swing
    int FindNearest(TreeInstance[] trees, Vector3 point, float range, out ResourceType bestType)
    {
        Vector3 size = data.size;
        Vector3 origin = terrain.GetPosition();
        int best = -1;
        bestType = null;
        float bestSqr = range * range;

        // loop through all assets and determine which is closest to the player
        for (int i = 0; i < trees.Length; i++)
        {
            ResourceType type = GetType(trees[i].prototypeIndex);
            if (type == null) continue;

            Vector3 world = Vector3.Scale(trees[i].position, size) + origin;
            if (Mathf.Abs(world.y - point.y) > 3f) continue; // don't hit things below a cliff

            float dx = world.x - point.x, dz = world.z - point.z;
            float sqr = dx * dx + dz * dz;

            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = i;
                bestType = type;
            }
        }
        return best;
    }

    ResourceType GetType(int prototypeIndex)
    {
        foreach (var t in types)
            if (t.prototypeIndex == prototypeIndex) return t;
        return null;
    }

    // tree colliders don't update on their own after removal; toggling the collider rebuilds them.
    void RefreshColliders()
    {
        var col = GetComponent<TerrainCollider>();
        if (col == null) return;
        col.enabled = false;
        col.enabled = true;
    }
}