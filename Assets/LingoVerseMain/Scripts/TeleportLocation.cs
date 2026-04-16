using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportLocation : MonoBehaviour
{
    public GameObject player;
    public GameObject teleportLocation;
    // Start is called before the first frame update
    private void Awake()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
    }
    public void Teleport()
    {
        player.transform.position = teleportLocation.transform.position;

    }
}
