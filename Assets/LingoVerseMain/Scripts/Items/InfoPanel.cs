using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    // Reference to the Info Panel
    public static InfoPanel Instance {  get; private set; }

    // References to the Panel Objects
    public TMP_Text infoText;

    // If panel is on
    public bool IsPanelOn;
    private ObjectInfo latestObjectInfo;
    private float offset = 0.3f;

    private void Awake()
    {
        if (Instance == null || Instance != this)
            Instance = this;

        gameObject.SetActive(false);
        IsPanelOn = false;
    }

    // Method to show the info using ObjectInfo and add the offset
    public void ShowInfo(ObjectInfo objectInfo)
    {
        latestObjectInfo = objectInfo;
        //parent the info panel to the gameObject
        //transform.parent = objectInfo.transform;


        Transform cameraTransform = Camera.main.transform;
        Vector3 spawnPosition = cameraTransform.position + cameraTransform.forward;

        // Calculate the offset to the right
        Vector3 offsetVec = cameraTransform.right * offset;

        // Final position
        Vector3 panelPosition = spawnPosition + offsetVec;

        // Set the position
        transform.position = panelPosition;

        infoText.text = objectInfo.Data.Info;

        // Activate the panel
        gameObject.SetActive(true);

        IsPanelOn = true;
        // Play the audio
        PlayAudio();
    }

    public void PlayAudio()
    {
        if (latestObjectInfo.Data.AudioClip != null)
            GameEventsManager.instance.audioEvents.Play(latestObjectInfo.Data.AudioClip);
    }

    public void HideInfo()
    {
        // Deparent the InfoPanel
        //transform.SetParent(null);

        // Deactivate the panel
        gameObject.SetActive(false);

        IsPanelOn = false;
    }
}
