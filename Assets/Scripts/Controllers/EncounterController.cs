using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterController : MonoBehaviour
{
    public static EncounterController instance;

    //Status text generated on entering a new tile
    public string partyStatus;
    public List<string> statusStrings;


    public string encounterText;

    public Survivor newSurvivor;// = new Survivor();

    // Singleton Initialization
    void Awake()
    {
        if (!EncounterController.instance)
        {
            EncounterController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    //Randomise chance of an encounter when moving
    public void RandomEncounter()
    {
        int randomInt = 7; // Random.Range(0, 20);
        Debug.Log("Encounter chance: " + randomInt);

        //survivor joins if there's room in the party and if there's room in a vehicle
        if (randomInt == 1)
        {
            Party party = PartyController.instance.party;

            if (party.partySurvivors.Count < 6)
            {
                //Randomise whether its a rescue or a request
                //Generate a new survivor
                GenerateNewSurvivor();

                encounterText = "Your party comes across another survivor.\n\n" + newSurvivor.survivorName + " would like to join.";
                UIController.instance.uiEncounter.SetActive(true);
                UIController.instance.UpdateEncounter("SurvivorJoins");
                //PartyController.instance.AddSurvivor(newSurvivor);
            }
        }

        //survivor rescue
        else if (randomInt == 2)
        {
            //Generate a new survivor
            GenerateNewSurvivor();

            encounterText = "A survivor is under attack and needs your help (" + newSurvivor.survivorName + ")\n\nWill you help?";
            UIController.instance.uiEncounter.SetActive(true);
            //Randomise the enemy
            UIController.instance.UpdateEncounter("SurvivorRescue");
        }
        //hold-up
        else if (randomInt == 3)
        {
            //randomise for either vehicle or inventory
            //if inventory, if they dont have anything, go straight into combat
            if (PartyController.instance.party.inVehicle == true)
            {
                encounterText = "Enemies ambush your vehicle. They demand you turn over it over, or they'll start attacking.\n\nDo you surrender your vehicle?";
                UIController.instance.UpdateEncounter("HoldupVehicle");
            }
            else
            {
                if (PartyController.instance.party.inventory.inventorySlots.Count > 0)
                {
                    encounterText = "Enemies ambush your party. They demand you turn over your entire inventory (including anything equipped), or they'll start attacking.\n\nDo you surrender your inventory?";
                    UIController.instance.UpdateEncounter("HoldupInventory");
                }
                else
                {
                    AmbushController.instance.SetupAmbush(false, false, true, null);
                }
            }

            UIController.instance.uiEncounter.SetActive(true);
            //Randomise the enemy type for the ambush to a raider or rogue soldier

        }
        //trader
        else if (randomInt == 4)
        {

        }
        //find a vehicle
        else if (randomInt == 5)
        {
            //Add a vehicle to the tile if the player is in an urban or motorway tile and there is no other vehicle currently there, output the details to the status bar
            WorldTileProps currentTile = WorldController.instance.currentTile.GetComponent<WorldTileProps>();
            if (currentTile.tileProps.biome == "Motorway" || currentTile.tileProps.biome == "Urban" && currentTile.tileProps.vehicles.Count == 0)
            {
                Vehicle randomVehicle = ConfigController.instance.vehicles.vehicles[Random.Range(0, ConfigController.instance.vehicles.vehicles.Count - 1)];
                WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.vehicles.Add(randomVehicle);
                Vehicle spawnedVehicle = WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.vehicles[WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.vehicles.Count - 1];
                spawnedVehicle.vehicleFuel = Random.Range(0, spawnedVehicle.vehicleMaxFuel / 4);
                spawnedVehicle.vehicleHp = Random.Range(0, 11);

                //Add status text with vehicle details
                statusStrings.Add(randomVehicle.vehicleName.ToUpper() + " found ");
            }
        }
        //trigger ambush
        else if (randomInt == 6)
        {
            //calculate threat level
            int threatLevel = WorldController.instance.CalculateThreatLevel(true, null);
            int threatChance = Random.Range(0, 20);
            Debug.Log("Ambush Chance: " + threatChance);
            //if threat level > threatChance trigger ambush
            if (threatChance > threatLevel)
            {
                if (PartyController.instance.party.inVehicle == false)
                {
                    Ambush();
                }
            }
        }
        //trigger quest
        else if (randomInt == 7)
        {
            QuestController.instance.SetupQuest();
        }
    }

    //Clears the party inventory if it's a holdup encounter (I could stick this in encounter)
    public void EncounterHoldup()
    {
        InventoryController.instance.ClearInventory(PartyController.instance.party.inventory);
        EncounterController.instance.AddToStatus("Your party hands over all inventory and the attackers let you move on.");
        EncounterController.instance.StatusStringBuilder();
        UIController.instance.CloseEncounterPrompt();
    }

    //Start an ambush combat sequence
    public void Ambush()
    {
        //randomise the enemy
        Enemy randomEnemy = ConfigController.instance.enemies.enemies[Random.Range(0, ConfigController.instance.enemies.enemies.Count - 1)];

        Debug.Log("Players are attacked by a " + randomEnemy.enemyName);

        AmbushController.instance.SetupAmbush(false, false, true, null);

    }

    //Adds a new line of status text
    public void AddToStatus(string status)
    {
        statusStrings.Add(status);
    }

    //Generates a status event (just generates transport at the moment)
    public void GenerateStatus()
    {
        float eventChance = UnityEngine.Random.Range(0f, 1f);
        //Get and store the current tile and its TileProps
        GameObject currentTile = WorldController.instance.currentTile;
        WorldTileProps currentTileProps = currentTile.GetComponent<WorldTileProps>();

        if (eventChance > 0.5f)
        {
            //MapController.instance.AddStatusEntry(StatusStringBuilder());
            //EventStatus(UnityEngine.Random.Range(0, 8));
        }
    }

    //Loops through the list of Status updates and sets the party status to whatever they're set to
    public void StatusStringBuilder()
    {
        //partyStatus = "Your party " + StatusVerb() + " " + StatusNoun();
        for (int i = 0; i < statusStrings.Count; i++)
        {
            if (i == 0)
            {
                partyStatus = statusStrings[i];
            }
            else
            {
                partyStatus += ". " + statusStrings[i];
            }
        }
        UIController.instance.UpdateHud();
        statusStrings.Clear();
    }

    //Minor function to procure a random verb based on frequency
    public string StatusVerb()
    {
        //If in a vehicle
        if (PartyController.instance.party.inVehicle)
        {
            return ConfigController.instance.verbsList.verbs[0].driving[UnityEngine.Random.Range(0, ConfigController.instance.verbsList.verbs[0].driving.Count)].verb;
        }
        else
        {
            return ConfigController.instance.verbsList.verbs[0].walking[UnityEngine.Random.Range(0, ConfigController.instance.verbsList.verbs[0].walking.Count)].verb;
        }
    }

    //Minor function to procure a random noun based on current tile biome and frequency
    public string StatusNoun()
    {
        GameObject currentTile = WorldController.instance.currentTile;
        WorldTileProps currentTileProps = currentTile.GetComponent<WorldTileProps>();
        string biome = currentTileProps.tileProps.biome.ToString();
        Debug.Log("biome: " + biome);
        string noun = null;

        if (biome == "Woodland")
        {
            noun = ConfigController.instance.nounsList.nouns[0].woodland[UnityEngine.Random.Range(0, ConfigController.instance.nounsList.nouns[0].woodland.Count)].noun;
        }
        else if (biome == "Urban")
        {
            noun = ConfigController.instance.nounsList.nouns[0].urban[UnityEngine.Random.Range(0, ConfigController.instance.nounsList.nouns[0].urban.Count)].noun;
        }
        else if (biome == "Motorway")
        {
            noun = ConfigController.instance.nounsList.nouns[0].motorway[UnityEngine.Random.Range(0, ConfigController.instance.nounsList.nouns[0].motorway.Count)].noun;
        }
        else if (biome == "Countryside")
        {
            noun = ConfigController.instance.nounsList.nouns[0].countryside[UnityEngine.Random.Range(0, ConfigController.instance.nounsList.nouns[0].countryside.Count)].noun;
        }
        else if (biome == "Mountain")
        {
            noun = ConfigController.instance.nounsList.nouns[0].mountain[UnityEngine.Random.Range(0, ConfigController.instance.nounsList.nouns[0].mountain.Count)].noun;
        }

        if (noun == null)
        {
            noun = "[missing value]";
        }

        return noun;
    }

    //Minor function to randomise a frequency value for generating verbs and nouns
    public string randomFrequency()
    {
        float r = UnityEngine.Random.Range(0f, 1f);
        string frequencyLookup = null;
        if (r > 0.9f)
        {
            frequencyLookup = "Very Rare";
        }
        else if (r > 0.75f)
        {
            frequencyLookup = "Rare";
        }
        else
        {
            frequencyLookup = "Common";
        }

        //Debug.Log("frequency: " + frequencyLookup);
        return frequencyLookup;
    }

    //Generate survivor details for encounters
    public void GenerateNewSurvivor()
    {
        //Randomise the survivors details
        newSurvivor = new Survivor();

        //Randomise gender
        int gender = Random.Range(0, 2);

        //Randomise name based on gender
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

        //randomise their HP
        newSurvivor.survivorHp = Random.Range(2, 11);

        //Randomise 10% chance of infection
        float infected = Random.Range(0f, 1f);
        if (infected > 0.9f)
        {
            newSurvivor.infection = Random.Range(1, 5);
        }
    }




}
