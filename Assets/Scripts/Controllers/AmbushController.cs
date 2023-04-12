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

    public bool isEncounterAmbush;
    public bool isQuest;
    public bool wonAmbush;

    public int partyAttackRoll;
    public int enemyAttackRoll;

    //new variables
    public Enemy enemy;
    public int enemyTotal;
    public int enemyAttack;
    public int enemyDefense;

    private float cameraZoom;

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
    public void SetupAmbush(bool isEncounter, bool isQuest, bool randomEnemy, string chosenEnemy)
    {
        if(isEncounter == true)
        {
            isEncounterAmbush = true;
        }

        if (isQuest == true)
        {
            isQuest = true;
        }

        //switch to ambush mode
        GameController.instance.gameMode = GameController.GameMode.ambush;
        Debug.Log("Setting up ambush");

        //Get the camera's zoom so it can be restored after completing, then set the zoom to the ambush screen ratio
        cameraZoom = Camera.main.GetComponent<Camera>().orthographicSize;
        Camera.main.GetComponent<Camera>().orthographicSize = 5f;

        //Randomise enemy and enemy total
        //Set the enemy type at random 
        if (randomEnemy == true)
        {
            enemy = ConfigController.instance.enemies.enemies[0]; //ConfigController.instance.enemies.enemies[Random.Range(0, ConfigController.instance.enemies.enemies.Count)];
        }
        else
        {
            //crap way of hardcoding this atm
            enemy = ConfigController.instance.enemies.enemies[2];
        }        
        enemyTotal = Random.Range(1, 7);

        //Status update
        EncounterController.instance.statusStrings.Clear();
        if (enemyTotal > 1)
        {
            EncounterController.instance.AddToStatus("Your party is ambushed by a group of " + enemy.enemyName);
        }
        else
        {
            EncounterController.instance.AddToStatus("Your party is ambushed by a " + enemy.enemyName);
        }

        //Calculate the party attack
        PartyController.instance.CalculatePartyAttack();
        CalculateEnemyAttack();

        //Update the status string
        EncounterController.instance.StatusStringBuilder();
        UIController.instance.UpdateHud();

        partyAttackRoll = 0;
        enemyAttackRoll = 0;

        //Call the UI to enable the Ambush screen
        UIController.instance.UpdateAmbush();
        ambush.SetActive(true);
        ambushUi.SetActive(true);
    }

    //Recalculates the enemy attack
    private void CalculateEnemyAttack()
    {
        enemyAttack = enemy.enemyAttack * enemyTotal;
    }

    //Get player input from the action menu
    public void PartyAction(string action)
    {
        if(action == "Attack")
        {           
            //get the players attack and the enemies defense stats
            partyAttackRoll = PartyController.instance.party.partyAttack + Random.Range(1, 20);
            //int defense = enemyDefense + Random.Range(1, 11);
            enemyAttackRoll = (enemyAttack * enemyTotal) + Random.Range(1, 20);

            Debug.Log("Party Attacking: " + partyAttackRoll + " vs Enemy Attack: " + enemyAttackRoll);

            //check if the party succeeds in hitting the enemy
            if (partyAttackRoll > enemyAttackRoll)
            {
                enemyTotal--;
                //add a status update for the party defeating an enemy

                EncounterController.instance.AddToStatus("Your party hits the enemy");
                EncounterController.instance.StatusStringBuilder();
            }
            else
            {
                //add a status update for the party missing an enemy
                EncounterController.instance.AddToStatus("Your party misses");
                EncounterController.instance.StatusStringBuilder();
            }

            //Update the attack values
            UIController.instance.uiAmbushPartyRoll.text = AmbushController.instance.partyAttackRoll.ToString();
            UIController.instance.uiAmbushPartyVal.text = "+" + PartyController.instance.party.partyAttack.ToString();
            UIController.instance.uiAmbushEnemyRoll.text = AmbushController.instance.enemyAttackRoll.ToString();
            UIController.instance.uiAmbushEnemyVal.text = "+" + AmbushController.instance.enemyAttack.ToString();

            StartCoroutine("PartyActionEnd");
        }
        else if (action == "Defend")
        {

        }
        else if (action == "UseItem")
        {

        }
        else if (action == "Flee")
        {
            //Attempt to flee via RNG
            bool partyEscapes = (Random.value > 0.5f);
            if(partyEscapes == true)
            {
                EncounterController.instance.AddToStatus("Your party manages to flee your ambushers.");
                CompleteAmbush();
            }
            else
            {
                EncounterController.instance.AddToStatus("Your party fails to outrun its ambushers.");
                EncounterController.instance.StatusStringBuilder();
                StartCoroutine("PartyActionEnd");
            }            
        }
    }

    //Completes the Ambush
    public void CompleteAmbush()
    {
        //If triggered by quest or encounter, give some sort of rewarded output
        if(wonAmbush == true)
        {
            if (isEncounterAmbush == true)
            {
                string survivorName = EncounterController.instance.newSurvivor.survivorName;

                //Survivor joins, but if not enough room, they give a random inventory item
                if (PartyController.instance.party.partySurvivors.Count < 6)
                {
                    PartyController.instance.AddSurvivor(EncounterController.instance.newSurvivor);
                    EncounterController.instance.AddToStatus(survivorName + " has been rescued. They join your party.");
                }
                else
                {
                    Loot randomItem = PartyController.instance.RandomItem(1f);
                    int lootQty = Random.Range(1, 3);
                    PartyController.instance.AddItem(randomItem.lootName, randomItem.lootDesc, randomItem.lootType, randomItem.lootTypeVal, randomItem.lootRarity, randomItem.lootWeight, randomItem.lootValue, lootQty, randomItem.lootBiome);
                    EncounterController.instance.AddToStatus(survivorName + " has been rescued. They offer you what little they have (x" + lootQty + " " + randomItem.lootName + ")");
                }
            }
        }       

        //Set the camera's zoom back again
        Camera.main.GetComponent<Camera>().orthographicSize = cameraZoom;

        EncounterController.instance.AddToStatus("Your party survives the ambush");
        EncounterController.instance.StatusStringBuilder();

        GameController.instance.gameMode = GameController.GameMode.worldmap;
        ambush.SetActive(false);
        //isEncounterAmbush = false;
        isQuest = false;
        wonAmbush = false;
    }  

    IEnumerator PartyActionEnd()
    {
        yield return new WaitForSeconds(2f);

        UIController.instance.UpdateAmbush();

        if (enemyTotal <= 0)
        {
            wonAmbush = true;
            CompleteAmbush();
        }
        else
        {
            //Disable the player buttons
            //ambushUi.SetActive(false);
            PartyController.instance.CalculatePartyAttack();
            CalculateEnemyAttack();
            StartCoroutine("EnemyAction");
        }        
    }

    IEnumerator EnemyAction()
    {
        yield return new WaitForSeconds(1f);

        //get the players attack and the enemies defense stats
        enemyAttackRoll = (enemyAttack * enemyTotal) + Random.Range(1, 20);
        partyAttackRoll = PartyController.instance.party.partyDefense + Random.Range(1, 20);

        Debug.Log("Enemy Attacking: " + enemyAttackRoll + " vs Party Attack: " + partyAttackRoll);

        //get a random party target
        int randomPartyMember = Random.Range(0, PartyController.instance.party.partySurvivors.Count);
        Debug.Log(PartyController.instance.party.partySurvivors[randomPartyMember].survivorName + " targeted");

        //check if the party succeeds in hitting the enemy
        if (enemyAttackRoll > partyAttackRoll)
        {
            if (PartyController.instance.party.partySurvivors.Count > 0)
            {
                //If its a zom attacking, if the randomly targeted survivor isn't yet infected, infect them - otherwise kill them
                if(enemy.enemyName == "Zom" && PartyController.instance.party.partySurvivors[randomPartyMember].infection == 0)
                {
                    Debug.Log("Zom attempting to infect");
                    PartyController.instance.party.partySurvivors[randomPartyMember].infection = 1;
                    WorldController.instance.AddLog(PartyController.instance.party.partySurvivors[randomPartyMember].survivorName + " was " + enemy.enemyVerb + " by a " + enemy.enemyName + " has been infected!");
                    //Add a status for the enemy infecting the party member
                    EncounterController.instance.AddToStatus(PartyController.instance.party.partySurvivors[randomPartyMember].survivorName + " was " + enemy.enemyVerb + " by a " + enemy.enemyName + " has been infected!");
                    EncounterController.instance.StatusStringBuilder();                    
                }
                else
                {
                    //Output status
                    EncounterController.instance.AddToStatus(PartyController.instance.party.partySurvivors[randomPartyMember].survivorName + " was " + enemy.enemyVerb + " by a " + enemy.enemyName + " and died from their injuries.");
                    EncounterController.instance.StatusStringBuilder();

                    WorldController.instance.AddLog(PartyController.instance.party.partySurvivors[randomPartyMember].survivorName + " was " + enemy.enemyVerb + " by a " + enemy.enemyName + " and died from their injuries.");
                    PartyController.instance.KillSurvivor(randomPartyMember);                    
                }
            }
        }
        else
        {
            //add a status update for the party missing an enemy
            EncounterController.instance.AddToStatus(enemy.enemyName + " attacked, but missed");
            EncounterController.instance.StatusStringBuilder();
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
            PartyController.instance.CalculatePartyAttack();
            CalculateEnemyAttack();
        }        
    }
}
