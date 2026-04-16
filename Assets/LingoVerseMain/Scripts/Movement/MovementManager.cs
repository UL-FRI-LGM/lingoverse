using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MovementManager : MonoBehaviour
{
    private GameObject xrOrigin;

    private void Start()
    {
        xrOrigin = GameObject.FindGameObjectWithTag("Player");
        ApplyRotationPrefs();
        ApplyWalkSpeedPrefs();
    }

    private void OnEnable()
    {
        GameEventsManager.instance.movementEvents.onEnableMovement += EnableMovement;
        GameEventsManager.instance.movementEvents.onDisableMovement += DisableMovement;
        GameEventsManager.instance.movementEvents.onEnableTeleportation += EnableTeleportation;
        GameEventsManager.instance.movementEvents.onDisableTeleportation += DisableTeleportation;
        GameEventsManager.instance.movementEvents.onTeleportPlayer += TeleportPlayer;
        GameEventsManager.instance.movementEvents.onSetRotationType += SetRotationType;
        GameEventsManager.instance.movementEvents.onSetWalkSpeed += SetWalkSpeed;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.movementEvents.onEnableMovement -= EnableMovement;
        GameEventsManager.instance.movementEvents.onDisableMovement -= DisableMovement;
        GameEventsManager.instance.movementEvents.onEnableTeleportation -= EnableTeleportation;
        GameEventsManager.instance.movementEvents.onDisableTeleportation -= DisableTeleportation;
        GameEventsManager.instance.movementEvents.onTeleportPlayer -= TeleportPlayer;
        GameEventsManager.instance.movementEvents.onSetRotationType -= SetRotationType;
        GameEventsManager.instance.movementEvents.onSetWalkSpeed -= SetWalkSpeed;
    }

    private void EnableMovement()
    {
        xrOrigin.GetComponent<ActionBasedContinuousMoveProvider>().enabled = true;
    }

    private void DisableMovement()
    {
        xrOrigin.GetComponent<ActionBasedContinuousMoveProvider>().enabled = false;
    }

    private void EnableTeleportation()
    {
        xrOrigin.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>().enabled = true;
    }

    private void DisableTeleportation()
    {
        xrOrigin.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider>().enabled = false;
    }

    private void TeleportPlayer(Transform target)
    {
        // Set the player's position to the transform's position, except for the y axis
        xrOrigin.transform.position = new Vector3(target.position.x, xrOrigin.transform.position.y, target.position.z);
        
        // Set the player's rotation to the transform's rotation
        xrOrigin.transform.rotation = target.rotation;
    }

    private void SetRotationType(int value)
    {
        PlayerPrefs.SetInt("turn", value);
        PlayerPrefs.Save();
        ApplyRotationPrefs();
    }

    private void SetWalkSpeed(float value)
    {
        PlayerPrefs.SetFloat("walkSpeed", value);
        PlayerPrefs.Save();
        ApplyWalkSpeedPrefs();
    }

    private void ApplyWalkSpeedPrefs()
    {
        if (PlayerPrefs.HasKey("walkSpeed"))
        {
            float value = PlayerPrefs.GetFloat("walkSpeed");
            xrOrigin.GetComponent<ActionBasedContinuousMoveProvider>().moveSpeed = value;
        }
    }

    private void ApplyRotationPrefs()
    {
        if (PlayerPrefs.HasKey("turn"))
        {
            int value = PlayerPrefs.GetInt("turn");
            if (value == 0)
            {
                xrOrigin.GetComponent<ActionBasedSnapTurnProvider>().enabled = false;
                xrOrigin.GetComponent<ActionBasedContinuousTurnProvider>().enabled = true;
            }
            else if (value == 1)
            {
                xrOrigin.GetComponent<ActionBasedSnapTurnProvider>().enabled = true;
                xrOrigin.GetComponent<ActionBasedContinuousTurnProvider>().enabled = false;
            }
        }
    }
}
