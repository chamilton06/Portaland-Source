using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool hovering;

    private Item heldItem;
    private int itemAmount;

    private Image iconImage;
    private TextMeshProUGUI amountTxt;
    
    private void Awake(){
        iconImage = transform.GetChild(0).GetComponent<Image>();
        amountTxt = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public Item GetItem(){
        return heldItem;
    }

    public int GetAmount(){
        return itemAmount;
    }

    public void SetItem(Item item,int amount = 1){
        heldItem = item;
        itemAmount = amount;

        UpdateSlot();
    }

    public bool HasItem(){
        return heldItem != null;
    }

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

    public void ClearSlot(){
        heldItem = null;
        itemAmount = 0;
        UpdateSlot();
    }

    public int AddAmount(int amountToAdd){
        itemAmount += amountToAdd;
        UpdateSlot();
        return itemAmount;
    }

    public int RemoveAmount(int amountToRem){
        itemAmount -= amountToRem;

        if (itemAmount <= 0){
            ClearSlot();
        } else {
            UpdateSlot();
        }

        return itemAmount;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}
