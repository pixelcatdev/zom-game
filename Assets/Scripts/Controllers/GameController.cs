using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum GameMode { worldmap, ambush, menu }
    public GameMode gameMode;
    public static GameController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!GameController.instance)
        {
            GameController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        NewGame();
    }

    public void NewGame()
    {
        //Start player at random urban tile
        GameObject randomStartingTile = WorldController.instance.GetRandomUrbanTile();
        PartyController.instance.partyObj.transform.position = randomStartingTile.transform.position;
        PartyController.instance.party.partyPosX = randomStartingTile.transform.position.x;
        PartyController.instance.party.partyPosY = randomStartingTile.transform.position.y;
        WorldController.instance.currentTile = randomStartingTile;
        Camera.main.transform.position = new Vector3(randomStartingTile.transform.position.x, randomStartingTile.transform.position.y, Camera.main.transform.position.z);

        //Add a random survivor
        //PartyController.instance.AddSurvivor();
        PartyController.instance.party.partyEnergy = 100;
        //Randomise starting location
        DateTime startDateTime = new System.DateTime(2025, 6, 6, 7, 0, 0);
        WorldController.instance.world.worldDateTime = startDateTime.ToString();

        gameMode = GameMode.worldmap;
    }
}
