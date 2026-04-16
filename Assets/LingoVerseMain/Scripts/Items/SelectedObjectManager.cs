using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class SelectedObjectManager : MonoBehaviour
{
    private static ObjectInfo previouslySelectedObj;

    //[ReadOnlyInspector]
    //[SerializeField]
    private string objectName;

    // Refactor code so that it will support both hands.

    private void OnEnable()
    {
        GameEventsManager.instance.itemEvents.onItemGrabbedPressed += DoStuff;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.itemEvents.onItemGrabbedPressed -= DoStuff;
    }

    public void DoStuff(ObjectInfo objectInfo) 
    {
        if(previouslySelectedObj == null) 
        {
            InfoPanel.Instance.ShowInfo(objectInfo);
        }
        else if(previouslySelectedObj.Data.Name == objectInfo.Data.Name)
        {
            if (InfoPanel.Instance.IsPanelOn)
            {
                InfoPanel.Instance.HideInfo();
            }
            else
            {
                InfoPanel.Instance.ShowInfo(objectInfo);
            }
        }
        else
        {
            InfoPanel.Instance.ShowInfo(objectInfo);
        }
        previouslySelectedObj = objectInfo;
        objectName = objectInfo.Data.Name;
    }
}
