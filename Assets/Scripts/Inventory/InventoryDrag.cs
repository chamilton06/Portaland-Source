using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryDrag : MonoBehaviour
{
    public Inventory inventory;
    public float ghostAlpha = 0.7f;      // the one following the mouse
    public float sourceAlpha = 0.35f;    // the one left behind

    public static InventoryDrag Instance { get; private set; }
    public bool IsDragging => dragSlot != null;

    private Image ghost;
    private Canvas ghostCanvas;
    private Slot dragSlot;
    private Vector2Int grabCell;         // which cell of the item we grabbed
    private Vector3 grabOffset;          // mouse to icon corner

    void Awake()
    {
        Instance = this;
        if (inventory == null) inventory = GetComponent<Inventory>();

        // ghost icon, doesnt block clicks
        ghost = new GameObject("DragGhost", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        ghost.transform.SetParent(transform, false);
        ghost.raycastTarget = false;
        ghostCanvas = ghost.gameObject.AddComponent<Canvas>();
        ghost.rectTransform.pivot = new Vector2(0, 1);
        ghost.gameObject.SetActive(false);
    }

    public void BeginDrag(Slot slot, PointerEventData eventData)
    {
        int index = inventory.InventorySlots.IndexOf(slot);
        if (index < 0 || !slot.HasItem()) return;

        dragSlot = slot;
        RectTransform icon = slot.iconImage.rectTransform;

        // copy the icon look + size
        ghost.sprite = slot.iconImage.sprite;
        ghost.preserveAspect = slot.iconImage.preserveAspect;
        ghost.color = new Color(1f, 1f, 1f, ghostAlpha);
        ghost.rectTransform.sizeDelta = icon.rect.size;
        ghost.rectTransform.localScale = new Vector3(icon.lossyScale.x / transform.lossyScale.x, icon.lossyScale.y / transform.lossyScale.y, 1f);
        ghost.transform.position = icon.position;
        ghost.transform.SetAsLastSibling();
        ghost.gameObject.SetActive(true);
        // set every time, unity can drop it after the object was off
        ghostCanvas.overrideSorting = true;
        ghostCanvas.sortingOrder = 3;   // above slot icons

        grabOffset = icon.position - PointerWorld(eventData);

        int w = inventory.GridWidth;
        int cell = NearestCell(eventData.position, eventData.pressEventCamera);
        Vector2Int size = slot.GetItem().inventorySize;
        grabCell = new Vector2Int(
            Mathf.Clamp(cell % w - index % w, 0, size.x - 1),
            Mathf.Clamp(cell / w - index / w, 0, size.y - 1));

        slot.iconImage.color = new Color(1f, 1f, 1f, sourceAlpha);
    }

    public void Drag(Slot slot, PointerEventData eventData)
    {
        if (slot != dragSlot) return;
        ghost.transform.position = PointerWorld(eventData) + grabOffset;
    }

    public void EndDrag(Slot slot, PointerEventData eventData)
    {
        if (slot != dragSlot) return;

        Slot target = GetTargetSlot(eventData.position, eventData.pressEventCamera);
        StopDrag();
        if (target != null) inventory.TryMoveItem(slot, target);
    }

    public void Cancel(Slot slot)
    {
        if (slot == dragSlot) StopDrag();
    }

    void StopDrag()
    {
        if (dragSlot != null) dragSlot.iconImage.color = Color.white;
        dragSlot = null;
        ghost.gameObject.SetActive(false);
    }

    // top left cell the item would land on, null if dropped off the grid
    Slot GetTargetSlot(Vector2 screenPos, Camera cam)
    {
        RectTransform grid = (RectTransform)inventory.inventorySlotParent.transform;
        if (!RectTransformUtility.RectangleContainsScreenPoint(grid, screenPos, cam)) return null;

        int w = inventory.GridWidth;
        int cell = NearestCell(screenPos, cam);
        int x = cell % w - grabCell.x;
        int y = cell / w - grabCell.y;
        int index = y * w + x;
        if (x < 0 || y < 0 || index >= inventory.InventorySlots.Count) return null;
        return inventory.InventorySlots[index];
    }

    // closest cell to the mouse, works in the gaps too
    int NearestCell(Vector2 screenPos, Camera cam)
    {
        int best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < inventory.InventorySlots.Count; i++)
        {
            RectTransform rect = (RectTransform)inventory.InventorySlots[i].transform;
            Vector2 center = RectTransformUtility.WorldToScreenPoint(cam, rect.TransformPoint(rect.rect.center));
            float dist = (center - screenPos).sqrMagnitude;
            if (dist < bestDist)
            {
                best = i;
                bestDist = dist;
            }
        }
        return best;
    }

    Vector3 PointerWorld(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(ghost.rectTransform, eventData.position, eventData.pressEventCamera, out Vector3 world);
        return world;
    }
}
