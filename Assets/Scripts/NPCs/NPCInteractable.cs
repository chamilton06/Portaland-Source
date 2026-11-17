using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    public GameObject prompt;
    public string offerText = "I'll forge you the best sword in the land, but you have to gather some materials first. Interested?";

    [Header("Quest given on Accept")]
    public Quest quest;
    public QuestTracker tracker;
    public JournalUI journal;

    bool playerInRange;

    bool QuestAccepted => quest != null && QuestLog.Instance.IsAccepted(quest);

    void Update()
    {
        if (!playerInRange || OfferPopup.Instance.IsOpen || QuestAccepted) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            prompt.SetActive(false);
            OfferPopup.Instance.Show(offerText, OnAccept, OnReject, OnClosed);
        }
    }

    void OnAccept()
    {
        QuestLog.Instance.Accept(quest);
        journal.DiscoverEntry(quest);
        tracker.Pin(quest);
    }

    void OnReject() => Debug.Log($"{name}: offer rejected");

    void OnClosed()
    {
        if (playerInRange && !QuestAccepted) prompt.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;
        playerInRange = true;
        if (!OfferPopup.Instance.IsOpen && !QuestAccepted) prompt.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;
        playerInRange = false;
        prompt.SetActive(false);
    }
}