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
            text += "<color=#2E7D32>+" + healthRestore + " Health</color>\n";
        }
        if (hungerRestore > 0f)
        {
            text += "<color=#8D5524>+" + hungerRestore + " Hunger</color>\n";
        }
        return text + base.GetStatsText();
    }
}
