// ============================================================================
// Slot.cs
// A single inventory or hotbar slot. Holds an Item and its stack amount,
// keeps the slot's icon and amount text in sync, and tracks whether the
// mouse is hovering over it.
// ============================================================================

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // ------------------------------------------------------------------------
    // Configuration
    // ------------------------------------------------------------------------

    public bool hovering;       // True while the mouse is over this slot

    // ------------------------------------------------------------------------
    // State
    // ------------------------------------------------------------------------

    private Item heldItem;
    private int itemAmount;

    private Image iconImage;
    private TextMeshProUGUI amountTxt;

    // ------------------------------------------------------------------------
    // Unity Callbacks
    // ------------------------------------------------------------------------

    // Caches the icon (child 0) and amount text (child 1) references.
    private void Awake(){
        iconImage = transform.GetChild(0).GetComponent<Image>();
        amountTxt = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    // ------------------------------------------------------------------------
    // Item Access
    // ------------------------------------------------------------------------

    // Returns the held item, or null if the slot is empty.
    public Item GetItem(){
        return heldItem;
    }

    // Returns the current stack amount.
    public int GetAmount(){
        return itemAmount;
    }

    // Returns true if the slot holds an item.
    public bool HasItem(){
        return heldItem != null;
    }

    // ------------------------------------------------------------------------
    // Item Modification
    // ------------------------------------------------------------------------

    // Sets the held item and amount, then refreshes the display.
    public void SetItem(Item item, int amount = 1)
    {
        heldItem = item;
        itemAmount = amount;

        UpdateSlot();
    }

    // Adds to the stack and returns the new amount.
    public int AddAmount(int amountToAdd)
    {
        itemAmount += amountToAdd;
        UpdateSlot();
        return itemAmount;
    }

    // Removes from the stack (clearing the slot at zero or below) and returns the new amount.
    public int RemoveAmount(int amountToRem)
    {
        itemAmount -= amountToRem;

        if (itemAmount <= 0)
        {
            ClearSlot();
        }
        else
        {
            UpdateSlot();
        }

        return itemAmount;
    }

    // Empties the slot and refreshes the display.
    public void ClearSlot()
    {
        heldItem = null;
        itemAmount = 0;
        UpdateSlot();
    }

    // ------------------------------------------------------------------------
    // Display
    // ------------------------------------------------------------------------

    // Shows the icon and amount if the slot has an item, otherwise hides both.
    public void UpdateSlot(){
        if (heldItem != null){
            iconImage.enabled = true;
            iconImage.sprite = heldItem.icon;
            amountTxt.text = itemAmount.ToString(); 
        }
        else {
            iconImage.enabled = false;
            amountTxt.text = "";
        }
    }

    // ------------------------------------------------------------------------
    // Pointer Events
    // ------------------------------------------------------------------------

    // Marks the slot as hovered.
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    // Clears the hovered flag.
    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}
