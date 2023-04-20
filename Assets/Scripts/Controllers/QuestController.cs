using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestType { fetch, search, escort, hunt }
public enum QuestDifficulty { easy, medium, hard }

public class QuestController : MonoBehaviour
{
    public static QuestController instance;
    public Quests quests;
    public Quest newQuest;
    public float distanceMinEasy;
    public float distanceMaxEasy;
    public float distanceMinMedium;
    public float distanceMaxMedium;
    public float distanceMinHard;
    public float distanceMaxHard;

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
        newQuest.questType = (QuestType)Random.Range(0, 4);
        newQuest.questDifficulty = (QuestDifficulty)Random.Range(0, 3);

        int distance = 0;
        int lootQty = 0;
        int timeLimit = 0;

        switch (newQuest.questDifficulty)
        {
            case QuestDifficulty.easy:
                //get random tile based on min/max distance
                newQuest.questTargetDateTime = WorldController.instance.world.worldDateTime;

                System.DateTime targetDateTime = System.DateTime.Parse(WorldController.instance.world.worldDateTime);
                newQuest.questTargetDateTime = targetDateTime.AddHours(UnityEngine.Random.Range(12, 24)).ToString();

                break;
            case QuestDifficulty.medium:
                break;
            case QuestDifficulty.hard:
                //get random tile based on min/max distance
                break;
            default:
                break;
        }

        string questText = "New Quest: " + newQuest.questType + " (Difficulty: " + newQuest.questDifficulty + ")\n\nQuest details will go here\n\nRewards: xRandom Loot";
        newQuest.questText = questText;
        EncounterController.instance.encounterText = questText;
        UIController.instance.uiEncounter.SetActive(true);
        UIController.instance.UpdateEncounter("Quest");
    }

    public void AddQuest()
    {
        PartyController.instance.party.quests.Add(newQuest);
        UIController.instance.CloseEncounterPrompt();
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
                        for (int x = 0; x < PartyController.instance.party.inventory.inventorySlots.Count; x++)
                        {
                            InventorySlot slot = PartyController.instance.party.inventory.inventorySlots[x];
                            if (slot.loot.lootName == quest.requiredItem && slot.slotQty >= quest.requiredItemQty)
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
    public Inventory questRewards;
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