using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTileProps : MonoBehaviour
{
    public TileData tileProps;

    private void Update()
    {
        //Turns on the scavenged marker
        if (tileProps.alreadyScavenged == true)
        {
            tileProps.alreadyScavengedIcon.SetActive(true);
        }
    }
}