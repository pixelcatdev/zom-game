using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyProps : MonoBehaviour
{
    public Transform target;

    public int hp;
    public float attackRange;
    public int attackDamage;

    private Seeker seeker;
    private CharacterController controller;
    private Path path;
    public int currentWaypoint = 0;

    void Start()
    {
        target = FindNearestTarget();
        seeker = GetComponent<Seeker>();
        seeker.StartPath(transform.position, target.position, OnPathComplete);
    }

    public void EnemyBehavior()
    {
        //check for the nearest survivor

        Transform nearestTarget = FindNearestTarget();
        float dist = Vector2.Distance(nearestTarget.position, transform.position);

        //Check if line of sight
        //raycast to target based on attackRange
        //if hits, attack

        if (dist < attackRange)
        {
            //Debug.Log(this.name + " attacking " + FindNearestTarget().name);
            //Attack

            //Check for line of sight between survivor and target
            LayerMask targetLayer = LayerMask.GetMask("zoneLayerSolid", "zoneSurvivorUnitsLayer");
            RaycastHit2D targetHit = Physics2D.Raycast(transform.position, nearestTarget.position - transform.position, attackRange, targetLayer);
            Debug.DrawRay(transform.position, (nearestTarget.position - transform.position) * attackRange, Color.red, 3f);

            if (targetHit == true)
            {
                //Debug.Log("target: " + targetHit.transform.name);
                //Debug.Log("target tag: " + targetHit.transform.tag);

                if (targetHit.transform.tag == "isSurvivor")
                {
                    targetHit.transform.gameObject.GetComponent<SurvivorProps>().TakeDamage(attackDamage);
                }
            }

        }
        else
        {
            Move();
        }
    }

    public void Move()
    {
        //Find the nearest survivor to chow down, maul or open fire on
        target = FindNearestTarget();

        // Calculate path to target
        //if (path == null)
        //{
         //   Debug.Log("a");
            //seeker.StartPath(transform.position, target.position, OnPathComplete);
        //    return;
        //}

        // Get the next node in the path
        //if (currentWaypoint >= path.vectorPath.Count)
        //{
        //    // End of path reached
        //    return;
        //}
        Vector3 nextNode = path.vectorPath[currentWaypoint + 1];
        //nextNode.y = transform.position.y;
        seeker.StartPath(transform.position, target.position, OnPathComplete);
        // Move towards the next node
        transform.position = nextNode;


        // Check if we are close to the next node
        if (Vector3.Distance(transform.position, nextNode) < 0.5f)
        {
            currentWaypoint++;
        }
    }

    public void TakeDamage(int amount)
    {
        if (hp - amount > 0)
        {
            hp -= amount;
        }
        else
        {
            Debug.Log(this.name + " killed");
            ZoneController.instance.enemies.Remove(gameObject);
            Destroy(this.gameObject);
        }
    }

    private Transform FindNearestTarget()
    {
        //Get the nearest survivor and set it as the target
        GameObject[] survivors = GameObject.FindGameObjectsWithTag("isSurvivor");

        // Set the initial nearest object to the first object in the array
        Transform target = survivors[0].transform;
        float nearestDistance = Vector3.Distance(transform.position, target.transform.position);

        // Iterate over the rest of the objects and find the nearest one
        foreach (GameObject survivor in survivors)
        {
            float distance = Vector3.Distance(transform.position, survivor.transform.position);
            if (distance < nearestDistance)
            {
                target = survivor.transform;
                nearestDistance = distance;
            }
        }

        return target;
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

}

