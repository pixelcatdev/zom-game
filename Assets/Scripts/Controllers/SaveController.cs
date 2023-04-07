using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    public SaveData saveData;

    public static SaveController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!SaveController.instance)
        {
            SaveController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    //Saves game to a binary file in Save Directory
    public void SaveGame()
    {
        // Copy data from PartyController.instance.party TO SaveData party
        saveData.party = PartyController.instance.party;

        //Inventory
        saveData.inventory = PartyController.instance.inventory;

        //Log
        saveData.log = WorldController.instance.log;

        //Clear worldTileData
        WorldController.instance.world.worldTileData.Clear();

        //Copy all the tileprops into tiledata
        foreach (GameObject tile in WorldController.instance.worldTileObjects)
        {
            WorldController.instance.world.worldTileData.Add(tile.GetComponent<WorldTileProps>().tileProps);
        }
        saveData.world = WorldController.instance.world;

        //Write save file
        string json = JsonUtility.ToJson(saveData);
        Debug.Log(json);
        // Write the JSON string to a file
        System.IO.File.WriteAllText(Application.dataPath + "/party.json", json);

        Debug.Log("Game saved to " + Application.dataPath + "/party.json");
    }

    public void LoadGame()
    {
        //Open the file for reading
        string json;
        using (StreamReader reader = new StreamReader(Application.dataPath + "/party.json"))
        {
            json = reader.ReadToEnd();
        }

        JsonUtility.FromJsonOverwrite(json, saveData);

        //Load the party data
        PartyController.instance.party = saveData.party;

        //Inventory
        PartyController.instance.inventory = saveData.inventory;

        //Log
        WorldController.instance.log = saveData.log;

        //Load the world data
        WorldController.instance.world = saveData.world;
        for (int i = 0; i < WorldController.instance.world.worldTileData.Count; i++)
        {
            WorldController.instance.worldTileObjects[i].GetComponent<WorldTileProps>().tileProps = WorldController.instance.world.worldTileData[i];
        }

        //Place the partyObj and the camera at the right tile
        PartyController.instance.partyObj.position = new Vector2(PartyController.instance.party.partyPosX, PartyController.instance.party.partyPosY);
        Camera.main.transform.position = new Vector3(PartyController.instance.partyObj.position.x, PartyController.instance.partyObj.position.y, Camera.main.transform.position.z);

        //Update the UI
        UIController.instance.UpdateHud();

        Debug.Log("Game loaded from " + Application.dataPath + "/party.json");
    }
}

[Serializable]
public class SaveData
{
    public Party party;
    public World world;
    public Inventory inventory;
    public Log log;
}