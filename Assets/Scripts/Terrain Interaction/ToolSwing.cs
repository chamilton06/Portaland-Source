// ============================================================================
// ToolSwing.cs
// When the player swings, hits the nearest tree/rock
//
// To use, put this script on the Player
// ============================================================================

using UnityEngine;

public class ToolSwing : MonoBehaviour
{
    public ToolType currentTool = ToolType.None;
    public float range = 2.5f;
    public int damage = 1;

    public TerrainResources terrainResources;

    public Inventory inventory;

    void Start()
    {
        if (terrainResources == null && Terrain.activeTerrain != null)
            terrainResources = Terrain.activeTerrain.GetComponent<TerrainResources>();
        if (inventory == null)
            inventory = GetComponent<Inventory>();
    }

    public void Swing()
    {
        Debug.Log($"[Gather] Swing. tool={currentTool}, terrain={(terrainResources != null)}, inventory={(inventory != null)}");
        if (currentTool == ToolType.None || terrainResources == null) return;

        var hit = terrainResources.Hit(SwingPoint(), range * 0.6f, currentTool, damage, out bool broken);
        if (hit == null) { Debug.Log("[Gather] Nothing in range"); return; }

        Debug.Log($"[Gather] Hit {hit.name}. needs={hit.requiredTool}, broken={broken}, item={(hit.resource != null ? hit.resource.name : "NONE")}, amount={hit.amount}");

        if (hit.requiredTool != currentTool)
            Debug.Log($"Need a {hit.requiredTool} for {hit.name}");
        else if (broken && inventory != null)
            inventory.AddItem(hit.resource, hit.amount);
    }

    // Point in front of the player, at their feet height (where tree/rock bases are).
    Vector3 SwingPoint()
    {
        Vector3 fwd = transform.forward;
        fwd.y = 0f;
        return transform.position + fwd.normalized * (range * 0.5f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(SwingPoint(), range * 0.6f);
    }
}