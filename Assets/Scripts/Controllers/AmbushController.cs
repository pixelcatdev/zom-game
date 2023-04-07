using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmbushController : MonoBehaviour
{
    public enum TargetMode { Attack, Heal }
    public TargetMode targetMode;
    public List<Enemy> spawnedEnemies;
    public int enemiesKilled;
    public GameObject ambush;
    public GameObject ambushUi;
    public Transform currentMarker;
    public int currentSurvivor;
    public int currentEnemy;

    //new variables
    public Enemy enemy;
    public int enemyTotal;
    public int enemyAttack;
    public int enemyDefense;

    public static AmbushController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!AmbushController.instance)
        {
            AmbushController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    //Initiates an ambush, setting the enemy type and total and loading the relevent screen
    public void SetupAmbush()
    {
        //switch to ambush mode
        GameController.instance.gameMode = GameController.GameMode.ambush;
        Debug.Log("Setting up ambush");

        //Randomise enemy and enemy total
        enemy = ConfigController.instance.enemies.enemies[Random.Range(0, ConfigController.instance.enemies.enemies.Count)];
        enemyTotal = Random.Range(1, 7);
        
        //Call the UI to enable the Ambush screen
        UIController.instance.UpdateAmbush();
        ambush.SetActive(true);
        ambushUi.SetActive(true);
    }

    public void Update()
    {
        if (GameController.instance.gameMode == GameController.GameMode.ambush)
        {
            //AmbushInteraction();
        }
    }

    //Get player input from the action menu
    public void PartyAction(string action)
    {
        if(action == "Attack")
        {           
            //get the players attack and the enemies defense stats
            int attack = PartyController.instance.party.partyAttack + Random.Range(1, 11);
            int defense = enemyDefense + Random.Range(1, 11);

            Debug.Log("Party Attacking: " + attack + " vs Enemy Defending: " + defense);

            //check if the party succeeds in hitting the enemy
            if (attack > defense)
            {
                enemyTotal--;
                //add a status update for the party defeating an enemy
            }
            else
            {
                //add a status update for the party missing an enemy
            }
        }
        else if (action == "Defend")
        {

        }
        else if (action == "UseItem")
        {

        }
        else if (action == "Flee")
        {

        }

        StartCoroutine("PartyActionEnd");
    }

    //Completes the Ambush
    public void CompleteAmbush()
    {
        GameController.instance.gameMode = GameController.GameMode.worldmap;
        ambush.SetActive(false);
        Debug.Log("Ambush completed");
    }  

    IEnumerator PartyActionEnd()
    {
        yield return new WaitForSeconds(2f);

        UIController.instance.UpdateAmbush();

        if (enemyTotal <= 0)
        {
            CompleteAmbush();
        }
        else
        {
            //Disable the player buttons
            ambushUi.SetActive(false);
            StartCoroutine("EnemyAction");
        }        
    }

    IEnumerator EnemyAction()
    {
        yield return new WaitForSeconds(1f);

        //get the players attack and the enemies defense stats
        int attack = enemyAttack + Random.Range(1, 11);
        int defense = PartyController.instance.party.partyDefense + Random.Range(1, 11);

        Debug.Log("Enemy Attacking: " + attack + " vs Party Defending: " + defense);

        //get a random party target
        int randomPartyMember = Random.Range(0, PartyController.instance.party.partySurvivors.Count);
        Debug.Log(PartyController.instance.party.partySurvivors[randomPartyMember].survivorName + " targeted");

        //check if the party succeeds in hitting the enemy
        if (attack > defense)
        {
            if (PartyController.instance.party.partySurvivors.Count > 0)
            {
                //If its a zom attacking, if the randomly targeted survivor isn't yet infected, infect them - otherwise kill them
                if(enemy.enemyName == "Zom")
                {
                    if (PartyController.instance.party.partySurvivors[randomPartyMember].infection == 0)
                    {
                        Debug.Log("Zom attempting to infect");
                        PartyController.instance.party.partySurvivors[randomPartyMember].infection = 1;
                        WorldController.instance.AddLog(PartyController.instance.party.partySurvivors[randomPartyMember].survivorName + " was bitten by a Zom and infected.");
                        //Add a status for the enemy infecting the party member
                        EncounterController.instance.AddToStatus(PartyController.instance.party.partySurvivors[randomPartyMember].survivorName + " has been infected!");
                    }
                    else
                    {
                        Debug.Log("Zom attacking");
                        PartyController.instance.KillSurvivor(randomPartyMember);
                    }
                }
                else
                {
                    PartyController.instance.KillSurvivor(randomPartyMember);
                }
            }
        }
        else
        {
            //add a status update for the party missing an enemy
            EncounterController.instance.AddToStatus(enemy.enemyName + " attacked, but missed");
        }

        //At the end of the enemy action, wait a second and then re-enable the party action menu
        yield return new WaitForSeconds(1f);

        UIController.instance.UpdateAmbush();

        if (PartyController.instance.party.partySurvivors.Count == 0)
        {
            //Game Over
        }
        else
        {
            ambushUi.SetActive(true);
        }        
    }
}
