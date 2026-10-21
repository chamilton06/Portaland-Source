using UnityEngine;

[CreateAssetMenu(fileName = "Food", menuName = "New Food")]
public class FoodItem : Item
{
    public float healthRestore;
    public float hungerRestore;

    public override string GetStatsText()
    {
        string text = "";
        if (healthRestore > 0f)
        {
            text += StatLine("Health", "+" + healthRestore, "#2E7D32");
        }
        if (hungerRestore > 0f)
        {
            text += StatLine("Hunger", "+" + hungerRestore, "#8D5524");
        }
        return text + base.GetStatsText();
    }
}
