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
    public string fart = "fart";
}