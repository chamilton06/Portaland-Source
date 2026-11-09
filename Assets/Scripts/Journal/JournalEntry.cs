using UnityEngine;

public enum JournalTab { Notes, Enemies, Materials, Crafting, Weapons, Quests }

[CreateAssetMenu(fileName = "JournalEntry", menuName = "Journal Entry")]
public class JournalEntry : ScriptableObject
{
    public string title;
    public JournalTab tab;
    public Sprite picture;

    [TextArea(8, 20)]
    public string loreText;

    public bool startsUnlocked = true;  // off = hidden till picked up

    // what the right page shows, quests add their tasks
    public virtual string GetPageText()
    {
        return loreText;
    }
}
