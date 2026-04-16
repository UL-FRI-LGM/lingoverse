using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfoBounds : MonoBehaviour
{

    void Update()
    {
        // if player is below -10 on y axis, respawn him to y = 0
        if (transform.position.y < -10)
        {
            transform.position = new Vector3(transform.position.x, 2, transform.position.z);
        }
    }
}
