using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    public GameObject prompt;
    public string offerText = "I'll forge you the best sword in the land, but you have to gather some materials first. Interested?";

    [Header("Quest given on Accept")]
    public Quest quest;
    public QuestTracker tracker;
    public JournalUI journal;

    [Header("Turn-in")]
    public Inventory inventory;
    public Item reward;
    public int returnTaskIndex = 2;   // "Bring the materials to the blacksmith"
    [TextArea] public string waitingText = "Still waiting on those materials.";
    [TextArea] public string readyText = "That's everything. Give me a moment at the forge... Here, your sword.";

    bool playerInRange;

    bool QuestAccepted => quest != null && QuestLog.Instance.IsAccepted(quest);
    bool QuestDone => quest != null && quest.IsComplete;

    void Update()
    {
        if (!playerInRange || OfferPopup.Instance.IsOpen || QuestDone) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        prompt.SetActive(false);

        if (!QuestAccepted)
            OfferPopup.Instance.Show(offerText, OnAccept, OnReject, OnClosed);
        else if (HasMaterials())
            OfferPopup.Instance.Show(readyText, OnComplete, null, OnClosed, "Complete Quest", "Not yet");
        else
            OfferPopup.Instance.Show(waitingText, null, null, OnClosed, "Okay", null);
    }

    void OnAccept()
    {
        QuestLog.Instance.Accept(quest);
        journal.DiscoverEntry(quest);
        tracker.Pin(quest);
    }

    void OnReject() => Debug.Log($"{name}: offer rejected");

    // requirements come from the QuestTaskCollect components on this NPC
    bool HasMaterials()
    {
        foreach (QuestTaskCollect req in GetComponents<QuestTaskCollect>())
            if (req.quest == quest && inventory.CountItem(req.item) < req.amount) return false;
        return true;
    }

    void OnComplete()
    {
        foreach (QuestTaskCollect req in GetComponents<QuestTaskCollect>())
        {
            if (req.quest != quest) continue;
            inventory.RemoveItem(req.item, req.amount);
            QuestLog.Instance.CompleteTask(quest, req.taskIndex);   // in case they were gathered before accepting
        }

        inventory.AddItem(reward, 1);
        QuestLog.Instance.CompleteTask(quest, returnTaskIndex);
    }

    void OnClosed()
    {
        if (playerInRange && !QuestDone) prompt.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;
        playerInRange = true;
        if (!OfferPopup.Instance.IsOpen && !QuestDone) prompt.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;
        playerInRange = false;
        prompt.SetActive(false);
    }
}