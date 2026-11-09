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

        titleText.text = PinnedQuest.title;
        string text = PinnedQuest.shortGoal;
        foreach (string task in PinnedQuest.tasks)
        {
            text += "\n- " + task;
        }
        goalText.text = text;
    }
}
