using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// pin / unpin the quest open on the right page
[RequireComponent(typeof(Button))]
public class QuestPinButton : MonoBehaviour
{
    public JournalUI journal;
    public QuestTracker tracker;

    private Quest shownQuest;
    private TextMeshProUGUI label;

    void Awake()
    {
        label = GetComponentInChildren<TextMeshProUGUI>();
        GetComponent<Button>().onClick.AddListener(TogglePin);
        journal.EntryShown += OnEntryShown;
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (journal != null) journal.EntryShown -= OnEntryShown;
    }

    void OnEntryShown(JournalEntry entry)
    {
        shownQuest = entry as Quest;   // null for normal entries
        gameObject.SetActive(shownQuest != null);
        RefreshLabel();
    }

    void TogglePin()
    {
        if (tracker.PinnedQuest == shownQuest) tracker.Unpin();
        else tracker.Pin(shownQuest);
        RefreshLabel();
    }

    void RefreshLabel()
    {
        if (label != null && shownQuest != null)
        {
            label.text = tracker.PinnedQuest == shownQuest ? "Unpin" : "Pin";
        }
    }
}
