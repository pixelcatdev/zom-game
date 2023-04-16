using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeController : MonoBehaviour
{

    public Inventory tradeInventory = new Inventory();
    public Inventory buyInventory = new Inventory();
    public Inventory sellInventory = new Inventory();
    public float buyTotal;
    public float sellTotal;

    public static TradeController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!TradeController.instance)
        {
            TradeController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    //Setup a new trade encounter, with 5 random items
    public void SetupTrade()
    {
        tradeInventory.inventorySlots.Clear();

        for (int i = 0; i < 5; i++)
        {
            //Get a random item
            Loot randomLoot = PartyController.instance.RandomItem(1f);
            int randomQty = 0;
            if (randomLoot.lootType != "WeaponRanged" || randomLoot.lootType != "WeaponMelee")
            {
                randomQty = Random.Range(1, 5);
            }
            else
            {
                randomQty = 1;
            }
            InventoryController.instance.AddItem(randomLoot, randomQty, tradeInventory);
        }

        //UI updates
        UIController.instance.uiTrade.SetActive(true);
        UIController.instance.UpdateTrade();
    }

    public void TradeBuy(bool increaseQty, int tradeIndex)
    {
        Loot loot = tradeInventory.inventorySlots[tradeIndex].loot;
        //copy the contents from the buy
        if (increaseQty == true)
        {
            //move the tradeIndex loot into the buyBasket with its qty and reduce the 
            bool hasItem = false;

            //if the itemId exists in the inventory list already, just add the qty
            for (int i = 0; i < buyInventory.inventorySlots.Count; i++)
            {
                //If the loot exists in the inventory already but is not a weapon, just add the quantity, else add a new slot for the weapon
                if (buyInventory.inventorySlots[i].loot == loot)// && (lootType != "WeaponRanged" || lootType != "WeaponMelee"))
                {
                    buyInventory.inventorySlots[i].slotQty += 1;
                    hasItem = true;
                    break;
                }
                else
                {
                    hasItem = false;
                }
            }

            //else add a new slot and populate it with the item
            if (hasItem == false)
            {
                buyInventory.inventorySlots.Add(new InventorySlot());
                buyInventory.inventorySlots[buyInventory.inventorySlots.Count - 1].loot = loot;
                buyInventory.inventorySlots[buyInventory.inventorySlots.Count - 1].slotQty = 1;
            }
        }
        else
        {
            bool hasItem = false;

            //if the itemId exists in the inventory list already, just add the qty
            for (int i = 0; i < tradeInventory.inventorySlots.Count; i++)
            {
                //If the loot exists in the inventory already but is not a weapon, just add the quantity, else add a new slot for the weapon
                if (tradeInventory.inventorySlots[i].loot == loot)// && (lootType != "WeaponRanged" || lootType != "WeaponMelee"))
                {
                    tradeInventory.inventorySlots[i].slotQty += 1;
                    hasItem = true;
                    break;
                }
                else
                {
                    hasItem = false;
                }
            }

            //else add a new slot and populate it with the item
            if (hasItem == false)
            {
                tradeInventory.inventorySlots.Add(new InventorySlot());
                tradeInventory.inventorySlots[tradeInventory.inventorySlots.Count - 1].loot = loot;
                tradeInventory.inventorySlots[tradeInventory.inventorySlots.Count - 1].slotQty = 1;
            }           
        }
    }

    public void TradeSell(bool increaseQty, int inventoryIndex)
    {

    }

    //Loops through the buyInventory and totals the loot value
    private void CalculateBuyTotal()
    {
        buyTotal = 0;
        for (int i = 0; i < buyInventory.inventorySlots.Count; i++)
        {
            buyTotal += buyInventory.inventorySlots[i].loot.lootValue * buyInventory.inventorySlots[i].slotQty;
        }
    }
}
