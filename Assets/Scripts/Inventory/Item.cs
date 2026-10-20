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