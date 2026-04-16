using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class PlayerSettingsPanel : MonoBehaviour
{
    private GameObject panel;
    private void Awake()
    {
        panel = gameObject.transform.GetChild(0).gameObject;
        panel.SetActive(false);
    }
    private void OnEnable()
    {
        GameEventsManager.instance.inputEvents.onYPressed += TogglePanel;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.inputEvents.onYPressed -= TogglePanel;
    }

    private void TogglePanel()
    {
        Debug.Log("Toggle Panel with status " + panel.activeSelf);
        if (panel.activeSelf)
        {
            panel.SetActive(false);
        }
        else
        {
            panel.SetActive(true);
        }
    }
}