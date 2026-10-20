using UnityEngine;

// goes on the player, mouse left/right turns the player
// stops turning and frees the mouse while the inventory is open
public class PlayerTurn : MonoBehaviour
{
    public float mouseSensitivity = 3f;
    public bool lockCursor = true;

    private PlayerCombat combat;     // inventory sets inventoryOpen on this
    private bool wasOpen = false;

    void Start()
    {
        combat = GetComponent<PlayerCombat>();

        if (lockCursor)
        {
            LockCursor(true);
        }
    }

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

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}