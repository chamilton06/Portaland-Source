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
    public Slot slotRef {get; private set;}
    private CanvasGroup hotbarCanvasGroup;

    private void Start()
    {
        RefreshOpacity();
    }

    public void RefreshOpacity()
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
        RefreshOpacity();
    }

    public void SetItem(Slot inventorySlot)
    {
        slotRef = inventorySlot;
        UpdateSlot(slotRef);
    }

    public void ClearRef()
    {
        slotRef = null;
        UpdateSlot(this);
    }
}