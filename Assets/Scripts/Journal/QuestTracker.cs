using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// pinned quest in the top left corner
public class QuestTracker : MonoBehaviour
{
    public GameObject trackerRoot;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI goalText;

    public Quest PinnedQuest { get; private set; }

    void Awake()
    {
        trackerRoot.SetActive(false);
    }

    void Start()
    {
        if (QuestLog.Instance != null) QuestLog.Instance.Changed += OnQuestChanged;
    }

    void OnQuestChanged(Quest quest)
    {
        if (quest == PinnedQuest) Refresh();
    }

    public void Pin(Quest quest)
    {
        PinnedQuest = quest;
        Refresh();
    }

    public void Unpin()
    {
        PinnedQuest = null;
        Refresh();
    }

    // call again when tasks change
    public void Refresh()
    {
        trackerRoot.SetActive(PinnedQuest != null);
        if (PinnedQuest == null) return;

        titleText.text = PinnedQuest.GetDisplayTitle() + (PinnedQuest.IsComplete ? " <color=#2E7D32>Completed!</color>" : "");
        string text = PinnedQuest.shortGoal;
        for (int i = 0; i < PinnedQuest.tasks.Count; i++)
        {
            text += "\n- " + PinnedQuest.TaskLine(i);
        }
        goalText.text = text;
    }
}
