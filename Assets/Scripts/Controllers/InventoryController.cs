using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!InventoryController.instance)
        {
            InventoryController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    //Adds an item to a target inventory
    public void AddItem(Loot loot, int lootQty, InventoryTest targetInventory)
    {
        bool hasItem = false;

        //if the itemId exists in the inventory list already, just add the qty
        for (int i = 0; i < targetInventory.inventorySlots.Count; i++)
        {
            //If the loot exists in the inventory already but is not a weapon, just add the quantity, else add a new slot for the weapon
            if (targetInventory.inventorySlots[i].loot == loot)// && (lootType != "WeaponRanged" || lootType != "WeaponMelee"))
            {
                if (loot.lootType != "WeaponRanged" && loot.lootType != "WeaponMelee")
                {
                    targetInventory.inventorySlots[i].slotQty += lootQty;
                    hasItem = true;
                    break;
                }
                else
                {
                    hasItem = false;
                }
            }
            else
            {
                hasItem = false;
            }
        }

        //else add a new slot and populate it with the item
        if (hasItem == false)
        {
            targetInventory.inventorySlots.Add(new InventorySlotTest());
            targetInventory.inventorySlots[targetInventory.inventorySlots.Count - 1].loot = loot;
            targetInventory.inventorySlots[targetInventory.inventorySlots.Count - 1].slotQty = lootQty;
        }
    }

    //Remove an item from a target inventory, clears up the slot if nothing is left
    public void DropItem()
    {

    }

    //Moves an item from one inventory to another
    public void MoveItem()
    {

    }

    //Clears an inventory out completely
    public void ClearInventory()
    {

    }

}

[System.Serializable]
public class Inventory
{
    public List<InventorySlot> inventorySlots;
}

[System.Serializable]
public class InventorySlot
{
    public Loot loot;
    public string lootName;
    public string lootDesc;
    public string lootType;
    public int lootTypeVal;
    public float lootWeight;
    public float lootRarity;
    public float lootValue;
    public int lootQty;
    public string lootBiome;
    public bool lootEquipped;
}

[System.Serializable]
public class InventoryTest
{
    public List<InventorySlotTest> inventorySlots;
}

[System.Serializable]
public class InventorySlotTest
{
    public Loot loot;
    public int slotQty;
    public bool slotEquipped;
}