using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType { fetch, search, escort, hunt }
public enum QuestDifficulty { easy, medium, hard }

public class QuestController : MonoBehaviour
{
    public static QuestController instance;
    public Quests quests;

    // Singleton Initialization
    void Awake()
    {
        if (!QuestController.instance)
        {
            QuestController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    //Randomises a quest type
    public void SetupQuest()
    {
        //Randomise the quest type and difficulty
        Quest newQuest = new Quest();

        //Add the new quest to the quests list
        quests.quests.Add(newQuest);

        newQuest.questType = QuestType.search; //(QuestType)Random.Range(0, 3);
        newQuest.questDifficulty = QuestDifficulty.easy; //(QuestDifficulty)Random.Range(0, 3);

        //set the radius based on the difficulty
        int radiusForTargetTile = 0;

        if(newQuest.questDifficulty == QuestDifficulty.easy)
        {
            radiusForTargetTile = 10;
        }
        else if (newQuest.questDifficulty == QuestDifficulty.medium)
        {
            radiusForTargetTile = 25;
        }
        else if (newQuest.questDifficulty == QuestDifficulty.hard)
        {
            radiusForTargetTile = 50;
        }

        //Get a random target tile within the radius and set the targetX and targetY as its vector
        GameObject targetTile = WorldController.instance.GetRandomTile(PartyController.instance.partyObj, radiusForTargetTile);
        newQuest.questTargetPosX = targetTile.transform.position.x;
        newQuest.questTargetPosY = targetTile.transform.position.y;

        //Randomise the target dateTime is the quest is timed
        newQuest.isTimed = true; // (Random.value > 0.5f);

        if (newQuest.isTimed)
        {
            string currentDateTime = WorldController.instance.world.worldDateTime;
            System.DateTime targetDate = System.DateTime.Parse(currentDateTime);
            int hoursToAdd = 0;
            if(newQuest.questDifficulty == QuestDifficulty.easy)
            {
                hoursToAdd = 24;
            }
            else if (newQuest.questDifficulty == QuestDifficulty.medium)
            {
                hoursToAdd = 12;
            }
            else if (newQuest.questDifficulty == QuestDifficulty.hard)
            {
                hoursToAdd = 6;
            }            
            targetDate = targetDate.AddHours(hoursToAdd);
            newQuest.questTargetDateTime = targetDate.ToString();
        }

        //Randomise the reward based on difficulty
        int lootTotal = 0;
        if (newQuest.questDifficulty == QuestDifficulty.easy)
        {
            lootTotal = 1;
        }
        else if (newQuest.questDifficulty == QuestDifficulty.medium)
        {
            lootTotal = 2;
        }
        else if (newQuest.questDifficulty == QuestDifficulty.hard)
        {
            lootTotal = 3;
        }

        //DEBUG -- Remove later
        lootTotal = 3;
        Debug.Log("Loot total: " + lootTotal);

        //Add rare items to the rewards
        for (int i = 0; i < lootTotal; i++)
        {
            Loot newLoot = PartyController.instance.RandomItem(9);
            Debug.Log("adding " + newLoot.lootName + " as a reward");
            newQuest.questRewards = new List<InventorySlot>();
            newQuest.questRewards.Add(new InventorySlot());
            newQuest.questRewards[quests.quests.Count - 1].lootName = newLoot.lootName;
            newQuest.questRewards[quests.quests.Count - 1].lootQty = lootTotal;
        }

        //Find quest specifics
        if (newQuest.questType == QuestType.search)
        {
            SetupFind(newQuest);
        }
        //Fetch quest specifics
        //Escort quest specifics
        //Hunt quest specifics
    }

    //Setup Find quest specifics
    private void SetupFind(Quest newQuest)
    {
        //get the nearest urban tile to the targetX and targetY
        //get the relative compass direction of the tile 
        //set the quest text with approx descriptions of where to go
    }

    //Checks if a quest can be completed by looping through all of the quests and evaluating win conditions for each
    public void CheckQuests()
    {
        for (int i = 0; i < quests.quests.Count; i++)
        {
            bool questFailed = false;
            Quest quest = quests.quests[i];
            Vector2 partyPos = new Vector2(PartyController.instance.party.partyPosX, PartyController.instance.party.partyPosY);
            Vector2 questTargetPos = new Vector2(quest.questTargetPosX, quest.questTargetPosY);
            Vector2 questOriginPos = new Vector2(quest.questPosX, quest.questPosY);
            //has timer expired, if so, fail quest
            if (quest.isTimed == true)
            {
                System.DateTime currentDateTime = System.DateTime.Parse(WorldController.instance.world.worldDateTime);
                System.DateTime targetDateTime = System.DateTime.Parse(quest.questTargetDateTime);

                if(currentDateTime > targetDateTime)
                {
                    questFailed = true;
                    FailQuest(i);
                }
            }

            //Check if each quest type is fulfilled
            if (questFailed == false)
            {
                //Search and Escort - are they at the correct target location?
                if (quest.questType == QuestType.search || quest.questType == QuestType.escort)
                {
                    if (partyPos == questTargetPos)
                    {
                        CompleteQuest(i);
                    }
                }

                //Fetch - have they returned to the quest location and have they got the required items?
                else if (quest.questType == QuestType.fetch)
                {
                    if (partyPos == questOriginPos)
                    {
                        //Check if they've got the required items and qty
                        for (int x = 0; x < PartyController.instance.inventory.inventorySlots.Count; x++)
                        {
                            InventorySlot slot = PartyController.instance.inventory.inventorySlots[x];
                            if (slot.lootName == quest.requiredItem && slot.lootQty >= quest.requiredItemQty)
                            {
                                CompleteQuest(i);
                            }
                        }
                    }
                }

                //Hunt - have they reached the correct target location - if so, trigger the Ambush with the given enemies
                else if (quest.questType == QuestType.hunt)
                {
                    if (partyPos == questOriginPos)
                    {
                        //Trigger Ambush with quest.questEnemyType and difficulty
                    }
                }
            }

        }
    }

    //Completes a quest
    public void CompleteQuest(int questIndex)
    {
        //Issue reward
        //Display status update
        //Add relevant log entry
        switch (quests.quests[questIndex].questType)
        {
            case QuestType.fetch:
                WorldController.instance.AddLog("Your party has completed a FETCH quest");
                break;
            case QuestType.search:
                WorldController.instance.AddLog("Your party has completed a SEARCH quest");
                break;
            case QuestType.escort:
                WorldController.instance.AddLog("Your party has completed an ESCORT quest");
                break;
            case QuestType.hunt:
                WorldController.instance.AddLog("Your party has completed a HUNT quest");
                break;
            default:
                break;
        }

        Debug.Log("quest completed!");
        quests.quests.RemoveAt(questIndex);
    }

    //Fail a quest
    public void FailQuest(int questIndex)
    {
        //if it's an escort quest, remove the survivor from the party (if they haven't died already)
        Debug.Log("quest failed");
        quests.quests.RemoveAt(questIndex);
    }

    //Abandon a quest
    public void AbandonQuest(int questIndex)
    {
        //if it's an escort quest, remove the survivor from the party (if they haven't died already)
        Debug.Log("quest abandoned");
        quests.quests.RemoveAt(questIndex);
        UIController.instance.UpdateQuests();
    }
}

[System.Serializable]
public class Quests
{
    public List<Quest> quests;
}

[System.Serializable]
public class Quest
{
    public QuestType questType;
    public QuestDifficulty questDifficulty;
    public string questText;
    public List<InventorySlot> questRewards;
    public float questPosX;
    public float questPosY;
    public float questTargetPosX;
    public float questTargetPosY;
    public bool isTimed;
    public string questTargetDateTime;
    public int questSurvivorIndex;
    public string requiredItem;
    public int requiredItemQty;
    public string ambushEnemyType;
    public int ambushDifficulty;
}