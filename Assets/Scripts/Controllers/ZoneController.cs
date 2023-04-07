using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pathfinding;

public class ZoneController : MonoBehaviour
{
    public Transform selectedSurvivor;
    private GameObject currentTile;
    public Transform zoneMap;                   //where the maps are generated
    public Transform tileSelector;
    public List<Transform> mapSpawns;
    public List<GameObject> maps;
    public List<GameObject> urbanPrefabs;
    public List<GameObject> woodlandPrefabs;
    public List<GameObject> countrysidePrefabs;
    public List<GameObject> motorwayPrefabs;
    public List<GameObject> mountainPrefabs;
    public List<GameObject> coastalPrefabs;
    public GameObject survivorPrefab;
    public GameObject enemyPrefab;
    public GameObject lootPrefab;
    public List<GameObject> survivors;
    public List<GameObject> enemies;
    public List<GameObject> loot;
    public bool respawnEnemies;
    public int maxEnemies = 0;
    public int minLoot = 0;
    public int maxLoot = 0;
    public List<GameObject> floorTiles;

    public static ZoneController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!ZoneController.instance)
        {
            ZoneController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        LoadZone();
    }

    private void LoadZone()
    {
        //Generate the zone from map chunks
        for (int i = 0; i < mapSpawns.Count; i++)
        {
            GameObject randomMap = urbanPrefabs[Random.Range(0, urbanPrefabs.Count)];
            GameObject mapSpawn = Instantiate(randomMap, mapSpawns[i].position, zoneMap.rotation, mapSpawns[i]);
            maps.Add(mapSpawn);
        }

        //Store available florTile references        
        GameObject[] isZoneFloorTiles = GameObject.FindGameObjectsWithTag("isZoneFloor");
        floorTiles = isZoneFloorTiles.ToList();

        //Rescan the A* paths
        AstarPath.active.Scan();
        Debug.Log("1 - Map generated");

        //Instantiate survivors from PartyController
        for (int i = 0; i < PartyController.instance.party.partySurvivors.Count; i++)
        {
            //Instantiate object (find a random tile for the moment)              
            Vector2 spawnPoint = randomTile().transform.position;

            GameObject survivorSpawn = Instantiate(survivorPrefab, spawnPoint, transform.rotation, transform);

            //Copy survivor stats from party
            survivorSpawn.GetComponent<SurvivorProps>().survivorId = i;
            survivorSpawn.GetComponent<SurvivorProps>().survivorName = PartyController.instance.party.partySurvivors[i].survivorName;
            //survivorSpawn.GetComponent<SurvivorProps>().attackRange = PartyController.instance.party.partySurvivors[i].attackRange;
            //survivorSpawn.GetComponent<SurvivorProps>().attackDamage = PartyController.instance.party.partySurvivors[i].attackDamage;

            //Set survivor color

            //Add survivor to survivors list
            survivors.Add(survivorSpawn);
        }
        Debug.Log("2 - Survivors spawned");

        //Instantiatie enemies based on threat level (hardcoded to 3 for testing)
        for (int i = 0; i < maxEnemies; i++)
        {
            //Instantiate object (find a random tile for the moment)              
            Vector2 spawnPoint = randomTile().transform.position;

            GameObject enemySpawn = Instantiate(enemyPrefab, spawnPoint, transform.rotation, transform);

            //Add enemy to enemies list
            enemies.Add(enemySpawn);
        }
        Debug.Log("3 - Enemies spawned");

        //Instantiatie loot (hardcoded between 1 and 2 for testing)
        int randomLootTotal = Random.Range(minLoot, maxLoot);

        for (int i = 0; i < randomLootTotal; i++)
        {
            //Instantiate object (find a random tile for the moment)              
            Vector2 spawnPoint = randomTile().transform.position;

            GameObject lootSpawn = Instantiate(lootPrefab, spawnPoint, transform.rotation, transform);

            //Add loot to loost list
            loot.Add(lootSpawn);
        }
        Debug.Log("4 - Loot spawned");
    }

    private void ClearZone(bool regenZone)
    {
        //remove all survivor spawns
        for (int i = 0; i < survivors.Count; i++)
        {
            GameObject survivor = survivors[i];
            Destroy(survivor);
        }
        survivors.Clear();

        //remove all enemy spawns
        for (int i = 0; i < enemies.Count; i++)
        {
            GameObject enemy = enemies[i];
            Destroy(enemy);
        }
        enemies.Clear();

        //remove all loot spawns
        for (int i = 0; i < loot.Count; i++)
        {
            GameObject lootY = loot[i];
            Destroy(lootY);
        }
        loot.Clear();

        //remove all map spawns
        for (int i = 0; i < maps.Count; i++)
        {
            GameObject map = maps[i];
            Destroy(map);
        }
        maps.Clear();

        if (regenZone == true)
        {
            LoadZone();
        }
    }

    GameObject randomTile()
    {
        GameObject tile = floorTiles[Random.Range(0, floorTiles.Count)];
        floorTiles.Remove(tile);
        return tile;
    }

    private void Update()
    {
        PlayerTurn();

        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("Regenerating Zone");
            ClearZone(true);
        }
    }

    public void PlayerTurn()
    {

        //Cast a ray to the Zone Layer (so we can detect for selecting a survivor, moving to a tile, picking up loot or attacking an enemy)
        LayerMask hitLayer = LayerMask.GetMask("zoneFloorLayer", "zoneSurvivorUnitsLayer", "zoneEnemyUnitsLayer");
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0f, hitLayer);

        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (h1, h2) => h2.transform.gameObject.layer.CompareTo(h1.transform.gameObject.layer));
            GameObject hit = hits[0].transform.gameObject;
            GameObject hitObj = hit.transform.gameObject;
            RenderTileSelector(hit);
            //Move the tile selector over the object
            tileSelector.position = hit.transform.position;

            //On Click if the party isn't resting
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //Selecting a survivor
                if (hit.transform.gameObject.tag == "isSurvivor")
                {                    
                    selectedSurvivor = hit.transform;
                }

                //Moving to an adjacent tile
                else if (hit.transform.gameObject.tag == "isZoneFloor")
                {
                    if (selectedSurvivor.GetComponent<SurvivorProps>().turnTaken == false)
                    {
                        //Calculate move distance based on survivors current Hp
                        float moveDistance = (selectedSurvivor.GetComponent<SurvivorProps>().hp / 2) + 0.5f;

                        float distanceToPlayer = Vector2.Distance(hit.transform.position, selectedSurvivor.position);

                        //If its close enough and there's enough moves left, move the player
                        if (distanceToPlayer <= moveDistance && hit.transform.gameObject != currentTile)
                        {
                            MoveSurvivor(hit.transform.gameObject);
                            currentTile = hit.transform.gameObject;
                            EndSurvivorTurn();
                        }                       
                        //Debug.Log("moving " + selectedSurvivor.name + " to " + hit.transform.name);
                    }
                }

                //Picking up Loot
                else if (hit.transform.gameObject.tag == "isLoot")
                {
                    float distanceToPlayer = Vector2.Distance(hit.transform.position, selectedSurvivor.position);

                    //If its close enough and there's enough moves left, move the player
                    if (distanceToPlayer <= 1.5 && hit.transform.gameObject != currentTile)
                    {
                        Destroy(hit);
                        //Add loot to temporary holding table
                        MoveSurvivor(hit.transform.gameObject);
                        EndSurvivorTurn();
                        Debug.Log("Loot collected");
                    }
                }
                //Attacking an enemy
                else if (hit.transform.gameObject.tag == "isEnemy")
                {
                    //If a survivor has been selected first
                    if (selectedSurvivor && selectedSurvivor.GetComponent<SurvivorProps>().turnTaken == false)
                    {
                        float attackRange = selectedSurvivor.GetComponent<SurvivorProps>().attackRange;
                        int attackDamage = selectedSurvivor.GetComponent<SurvivorProps>().attackDamage;

                        //Check for line of sight between survivor and target
                        LayerMask targetLayer = LayerMask.GetMask("zoneLayerSolid", "zoneEnemyUnitsLayer");
                        RaycastHit2D targetHit = Physics2D.Raycast(selectedSurvivor.position, hit.transform.position - selectedSurvivor.position, attackRange, targetLayer);
                        Debug.DrawRay(selectedSurvivor.position, (hit.transform.position - selectedSurvivor.position) * attackRange, Color.red, 3f);

                        if (targetHit == true)
                        {
                            Debug.Log("target: " + targetHit.transform.name);
                            Debug.Log("target tag: " + targetHit.transform.tag);

                            if (targetHit.transform.tag == "isEnemy")
                            {
                                targetHit.transform.gameObject.GetComponent<EnemyProps>().TakeDamage(attackDamage);
                                EndSurvivorTurn();
                            }
                        }
                    }
                }
            }
        }

        //End player turn
        if (Input.GetKeyDown(KeyCode.Return))
        {
            EnemyTurn();
        }
    }

    private void RenderTileSelector(GameObject hit)
    {
        if (hit.transform.gameObject.tag == "isSurvivor")
        {
            //Set the selector color
            tileSelector.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
        }
        else if (hit.transform.gameObject.tag == "isZoneFloor" || hit.transform.gameObject.tag == "isLoot")
        {
            //Set the selector color
            tileSelector.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        }
        else if (hit.transform.gameObject.tag == "isEnemy")
        {
            //Set the selector color
            tileSelector.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    private void EndSurvivorTurn()
    {
        selectedSurvivor.GetComponent<SurvivorProps>().turnTaken = true;
        selectedSurvivor.GetComponent<SpriteRenderer>().color = Color.grey;
    }

    private void MoveSurvivor(GameObject targetTile)
    {
        selectedSurvivor.position = new Vector2(targetTile.transform.position.x, targetTile.transform.position.y);
    }

    private void EnemyTurn()
    {
        //Loop through all spawned enemies
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<EnemyProps>().EnemyBehavior();
        }

        //Loop through all survivors and set turnTaken to false
        foreach (GameObject survivor in survivors)
        {
            survivor.GetComponent<SurvivorProps>().turnTaken = false;
            survivor.GetComponent<SpriteRenderer>().color = Color.white;
        }

        //reset the selected survivor
        selectedSurvivor = null;
    }
}
