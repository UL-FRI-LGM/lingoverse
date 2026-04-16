using System;
using UnityEngine;

public class ItemEvents
{
    public Action<ObjectInfo> onItemGrabbedPressed;
    public void ItemGrabbedPressed(ObjectInfo item)
    {
        onItemGrabbedPressed?.Invoke(item);
    }

    public Action<ObjectInfo> onItemGrabbed;
    public void ItemGrabbed(ObjectInfo item)
    {
        onItemGrabbed?.Invoke(item);
    }

    public Action<GameObject, Collider> onItemTriggerEnter;
    public void ItemTriggerEnter(GameObject item, Collider collider)
    {
        onItemTriggerEnter?.Invoke(item, collider);
    }
}
