using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Item testItem;
    public Item testItem2;

    public GameObject hotbarObject;
    public GameObject inventorySlotParent;
    public PlayerCombat playerCombat;

    private List<Slot> inventorySlots = new List<Slot>();
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();

    private void Awake(){
        if (playerCombat == null)
        {
            playerCombat = GetComponent<PlayerCombat>();
        }
        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>(true));
        hotbarSlots.AddRange(hotbarObject.GetComponentsInChildren<Slot>(true));
        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);
        playerCombat.inventoryOpen = inventorySlotParent.activeSelf;
    }

    // Update is called once per frame
    [System.Obsolete]
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            inventorySlotParent.SetActive(!inventorySlotParent.activeSelf);
            playerCombat.inventoryOpen = inventorySlotParent.activeSelf;
        }

        if (Input.GetKeyDown(KeyCode.A)){ //Spawn axe
            AddItem(testItem, 1);
        }
        if (Input.GetKeyDown(KeyCode.S)){ //Spawn sword
            AddItem(testItem2, 1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)){  //Equip weapon in hotbar slot 1
            if (hotbarSlots[0].GetItem() is Weapon weapon)
            {
                playerCombat.SetEquippedWeapon(weapon);
            }
            else
            {
                playerCombat.SetEquippedWeapon(null);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)){  //Equip weapon in hotbar slot 2
            if (hotbarSlots[1].GetItem() is Weapon weapon)
            {
                playerCombat.SetEquippedWeapon(weapon);
            }
            else {
                playerCombat.SetEquippedWeapon(null);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)){  //Equip weapon in hotbar slot 3
            if (hotbarSlots[2].GetItem() is Weapon weapon)
            {
                playerCombat.SetEquippedWeapon(weapon);
            }
            else {
                playerCombat.SetEquippedWeapon(null);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)){  //Equip weapon in hotbar slot 4
            if (hotbarSlots[3].GetItem() is Weapon weapon)
            {
                playerCombat.SetEquippedWeapon(weapon);
            }
            else {
                playerCombat.SetEquippedWeapon(null);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha5)){  //Equip weapon in hotbar slot 5
            if (hotbarSlots[4].GetItem() is Weapon weapon)
            {
                playerCombat.SetEquippedWeapon(weapon);
            }
            else {
                playerCombat.SetEquippedWeapon(null);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha6)){  //Equip weapon in hotbar slot 6
            if (hotbarSlots[5].GetItem() is Weapon weapon)
            {
                playerCombat.SetEquippedWeapon(weapon);
            }
            else {
                playerCombat.SetEquippedWeapon(null);
            }
        }
    }

    public void AddItem(Item item, int amount = 1){
        int remaining = amount;
        foreach (Slot slot in allSlots){
            if (slot.HasItem() && slot.GetItem() == item && slot.GetAmount() < item.maxStackSize){
                int space = item.maxStackSize - slot.GetAmount();
                int toAdd = Mathf.Min(space, remaining);
                slot.SetItem(item, slot.GetAmount() + toAdd);
                remaining -= toAdd;
                if (remaining <= 0) return;
            }
        }

        foreach (Slot slot in allSlots){
            if (!slot.HasItem()){
                int toAdd = Mathf.Min(item.maxStackSize, remaining);
                slot.SetItem(item, toAdd);
                remaining -= toAdd;
                if (remaining <= 0) return;
            }
        }

        if (remaining > 0){
            Debug.Log("Not enough space in inventory for " + remaining + " " + item.itemName);
        }
    }
}
