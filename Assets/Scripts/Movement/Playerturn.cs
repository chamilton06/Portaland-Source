// ============================================================================
// PlayerTurn.cs
// Goes on the player. Mouse left/right turns the player. Stops turning and
// frees the cursor while the inventory is open; Escape also frees the cursor
// for testing in the editor.
// ============================================================================

using UnityEngine;

// goes on the player, mouse left/right turns the player
// stops turning and frees the mouse while the inventory is open
public class PlayerTurn : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Configuration
    // ------------------------------------------------------------------------
    public float mouseSensitivity = 3f;
    public bool lockCursor = true;

    // ------------------------------------------------------------------------
    // State
    // ------------------------------------------------------------------------
    private PlayerCombat combat;     // inventory sets inventoryOpen on this
    private bool wasOpen = false;

    // ------------------------------------------------------------------------
    // Unity Callbacks
    // ------------------------------------------------------------------------

    // Caches PlayerCombat and locks the cursor if lockCursor is on.
    void Start()
    {
        combat = GetComponent<PlayerCombat>();

        if (lockCursor)
        {
            LockCursor(true);
        }
    }

    // Turns the player with the mouse and syncs the cursor with the inventory state.
    void Update()
    {
        bool open = combat != null && combat.inventoryOpen;

        // only change the cursor when inventory opens or closes
        if (open != wasOpen)
        {
            LockCursor(!open);
            wasOpen = open;
        }

        if (!open)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            transform.Rotate(0f, mouseX, 0f);
        }

        // escape frees the cursor for testing in the editor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LockCursor(false);
        }
    }

    // ------------------------------------------------------------------------
    // Cursor
    // ------------------------------------------------------------------------

    // Locks and hides the cursor, or frees and shows it.
    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}