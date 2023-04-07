using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{    
    public List<GameObject> worldTileObjects;
    public World world = new World();
    public Log log = new Log();
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
}

[System.Serializable]
public class World
{
    public string worldDateTime;

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

[System.Serializable]
public class Log
{
    public List<LogEntry> logEntries = new List<LogEntry>();
}

[System.Serializable]
public class LogEntry
{
    public string logDateTime;
    public string logEntry;
}