// ============================================================================
// NPCTrader.cs
// Same setup as NPCInteractable, only opens after Required Quest is done.
//
// To use, drag onto any NPC.
// ============================================================================

using UnityEngine;

public class NPCTrader : MonoBehaviour
{
    public Shop shop;
    public Inventory inventory;
    public GameObject prompt;
    [Tooltip("Optional. Shop stays closed until this quest is complete.")]
    public Quest requiredQuest;
    public KeyCode interactKey = KeyCode.E;

    int[] stockLeft;   // runtime copy so play mode never edits the Shop asset
    bool playerInRange;

    bool Unlocked => requiredQuest == null || requiredQuest.IsComplete;

    void Start()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();

        stockLeft = new int[shop.offers.Count];
        for (int i = 0; i < stockLeft.Length; i++)
            stockLeft[i] = shop.offers[i].stock;
    }

    void Update()
    {
        if (!playerInRange || !Unlocked) return;
        if (TradeMenu.Instance.IsOpen) return;
        if (OfferPopup.Instance != null && OfferPopup.Instance.IsOpen) return;

        if (prompt != null && !prompt.activeSelf) prompt.SetActive(true);

        if (Input.GetKeyDown(interactKey))
        {
            if (prompt != null) prompt.SetActive(false);
            TradeMenu.Instance.Open(this);
        }
    }

    public int StockLeft(int offerIndex) => stockLeft[offerIndex];

    public void UseStock(int offerIndex)
    {
        if (stockLeft[offerIndex] > 0) stockLeft[offerIndex]--;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;
        playerInRange = false;
        if (prompt != null) prompt.SetActive(false);
        if (TradeMenu.Instance.IsOpen && TradeMenu.Instance.Trader == this)
            TradeMenu.Instance.Close();
    }
}