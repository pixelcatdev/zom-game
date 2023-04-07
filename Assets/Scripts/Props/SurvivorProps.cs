using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivorProps : MonoBehaviour
{
    public bool turnTaken;
    public int survivorId;
    public string survivorName;
    public int hp;
    public float attackRange;
    public int attackDamage;

    public void TakeDamage(int amount)
    {
        if(hp - amount > 0)
        {
            hp -= amount;
        }
        else
        {
            //work out how the survivor is linked back to the party index
            PartyController.instance.KillSurvivor(survivorId);
            ZoneController.instance.survivors.Remove(gameObject);
            Destroy(this.gameObject);
            Debug.Log("Survivor killed");
        }
    }
}
