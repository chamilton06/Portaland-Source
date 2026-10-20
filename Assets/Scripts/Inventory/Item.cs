// ============================================================================
// Item.cs
// Defines the Item ScriptableObject (name, icon, stack size, prefabs) used by
// the inventory, plus an Equipment subclass reserved for future armour and
// other equippable types.
// ============================================================================

using UnityEngine;

// ============================================================================
// Item
// ============================================================================

[CreateAssetMenu(fileName = "Item", menuName = "NewItem")]
public class Item : ScriptableObject
{
    // ------------------------------------------------------------------------
    // Info
    // ------------------------------------------------------------------------
    public string itemName;
    public Sprite icon;
    public int maxStackSize;

    // ------------------------------------------------------------------------
    // Prefabs
    // ------------------------------------------------------------------------
    public GameObject itemPrefab;
    public GameObject handItemPrefab; 
}

// ============================================================================
// Equipment
// ============================================================================

// Placeholder for armour and other equipment types. Currently empty.
public class Equipment : Item
{
}