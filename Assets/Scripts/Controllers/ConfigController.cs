using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used to load JSON def files into memory (names, nouns, items, weapons, enemies, vehicles etc)
public class ConfigController : MonoBehaviour
{
    public TextAsset itemsJSON;
    public TextAsset craftingRecipesJSON;
    public TextAsset weaponsJSON;
    public TextAsset vehiclesJSON;
    public TextAsset enemiesJSON;
    public TextAsset encountersJSON;
    public TextAsset verbsJSON;
    public TextAsset nounsJSON;

    public List<string> maleNames;
    public List<string> femaleNames;

    public LootList loot = new LootList();
    public RecipeList recipes = new RecipeList();
    public WeaponList weapons = new WeaponList();
    public VehicleList vehicles = new VehicleList();
    public EnemyList enemies = new EnemyList();
    public EncounterList encounters = new EncounterList();
    public NounList nounsList = new NounList();
    public VerbList verbsList = new VerbList();

    public static ConfigController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!ConfigController.instance)
        {
            ConfigController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadResources();
    }

    void LoadResources()
    {
        //Load male names
        string[] namesMale = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, "ForenamesMale.txt"));
        maleNames = new List<string>(namesMale);

        //Load female names
        string[] namesFemale = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, "ForenamesFemale.txt"));
        femaleNames = new List<string>(namesFemale);

        //Load all definitions
        loot = JsonUtility.FromJson<LootList>(itemsJSON.text);
        recipes = JsonUtility.FromJson<RecipeList>(craftingRecipesJSON.text);
        weapons = JsonUtility.FromJson<WeaponList>(weaponsJSON.text);
        vehicles = JsonUtility.FromJson<VehicleList>(vehiclesJSON.text);
        enemies = JsonUtility.FromJson<EnemyList>(enemiesJSON.text);
        encounters = JsonUtility.FromJson<EncounterList>(encountersJSON.text);
        nounsList = JsonUtility.FromJson<NounList>(nounsJSON.text);
        verbsList = JsonUtility.FromJson<VerbList>(verbsJSON.text);
    }

}

//================== Items =====================================
[System.Serializable]
public class Loot
{
    public string lootName;
    public string lootDesc;
    public string lootType;
    public int lootTypeVal;
    public float lootWeight;
    public float lootRarity;
    public float lootValue;
    public string lootBiome;
}

[System.Serializable]
public class LootList
{
    public List<Loot> loot;
}

//================== Crafting Recipes ======================================
[System.Serializable]
public class Recipe
{
    public string lootCrafted;
    public string description;
    public int buildTimeMinutes;
    public int intelligence;
    public List<RecipeIngredients> recipeList;
}

[System.Serializable]
public class RecipeIngredients
{
    public string lootName;
    public int requiredQty;
}

[System.Serializable]
public class RecipeList
{
    public List<Recipe> recipes;
}

//================== Weapons =====================================
[System.Serializable]
public class Weapon
{
    public string weaponName;
    public float weaponWeight;
    public float weaponRarity;
    public string weaponType;
    public int weaponAttack;
    public float weaponValue;
}

[System.Serializable]
public class WeaponList
{
    public List<Weapon> weapons;
}

//================== Vehicles =====================================
[System.Serializable]
public class Vehicle
{
    public string vehicleName;
    public int vehiclePassengers;
    public int vehicleFuel;
    public int vehicleMaxFuel;
    public int vehicleHp;
    public int vehicleCarry;
}

[System.Serializable]
public class VehicleList
{
    public List<Vehicle> vehicles;
}

//================== Enemies =====================================
[System.Serializable]
public class Enemy
{
    public string enemyName;
    public int enemyAttack;
    public string enemyVerb;
    public string canInfect;
    public string enemySprite;
}

[System.Serializable]
public class EnemyList
{
    public List<Enemy> enemies;
}

//================== Encounters =====================================

[System.Serializable]
public class Encounter
{
    public string encounterType;
    public string encounterTitle;
    public List<EncounterText> encounterText;
    public bool randomiseOptions;
    public List<EncounterOptions> encounterOptions;
    public List<EncounterWin> encounterWin;
    public List<EncounterLose> encounterLose;
    public string encounterSkill;
}

[System.Serializable]
public class EncounterList
{
    public List<Encounter> encounters;
}

[System.Serializable]
public class EncounterText
{
    public string text;
}

[System.Serializable]
public class EncounterOptions
{
    public string option;
}

[System.Serializable]
public class EncounterWin
{
    public string win;
}

[System.Serializable]
public class EncounterLose
{
    public string lose;
}

//================== Verbs ==========================================

[System.Serializable]
public class VerbList
{
    public List<Verb> verbs;
}

[System.Serializable]
public class Verb
{
    public List<WalkingVerb> walking;
    public List<DrivingVerb> driving;
}

[System.Serializable]
public class WalkingVerb
{
    public string verb;
}

[System.Serializable]
public class DrivingVerb
{
    public string verb;
}

//================== Nouns ======================================

[System.Serializable]
public class NounList
{
    public List<Noun> nouns;
}

[System.Serializable]
public class Noun
{
    public List<Woodland> woodland;
    public List<Urban> urban;
    public List<Motorway> motorway;
    public List<Countryside> countryside;
    public List<Mountain> mountain;
}

[System.Serializable]
public class Woodland
{
    public string noun;
}

[System.Serializable]
public class Urban
{
    public string noun;
}

[System.Serializable]
public class Motorway
{
    public string noun;
}

[System.Serializable]
public class Countryside
{
    public string noun;
}

[System.Serializable]
public class Mountain
{
    public string noun;
}
