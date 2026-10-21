using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "New Weapon")]
public class Weapon : Equipment
{
    public WeaponStats stats;
    public GameObject weaponPrefab;

    public override string GetStatsText()
    {
        string text = StatLine("Attack", "+" + stats.damage, "#B71C1C");
        text += StatLine("Range", stats.attackRange.ToString("0.0"));
        text += StatLine("Speed", (1f / stats.attackCooldown).ToString("0.0") + "/sec");
        return text + base.GetStatsText();
    }
}

[System.Serializable]
public class WeaponStats
{
    public int damage = 10;
    public float attackRange = 2.5f;
    public float attackCooldown = 0.5f;
}
