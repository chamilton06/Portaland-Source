using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// i opens and closes the inventory panel, panel is empty for now
public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public KeyCode toggleKey = KeyCode.I;

    // so you cant attack or look around with it open
    // public PlayerCombat combat;
    public PlayerTurn playerTurn;
    public CameraFollow cameraFollow;

    private bool isOpen = false;

    void Start()
    {
        inventoryPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        // free mouse when open, lock when closed
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

        //if (combat != null)
        // {
            // combat.inventoryOpen = isOpen;
        // }
        if (playerTurn != null)
        {
            playerTurn.enabled = !isOpen;
        }
        if (cameraFollow != null)
        {
            cameraFollow.canLook = !isOpen;
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}