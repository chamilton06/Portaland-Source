using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    //private static readonly HashSet<Slot> activeSlots = new HashSet<Slot>();

    public bool hovering;
    public bool IsBlocked { get; private set; }

    public static event System.Action<Slot, PointerEventData.InputButton> Clicked;

    public Item HeldItem {get; private set;}
    public int itemAmount {get; private set;}

    public Image iconImage {get; private set;}
    public TextMeshProUGUI amountTxt {get; private set;}
    private CanvasGroup canvasGroup;

    private void Awake(){
        iconImage = transform.GetChild(0).GetComponent<Image>();
        amountTxt = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        UpdateSlot(this);   // empty slots start with icon off
    }

    private void OnDisable()
    {
        hovering = false;
    }

    public Item GetItem(){
        return HeldItem;
    }

    public int GetAmount(){
        return itemAmount;
    }

    public bool HasItem(){
        return HeldItem != null;
    }

    public void SetItem(Item item, int amount = 1)
    {
        HeldItem = item;
        itemAmount = amount;

        // Resize the icon based on the item's inventory size parameters
        RectTransform iconRect = iconImage.rectTransform;
        iconRect.anchorMin = new Vector2(0, 1);
        iconRect.anchorMax = new Vector2(0, 1);
        iconRect.pivot = new Vector2(0, 1);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(100 + ((HeldItem.inventorySize.x-1) * 110), 100 + ((HeldItem.inventorySize.y-1) * 110));
        
        UpdateSlot(this);
        //RefreshBlockedSlots();
    }

    public int AddAmount(int amountToAdd)
    {
        itemAmount += amountToAdd;
        UpdateSlot(this);
        return itemAmount;
    }

    public int RemoveAmount(int amountToRem)
    {
        itemAmount -= amountToRem;

        if (itemAmount <= 0)
        {
            ClearSlot();
        }
        else
        {
            UpdateSlot(this);
        }

        return itemAmount;
    }

    public void ClearSlot()
    {
        HeldItem = null;
        itemAmount = 0;
        UpdateSlot(this);
        //RefreshBlockedSlots();
    }

    //private static void RefreshBlockedSlots()
    //{
    //    foreach (Slot slot in activeSlots)
    //    {
    //        if (slot.HasItem() || slot is HotbarSlot)
    //        {
    //            slot.iconImage.enabled = true;
    //        } 
    //        else
    //        {
    //            slot.iconImage.enabled = false;
    //        }
    //        slot.IsBlocked = false;
    //    }

    //    foreach (Slot owner in activeSlots)
    //    {
    //        if (owner.HeldItem == null || owner.iconImage == null)
    //        {
    //            continue;
    //        }

    //        Rect itemBounds = GetWorldBounds(owner.iconImage.rectTransform);
    //        foreach (Slot candidate in activeSlots)
    //        {
    //            if (candidate == owner)
    //            {
    //                continue;
    //            }

    //            RectTransform candidateRect = candidate.transform as RectTransform;
    //            if (candidateRect != null && itemBounds.Overlaps(GetWorldBounds(candidateRect)))
    //            {
    //                candidate.IsBlocked = true;
    //                candidate.hovering = false;
    //            }
    //        }
    //    }

    //    foreach (Slot slot in activeSlots)
    //    {  
    //        if (slot is not HotbarSlot)
    //        {
    //        slot.canvasGroup.alpha = slot.IsBlocked ? 0f : 1f;
    //        slot.canvasGroup.blocksRaycasts = !slot.IsBlocked;
    //        }
    //    }
    //}

    //private static Rect GetWorldBounds(RectTransform rectTransform)
    //{
    //    Vector3[] corners = new Vector3[4];
    //    rectTransform.GetWorldCorners(corners);
    //    return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
    //}


    // fixing inventory
    // inventory tells us if a big item is covering us
    public void SetBlocked(bool blocked)
    {
        IsBlocked = blocked;
        //canvasGroup.alpha = blocked ? 0f : 1f;
        canvasGroup.blocksRaycasts = !blocked;
        if (blocked) hovering = false;
    }

    public void UpdateSlot(Slot slot){
        if (slot.HasItem()){
            iconImage.enabled = true;
            iconImage.sprite = slot.HeldItem.icon;
            if (slot.itemAmount > 1){
                amountTxt.text = slot.itemAmount.ToString();
            } else {
                amountTxt.text = "";
            }
        }
        else {
            iconImage.enabled = false;
            amountTxt.text = "";
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = !IsBlocked;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsBlocked && Clicked != null)
        {
            Clicked(this, eventData.button);
        }
    }
}
