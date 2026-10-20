using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoPanel : MonoBehaviour
{
    public GameObject panelRoot;
    public RectTransform[] slideLeft;           // InventorySlots + HotbarSlots
    public float slideDistance = 350f;          // how far they move left
    public float slideSpeed = 12f;

    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI descriptionText;
    public Button journalButton;                // hidden if theres no journal

    public Inventory inventory;
    public JournalUI journal;

    private Item currentItem;
    private Vector2[] homePositions;

    void Awake()
    {
        if (panelRoot == null || iconImage == null || nameText == null || statsText == null || descriptionText == null)
        {
            Debug.LogError("ItemInfoPanel: some fields are still None, fill them all in on the Inventory object");
            enabled = false;
            return;
        }
        if (panelRoot == gameObject)
        {
            Debug.LogError("ItemInfoPanel should sit on the Inventory object, not on the panel itself");
        }

        homePositions = new Vector2[slideLeft.Length];
        for (int i = 0; i < slideLeft.Length; i++)
        {
            homePositions[i] = slideLeft[i].anchoredPosition;
        }

        panelRoot.SetActive(false);

        if (journalButton != null)
        {
            journalButton.onClick.AddListener(OpenInJournal);
        }
    }

    void Update()
    {
        float offset = panelRoot.activeSelf ? -slideDistance : 0f;
        float t = slideSpeed * Time.unscaledDeltaTime;

        for (int i = 0; i < slideLeft.Length; i++)
        {
            Vector2 target = homePositions[i] + new Vector2(offset, 0f);
            slideLeft[i].anchoredPosition = Vector2.Lerp(slideLeft[i].anchoredPosition, target, t);
        }
    }

    public void Show(Item item)
    {
        currentItem = item;

        iconImage.sprite = item.icon;
        iconImage.enabled = item.icon != null;
        nameText.text = item.itemName;
        statsText.text = item.GetStatsText();
        descriptionText.text = item.shortDescription;

        if (journalButton != null)
        {
            journalButton.gameObject.SetActive(journal != null);
        }

        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        currentItem = null;
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    void OpenInJournal()
    {
        if (currentItem == null || journal == null)
        {
            return;
        }

        JournalEntry entry = journal.GetEntry(currentItem);
        if (inventory != null)
        {
            inventory.SetOpen(false);
        }
        journal.OpenEntry(entry);
    }
}
