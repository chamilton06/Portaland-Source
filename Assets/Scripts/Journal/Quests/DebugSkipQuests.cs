// ============================================================================
// DebugSkipQuests.cs
// Debug script that skips any pinned quest by clicking F10
// ============================================================================

using UnityEngine;

public class DebugSkipQuest : MonoBehaviour
{
    public KeyCode key = KeyCode.F10;
    public QuestTracker tracker;

    void Start()
    {
        if (tracker == null)
            tracker = FindFirstObjectByType<QuestTracker>();
    }

    void Update()
    {
        if (Input.GetKeyDown(key)) SkipPinnedQuest();
    }

    public void SkipPinnedQuest()
    {
        Quest quest = tracker != null ? tracker.PinnedQuest : null;
        if (quest == null || QuestLog.Instance == null)
        {
            Debug.Log("[Debug] No pinned quest to skip.");
            return;
        }

        // complete each task
        for (int i = 0; i < quest.tasks.Count; i++)
            QuestLog.Instance.CompleteTask(quest, i);

        Debug.Log($"[Debug] Skipped quest: {quest.name}");
    }
}
