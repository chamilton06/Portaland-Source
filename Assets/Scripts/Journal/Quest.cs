using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Quest")]
public class Quest : JournalEntry
{
    [TextArea(2, 3)]
    public string shortGoal;                          // one line for the pinned tracker
    public List<string> tasks = new List<string>();

    // quests always live in the quests tab
    void OnEnable()
    {
        tab = JournalTab.Quests;
    }

    public override string GetPageText()
    {
        string text = loreText;
        if (!string.IsNullOrEmpty(shortGoal))
        {
            text += "\n\n<b>Goal:</b> " + shortGoal;
        }
        if (tasks.Count > 0)
        {
            text += "\n\n<b>Tasks</b>";
            for (int i = 0; i < tasks.Count; i++)
            {
                text += "\n- " + TaskLine(i);
            }
        }
        if (IsComplete)
        {
            text += "\n\n<color=#2E7D32><b>Completed!</b></color>";
        }
        return text;
    }

    // crossed out once done
    public string TaskLine(int index)
    {
        bool done = QuestLog.Instance != null && QuestLog.Instance.IsTaskDone(this, index);
        return done ? "<s>" + tasks[index] + "</s>" : tasks[index];
    }


    public override string GetDisplayTitle()
    {
        return IsComplete ? "<s>" + title + "</s>" : title;
    }

    public bool IsComplete => QuestLog.Instance != null && QuestLog.Instance.IsComplete(this);
}
