using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public List<GameObject> worldTileObjects;
    public List<GameObject> urbanTiles;
    public World world = new World();
    public GameObject currentTile;

    public static WorldController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!WorldController.instance)
        {
            WorldController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        SetupWorld();
    }

    public void SetupWorld()
    {
        // Find all game objects with the specified script attached and add them to the list
        MonoBehaviour[] scripts = FindObjectsOfType<WorldTileProps>();
        foreach (MonoBehaviour script in scripts)
        {
            worldTileObjects.Add(script.gameObject);
        }
        Debug.Log(worldTileObjects.Count.ToString() + " tiles found");

        // Sort the list based on the x-axis position of the game objects
        worldTileObjects.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        GameObject[] urbanTilesFound = GameObject.FindGameObjectsWithTag("tileUrban");

        // Loop through the found objects and do something with them
        foreach (GameObject tile in urbanTilesFound)
        {
            // Do something with the object here
            urbanTiles.Add(tile);
        }

        world.worldThreatLevel = 1;
    }

    public GameObject GetRandomUrbanTile()
    {
        GameObject randomUrbanTile = urbanTiles[UnityEngine.Random.Range(0, WorldController.instance.urbanTiles.Count - 1)];

        return randomUrbanTile;
    }

    //Calculates the threat level of the tile based on numerous factors
    public int CalculateThreatLevel(bool calculateForCurrentTile, GameObject targetedTile)
    {
        int party = PartyController.instance.party.partyThreatLevel;
        int world = WorldController.instance.world.worldThreatLevel;

        //Determine time of day threat level
        int timeOfDay = 0;
        if (isNightTime() == true)
        {
            timeOfDay = 5;
        }
        else
        {
            timeOfDay = 1;
        }

        //Calculate the current tile's biome threat level, or the targeted tile's biome threat level based on flag
        int biome = 0;
        if (calculateForCurrentTile == false)
        {
            biome = targetedTile.GetComponent<WorldTileProps>().tileProps.biomeThreatLevel;
        }
        else
        {
            biome = currentTile.GetComponent<WorldTileProps>().tileProps.biomeThreatLevel;
        }

        int calculatedThreatLevel = party + biome + world + timeOfDay;

        return calculatedThreatLevel;
    }

    //Moves time forward by the given value
    public void AdvanceTime(float minutes)
    {
        DateTime newDateTime = DateTime.Parse(world.worldDateTime);
        newDateTime = newDateTime.AddMinutes(minutes);
        world.worldDateTime = newDateTime.ToString();
    }

    //Returns true if it's night
    private bool isNightTime()
    {
        DateTime currentDateTime = DateTime.Parse(world.worldDateTime);

        if (currentDateTime.TimeOfDay >= TimeSpan.FromHours(19) || currentDateTime.TimeOfDay < TimeSpan.FromHours(6))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void UpdateJournalNotes()
    {
        world.journalNotes = UIController.instance.uiNotesInput.text;
    }

    //Adds a new log to the log strings
    public void AddLog(string logString)
    {
        world.logs.Add(world.worldDateTime + ": " + logString);
    }

    //Geta random tile at a given position
    public GameObject GetRandomTile(Transform origin, float radius)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(origin.position, radius, LayerMask.GetMask("WorldTile"));
        GameObject[] tilesInRange = new GameObject[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            tilesInRange[i] = colliders[i].gameObject;
        }
        GameObject randomTile = tilesInRange[UnityEngine.Random.Range(0, tilesInRange.Length)];
        return randomTile;
    }
}

[System.Serializable]
public class World
{
    public string worldDateTime;
    public int worldThreatLevel;
    public int timeOfDayThreatLevel;
    public string journalNotes;
    public List<string> logs;
    //All tiles in the map and their associated properties
    public List<TileData> worldTileData = new List<TileData>();
}

[System.Serializable]
public class TileData
{
    public string name;
    public string biome;
    public int biomeThreatLevel;
    public bool alreadyScavenged;
    public List<Vehicle> vehicles = new List<Vehicle>();
}