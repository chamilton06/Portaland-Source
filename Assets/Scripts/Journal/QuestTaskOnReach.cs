using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ticks off a quest task when the player walks in here
[RequireComponent(typeof(Collider))]
public class QuestTaskOnReach : MonoBehaviour
{
    public Quest quest;
    public int taskIndex;            // 0 = first task in the list
    public bool destroyAfter = true; // only needs to tick once

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerStats>() == null || QuestLog.Instance == null) return;

        QuestLog.Instance.CompleteTask(quest, taskIndex);
        if (destroyAfter) Destroy(gameObject);
    }
}
