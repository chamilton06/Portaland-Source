using System.Collections;
using System.Collections.Generic;
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
            foreach (string task in tasks)
            {
                text += "\n- " + task;
            }
        }
        return text;
    }
}
