using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "New Weapon")]
public class Weapon : Equipment
{
    public WeaponStats stats;
    public GameObject weaponPrefab;
}

[System.Serializable]
public class WeaponStats
{
    public int damage = 10;
    public float attackRange = 2.5f;
    public float attackCooldown = 0.5f;
}