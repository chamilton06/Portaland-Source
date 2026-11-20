// ============================================================================
// TradeMenu.cs
// UI script for a trade menu
//
// Only need one in the scene, on a Canvas.
// ============================================================================

using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeMenu : MonoBehaviour
{
    public static TradeMenu Instance { get; private set; }

    [Header("UI")]
    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text greetingText;
    public Transform rowContainer;   // has a Vertical Layout Group
    public TradeRow rowPrefab;
    public Button closeButton;

    [Header("While open")]
    public Behaviour[] disableWhileOpen;

    [Header("Colours")]
    public Color haveColor = new Color(0.85f, 0.85f, 0.85f);
    public Color missingColor = new Color(0.9f, 0.35f, 0.3f);

    public bool IsOpen => panel.activeSelf;
    public NPCTrader Trader { get; private set; }

    readonly List<TradeRow> rows = new List<TradeRow>();

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        closeButton.onClick.AddListener(Close);
    }

    // leave using escape key
    void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    // on open, receive data from trader
    public void Open(NPCTrader trader)
    {
        Trader = trader;
        titleText.text = trader.shop.shopName;
        greetingText.text = trader.shop.greeting;
        BuildRows();
        panel.SetActive(true);
        SetPlayerControl(false);
    }

    public void Close()
    {
        panel.SetActive(false);
        Trader = null;
        SetPlayerControl(true);
    }

    void BuildRows()
    {
        foreach (var r in rows) Destroy(r.gameObject);
        rows.Clear();

        for (int i = 0; i < Trader.shop.offers.Count; i++)
        {
            var row = Instantiate(rowPrefab, rowContainer);
            int index = i;   // captured copy for the lambda
            row.button.onClick.AddListener(() => Trade(index));
            rows.Add(row);
        }
        Refresh();
    }

    void Refresh()
    {
        for (int i = 0; i < rows.Count; i++)
        {
            var offer = Trader.shop.offers[i];
            int stock = Trader.StockLeft(i);
            rows[i].label.text = Describe(offer, stock);
            rows[i].button.interactable = stock != 0 && CanAfford(offer);
        }
    }

    bool CanAfford(TradeOffer offer)
    {
        foreach (var c in offer.cost)
            if (c.item != null && Trader.inventory.CountItem(c.item) < c.amount)
                return false;
        return true;
    }

    void Trade(int index)
    {
        var offer = Trader.shop.offers[index];
        if (Trader.StockLeft(index) == 0 || !CanAfford(offer)) return;

        foreach (var c in offer.cost)
            if (c.item != null) Trader.inventory.RemoveItem(c.item, c.amount);
        foreach (var r in offer.reward)
            if (r.item != null) Trader.inventory.AddItem(r.item, r.amount);

        Trader.UseStock(index);
        Refresh();
    }

    // "Wood 4/10 + Stone 5/5  →  Sword  (1 left)", missing materials in red.
    string Describe(TradeOffer offer, int stock)
    {
        var sb = new StringBuilder();

        if (offer.cost.Count == 0) sb.Append("Free");
        for (int i = 0; i < offer.cost.Count; i++)
        {
            var c = offer.cost[i];
            if (c.item == null) continue;
            if (i > 0) sb.Append(" + ");
            int have = Trader.inventory.CountItem(c.item);
            string col = ColorUtility.ToHtmlStringRGB(have >= c.amount ? haveColor : missingColor);
            sb.Append($"<color=#{col}>{c.item.name} {have}/{c.amount}</color>");
        }

        sb.Append("  →  ");

        for (int i = 0; i < offer.reward.Count; i++)
        {
            var r = offer.reward[i];
            if (r.item == null) continue;
            if (i > 0) sb.Append(" + ");
            sb.Append(r.amount > 1 ? $"{r.amount}× {r.item.name}" : r.item.name);
        }

        if (stock == 0) sb.Append("  (sold out)");
        else if (stock > 0) sb.Append($"  ({stock} left)");

        return sb.ToString();
    }

    void SetPlayerControl(bool on)
    {
        foreach (var b in disableWhileOpen)
            if (b != null) b.enabled = on;

        Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !on;
    }
}
