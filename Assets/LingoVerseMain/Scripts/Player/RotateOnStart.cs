using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnStart : MonoBehaviour
{
    public int degrees;

    void Start()
    {
        Vector3 rotation = transform.rotation.eulerAngles;
        rotation.y = degrees;
        transform.rotation = Quaternion.Euler(rotation);
    }
}
