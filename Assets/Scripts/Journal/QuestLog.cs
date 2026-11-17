using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// keeps which quest tasks are done while playing
public class QuestLog : MonoBehaviour
{
    public static QuestLog Instance { get; private set; }

    public event System.Action<Quest> Changed;   // tracker listens

    private readonly Dictionary<Quest, HashSet<int>> doneTasks = new Dictionary<Quest, HashSet<int>>();

    private readonly HashSet<Quest> accepted = new HashSet<Quest>();

    public bool IsAccepted(Quest quest) => accepted.Contains(quest);

    public void Accept(Quest quest)
    {
        if (quest != null && accepted.Add(quest)) Changed?.Invoke(quest);
    }

    void Awake()
    {
        Instance = this;
    }

    public bool IsTaskDone(Quest quest, int index)
    {
        return doneTasks.TryGetValue(quest, out HashSet<int> done) && done.Contains(index);
    }

    public bool IsComplete(Quest quest)
    {
        return quest.tasks.Count > 0 && doneTasks.TryGetValue(quest, out HashSet<int> done) && done.Count >= quest.tasks.Count;
    }

    // index 0 = first task in the quest's list
    public void CompleteTask(Quest quest, int index)
    {
        if (quest == null || index < 0 || index >= quest.tasks.Count) return;

        if (!doneTasks.TryGetValue(quest, out HashSet<int> done))
        {
            done = new HashSet<int>();
            doneTasks[quest] = done;
        }
        if (done.Add(index))
        {
            Changed?.Invoke(quest);
        }
    }
}