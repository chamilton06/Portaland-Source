// ============================================================================
// NPCInteractable.cs
// Controls the opening of an NPC interaction (player closeness)
//
// To use, drag onto any NPC GameObject, and set Prompt to the same NPC's canvas
// ============================================================================

using UnityEngine;
using UnityEngine.Events;

public class NPCInteractable : MonoBehaviour {
    public GameObject prompt;            // the "Press E" canvas
    public UnityEvent onInteract;        // dialogue

    bool playerInRange;

    public string offerText = "I'll forge you a fine sword. Interested?";

    void Update()
    {
        if (!playerInRange || OfferPopup.Instance.IsOpen) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            prompt.SetActive(false);
            OfferPopup.Instance.Show(offerText, OnAccept, OnReject, OnClosed);
        }
    }

    // debug statements (DELETE)
    void OnAccept() => Debug.Log($"{name}: offer accepted");
    void OnReject() => Debug.Log($"{name}: offer rejected");

    void OnClosed()
    {
        if (playerInRange) prompt.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;
        playerInRange = true;
        if (!OfferPopup.Instance.IsOpen) prompt.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;
        playerInRange = false;
        prompt.SetActive(false);
    }
}