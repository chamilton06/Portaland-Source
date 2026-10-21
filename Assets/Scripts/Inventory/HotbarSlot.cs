using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HotbarSlot : Slot
{
    public int slotIndex;
    public bool isSelected;
    private Slot slotRef;
    private CanvasGroup hotbarCanvasGroup;

    private void Update()
    {
        if (hotbarCanvasGroup == null)
        {
            hotbarCanvasGroup = GetComponent<CanvasGroup>();
        }

        if (isSelected)
        {
            hotbarCanvasGroup.alpha = 1f;
        }
        else
        {
            hotbarCanvasGroup.alpha = 0.5f;
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    public void SetItem(Slot inventorySlot)
    {
        slotRef = inventorySlot;
        UpdateSlot(slotRef);
    }
}