using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyController : MonoBehaviour
{
    public Transform partyObj;
    public Party party = new Party();
    public Inventory inventory = new Inventory();
    public static PartyController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!PartyController.instance)
        {
            PartyController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.instance.gameMode == GameController.GameMode.worldmap)
        {
            TileInteraction();
        }
    }

    //Get Tile input from the mouse
    void TileInteraction()
    {
        LayerMask hitLayer = LayerMask.GetMask("WorldTile");
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0f, hitLayer);

        if (hits.Length > 0)
        {
            GameObject hit = hits[0].transform.gameObject;
            GameObject hitObj = hit.transform.gameObject;

            //Display the tiles name and biome
            UIController.instance.hud_biome.text = hitObj.GetComponent<WorldTileProps>().tileProps.biome;
            UIController.instance.hud_tileName.text = hitObj.GetComponent<WorldTileProps>().tileProps.name;

            //On left-click, if the tile is within 1.5 unit of the party's position
            if (Input.GetMouseButtonDown(0) && Vector2.Distance(PartyController.instance.partyObj.position, hit.transform.position) <= 1.5f)
            {
                //Set the target tile to be a reference to the current tile
                WorldController.instance.currentTile = hitObj;

                //Call PartyController MoveParty (with tile Vector2 pos and the biome)
                MoveParty(hit.transform.position, hit.GetComponent<WorldTileProps>().tileProps.biome);
            }
        }
    }

    //Move the party to a target tile
    public void MoveParty(Vector2 tilePos, string tileBiome)
    {
        bool canMove = false;
        int energyCost = 5;

        //If not in a vehicle and energy is > 0
        if (party.inVehicle == false && party.partyEnergy - energyCost >= 0)
        {
            canMove = true;
        }
        //if in a vehicle, fuel > 0 and the target tile is a motorway
        else if (party.inVehicle == true && party.partyVehicle.vehicleFuel >= 0 && (tileBiome == "Urban" || tileBiome == "Motorway"))
        {
            canMove = true;
        }

        //If they can move
        if (canMove == true)
        {
            Debug.Log("Moved Party");
            //Deduct Energy or Vehicle Fuel and advance time based on speed
            if (party.inVehicle == false)
            {
                party.partyEnergy -= energyCost;
                float speedInMilesPerMinute = party.walkingSpeed / 60.0f;
                float travelTimeInMinutes = 1.0f / speedInMilesPerMinute;

                WorldController.instance.AdvanceTime(travelTimeInMinutes);
            }
            else
            {
                party.partyVehicle.vehicleFuel -= 1;
                float speedInMilesPerMinute = party.vehicleSpeed / 60.0f;
                float travelTimeInMinutes = 1.0f / speedInMilesPerMinute;

                WorldController.instance.AdvanceTime(travelTimeInMinutes);
            }

            //Move the Party obj to that position
            partyObj.position = tilePos;

            //Clear the previous status text, randomise a new status and randomise an event
            EncounterController.instance.statusStrings.Clear();
            EncounterController.instance.AddToStatus("Your party " + EncounterController.instance.StatusVerb() + " " + EncounterController.instance.StatusNoun());
            EncounterController.instance.RandomEncounter();

            //Set the party's x/y pos now that they've moved
            party.partyPosX = tilePos.x;
            party.partyPosY = tilePos.y;

            //Move the camera
            Camera.main.transform.position = new Vector3(tilePos.x, tilePos.y, Camera.main.transform.position.z);

            //Check the party status
            CheckPartyStatus();

            //Output the turns status
            EncounterController.instance.StatusStringBuilder();

            //Update the HUD
            UIController.instance.UpdateHud();
        }
    }

    //Restore Energy to the party over 6 hours of rest, risking an ambush every hour
    public void RestParty()
    {
        StartCoroutine("RestCoroutine", 60f);
    }

    //Scavenge for resources at the risk of triggering an ambush or encounter
    public void Scavenge()
    {
        //Check the tile to see if they've already tried looting here
        if (WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.alreadyScavenged == false)
        {
            WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.alreadyScavenged = true;
            StartCoroutine("ScavengeCoroutine", 2f);
        }
        else
        {
            Debug.Log("Cannot scavenge, you've already scavenged here");
        }
    }

    //Checks the party status and increases infection rates etc if injured
    public void CheckPartyStatus()
    {
        for (int i = 0; i < party.partySurvivors.Count; i++)
        {
            //Increase infection if the survivor is infected
            if (party.partySurvivors[i].infection > 0)
            {
                if (party.partySurvivors[i].infection < 10)
                {
                    party.partySurvivors[i].infection++;
                }
                //Reduce survivor health if infection is at max level
                else
                {
                    KillSurvivor(i);
                    //Add event turn survivor and start ambush
                }
            }
        }
    }

    //Adds a new survivor to the group
    public void AddSurvivor()
    {
        if (party.partySurvivors.Count < 6)
        {
            party.partySurvivors.Add(new Survivor());            

            //Randomise gender
            int gender = Random.Range(0, 2);

            //Randomise name based on gender
            Survivor newSurvivor = party.partySurvivors[party.partySurvivors.Count - 1];

            if (gender == 0)
            {
                newSurvivor.survivorName = ConfigController.instance.maleNames[Random.Range(0, ConfigController.instance.maleNames.Count - 1)];
            }
            else
            {
                newSurvivor.survivorName = ConfigController.instance.femaleNames[Random.Range(0, ConfigController.instance.femaleNames.Count - 1)];
            }

            //Set the survivor to unequipped
            newSurvivor.equippedWeaponIndex = -1;

            //Randomise chance of new quest

            //Update the UI
            UIController.instance.UpdateHud();
        }
    }

    //Kill a survivor from the party
    public void KillSurvivor(int survivorId)
    {
        //Add death to journal log
        EncounterController.instance.AddToStatus(party.partySurvivors[survivorId].survivorName + " has been killed.");

        //Turn the survivor into a zom if they are infected
        if (party.partySurvivors[survivorId].infection > 0 && GameController.instance.gameMode == GameController.GameMode.worldmap)
        {
            Debug.Log(party.partySurvivors[survivorId].survivorName + " has turned. Starting ambush");
        }

        //Remove from Survivor list
        RemoveSurvivorFromParty(survivorId);
    }

    //Abandon a survivor
    public void AbandonSurvivor(int survivorId)
    {
        if(party.partySurvivors.Count > 1)
        {
            Debug.Log("Abandoning " + party.partySurvivors[survivorId].survivorName);
            RemoveSurvivorFromParty(survivorId);
        }
        else
        {
            Debug.Log("Cannot abandon the last survivor");
        }
        
    }

    public void RemoveSurvivorFromParty(int survivorId)
    {
        //Unequip any equipment they currently hold
        UnequipWeapon(survivorId);

        //Remove from Survivor list
        party.partySurvivors.RemoveAt(survivorId);

        //Update UI Elements
        UIController.instance.UpdateHud();
        UIController.instance.UpdateParty();
    }

    //Select a weapon
    public void EquipWeapon(int weaponIndexInventory, int survivorIndex)
    {
        Debug.Log("Player is equipping index " + weaponIndexInventory);
        //Unequip whatever the survivor currently has equipped first
        if (party.partySurvivors[survivorIndex].equippedWeaponIndex != -1)
        {
            UnequipWeapon(survivorIndex);
        }

        //Update the characters stats, sets the weapon as equipped and refresh the party screen
        party.partySurvivors[survivorIndex].equippedWeaponIndex = weaponIndexInventory;
        //Set the text of the players current weapon to that of the inventory slot
        party.partySurvivors[survivorIndex].attack = inventory.inventorySlots[weaponIndexInventory].lootTypeVal;
        //reclaulctae the partys attack strength
        inventory.inventorySlots[weaponIndexInventory].lootEquipped = true;
        UIController.instance.CloseWeaponMenu();
        UIController.instance.UpdateParty();
    }

    //Unequips a currently equipped weapon
    public void UnequipWeapon(int survivorIndex)
    {        
        int equippedWeaponIndex = party.partySurvivors[survivorIndex].equippedWeaponIndex;
        if(equippedWeaponIndex != -1)
        {
            party.partySurvivors[survivorIndex].equippedWeaponIndex = -1;
            party.partySurvivors[survivorIndex].attack = 0;
            inventory.inventorySlots[equippedWeaponIndex].lootEquipped = false;
            UIController.instance.CloseWeaponMenu();
            UIController.instance.UpdateParty();
        }        
    }

    //Enter a vehicle
    public void EnterVehicle(Vehicle targetVehicle, int fuel, int hp, int tileIndex)
    {
        party.partyVehicle = targetVehicle;
        party.partyVehicle.vehicleFuel = fuel;
        party.partyVehicle.vehicleHp = hp;
        WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.vehicles.RemoveAt(tileIndex);
        UIController.instance.UpdateHud();
        UIController.instance.UpdateVehicles();
        party.inVehicle = true;
    }

    //Exit a vehicle
    public void ExitVehicle()
    {
        WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.vehicles.Add(party.partyVehicle);
        party.partyVehicle = null;
        UIController.instance.UpdateHud();
        UIController.instance.UpdateVehicles();
        party.inVehicle = false;
    }

    //Refuel a vehicle
    public void RefuelVehicle()
    {
        //Find the inventory slot holding fuel
        int fuelIndex = -1;
        for (int i = 0; i < inventory.inventorySlots.Count; i++)
        {
            if(inventory.inventorySlots[i].lootName == "Fuel")
            {
                fuelIndex = i;
            }
        }

        //if fuel has been found in the inventory
        if(fuelIndex != -1)
        {
            if(inventory.inventorySlots[fuelIndex].lootQty - 1 > 0)
            {
                DropItem(fuelIndex, false);
                party.partyVehicle.vehicleFuel++;
                UIController.instance.UpdateHud();
                UIController.instance.UpdateVehicles();
            }
            else
            {
                Debug.Log("Cannot refuel, no fuel left");
            }
        }
    }

    //Repair a vehicle
    public void RepairVehicle()
    {

    }

    //Adds an item to the party inventory - ok, so this should be in its own Inventory class, but kiss my arse
    public void AddItem(string lootName, string lootDesc, string lootType, int lootTypeVal, float lootRarity, float lootWeight, float lootValue, int lootQty, int lootStack)
    {
        bool hasItem = false;

        //if the itemId exists in the inventory list already, just add the qty
        for (int i = 0; i < inventory.inventorySlots.Count; i++)
        {
            //If the loot exists in the inventory already but is not a weapon, just add the quantity, else add a new slot for the weapon
            if (inventory.inventorySlots[i].lootName == lootName && !(lootType == "WeaponRanged" || lootType == "WeaponMelee"))
            {
                inventory.inventorySlots[i].lootQty += lootQty;
                hasItem = true;
            }
            else
            {
                hasItem = false;
            }
        }

        //else add a new slot and populate it with the item
        if (hasItem == false)
        {
            inventory.inventorySlots.Add(new InventorySlot());
            inventory.inventorySlots[inventory.inventorySlots.Count - 1].lootName = lootName;
            inventory.inventorySlots[inventory.inventorySlots.Count - 1].lootDesc = lootDesc;
            inventory.inventorySlots[inventory.inventorySlots.Count - 1].lootType = lootType;
            inventory.inventorySlots[inventory.inventorySlots.Count - 1].lootTypeVal = lootTypeVal;
            inventory.inventorySlots[inventory.inventorySlots.Count - 1].lootRarity = lootRarity;
            inventory.inventorySlots[inventory.inventorySlots.Count - 1].lootWeight = lootWeight;
            inventory.inventorySlots[inventory.inventorySlots.Count - 1].lootValue = lootValue;
            inventory.inventorySlots[inventory.inventorySlots.Count - 1].lootQty = lootQty;
            inventory.inventorySlots[inventory.inventorySlots.Count - 1].lootStack = lootStack;
        }

        //Calculate the party weight
        CalculateWeight();

    }

    //Drop one or all items from a party inventory slot
    public void DropItem(int index, bool dropAll)
    {
        //Only allow it to be dropped if its not equipped
        if(inventory.inventorySlots[index].lootEquipped == false)
        {
            if (dropAll == false)
            {
                Debug.Log("Dropping x1 " + inventory.inventorySlots[index].lootName);
                //Remove one, but if there's none left, remove the entire slot
                if (inventory.inventorySlots[index].lootQty - 1 > 0)
                {
                    inventory.inventorySlots[index].lootQty--;
                }
                else
                {
                    inventory.inventorySlots.RemoveAt(index);
                }

                UIController.instance.UpdateInventory();
            }
            else
            {
                Debug.Log("Dropping all " + inventory.inventorySlots[index].lootName);
                inventory.inventorySlots.RemoveAt(index);
                UIController.instance.UpdateInventory();
            }

            //Recalculate the party weight
            CalculateWeight();
        }
        else
        {
            Debug.Log("Cannot drop an equipped item, must unequip first from the Party Screen");
        }
        
    }

    //recalculate the party weight
    public void CalculateWeight()
    {
        float tempWeight = 0f;

        for (int i = 0; i < inventory.inventorySlots.Count; i++)
        {
            tempWeight += inventory.inventorySlots[i].lootWeight * inventory.inventorySlots[i].lootQty;
        }

        party.partyWeight = tempWeight;
    }

    //recalculate the party attack and defense
    public void CalculateAttackDefense()
    {
        //loop through each survivor
        //get the survivors attack value
        //get the survivors defense value
    }

    //Advances time every seconds by X minutes
    IEnumerator RestCoroutine(float minutes)
    {
        for (int i = 0; i < 6; i++)
        {
            yield return new WaitForSeconds(1f);
            WorldController.instance.AdvanceTime(minutes);
            if (party.partyEnergy + 10 < 100)
            {
                party.partyEnergy += 10;
            }
            else
            {
                party.partyEnergy = 100;
            }

            //Check for ambush based on tiles threat level

            EncounterController.instance.AddToStatus("Your party rests");
            UIController.instance.UpdateHud();
        }
    }

    //Advances time every seconds by X minutes
    IEnumerator ScavengeCoroutine(float minutes)
    {
        for (int i = 0; i < 6; i++)
        {
            yield return new WaitForSeconds(1f);
            WorldController.instance.AdvanceTime(minutes);

            //Add random loot or end the scavenging with an encounter or an ambush
            //Call inventory pickup method - add item to existing slot or create new slot (pass across all the items details)            
            int randomQty = Random.Range(1, 3);
            Loot randomItem = ConfigController.instance.loot.loot[Random.Range(0, ConfigController.instance.loot.loot.Count - 1)];
            AddItem(randomItem.lootName, randomItem.lootDesc, randomItem.lootType, randomItem.lootTypeVal, randomItem.lootRarity, randomItem.lootWeight, randomItem.lootValue, randomQty, randomItem.lootStack);

            //Update the status text to state what was picked up
            EncounterController.instance.AddToStatus("Picked up: " + randomItem.lootName + " x" + randomQty);
            EncounterController.instance.StatusStringBuilder();
            UIController.instance.UpdateHud();
        }
    }
}

