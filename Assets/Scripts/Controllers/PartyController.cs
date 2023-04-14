using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyController : MonoBehaviour
{
    public Transform partyObj;
    public Party party = new Party();
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

            //Display the tiles name, biome, and threat details
            UIController.instance.hud_biome.text = hitObj.GetComponent<WorldTileProps>().tileProps.biome;
            UIController.instance.hud_tileName.text = hitObj.GetComponent<WorldTileProps>().tileProps.name;
            UIController.instance.hud_threatCalc.text = "Threat Level: " + WorldController.instance.CalculateThreatLevel(false, hitObj);

            //Output the name of the title if present
            if (hitObj.GetComponent<WorldTileProps>().tileProps.name.Length > 0)
            {
                UIController.instance.uiCursorTooltip.SetActive(true);
                UIController.instance.uiCursorTooltipText.text = hitObj.GetComponent<WorldTileProps>().tileProps.name;
            }
            else
            {
                UIController.instance.uiCursorTooltip.SetActive(false);
                UIController.instance.uiCursorTooltipText.text = null;
            }

            //On left-click, if the tile is within 1.5 unit of the party's position
            if (Input.GetMouseButtonDown(0) && Vector2.Distance(PartyController.instance.partyObj.position, hit.transform.position) <= 1.5f)
            {
                //Call PartyController MoveParty (with tile Vector2 pos and the biome)
                MoveParty(hitObj, hit.transform.position, hit.GetComponent<WorldTileProps>().tileProps.biome);
            }
        }
        else
        {
            UIController.instance.uiCursorTooltip.SetActive(false);
            UIController.instance.uiCursorTooltipText.text = null;
        }
    }

    //Move the party to a target tile
    public void MoveParty(GameObject selectedTile, Vector2 tilePos, string tileBiome)
    {
        bool canMove = false;
        int energyCost = 5;

        //If not in a vehicle and energy is > 0
        if (party.inVehicle == false && party.partyEnergy - energyCost >= 0)
        {
            canMove = true;
        }
        //if in a vehicle, fuel > 0 and the target tile is a motorway
        else if (party.inVehicle == true && party.partyVehicle.vehicleFuel > 0 && (tileBiome == "Urban" || tileBiome == "Motorway"))
        {
            canMove = true;
        }

        //If they can move
        if (canMove == true)
        {
            //Set the target tile to be a reference to the current tile
            WorldController.instance.currentTile = selectedTile;

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

            //Check the log to see if this is the first time the party has visited this location
            string tileName = selectedTile.GetComponent<WorldTileProps>().tileProps.name;
            if (tileName.Length > 0)
            {
                bool locationVisited = WorldController.instance.world.logs.Exists(s => s.Contains(tileName));
                if (locationVisited == false)
                {
                    WorldController.instance.AddLog("Your party visits " + tileName + " for the first time");
                }
            }

            //Check the party status
            CheckPartyStatus();

            //Check the quest list to see if any quests have completed
            QuestController.instance.CheckQuests();

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

    //Restore Energy to the party using food
    public void FeedParty(int inventoryIndex, int amount)
    {
        bool partyFed = false;
        if (party.partyEnergy + amount <= 100)
        {
            party.partyEnergy += amount;
            partyFed = true;
        }
        else
        {
            party.partyEnergy = 100;
            partyFed = false;
        }

        if (partyFed == true)
        {
            InventoryController.instance.RemoveItem(inventoryIndex, false, party.inventory);
            Debug.Log("Party energy increased, food consumed");
        }

        UIController.instance.UpdateHud();
        UIController.instance.UpdateParty();
    }

    //Decrease Infection to the party using food
    public void HealParty(int inventoryIndex, int amount)
    {
        bool partyHealed = false;

        //Loop through each person, if theyre infected, heal - otherwise don't
        for (int i = 0; i < party.partySurvivors.Count; i++)
        {
            if (party.partySurvivors[i].infection > 0)
            {
                if (party.partySurvivors[i].infection - amount > 0)
                {
                    party.partySurvivors[i].infection -= amount;
                }
                else
                {
                    party.partySurvivors[i].infection = 0;
                }

                partyHealed = true;
            }
        }

        if (partyHealed == true)
        {
            InventoryController.instance.RemoveItem(inventoryIndex, false,party.inventory);
            Debug.Log("Infection decreased within your party, medicine consumed");
        }

        UIController.instance.UpdateHud();
        UIController.instance.UpdateParty();
    }

    //Scavenge for resources at the risk of triggering an ambush or encounter
    public void Scavenge()
    {
        //Check the tile to see if they've already tried looting here
        if (WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.alreadyScavenged == false)
        {
            WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.alreadyScavenged = true;
            WorldController.instance.currentTile.GetComponent<WorldTileProps>().alreadyScavengedIcon.SetActive(true);
            StartCoroutine("ScavengeCoroutine", 2f);
        }
        else
        {
            EncounterController.instance.AddToStatus("Your party has scavenged all they can from here.");
            EncounterController.instance.StatusStringBuilder();
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
    public void AddSurvivor(Survivor newSurvivor)
    {
        party.partySurvivors.Add(newSurvivor);

        //Recalc the threat level of the party
        CalculatePartyThreatLevel();

        //Randomise chance of new quest

        //Update the logs
        WorldController.instance.AddLog(newSurvivor.survivorName + " joins the party.");

        //Update the UI
        UIController.instance.UpdateHud();
    }

    //Kill a survivor from the party
    public void KillSurvivor(int survivorId)
    {
        //Add death to journal log
        EncounterController.instance.AddToStatus(party.partySurvivors[survivorId].survivorName + " has been killed.");

        //Turn the survivor into a zom if they are infected, triggering a single zom ambush
        if (party.partySurvivors[survivorId].infection > 0 && GameController.instance.gameMode == GameController.GameMode.worldmap)
        {
            if (party.partySurvivors.Count > 1)
            {
                //trigger an ambush with 1 zom
                //AmbushController.instance.SetupAmbush();

                //Update the logs
                WorldController.instance.AddLog(party.partySurvivors[survivorId].survivorName + " passes away due to their infection, and quickly reanimates.");
            }
            else
            {
                //Gameover();
                Debug.Log("Game over - your last survivor turned");
            }

        }

        //Remove from Survivor list
        RemoveSurvivorFromParty(survivorId);
    }

    //Abandon a survivor
    public void AbandonSurvivor(int survivorId)
    {
        if (party.partySurvivors.Count > 1)
        {
            Debug.Log("Abandoning " + party.partySurvivors[survivorId].survivorName);
            WorldController.instance.AddLog(party.partySurvivors[survivorId].survivorName + " leaves the party.");
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

        //Recalc the threat level of the party
        CalculatePartyThreatLevel();

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
        party.partySurvivors[survivorIndex].attack = party.inventory.inventorySlots[weaponIndexInventory].loot.lootTypeVal;
        //reclaulctae the partys attack strength
        party.inventory.inventorySlots[weaponIndexInventory].slotEquipped = true;
        UIController.instance.CloseWeaponMenu();
        UIController.instance.UpdateParty();
    }

    //Unequips a currently equipped weapon
    public void UnequipWeapon(int survivorIndex)
    {
        int equippedWeaponIndex = party.partySurvivors[survivorIndex].equippedWeaponIndex;
        if (equippedWeaponIndex != -1)
        {
            party.partySurvivors[survivorIndex].equippedWeaponIndex = -1;
            party.partySurvivors[survivorIndex].attack = 0;
            party.inventory.inventorySlots[equippedWeaponIndex].slotEquipped = false;
            UIController.instance.CloseWeaponMenu();
            UIController.instance.UpdateParty();
        }
    }

    //Enter a vehicle
    public void EnterVehicle(Vehicle targetVehicle, int fuel, int hp, int tileIndex)
    {
        if(party.partySurvivors.Count <= targetVehicle.vehiclePassengers)
        {
            party.partyVehicle = targetVehicle;
            party.partyVehicle.vehicleFuel = fuel;
            party.partyVehicle.vehicleHp = hp;
            WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.vehicles.RemoveAt(tileIndex);
            party.inVehicle = true;
            UIController.instance.UpdateHud();
            UIController.instance.UpdateVehicles();
        }
        else
        {
            Debug.Log("Too many passengers");
        }
    }

    //Exit a vehicle
    public void ExitVehicle()
    {
        WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.vehicles.Add(party.partyVehicle);
        party.partyVehicle = null;
        party.inVehicle = false;
        UIController.instance.UpdateHud();
        UIController.instance.UpdateVehicles();
    }

    //Refuel a vehicle
    public void RefuelVehicle()
    {
        //Find the inventory slot holding fuel
        int fuelIndex = -1;
        for (int i = 0; i < party.inventory.inventorySlots.Count; i++)
        {
            if (party.inventory.inventorySlots[i].loot.lootName == "Fuel")
            {
                fuelIndex = i;
            }
        }

        //if fuel has been found in the inventory
        if (fuelIndex != -1)
        {
            if (party.inventory.inventorySlots[fuelIndex].slotQty - 1 > 0)
            {
                InventoryController.instance.RemoveItem(fuelIndex, false, party.inventory);
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

    //Remove vehicle
    public void ClearVehicle()
    {
        EncounterController.instance.AddToStatus("Your party hands over the vehicle and your ambushers drive away.");
        EncounterController.instance.StatusStringBuilder();
        UIController.instance.CloseEncounterPrompt();
        party.inVehicle = false;
        party.partyVehicle = null;
        UIController.instance.UpdateHud();
        UIController.instance.UpdateVehicles();
    }

    //recalculate the party attack
    public void CalculatePartyAttack()
    {
        party.partyAttack = 0;
        for (int i = 0; i < party.partySurvivors.Count; i++)
        {
            party.partyAttack += party.partySurvivors[i].attack;
        }
    }

    //recalculate the party threat level based on party size and any trait effects
    public void CalculatePartyThreatLevel()
    {
        int partySize = party.partySurvivors.Count;

        //Loop through each survivor, check if they have a threat trait and add it to the score, then half that

        party.partyThreatLevel = Mathf.RoundToInt(partySize / 2);
    }

    //Return random item
    public Loot RandomItem(float lootChance)
    {
        Loot randomItem = new Loot();

        //get current biome
        string currentBiome = WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.biome;
        Debug.Log("Scavenging  - Biome: " + currentBiome + " - lootChance: " + lootChance);

        //return all items for that biome and up to that rarity
        List<Loot> biomeItems = new List<Loot>();

        for (int i = 0; i < ConfigController.instance.loot.loot.Count; i++)
        {
            if (ConfigController.instance.loot.loot[i].lootBiome == currentBiome && ConfigController.instance.loot.loot[i].lootRarity <= lootChance)
            {
                biomeItems.Add(ConfigController.instance.loot.loot[i]);
            }
        }

        //select a random item from that new list
        randomItem = biomeItems[Random.Range(0, biomeItems.Count - 1)];
        return randomItem;
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
        float lootChance = Random.Range(0.5f, 1f);

        for (int i = 0; i < 6; i++)
        {
            yield return new WaitForSeconds(1f);
            WorldController.instance.AdvanceTime(minutes);

            //calculate threat level
            int threatLevel = WorldController.instance.CalculateThreatLevel(true, null);
            int threatChance = 0; // Random.Range(0, 10);

            if (threatChance > threatLevel)
            {
                AmbushController.instance.SetupAmbush(false, false, true, null);
                //Display a status

                StopCoroutine("ScavengeCoroutine");
            }
            else
            {
                //Add random loot or end the scavenging with an encounter or an ambush
                //Call inventory pickup method - add item to existing slot or create new slot (pass across all the items details)            
                int lootQty = 0;
                Loot randomItem = RandomItem(lootChance);

                if (randomItem.lootType == "WeaponRanged" || randomItem.lootType == "WeaponMelee")
                {
                    lootQty = 1;
                }
                else
                {
                    lootQty = Random.Range(1, 3);
                }

                //AddItem(randomItem.lootName, randomItem.lootDesc, randomItem.lootType, randomItem.lootTypeVal, randomItem.lootRarity, randomItem.lootWeight, randomItem.lootValue, lootQty, randomItem.lootBiome);
                InventoryController.instance.AddItem(randomItem, lootQty, party.inventory);
                //Update the status text to state what was picked up
                EncounterController.instance.AddToStatus("Picked up: " + randomItem.lootName + " x" + lootQty);
                EncounterController.instance.StatusStringBuilder();
                //WorldController.instance.AdvanceTime(15f);
                UIController.instance.UpdateHud();
            }            
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

    //Party Threat Level
    public int partyThreatLevel;

    //Party weight based on inventory
    public float partyWeight;

    //List of survivors currently in the party
    public List<Survivor> partySurvivors = new List<Survivor>();

    //Inventory
    public Inventory inventory = new Inventory();

    //Recipes known
    public RecipeList recipesKnown = new RecipeList();

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
}
