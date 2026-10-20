using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "NewItem")]

public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStackSize;
    public GameObject itemPrefab;
    public GameObject handItemPrefab; 
}

public class Equipment : Item
{
    // This will be used in future for armour or other equipment types. For now, it is just a placeholder.
}