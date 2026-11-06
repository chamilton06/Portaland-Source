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
            text += StatLine("Weight", weight.ToString("0.0"));
        }
        if (maxDurability > 0)
        {
            text += StatLine("Durability", maxDurability.ToString());
        }
        return text;
    }

    // label on the left, value lined up in a column
    protected static string StatLine(string label, string value, string color = null)
    {
        if (color != null)
        {
            value = "<color=" + color + ">" + value + "</color>";
        }
        return label + "<pos=60%>" + value + "\n";
    }
}

public class Equipment : Item
{
}
