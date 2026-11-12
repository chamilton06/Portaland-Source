using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// one big frame behind items that take up more than one cell
[RequireComponent(typeof(Slot))]
public class SlotFootprint : MonoBehaviour
{
    public RectTransform footprint;   // first child of the slot, drawn under the icon

    private Slot slot;
    private Item shownItem;

    void Awake()
    {
        slot = GetComponent<Slot>();

        // hotbar always shows its frame, nothing to track
        if (slot is HotbarSlot)
        {
            enabled = false;
            return;
        }

        footprint.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        Item item = slot.GetItem();
        if (item == shownItem) return;   // only redo when the item changes
        shownItem = item;

        footprint.gameObject.SetActive(item != null);
        if (item == null) return;

        // same size + spot as the icon
        RectTransform icon = slot.iconImage.rectTransform;
        footprint.anchorMin = icon.anchorMin;
        footprint.anchorMax = icon.anchorMax;
        footprint.pivot = icon.pivot;
        footprint.anchoredPosition = icon.anchoredPosition;
        footprint.sizeDelta = icon.sizeDelta;
    }
}