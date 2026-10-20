using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "New Weapon")]
public class Weapon : Equipment
{
    public WeaponStats stats;
    public GameObject weaponPrefab;

    public override string GetStatsText()
    {
        string text = "<color=#B71C1C>+" + stats.damage + " Attack</color>\n";
        text += "Range: " + stats.attackRange.ToString("0.0") + "\n";
        text += "Speed: " + (1f / stats.attackCooldown).ToString("0.0") + " hits/sec\n";
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
