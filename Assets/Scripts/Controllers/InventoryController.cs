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
    public void AddItem(Loot loot, int lootQty, Inventory targetInventory)
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
            targetInventory.inventorySlots.Add(new InventorySlot());
            targetInventory.inventorySlots[targetInventory.inventorySlots.Count - 1].loot = loot;
            targetInventory.inventorySlots[targetInventory.inventorySlots.Count - 1].slotQty = lootQty;
        }
    }

    //Remove an item from a target inventory, clears up the slot if nothing is left
    public void DropItem(int slotIndex, bool dropAll, Inventory targetInventory)
    {
        //Only allow it to be dropped if its not equipped
        if (targetInventory.inventorySlots[slotIndex].slotEquipped == false)
        {
            if (dropAll == false)
            {
                Debug.Log("Dropping x1 " + targetInventory.inventorySlots[slotIndex].loot.lootName);
                //Remove one, but if there's none left, remove the entire slot
                if (targetInventory.inventorySlots[slotIndex].slotQty - 1 > 0)
                {
                    targetInventory.inventorySlots[slotIndex].slotQty--;
                }
                else
                {
                    targetInventory.inventorySlots.RemoveAt(slotIndex);

                    //now that an index has been removed, correct all equipped weapon indexes if they were after this index
                    for (int i = 0; i < PartyController.instance.party.partySurvivors.Count; i++)
                    {
                        if (PartyController.instance.party.partySurvivors[i].equippedWeaponIndex > slotIndex)
                        {
                            PartyController.instance.party.partySurvivors[i].equippedWeaponIndex--;
                        }
                    }
                }

                UIController.instance.UpdateInventory();
            }
            else
            {
                Debug.Log("Dropping all " + targetInventory.inventorySlots[slotIndex].loot.lootName);
                targetInventory.inventorySlots.RemoveAt(slotIndex);

                //now that an index has been removed, correct all equipped weapon indexes if they were after this index
                for (int i = 0; i < PartyController.instance.party.partySurvivors.Count; i++)
                {
                    if (PartyController.instance.party.partySurvivors[i].equippedWeaponIndex > slotIndex)
                    {
                        PartyController.instance.party.partySurvivors[i].equippedWeaponIndex--;
                    }
                }

                UIController.instance.UpdateInventory();
            }

            //Recalculate the party weight
            //CalculateWeight();
        }
        else
        {
            Debug.Log("Cannot drop an equipped item, must unequip first from the Party Screen");
        }
    }

    //Moves an item from one inventory to another
    public void MoveItem()
    {

    }

    //Clears an inventory out completely
    public void ClearInventory(Inventory targetInventory)
    {
        //if its for the party inventory, loop through every player and force them to drop their item
        if(targetInventory == PartyController.instance.party.inventory)
        {
            for (int i = 0; i < PartyController.instance.party.partySurvivors.Count; i++)
            {
                PartyController.instance.party.partySurvivors[i].attack = 0;
                PartyController.instance.party.partySurvivors[i].equippedWeaponIndex = -1;
            }
        }       

        targetInventory.inventorySlots.Clear();
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
    public int slotQty;
    public bool slotEquipped;
}