using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Slot))]
public class SlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Slot slot;

    void Awake()
    {
        slot = GetComponent<Slot>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || InventoryDrag.Instance == null) return;
        InventoryDrag.Instance.BeginDrag(slot, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (InventoryDrag.Instance != null) InventoryDrag.Instance.Drag(slot, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (InventoryDrag.Instance != null) InventoryDrag.Instance.EndDrag(slot, eventData);
    }

    // inventory closed mid drag
    void OnDisable()
    {
        if (InventoryDrag.Instance != null) InventoryDrag.Instance.Cancel(slot);
    }
}

