using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ticks off a quest task when this thing dies
[RequireComponent(typeof(Health))]
public class QuestTaskOnDeath : MonoBehaviour
{
    public Quest quest;
    public int taskIndex;   // 0 = first task in the list

    void Awake()
    {
        GetComponent<Health>().onDeath.AddListener(OnDied);
    }

    void OnDied()
    {
        if (QuestLog.Instance != null) QuestLog.Instance.CompleteTask(quest, taskIndex);
    }
}