using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapPanel : MonoBehaviour
{
    private GameObject panel;
    public GameObject minimapCamera;
    private void Awake()
    {
        panel = gameObject.transform.GetChild(0).gameObject;
        panel.SetActive(false);
        minimapCamera.SetActive(false);
    }
    private void OnEnable()
    {
        GameEventsManager.instance.inputEvents.onXPressed += TogglePanel;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.inputEvents.onXPressed -= TogglePanel;
    }

    private void TogglePanel()
    {
        Debug.Log("Toggle minimap with status " + panel.activeSelf);
        if (panel.activeSelf)
        {
            panel.SetActive(false);
            minimapCamera.SetActive(false);
        }
        else
        {
            panel.SetActive(true);
            minimapCamera.SetActive(true);
        }
    }
}
