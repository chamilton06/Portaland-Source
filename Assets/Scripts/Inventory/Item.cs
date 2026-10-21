using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "NewItem")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStackSize;
    public Vector2Int inventorySize;

    [TextArea(2, 4)]
    public string shortDescription;
    public float weight;
    public int maxDurability;               // 0 = doesnt break
    public JournalEntry journalEntry;       // lore page

    public GameObject itemPrefab;
    public GameObject handItemPrefab;

    public virtual string GetStatsText()
    {
        string text = "";
        if (weight > 0f)
        {
            text += "Weight: " + weight.ToString("0.0") + "\n";
        }
        if (maxDurability > 0)
        {
            text += "Durability: " + maxDurability + "\n";
        }
        return text;
    }
}

public class Equipment : Item
{
}
