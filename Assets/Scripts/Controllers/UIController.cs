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
    public TextMeshProUGUI hud_threatCalc;
    public TextMeshProUGUI hud_travel;
    public TextMeshProUGUI hud_travelStats;
    public TextMeshProUGUI hud_tileName;
    public TextMeshProUGUI hud_partyEnergy;
    public List<TextMeshProUGUI> hud_partySurvivors;
    public TextMeshProUGUI hud_partyVehicleType;
    public TextMeshProUGUI hud_partyVehicleFuel;
    public TextMeshProUGUI hud_statusText;

    public GameObject uiCursorMap;
    public GameObject uiCursorTooltip;
    public TextMeshProUGUI uiCursorTooltipText;

    public GameObject uiEncounter;
    public GameObject uiEncounterPrompt;

    public GameObject uiInventory;
    public Transform uiInventorySlots;
    public GameObject uiInventorySlotObj;
    public TextMeshProUGUI uiInventoryPartyWeight;

    public GameObject uiCrafting;
    public Transform uiCraftingSlots;
    public GameObject uiCraftingSlotObj;

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

    public Button uiAmbushAttack;
    public Button uiAmbushUseitem;
    public Button uiAmbushFlee;
    public TextMeshProUGUI uiAmbushPartyCalc;
    public TextMeshProUGUI uiAmbushPartyAttackTotal;
    public TextMeshProUGUI uiAmbushEnemyCalc;
    public TextMeshProUGUI uiAmbushEnemyAttackTotal;

    public GameObject uiTrade;
    public Transform uiTradeBuySlots;
    public GameObject uiTradeBuySlotObj;
    public Transform uiTradeSellSlots;
    public GameObject uiTradeSellSlotObj;

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

        DrawCursors();
    }

    public void DrawCursors()
    {
        // Convert the mouse position to world coordinates
        Vector3 newPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10);

        uiCursorMap.GetComponent<RectTransform>().position = newPos;
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

    //Updates the Encounter text - currently for new survivors, but needs to be more fluid
    public void UpdateEncounter(string encounterType)
    {
        uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiEncounterText.text = EncounterController.instance.encounterText;
        Survivor newSurvivor = EncounterController.instance.newSurvivor;

        //Update the button based on the encounter type
        if (encounterType == "SurvivorJoins")
        {
            uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiAccept.GetComponent<Button>().onClick.AddListener(delegate { PartyController.instance.AddSurvivor(newSurvivor); });
            uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiAccept.GetComponent<Button>().onClick.AddListener(delegate { CloseEncounterPrompt(); });

        }
        else if (encounterType == "SurvivorRescue")
        {
            uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiAccept.GetComponent<Button>().onClick.AddListener(delegate { AmbushController.instance.SetupAmbush(true, false, true, null); });
            uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiAccept.GetComponent<Button>().onClick.AddListener(delegate { CloseEncounterPrompt(); });
        }
        else if (encounterType == "HoldupVehicle")
        {
            uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiDecline.GetComponent<Button>().onClick.AddListener(delegate { AmbushController.instance.SetupAmbush(false, false, false, "Raider"); });
            uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiAccept.GetComponent<Button>().onClick.AddListener(delegate { PartyController.instance.ClearVehicle(); });
        }
        else if (encounterType == "HoldupInventory")
        {
            uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiDecline.GetComponent<Button>().onClick.AddListener(delegate { AmbushController.instance.SetupAmbush(false, false, false, "Raider"); });
            uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiAccept.GetComponent<Button>().onClick.AddListener(delegate { EncounterController.instance.EncounterHoldup(); });
        }
        else if (encounterType == "Trade")
        {
            uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiAccept.GetComponent<Button>().onClick.AddListener(delegate { CloseEncounterPrompt(); });
            uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiDecline.GetComponent<Button>().onClick.AddListener(delegate { });
        }
        else if (encounterType == "Quest")
        {
            uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiAccept.GetComponent<Button>().onClick.AddListener(delegate { QuestController.instance.AddQuest(); });
            uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiDecline.GetComponent<Button>().onClick.AddListener(delegate { CloseEncounterPrompt(); });
        }

        //Add the default close and clear method too
    }

    //Closes the encounter prompt and clears any listeners added to either of the buttons
    public void CloseEncounterPrompt()
    {
        uiEncounter.SetActive(false);
        uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiAccept.GetComponent<Button>().onClick.RemoveAllListeners();
        uiEncounterPrompt.GetComponent<UiEncounterPrompt>().uiDecline.GetComponent<Button>().onClick.RemoveAllListeners();
    }

    //Opens the Crafting tab in the Inventory
    public void OpenInventoryCrafting()
    {
        uiCrafting.SetActive(true);
        UpdateCrafting();
    }

    //Closes the Crafting tab in the Inventory
    public void CloseInventoryCrafting()
    {
        uiCrafting.SetActive(false);
    }

    //Updates the Inventory menu
    public void UpdateInventory()
    {
        //loop through the inventory and instantiate the inv slot under the Slots Transform, setting the text of each object as its dropped
        foreach (Transform child in uiInventorySlots)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < PartyController.instance.party.inventory.inventorySlots.Count; i++)
        {
            InventorySlot slot = PartyController.instance.party.inventory.inventorySlots[i];

            //Set the slot loot name, but flag it as equipped if it's equipped
            GameObject newSlot = Instantiate(uiInventorySlotObj, uiInventorySlots);
            if (slot.slotEquipped == true)
            {
                for (int x = 0; x < PartyController.instance.party.partySurvivors.Count; x++)
                {
                    if (PartyController.instance.party.partySurvivors[x].equippedWeaponIndex == i)
                    {
                        newSlot.GetComponent<UiInventorySlotProps>().uiItemName.text = "[E] " + slot.loot.lootName;
                    }
                }
            }
            else
            {
                newSlot.GetComponent<UiInventorySlotProps>().uiItemName.text = slot.loot.lootName;
            }
            newSlot.GetComponent<UiInventorySlotProps>().uiQty.text = "x" + slot.slotQty;
            newSlot.GetComponent<UiInventorySlotProps>().uiWeight.text = slot.loot.lootWeight + "(lb)";
            newSlot.GetComponent<UiInventorySlotProps>().uiTotalWeight.text = slot.slotQty * slot.loot.lootWeight + "(lb)";

            int slotId = i;
            //If the slot is food or medicine, activate the Use Item button
            if (slot.loot.lootType == "Food")
            {
                newSlot.GetComponent<UiInventorySlotProps>().uiUseItemButton.gameObject.SetActive(true);
                newSlot.GetComponent<UiInventorySlotProps>().uiUseItemButton.GetComponent<Button>().onClick.AddListener(delegate { PartyController.instance.FeedParty(slotId, slot.loot.lootTypeVal); });
            }
            else if (slot.loot.lootType == "Medicine")
            {
                newSlot.GetComponent<UiInventorySlotProps>().uiUseItemButton.gameObject.SetActive(true);
                newSlot.GetComponent<UiInventorySlotProps>().uiUseItemButton.GetComponent<Button>().onClick.AddListener(delegate { PartyController.instance.HealParty(slotId, slot.loot.lootTypeVal); });
            }
            newSlot.GetComponent<UiInventorySlotProps>().uiDiscardOneButton.GetComponent<Button>().onClick.AddListener(delegate { InventoryController.instance.RemoveItem(slotId, 1, false, PartyController.instance.party.inventory); });
            newSlot.GetComponent<UiInventorySlotProps>().uiDiscardAllButton.GetComponent<Button>().onClick.AddListener(delegate { InventoryController.instance.RemoveItem(slotId, 0, true, PartyController.instance.party.inventory); });
        }

        //Update the weight and max weight
        uiInventoryPartyWeight.text = "Total Weight: " + PartyController.instance.party.partyWeight + "(lb) / Max Weight: " + PartyController.instance.party.partyWeightMax + "(lb)";
    }

    //Updates the Inventory Crafting menu
    public void UpdateCrafting()
    {
        //loop through the known recipes and instantiate the crafting slot under the Slots Transform, setting the text of each object as its dropped
        foreach (Transform child in uiCraftingSlots)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < PartyController.instance.party.recipesKnown.recipes.Count; i++)
        {
            GameObject newSlot = Instantiate(uiCraftingSlotObj, uiCraftingSlots);

            newSlot.GetComponent<UiCraftingSlotProps>().uiRecipeName.text = PartyController.instance.party.recipesKnown.recipes[i].lootCrafted;
            //newSlot.GetComponent<UiCraftingSlotProps>().uiIngredients.text = PartyController.instance.party.recipesKnown.recipes[i].lootCrafted;

            string ingredients = null;
            for (int x = 0; x < PartyController.instance.party.recipesKnown.recipes[i].ingredients.Count; x++)
            {
                ingredients += PartyController.instance.party.recipesKnown.recipes[i].ingredients[x].lootName + " x" + PartyController.instance.party.recipesKnown.recipes[i].ingredients[x].requiredQty + ", ";
            }

            newSlot.GetComponent<UiCraftingSlotProps>().uiIngredients.text = ingredients;
            newSlot.GetComponent<UiCraftingSlotProps>().uiBuildTime.text = PartyController.instance.party.recipesKnown.recipes[i].buildTimeMinutes + " (mins)";

            int slotId = i;

            //Add the method to build the item here
            newSlot.GetComponent<UiCraftingSlotProps>().uiCraftRecipeButton.GetComponent<Button>().onClick.AddListener(delegate { InventoryController.instance.CheckCraftingRecipe(slotId); });
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

            survivorSlot.uiSurvivorName.text = survivor.survivorName + "(" + survivor.survivorHp + "HP)";
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
                survivorSlot.uiEquipText.text = PartyController.instance.party.inventory.inventorySlots[survivor.equippedWeaponIndex].loot.lootName;
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
        for (int i = 0; i < PartyController.instance.party.inventory.inventorySlots.Count; i++)
        {
            InventorySlot slot = PartyController.instance.party.inventory.inventorySlots[i];
            if ((slot.loot.lootType == "WeaponRanged" || slot.loot.lootType == "WeaponMelee") && slot.slotEquipped == false)
            {
                GameObject newWeaponSlot = Instantiate(uiWeaponSlotObj, uiWeaponsSlots);
                Debug.Log(PartyController.instance.party.inventory.inventorySlots[i].loot.lootName + " - " + i);
                UiWeaponsSlotProps weaponSlot = newWeaponSlot.GetComponent<UiWeaponsSlotProps>();
                weaponSlot.uiWeaponName.text = slot.loot.lootName;
                weaponSlot.uiAttack.text = "+" + slot.loot.lootTypeVal;
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
        for (int i = 0; i < PartyController.instance.party.inventory.inventorySlots.Count; i++)
        {
            InventorySlot slot = PartyController.instance.party.inventory.inventorySlots[i];
            if ((slot.loot.lootType == "WeaponRanged" || slot.loot.lootType == "WeaponMelee") && slot.slotEquipped == false)
            {
                equippableWeapons.Add(slot.loot.lootName);
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

        //Set the hud travel text based on whether walking or driving
        if (PartyController.instance.party.inVehicle == false)
        {
            //Party Energy
            hud_travel.text = "Travel: Walking";
            hud_travelStats.text = "Energy: " + PartyController.instance.party.partyEnergy;
        }
        else
        {
            //Party Energy
            hud_travel.text = "Travel: Driving (" + PartyController.instance.party.partyVehicle.vehicleName + " (" + PartyController.instance.party.partyVehicle.vehicleHp + "HP)";
            hud_travelStats.text = "Fuel: " + PartyController.instance.party.partyVehicle.vehicleFuel + " (miles)";
        }

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
            hud_partySurvivors[i].text = PartyController.instance.party.partySurvivors[i].survivorName + "(" + PartyController.instance.party.partySurvivors[i].survivorHp + "HP)";
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

    //Updates the Ambush UI
    public void UpdateAmbushStats()
    {
        UIController.instance.uiCursorTooltip.SetActive(false);

        UIController.instance.uiAmbushPartyCalc.text = "Rolled (" + AmbushController.instance.partyAttackRoll.ToString() + ") " + PartyController.instance.party.partyAttack.ToString();
        UIController.instance.uiAmbushPartyAttackTotal.text = (AmbushController.instance.partyAttackRoll + PartyController.instance.party.partyAttack).ToString();

        UIController.instance.uiAmbushEnemyCalc.text = "Rolled (" + AmbushController.instance.enemyAttackRoll.ToString() + ") " + AmbushController.instance.enemyAttack.ToString();
        UIController.instance.uiAmbushEnemyAttackTotal.text = (AmbushController.instance.enemyAttackRoll + AmbushController.instance.enemyAttack).ToString();
    }

    //Updates the Ambush sprites
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

    //Updates the trade UI
    public void UpdateTrade()
    {
        //loop through the party survivors and instantiate the survivor slot under the Slots Transform, setting the text of each object as its dropped
        foreach (Transform child in uiTradeBuySlots)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < TradeController.instance.tradeInventory.inventorySlots.Count; i++)
        {
            GameObject newSlot = Instantiate(uiTradeBuySlotObj, uiTradeBuySlots);
            UiTradeSlotProps tradeSlot = newSlot.GetComponent<UiTradeSlotProps>();

            tradeSlot.uiLootName.text = TradeController.instance.tradeInventory.inventorySlots[i].loot.lootName;
            tradeSlot.uiLootQty.text = "x" + TradeController.instance.tradeInventory.inventorySlots[i].slotQty;
            tradeSlot.uiLootWeight.text = TradeController.instance.tradeInventory.inventorySlots[i].loot.lootWeight + "(lb)";
            tradeSlot.uiLootValue.text = TradeController.instance.tradeInventory.inventorySlots[i].loot.lootValue.ToString();
            //tradeSlot.uiTotal.text = EncounterController.instance.tradeInventory.inventorySlots[i].lootQty;

            tradeSlot.uiBuyIncreaseButton.GetComponent<Button>().onClick.AddListener(delegate { TradeController.instance.TradeBuy(true, 0); });
            tradeSlot.uiBuyDecreaseButton.GetComponent<Button>().onClick.AddListener(delegate { TradeController.instance.TradeBuy(false, 0); });

        }
    }

}
