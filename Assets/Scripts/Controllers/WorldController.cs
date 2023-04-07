using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{    
    public List<GameObject> worldTileObjects;
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
    }

    //Moves time forward by the given value
    public void AdvanceTime(float minutes)
    {
        DateTime newDateTime = DateTime.Parse(world.worldDateTime);
        newDateTime = newDateTime.AddMinutes(minutes);
        world.worldDateTime = newDateTime.ToString();
    }

    public void UpdateJournalNotes()
    {
        world.journalNotes = UIController.instance.uiNotesInput.text;
    }

    //Adds a new log to the log strings
    public void AddLog(string logString)
    {
        world.logs.Add(world.worldDateTime +": " + logString);
    }
}

[System.Serializable]
public class World
{
    public string worldDateTime;
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
    public float threatLevel;
    public bool alreadyScavenged;
    public GameObject alreadyScavengedIcon;
    public List<Vehicle> vehicles = new List<Vehicle>();
}