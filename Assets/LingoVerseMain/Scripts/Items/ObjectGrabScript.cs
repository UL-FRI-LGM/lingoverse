using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectGrabScript : MonoBehaviour
{
    public InputActionReference buttonAReference;
    public InputActionReference selectReference;

    public ObjectInfo objInfo;

    [SerializeField]
    public bool selected { get; set; }

    void Update()
    {

        if (buttonAReference.action.triggered && selected)
        {
            GameEventsManager.instance.itemEvents.ItemGrabbedPressed(objInfo);
            //Debug.Log("Object " + gameObject.name + " pressed");
        }
        if (selected)
        {
            GameEventsManager.instance.itemEvents.ItemGrabbed(objInfo);
            //Debug.Log("Object " + gameObject.name + " selected");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug what it collided with what object
        if (selected)
        {
            //Debug.Log("Object " + gameObject.name + " collided with " + other.gameObject.name);
            GameEventsManager.instance.itemEvents.ItemTriggerEnter(gameObject, other);
        }
        // Debug.Log("Collided with: " + other.gameObject.name);
    }
}
