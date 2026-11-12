using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ticks off a quest task after picking up enough of an item
public class QuestTaskCollect : MonoBehaviour
{
    public Quest quest;
    public int taskIndex;          // 0 = first task in the list
    public Item item;
    public int amount = 1;
    public Inventory inventory;    // finds itself if empty

    private int collected;

    void Start()
    {
        if (inventory == null) inventory = FindFirstObjectByType<Inventory>();
        inventory.ItemAdded += OnItemAdded;
    }

    void OnDestroy()
    {
        if (inventory != null) inventory.ItemAdded -= OnItemAdded;
    }

    void OnItemAdded(Item added, int count)
    {
        if (added != item || QuestLog.Instance == null) return;

        collected += count;
        if (collected >= amount)
        {
            QuestLog.Instance.CompleteTask(quest, taskIndex);
        }
    }
}
