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

    public Survivor newSurvivor = new Survivor();

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
        int randomInt = 1; // Random.Range(0, 5);
        Debug.Log("Encounter chance: " + randomInt);

        //find a survivor
        if (randomInt == 1)
        {
            Party party = PartyController.instance.party;

            if (party.partySurvivors.Count < 6)
            {
                //Randomise whether its a rescue or a request
                Debug.Log("Survivor would like to join");

                //Randomise the survivors details
                //newSurvivor = new Survivor();

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

                //Randomise 10% chance of infection
                float infected = Random.Range(0f, 1f);
                if (infected > 0.9f)
                {
                    newSurvivor.infection = Random.Range(1, 5);
                }
                encounterText = "Your party comes across another survivor.\n\n" + newSurvivor.survivorName + " would like to join.";
                UIController.instance.uiEncounter.SetActive(true);
                UIController.instance.UpdateEncounter();
                //PartyController.instance.AddSurvivor(newSurvivor);
            }
        }
        //find a vehicle
        else if (randomInt == 2)
        {
            //Add a vehicle to the tile if the player is in an urban or motorway tile, output the details to the status bar
            WorldTileProps currentTile = WorldController.instance.currentTile.GetComponent<WorldTileProps>();
            if (currentTile.tileProps.biome == "Motorway" || currentTile.tileProps.biome == "Urban")
            {
                Vehicle randomVehicle = ConfigController.instance.vehicles.vehicles[Random.Range(0, ConfigController.instance.vehicles.vehicles.Count - 1)];
                WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.vehicles.Add(randomVehicle);
                Vehicle spawnedVehicle = WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.vehicles[WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.vehicles.Count-1];
                spawnedVehicle.vehicleFuel = Random.Range(0, spawnedVehicle.vehicleMaxFuel / 4);
                spawnedVehicle.vehicleHp = Random.Range(0, 11);

                //Add status text with vehicle details
                statusStrings.Add(randomVehicle.vehicleName.ToUpper() + " found ");
                //partyStatus += " " + randomVehicle.vehicleName.ToUpper() + " found ";
            }            
        }
        //trigger encounter
        else if (randomInt == 3)
        {

        }
        //trigger ambush
        else if (randomInt == 4)
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
    }

    //Start an ambush combat sequence
    public void Ambush()
    {
        //randomise the enemy
        Enemy randomEnemy = ConfigController.instance.enemies.enemies[Random.Range(0, ConfigController.instance.enemies.enemies.Count - 1)];

        Debug.Log("Players are attacked by a " + randomEnemy.enemyName);

        AmbushController.instance.SetupAmbush();

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
}
