using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject hotbarObject;
    public GameObject inventorySlotParent;
    public PlayerCombat playerCombat;
    public ItemInfoPanel infoPanel;             // stats panel on the right
    public JournalUI journal;

    public Item testItem;
    public Item testItem2;
    public Item testItem3;

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

    // start closed (in Start so slots wake up first)
    private void Start()
    {
        SetOpen(false);
    }

    private void OnEnable()
    {
        Slot.Clicked += OnSlotClicked;
    }

    private void OnDisable()
    {
        Slot.Clicked -= OnSlotClicked;
    }

    [System.Obsolete]
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetOpen(!IsOpen());
        }

        if (Input.GetKeyDown(KeyCode.E)){ // spawn axe
            AddItem(testItem, 1);
        }
        if (Input.GetKeyDown(KeyCode.R)){ // spawn sword
            AddItem(testItem2, 1);
        }
        if (Input.GetKeyDown(KeyCode.T)){ // spawn apple
            AddItem(testItem3, 1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)){  // equip weapon in hotbar slot 1
            if (hotbarSlots[0].GetItem() is Weapon weapon)
            {
                playerCombat.SetEquippedWeapon(weapon);
            }
            else
            {
                playerCombat.SetEquippedWeapon(null);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)){  // equip weapon in hotbar slot 2
            if (hotbarSlots[1].GetItem() is Weapon weapon)
            {
                playerCombat.SetEquippedWeapon(weapon);
            }
            else {
                playerCombat.SetEquippedWeapon(null);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)){  // equip weapon in hotbar slot 3
            if (hotbarSlots[2].GetItem() is Weapon weapon)
            {
                playerCombat.SetEquippedWeapon(weapon);
            }
            else {
                playerCombat.SetEquippedWeapon(null);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)){  // equip weapon in hotbar slot 4
            if (hotbarSlots[3].GetItem() is Weapon weapon)
            {
                playerCombat.SetEquippedWeapon(weapon);
            }
            else {
                playerCombat.SetEquippedWeapon(null);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha5)){  // equip weapon in hotbar slot 5
            if (hotbarSlots[4].GetItem() is Weapon weapon)
            {
                playerCombat.SetEquippedWeapon(weapon);
            }
            else {
                playerCombat.SetEquippedWeapon(null);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha6)){  // equip weapon in hotbar slot 6
            if (hotbarSlots[5].GetItem() is Weapon weapon)
            {
                playerCombat.SetEquippedWeapon(weapon);
            }
            else {
                playerCombat.SetEquippedWeapon(null);
            }
        }
    }

    public bool IsOpen()
    {
        return inventorySlotParent.activeSelf;
    }

    public void SetOpen(bool open)
    {
        if (open && journal != null && journal.IsOpen())
        {
            journal.Close();
        }

        inventorySlotParent.SetActive(open);
        playerCombat.inventoryOpen = open;

        if (!open && infoPanel != null)
        {
            infoPanel.Hide();
        }
    }

    public void AddItem(Item item, int amount = 1){
        if (journal != null)
        {
            journal.Discover(item);  // add to journal
        }

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
            if (!slot.HasItem() && CheckValidSlotForItem(slot, item)){
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

    private void OnSlotClicked(Slot slot)
    {
        if (infoPanel == null || !IsOpen())
        {
            return;
        }

        if (slot.HasItem())
        {
            infoPanel.Show(slot.GetItem());
        }
        else
        {
            infoPanel.Hide();
        }
    }

    private bool CheckValidSlotForItem(Slot slot, Item item)
    {
        // Check if the slot is blocked
        if (slot.IsBlocked)
        {
            return false;
        }

        Vector2Int itemSize = item.inventorySize;
        const int inventoryWidth = 6;
        const int inventoryHeight = 4;

        if (itemSize.x <= 0 || itemSize.y <= 0)
        {
            return false;
        }

        int startIndex = inventorySlots.IndexOf(slot);
        if (startIndex < 0)
        {
            return false;
        }

        int startX = startIndex % inventoryWidth;
        int startY = startIndex / inventoryWidth;
        if (startX + itemSize.x > inventoryWidth || startY + itemSize.y > inventoryHeight)
        {
            return false;
        }

        int requiredSlots = itemSize.x * itemSize.y;
        int overlappingSlots = 0;
        for (int y = startY; y < startY + itemSize.y; y++)
        {
            for (int x = startX; x < startX + itemSize.x; x++)
            {
                int index = y * inventoryWidth + x;
                if (index >= inventorySlots.Count || inventorySlots[index].IsBlocked)
                {
                    return false;
                }

                overlappingSlots++;
            }
        }

        if (overlappingSlots != requiredSlots)
        {
            return false;
        }

        return true;
    }
}
