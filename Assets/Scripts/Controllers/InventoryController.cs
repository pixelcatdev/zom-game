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
    public void RemoveItem(int slotIndex, int qty, bool dropAll, Inventory targetInventory)
    {
        //Only allow it to be dropped if its not equipped
        if (targetInventory.inventorySlots[slotIndex].slotEquipped == false)
        {
            if (dropAll == false)
            {
                Debug.Log("Dropping x1 " + targetInventory.inventorySlots[slotIndex].loot.lootName);
                //Remove one, but if there's none left, remove the entire slot
                if (targetInventory.inventorySlots[slotIndex].slotQty - qty > 0)
                {
                    targetInventory.inventorySlots[slotIndex].slotQty-= qty;
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
                //Recalculate the party weight
                PartyController.instance.CalculatePartyWeight();
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
                //Recalculate the party weight
                PartyController.instance.CalculatePartyWeight();
                UIController.instance.UpdateInventory();
            }
            
        }
        else
        {
            Debug.Log("Cannot drop an equipped item, must unequip first from the Party Screen");
        }
    }

    //Moves an item from one inventory to another
    public void MoveItem(Inventory sourceInventory, int sourceIndex, Inventory targetInventory, int qtyToMove)
    {
        //get the reference of the item to move and move it to the target inventory
        Loot lootToMove = sourceInventory.inventorySlots[sourceIndex].loot;
        AddItem(lootToMove, qtyToMove, targetInventory);

        //remove the item from the source inventory
        RemoveItem(sourceIndex, qtyToMove, false, sourceInventory);
    }

    //Clears an inventory out completely
    public void ClearInventory(Inventory targetInventory)
    {
        //if its for the party inventory, loop through every player and force them to drop their item (and recalc the party weight)
        if(targetInventory == PartyController.instance.party.inventory)
        {
            for (int i = 0; i < PartyController.instance.party.partySurvivors.Count; i++)
            {
                PartyController.instance.party.partySurvivors[i].attack = 0;
                PartyController.instance.party.partySurvivors[i].equippedWeaponIndex = -1;
            }

            PartyController.instance.CalculatePartyWeight();
        }

        targetInventory.inventorySlots.Clear();
    }

    //Checks if an item can be crafted
    public bool CheckCraftingRecipe(int craftingIndex)
    {
        //get each ingredient from the recipe at craftingIndex, if the required qty is in the inventory, loop to the next one, else break and return false
        Debug.Log("Checking inventory for ingredients for " + PartyController.instance.party.recipesKnown.recipes[craftingIndex].lootCrafted);

        bool canCraft = false;
        int ingredientsFulfilled = 0;

        for (int x = 0; x < PartyController.instance.party.inventory.inventorySlots.Count; x++)
        {
            InventorySlot inventorySlot = PartyController.instance.party.inventory.inventorySlots[x];
            for (int i = 0; i < PartyController.instance.party.recipesKnown.recipes[craftingIndex].ingredients.Count; i++)
            {
                string ingredientLoot = PartyController.instance.party.recipesKnown.recipes[craftingIndex].ingredients[i].lootName;
                int ingredientQty = PartyController.instance.party.recipesKnown.recipes[craftingIndex].ingredients[i].requiredQty;

                if (inventorySlot.loot.lootName == ingredientLoot && inventorySlot.slotQty >= ingredientQty)
                {
                    ingredientsFulfilled++;
                }
            }
        }

        if(ingredientsFulfilled == PartyController.instance.party.recipesKnown.recipes[craftingIndex].ingredients.Count)
        {
            canCraft = true;
        }
        else
        {
            canCraft = false;
        }
        Debug.Log("Can craft: " + canCraft);

        //Craft the item
        if(canCraft == true)
        {
            CraftItem(craftingIndex);
        }

        return canCraft;
    }

    //Adds the new item to the inventory and removes the resources for the recipe
    public void CraftItem(int craftingIndex)
    {
        Loot craftedLoot = null;

        //find the crafted loot from the configs
        for (int i = 0; i < ConfigController.instance.loot.loot.Count; i++)
        {
            if(PartyController.instance.party.recipesKnown.recipes[craftingIndex].lootCrafted == ConfigController.instance.loot.loot[i].lootName)
            {
                craftedLoot = ConfigController.instance.loot.loot[i];
            }
        }

        for (int i = 0; i < PartyController.instance.party.recipesKnown.recipes[craftingIndex].ingredients.Count; i++)
        {
            string ingredientLoot = PartyController.instance.party.recipesKnown.recipes[craftingIndex].ingredients[i].lootName;
            int ingredientQty = PartyController.instance.party.recipesKnown.recipes[craftingIndex].ingredients[i].requiredQty;

            for (int x = 0; x < PartyController.instance.party.inventory.inventorySlots.Count; x++)
            {
                InventorySlot inventorySlot = PartyController.instance.party.inventory.inventorySlots[x];

                if (inventorySlot.loot.lootName == ingredientLoot)
                {
                    RemoveItem(x, ingredientQty, false, PartyController.instance.party.inventory);
                    break;
                }
            }
        }

        //add the item and update the inventory
        WorldController.instance.AdvanceTime(PartyController.instance.party.recipesKnown.recipes[craftingIndex].buildTimeMinutes);
        AddItem(craftedLoot, 1, PartyController.instance.party.inventory);
        UIController.instance.UpdateInventory();
        Debug.Log("Item Crafted");
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