[System.Serializable]
public class Party
{
    //Worldmap location
    public float partyPosX;
    public float partyPosY;

    //How much energy the party has left
    public int partyEnergy;

    //Party stats
    public int partyAttack;
    public int partyDefense;

    //Party weight based on inventory
    public float partyWeight;

    //List of survivors currently in the party
    public List<Survivor> partySurvivors = new List<Survivor>();

    //List of current quests
    public List<Quest> quests = new List<Quest>();

    //Travel speeds
    public float walkingSpeed = 4f;
    public float vehicleSpeed = 50f;

    //True if party is travelling by vehicle
    public bool inVehicle;

    //Vehicle stats
    public Vehicle partyVehicle;
}


[System.Serializable]
public class Survivor
{
    public string survivorName;
    public string survivorClass;
    public int infection;
    public int equippedWeaponIndex;
    public int attack;
    public int defense;
    public int damage;
}

[System.Serializable]
public class Inventory
{
    public List<InventorySlot> inventorySlots;
}

[System.Serializable]
public class InventorySlot
{
    public string lootName;
    public string lootDesc;
    public string lootType;
    public int lootTypeVal;
    public float lootWeight;
    public float lootRarity;
    public float lootValue;
    public int lootQty;
    public int lootStack;
    public bool lootEquipped;
}