using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController instance;

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
}

[System.Serializable]
public class Quest
{
    public enum QuestType { fetch, find, escort }
    public QuestType type;
    public string questText;
    public List<InventorySlot> questRewards;
    public GameObject questGiver;
    public GameObject questLocation;
    public string questTargetDate;
}