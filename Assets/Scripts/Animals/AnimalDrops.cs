using System.Collections;
using System.Collections.Generic;

using UnityEngine;

// puts loot in the players bag when this animal dies
// every drop rolls its own chance so rare stuff only shows up sometimes
[RequireComponent(typeof(Health))]
public class AnimalDrops : MonoBehaviour
{
    [System.Serializable]
    public class Drop
    {
        public Item item;
        public int min = 1;
        public int max = 1;
        [Range(0f, 1f)] public float chance = 1f;   // 1 = always, 0.05 = super rare
    }

    public List<Drop> drops = new List<Drop>();
    public float lootMultiplier = 1f;   // more of everything, bosses use 2
    public float rareBoost = 1f;        // multiplies every chance, bosses use like 3

    Inventory inventory;

    void Awake()
    {
        GetComponent<Health>().onDeath.AddListener(GiveLoot);
    }

    void GiveLoot()
    {
        if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
        if (inventory == null) return;

        foreach (Drop drop in drops)
        {
            if (drop.item == null) continue;
            if (Random.value > drop.chance * rareBoost) continue;   // unlucky

            int amount = Mathf.RoundToInt(Random.Range(drop.min, drop.max + 1) * lootMultiplier);
            if (amount <= 0) continue;

            inventory.AddItem(drop.item, amount);
            Debug.Log(name + " dropped " + amount + " " + drop.item.itemName);
        }
    }
}