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
        //Add a random survivor
        //PartyController.instance.AddSurvivor();
        PartyController.instance.party.partyEnergy = 100;
        //Randomise starting location
        DateTime startDateTime = new System.DateTime(2025, 6, 6, 7, 0, 0);
        WorldController.instance.world.worldDateTime = startDateTime.ToString();

        gameMode = GameMode.worldmap;
    }
}
