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

    void Update() {
        if (playerInRange && Input.GetKeyDown(KeyCode.E)) {
            Debug.Log($"Talking to {name}");
            onInteract.Invoke();
        }
    }

    void OnTriggerEnter(Collider other) {
        Debug.Log($"Trigger entered by {other.name}, tag={other.tag}");
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        prompt.SetActive(true);
    }

    void OnTriggerExit(Collider other) {
        Debug.Log($"Trigger exited by {other.name}, tag={other.tag}");
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        prompt.SetActive(false);
    }
}