using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI hud_date;
    public TextMeshProUGUI hud_time;
    public TextMeshProUGUI hud_biome;
    public TextMeshProUGUI hud_tileName;
    public TextMeshProUGUI hud_partyEnergy;
    public List<TextMeshProUGUI> hud_partySurvivors;
    public TextMeshProUGUI hud_partyVehicleType;
    public TextMeshProUGUI hud_partyVehicleFuel;
    public TextMeshProUGUI hud_statusText;

    public GameObject uiInventory;
    public Transform uiInventorySlots;
    public GameObject uiInventorySlotObj;

    public GameObject uiVehicles;
    public Transform uiVehicleSlots;
    public GameObject uiVehicleSlotObj;
    public TextMeshProUGUI uiPartyVehicleName;
    public TextMeshProUGUI uiPartyVehicleFuel;
    public TextMeshProUGUI uiPartyVehicleHp;
    public TextMeshProUGUI uiPartyVehiclePassengers;
    public TextMeshProUGUI uiPartyVehicleCarry;
    public TextMeshProUGUI uiPartyVehicleExitButton;
    public TextMeshProUGUI uiPartyVehicleRefuelButton;
    public TextMeshProUGUI uiPartyVehicleRepairButton;

    public GameObject uiParty;
    public Transform uiPartySlots;
    public GameObject uiPartySlotObj;
    public GameObject uiWeapons;
    public Transform uiWeaponsSlots;
    public GameObject uiWeaponSlotObj;

    public GameObject uiJournal;
    public TextMeshProUGUI uiJournalLogs;
    public GameObject uiNotes;
    public TMP_InputField uiNotesInput;

    public GameObject uiQuests;
    public Transform uiQuestsSlots;
    public GameObject uiQuestSlotObj;

    public List<GameObject> survivorObjs;
    public List<GameObject> enemyObjs;

    public static UIController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!UIController.instance)
        {
            UIController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        UpdateHud();
    }

    private void Update()
    {
        //Escape key closes all menus
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAllMenus();
        }
    }

    public void OpenMenu(string targetMenu)
    {
        CloseAllMenus();
        if (targetMenu == "Inventory")
        {
            uiInventory.SetActive(true);
            UpdateInventory();
        }
        else if (targetMenu == "Vehicles")
        {
            uiVehicles.SetActive(true);
            UpdateVehicles();
        }
        else if (targetMenu == "Party")
        {
            uiParty.SetActive(true);
            UpdateParty();
        }
        else if (targetMenu == "Journal")
        {
            uiJournal.SetActive(true);
            UpdateJournal();
        }
        else if (targetMenu == "Quests")
        {
            uiQuests.SetActive(true);
            UpdateQuests();
        }
    }

    //Closes all menus when returning to the worldmap
    public void CloseAllMenus()
    {
        uiInventory.SetActive(false);
        uiVehicles.SetActive(false);
        uiParty.SetActive(false);
        uiWeapons.SetActive(false);
        uiJournal.SetActive(false);
        uiNotes.SetActive(false);
        uiQuests.SetActive(false);
    }

    //Updates the Inventory menu
    public void UpdateInventory()
    {
        //loop through the inventory and instantiate the inv slot under the Slots Transform, setting the text of each object as its dropped
        foreach (Transform child in uiInventorySlots)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < PartyController.instance.inventory.inventorySlots.Count; i++)
        {
            InventorySlot slot = PartyController.instance.inventory.inventorySlots[i];

            //Set the slot loot name, but flag it as equipped if it's equipped
            GameObject newSlot = Instantiate(uiInventorySlotObj, uiInventorySlots);
            if (slot.lootEquipped == true)
            {
                for (int x = 0; x < PartyController.instance.party.partySurvivors.Count; x++)
                {
                    if (PartyController.instance.party.partySurvivors[x].equippedWeaponIndex == i)
                    {
                        newSlot.GetComponent<UiInventorySlotProps>().uiItemName.text = "[E] " + slot.lootName;
                    }
                }
            }
            else
            {
                newSlot.GetComponent<UiInventorySlotProps>().uiItemName.text = slot.lootName;
            }
            newSlot.GetComponent<UiInventorySlotProps>().uiQty.text = "x" + slot.lootQty;
            newSlot.GetComponent<UiInventorySlotProps>().uiWeight.text = slot.lootWeight + "(lb)";
            newSlot.GetComponent<UiInventorySlotProps>().uiTotalWeight.text = slot.lootQty * slot.lootWeight + "(lb)";

            int slotId = i;
            //If the slot is food or medicine, activate the Use Item button
            if (slot.lootType == "Food")
            {
                newSlot.GetComponent<UiInventorySlotProps>().uiUseItemButton.gameObject.SetActive(true);
                newSlot.GetComponent<UiInventorySlotProps>().uiUseItemButton.GetComponent<Button>().onClick.AddListener(delegate { PartyController.instance.FeedParty(slotId, slot.lootTypeVal); });
            }
            else if (slot.lootType == "Medicine")
            {
                newSlot.GetComponent<UiInventorySlotProps>().uiUseItemButton.gameObject.SetActive(true);
                newSlot.GetComponent<UiInventorySlotProps>().uiUseItemButton.GetComponent<Button>().onClick.AddListener(delegate { PartyController.instance.HealParty(slotId, slot.lootTypeVal); });
            }
            newSlot.GetComponent<UiInventorySlotProps>().uiDiscardOneButton.GetComponent<Button>().onClick.AddListener(delegate { PartyController.instance.DropItem(slotId, false); });
            newSlot.GetComponent<UiInventorySlotProps>().uiDiscardAllButton.GetComponent<Button>().onClick.AddListener(delegate { PartyController.instance.DropItem(slotId, true); });
        }
    }

    //Updates the Vehicles menu
    public void UpdateVehicles()
    {
        //loop through the world tiles vehicles and instantiate the vehicle slot under the Slots Transform, setting the text of each object as its dropped
        foreach (Transform child in uiVehicleSlots)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.vehicles.Count; i++)
        {
            Vehicle vehicle = WorldController.instance.currentTile.GetComponent<WorldTileProps>().tileProps.vehicles[i];

            GameObject newSlot = Instantiate(uiVehicleSlotObj, uiVehicleSlots);
            newSlot.GetComponent<UiVehicleSlotProps>().uiVehicleName.text = vehicle.vehicleName;
            newSlot.GetComponent<UiVehicleSlotProps>().uiVehicleFuel.text = vehicle.vehicleFuel + "/" + vehicle.vehicleMaxFuel;
            newSlot.GetComponent<UiVehicleSlotProps>().uiVehicleHp.text = vehicle.vehicleHp + "HP";
            newSlot.GetComponent<UiVehicleSlotProps>().uiVehiclePassengers.text = vehicle.vehiclePassengers.ToString();
            newSlot.GetComponent<UiVehicleSlotProps>().uiVehicleCarry.text = vehicle.vehicleCarry + "(lb)";

            int slotId = i;
            newSlot.GetComponent<UiVehicleSlotProps>().uiVehicleEnterButton.GetComponent<Button>().onClick.AddListener(delegate { PartyController.instance.EnterVehicle(vehicle, vehicle.vehicleFuel, vehicle.vehicleHp, slotId); });
        }

        //Update the party vehicle if its populated
        if (PartyController.instance.party.partyVehicle != null)
        {
            uiPartyVehicleName.text = PartyController.instance.party.partyVehicle.vehicleName;
            uiPartyVehicleFuel.text = PartyController.instance.party.partyVehicle.vehicleFuel + "/" + PartyController.instance.party.partyVehicle.vehicleMaxFuel;
            uiPartyVehicleHp.text = PartyController.instance.party.partyVehicle.vehicleHp + "HP";
            uiPartyVehiclePassengers.text = PartyController.instance.party.partyVehicle.vehiclePassengers.ToString();
            uiPartyVehiclePassengers.text = PartyController.instance.party.partyVehicle.vehicleCarry + "(lb)";
        }
        else
        {
            uiPartyVehicleName.text = null;
            uiPartyVehicleFuel.text = null;
            uiPartyVehicleHp.text = null;
            uiPartyVehiclePassengers.text = null;
            uiPartyVehicleCarry.text = null;
        }
    }

    //Updates the Party menu
    public void UpdateParty()
    {
        //loop through the party survivors and instantiate the survivor slot under the Slots Transform, setting the text of each object as its dropped
        foreach (Transform child in uiPartySlots)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < PartyController.instance.party.partySurvivors.Count; i++)
        {
            Survivor survivor = PartyController.instance.party.partySurvivors[i];

            GameObject newSlot = Instantiate(uiPartySlotObj, uiPartySlots);
            UiSurvivorSlotProps survivorSlot = newSlot.GetComponent<UiSurvivorSlotProps>();

            survivorSlot.uiSurvivorName.text = survivor.survivorName;
            survivorSlot.uiSkill.text = "Skills TBC";
            survivorSlot.uiTrait.text = "Traits TBC";
            survivorSlot.uiInfection.text = survivor.infection.ToString();
            survivorSlot.uiAttack.text = "+" + survivor.attack;
            if (survivor.equippedWeaponIndex == -1)
            {
                survivorSlot.uiEquipText.text = "unequipped";
            }
            else
            {
                survivorSlot.uiEquipText.text = PartyController.instance.inventory.inventorySlots[survivor.equippedWeaponIndex].lootName;
            }

            int slotId = i;

            //add the listener to the equip button to load the equipmenu, and the value is that of the survivor
            survivorSlot.uiEquipBtn.GetComponent<Button>().onClick.AddListener(delegate { OpenWeaponMenu(slotId); });
            survivorSlot.uiUnequip.GetComponent<Button>().onClick.AddListener(delegate { PartyController.instance.UnequipWeapon(slotId); });
            survivorSlot.uiAbandon.GetComponent<Button>().onClick.AddListener(delegate { PartyController.instance.AbandonSurvivor(slotId); });
        }
    }

    //Opens the Equip submenu
    public void OpenWeaponMenu(int survivorIndex)
    {
        uiWeapons.SetActive(true);
        //loop through the party survivors and instantiate the survivor slot under the Slots Transform, setting the text of each object as its dropped
        foreach (Transform child in uiWeaponsSlots)
        {
            GameObject.Destroy(child.gameObject);
        }

        //get a list of all the weapons in the inventory and add that list to a list of equippable items
        for (int i = 0; i < PartyController.instance.inventory.inventorySlots.Count; i++)
        {
            InventorySlot slot = PartyController.instance.inventory.inventorySlots[i];
            if ((slot.lootType == "WeaponRanged" || slot.lootType == "WeaponMelee") && slot.lootEquipped == false)
            {
                GameObject newWeaponSlot = Instantiate(uiWeaponSlotObj, uiWeaponsSlots);
                Debug.Log(PartyController.instance.inventory.inventorySlots[i].lootName + " - " + i);
                UiWeaponsSlotProps weaponSlot = newWeaponSlot.GetComponent<UiWeaponsSlotProps>();
                weaponSlot.uiWeaponName.text = slot.lootName;
                weaponSlot.uiAttack.text = "+" + slot.lootTypeVal;
                weaponSlot.weaponIndexInventory = i;
                //add the listener to weapon, and add the weaponIndexInventory value?
                weaponSlot.uiEquipWeapon.GetComponent<Button>().onClick.AddListener(delegate { PartyController.instance.EquipWeapon(weaponSlot.weaponIndexInventory, survivorIndex); });
            }
        }
    }

    public void CloseWeaponMenu()
    {
        uiWeapons.SetActive(false);
        UpdateParty();
    }

    //Updates the Party equip dropdown menu
    public List<string> UpdatePartyEquipDropdown()
    {
        //get a list of all the weapons in the inventory and add that list to a list of equippable items
        List<string> equippableWeapons = new List<string>();
        for (int i = 0; i < PartyController.instance.inventory.inventorySlots.Count; i++)
        {
            InventorySlot slot = PartyController.instance.inventory.inventorySlots[i];
            if ((slot.lootType == "WeaponRanged" || slot.lootType == "WeaponMelee") && slot.lootEquipped == false)
            {
                equippableWeapons.Add(slot.lootName);                
            }
        }

        return equippableWeapons;
    }

    //Updates the Journal with previously recorded events
    public void UpdateJournal()
    {
        string logsString = null;

        for (int i = 0; i < WorldController.instance.world.logs.Count; i++)
        {
            logsString += WorldController.instance.world.logs[i] + "\n";
        }
        uiJournalLogs.text = logsString;
    }

    //Opens the Notes tab in the journal
    public void OpenJournalNotes()
    {
        uiNotes.SetActive(true);
        UpdateNotes();
    }

    //Closes the Notes tab in the journal
    public void CloseJournalNotes()
    {
        uiNotes.SetActive(false);
    }

    //Updates the notes tab to load the players previously written notes back in
    public void UpdateNotes()
    {
        uiNotesInput.text = WorldController.instance.world.journalNotes;
    }

    //Updates the quests menu
    public void UpdateQuests()
    {
        //loop through the party survivors and instantiate the survivor slot under the Slots Transform, setting the text of each object as its dropped
        foreach (Transform child in uiQuestsSlots)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < QuestController.instance.quests.quests.Count; i++)
        {
            Quest quest = QuestController.instance.quests.quests[i];

            GameObject newSlot = Instantiate(uiQuestSlotObj, uiQuestsSlots);
            UiQuestSlotProps questSlot = newSlot.GetComponent<UiQuestSlotProps>();

            questSlot.uiQuestType.text = "Quest: " + quest.questType.ToString();
            questSlot.uiQuestText.text = quest.questText;
            questSlot.uiCompleteBy.text = "Complete by: " + quest.questTargetDateTime;
            questSlot.uiRewards.text = "Rewards: "; //build a comma seperated string of the rewards and their quantities

            int slotId = i;

            //add the listener to the equip button to load the equipmenu, and the value is that of the survivor
            questSlot.uiAbandonQuest.GetComponent<Button>().onClick.AddListener(delegate { QuestController.instance.AbandonQuest(slotId); });
        }
    }

    //Updates all UI elements on the main screen
    public void UpdateHud()
    {
        //Get the current world datetime and convert it to an actual datetime
        DateTime currentDateTime = DateTime.Parse(WorldController.instance.world.worldDateTime);

        hud_date.text = currentDateTime.ToString("dd/MM/yyyy");
        hud_time.text = currentDateTime.TimeOfDay.ToString("hh\\:mm");

        //Party Energy
        hud_partyEnergy.text = "Energy: " + PartyController.instance.party.partyEnergy;

        //Update party member stats
        //Clear all stats first
        for (int i = 0; i < hud_partySurvivors.Count; i++)
        {
            hud_partySurvivors[i].text = null;
        }

        //Update all stats based on available party members
        for (int i = 0; i < PartyController.instance.party.partySurvivors.Count; i++)
        {
            hud_partySurvivors[i].text = PartyController.instance.party.partySurvivors[i].survivorName;
        }

        //Vehicle stats
        if (PartyController.instance.party.partyVehicle != null)
        {
            hud_partyVehicleType.text = PartyController.instance.party.partyVehicle.vehicleName + " (" + PartyController.instance.party.partyVehicle.vehicleHp + "HP)";
            hud_partyVehicleFuel.text = "Fuel: " + PartyController.instance.party.partyVehicle.vehicleFuel + " (miles)";
        }
        else
        {
            hud_partyVehicleType.text = null;
            hud_partyVehicleFuel.text = null;
        }


        //Party Status
        hud_statusText.text = EncounterController.instance.partyStatus;
    }

    //Updates the Ambush screen
    public void UpdateAmbush()
    {
        //Update the status string
        EncounterController.instance.statusStrings.Clear();
        EncounterController.instance.StatusStringBuilder();
        UpdateHud();

        //activate and set each survivor object
        for (int i = 0; i < survivorObjs.Count; i++)
        {
            survivorObjs[i].SetActive(false);
        }

        for (int i = 0; i < PartyController.instance.party.partySurvivors.Count; i++)
        {
            survivorObjs[i].SetActive(true);
            survivorObjs[i].GetComponentInChildren<TextMeshProUGUI>().text = PartyController.instance.party.partySurvivors[i].survivorName;
        }

        //activate and set each enemy object
        for (int i = 0; i < enemyObjs.Count; i++)
        {
            enemyObjs[i].SetActive(false);
        }

        for (int i = 0; i < AmbushController.instance.enemyTotal; i++)
        {
            enemyObjs[i].SetActive(true);
            enemyObjs[i].GetComponentInChildren<TextMeshProUGUI>().text = AmbushController.instance.enemy.enemyName;
        }
    }

}
