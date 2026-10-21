using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JournalUI : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.J;
    public GameObject journalRoot;
    public bool pauseGameWhileOpen = true;
    public Inventory inventory;

    public List<JournalEntry> entries = new List<JournalEntry>();

    public Button[] tabButtons;                 // same order as JournalTab enum
    public Color activeTabColor = new Color(0.96f, 0.91f, 0.78f);
    public Color inactiveTabColor = new Color(0.72f, 0.60f, 0.42f);

    public Transform entryListParent;
    public Button entryButtonPrefab;            // button with tmp text inside

    public Image entryPicture;
    public TextMeshProUGUI entryTitle;
    public TextMeshProUGUI entryText;
    public string emptyTabText = "Nothing written here yet...";

    public string newTag = " <color=#B22222>NEW!</color>";
    public TextMeshProUGUI newEntryPopup;       // optional popup
    public float popupTime = 3f;

    private readonly HashSet<JournalEntry> unlocked = new HashSet<JournalEntry>();
    private readonly HashSet<JournalEntry> unread = new HashSet<JournalEntry>();
    private readonly Dictionary<Item, JournalEntry> autoEntries = new Dictionary<Item, JournalEntry>();
    private readonly Dictionary<JournalEntry, TextMeshProUGUI> entryLabels = new Dictionary<JournalEntry, TextMeshProUGUI>();
    private string[] tabNames;
    private JournalTab currentTab = JournalTab.Notes;
    private float popupHideTime;

    void Awake()
    {
        foreach (JournalEntry entry in entries)
        {
            if (entry != null && entry.startsUnlocked)
            {
                unlocked.Add(entry);
            }
        }

        tabNames = new string[tabButtons.Length];
        for (int i = 0; i < tabButtons.Length; i++)
        {
            JournalTab tab = (JournalTab)i;
            tabButtons[i].onClick.AddListener(() => ShowTab(tab));
            tabNames[i] = tabButtons[i].GetComponentInChildren<TextMeshProUGUI>().text;
        }

        if (entryListParent == null || entryButtonPrefab == null || entryPicture == null || entryTitle == null || entryText == null)
        {
            Debug.LogError("JournalUI: some fields on UIManager are still None, fill them all in");
        }
        if (journalRoot == gameObject)
        {
            Debug.LogError("JournalUI must sit on an object that stays active, not on journalRoot itself (J would stop working).");
        }
        journalRoot.SetActive(false);

        if (newEntryPopup != null)
        {
            newEntryPopup.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (IsOpen())
            {
                Close();
            }
            else
            {
                Open();
            }
        }
        else if (IsOpen() && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }

        // hide the popup after a bit
        if (newEntryPopup != null && newEntryPopup.gameObject.activeSelf && Time.unscaledTime > popupHideTime)
        {
            newEntryPopup.gameObject.SetActive(false);
        }
    }

    public bool IsOpen()
    {
        return journalRoot.activeSelf;
    }

    public void Open()
    {
        if (inventory != null && inventory.IsOpen())
        {
            inventory.SetOpen(false);
        }

        journalRoot.SetActive(true);
        MenuState.SetOpen(this, true);
        if (pauseGameWhileOpen)
        {
            Time.timeScale = 0f;
        }

        ShowTab(currentTab);
    }

    public void Close()
    {
        journalRoot.SetActive(false);
        MenuState.SetOpen(this, false);
        if (pauseGameWhileOpen)
        {
            Time.timeScale = 1f;
        }
    }

    public void OpenEntry(JournalEntry entry)
    {
        Unlock(entry);
        currentTab = entry.tab;
        Open();
        ShowEntry(entry);
    }

    // items own entry or an auto one
    public JournalEntry GetEntry(Item item)
    {
        if (item.journalEntry != null)
        {
            return item.journalEntry;
        }

        JournalEntry entry;
        if (!autoEntries.TryGetValue(item, out entry))
        {
            entry = ScriptableObject.CreateInstance<JournalEntry>();
            entry.title = item.itemName;
            entry.picture = item.icon;
            entry.tab = item is Weapon ? JournalTab.Weapons : JournalTab.Materials;
            entry.loreText = item.shortDescription + "\n\nNot much else is known about this yet...";
            autoEntries[item] = entry;
        }
        return entry;
    }

    // on pickup, adds it as NEW!
    public void Discover(Item item)
    {
        JournalEntry entry = GetEntry(item);
        if (unlocked.Contains(entry))
        {
            return;
        }

        Unlock(entry);
        unread.Add(entry);

        if (newEntryPopup != null)
        {
            newEntryPopup.text = "Journal updated: " + entry.title;
            newEntryPopup.gameObject.SetActive(true);
            popupHideTime = Time.unscaledTime + popupTime;
        }
    }

    public void Unlock(JournalEntry entry)
    {
        if (entry == null)
        {
            return;
        }
        if (!entries.Contains(entry))
        {
            entries.Add(entry);
        }
        unlocked.Add(entry);
    }

    public void ShowTab(JournalTab tab)
    {
        currentTab = tab;

        foreach (Transform child in entryListParent)
        {
            Destroy(child.gameObject);
        }
        entryLabels.Clear();

        JournalEntry first = null;
        foreach (JournalEntry entry in entries)
        {
            if (entry == null || entry.tab != tab || !unlocked.Contains(entry))
            {
                continue;
            }

            if (first == null)
            {
                first = entry;
            }

            Button button = Instantiate(entryButtonPrefab, entryListParent);
            entryLabels[entry] = button.GetComponentInChildren<TextMeshProUGUI>();
            JournalEntry captured = entry;
            button.onClick.AddListener(() => ShowEntry(captured));
        }

        ShowEntry(first, false);   // not read till clicked
    }

    public void ShowEntry(JournalEntry entry)
    {
        ShowEntry(entry, true);
    }

    void ShowEntry(JournalEntry entry, bool markRead)
    {
        if (entry == null)
        {
            entryTitle.text = currentTab.ToString();
            entryText.text = emptyTabText;
            entryPicture.gameObject.SetActive(false);   // hide so the layout closes the gap
            RefreshLabels();
            return;
        }

        if (markRead)
        {
            unread.Remove(entry);
        }

        entryTitle.text = entry.title;
        entryText.text = entry.loreText;
        entryPicture.sprite = entry.picture;
        entryPicture.gameObject.SetActive(entry.picture != null);
        RefreshLabels();
    }

    // NEW! tags and tab dots
    void RefreshLabels()
    {
        foreach (KeyValuePair<JournalEntry, TextMeshProUGUI> pair in entryLabels)
        {
            pair.Value.text = pair.Key.title + (unread.Contains(pair.Key) ? newTag : "");
        }

        for (int i = 0; i < tabButtons.Length; i++)
        {
            bool hasNew = false;
            foreach (JournalEntry entry in unread)
            {
                if ((int)entry.tab == i)
                {
                    hasNew = true;
                }
            }

            tabButtons[i].image.color = (i == (int)currentTab) ? activeTabColor : inactiveTabColor;
            tabButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = tabNames[i] + (hasNew ? " <color=#B22222>•</color>" : "");
        }
    }
}
