using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// right journal page for recipes: what it makes, what it needs, craft button
public class CraftingPage : MonoBehaviour
{
    public JournalUI journal;
    public Inventory inventory;
    public GameObject page;                  // covers the normal right page
    public Button openButton;                // "craft" button on the recipe's journal page
    public Button backButton;                // back to the description
    public Image resultIcon;
    public TextMeshProUGUI resultName;
    public Transform ingredientList;         // rows get made in here
    public GameObject ingredientRow;         // template row with Icon, Name and Count children
    public TextMeshProUGUI amountText;       // "1 / 10"
    public Button minusButton;
    public Button plusButton;
    public Button craftButton;
    public TextMeshProUGUI messageText;      // "not enough materials"
    public float messageTime = 2f;
    public Color haveColor = new Color(0.18f, 0.45f, 0.18f);     // green ink
    public Color missingColor = new Color(0.7f, 0.15f, 0.1f);    // red ink

    private Recipe recipe;
    private int amount;
    private int maxAmount;
    private float hideMessageAt;
    private readonly List<GameObject> rows = new List<GameObject>();

    void Awake()
    {
        journal.EntryShown += OnEntryShown;
        minusButton.onClick.AddListener(() => ChangeAmount(-1));
        plusButton.onClick.AddListener(() => ChangeAmount(1));
        craftButton.onClick.AddListener(CraftNow);
        openButton.onClick.AddListener(OpenPage);
        backButton.onClick.AddListener(() => page.SetActive(false));
        openButton.gameObject.SetActive(false);

        ingredientRow.SetActive(false);
        messageText.gameObject.SetActive(false);
        page.SetActive(false);
    }

    void OnDestroy()
    {
        if (journal != null) journal.EntryShown -= OnEntryShown;
    }

    // journal is paused so use real time
    void Update()
    {
        if (messageText.gameObject.activeSelf && Time.unscaledTime > hideMessageAt)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    // recipe pages get a craft button, everything else hides it
    void OnEntryShown(JournalEntry entry)
    {
        recipe = entry as Recipe;   // null for normal entries
        page.SetActive(false);
        openButton.gameObject.SetActive(recipe != null);
    }

    void OpenPage()
    {
        page.SetActive(true);
        amount = 1;
        Refresh();
    }

    void Refresh()
    {
        resultIcon.sprite = recipe.result != null ? recipe.result.icon : null;
        resultName.text = recipe.GetDisplayTitle() + (recipe.resultAmount > 1 ? " x" + recipe.resultAmount : "");

        // how many we could make with what's in the bag
        maxAmount = recipe.ingredients.Count > 0 ? int.MaxValue : 0;
        foreach (Recipe.Ingredient ingredient in recipe.ingredients)
        {
            maxAmount = Mathf.Min(maxAmount, Crafting.Count(inventory, ingredient.item) / Mathf.Max(1, ingredient.amount));
        }
        amount = Mathf.Clamp(amount, maxAmount > 0 ? 1 : 0, maxAmount);

        // one row per ingredient, need goes up with the amount picked
        foreach (GameObject old in rows) Destroy(old);
        rows.Clear();
        foreach (Recipe.Ingredient ingredient in recipe.ingredients)
        {
            int have = Crafting.Count(inventory, ingredient.item);
            int need = ingredient.amount * Mathf.Max(1, amount);

            GameObject row = Instantiate(ingredientRow, ingredientList);
            row.SetActive(true);
            row.transform.Find("Icon").GetComponent<Image>().sprite = ingredient.item.icon;
            row.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = ingredient.item.itemName;
            TextMeshProUGUI count = row.transform.Find("Count").GetComponent<TextMeshProUGUI>();
            count.text = have + " / " + need;
            count.color = have >= need ? haveColor : missingColor;
            rows.Add(row);
        }

        amountText.text = amount + " / " + maxAmount;
        craftButton.interactable = amount > 0;
    }

    void ChangeAmount(int change)
    {
        if (change > 0 && amount >= maxAmount)
        {
            ShowMessage("Not enough materials", missingColor);
            return;
        }
        amount = Mathf.Max(maxAmount > 0 ? 1 : 0, amount + change);
        Refresh();
    }

    void CraftNow()
    {
        int made = 0;
        for (int i = 0; i < amount; i++)
        {
            if (Crafting.Craft(inventory, recipe)) made++;
        }

        if (made > 0) ShowMessage("Crafted " + recipe.GetDisplayTitle() + " x" + made * recipe.resultAmount, haveColor);
        else ShowMessage("Not enough materials", missingColor);

        amount = 1;
        Refresh();
    }

    void ShowMessage(string text, Color color)
    {
        messageText.text = text;
        messageText.color = color;
        messageText.gameObject.SetActive(true);
        hideMessageAt = Time.unscaledTime + messageTime;
    }
}