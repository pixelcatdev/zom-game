using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTileProps : MonoBehaviour
{
    public TileData tileProps;
    public bool changeTimeOfDayColor;
    public Color colorNight;
    public Color colorDay;
    public GameObject alreadyScavengedIcon;

    private void Start()
    {
        if (changeTimeOfDayColor == true)
        {
            GetComponent<SpriteRenderer>().color = colorDay;
        }
    }

    private void Update()
    {
        //Turns on the scavenged marker
        if (tileProps.alreadyScavenged == true)
        {
            alreadyScavengedIcon.SetActive(true);
        }               
    }

    private void RenderTile()
    {
        //Twilight mode if night-time
        if (changeTimeOfDayColor == true)
        {
            string dateTime = WorldController.instance.world.worldDateTime;
            DateTime date;
            if (DateTime.TryParse(dateTime, out date)) // Try to parse the string to a DateTime object
            {
                // Get the time of day
                TimeSpan time = date.TimeOfDay;

                // Check if the time falls between 7pm and 6am
                if (time >= TimeSpan.FromHours(19) || time < TimeSpan.FromHours(6))
                {
                    GetComponent<SpriteRenderer>().color = colorNight;
                }
                else
                {
                    GetComponent<SpriteRenderer>().color = colorDay;
                }
            }
            else
            {
                Console.WriteLine("Invalid date format.");
            }
        }
    }
}