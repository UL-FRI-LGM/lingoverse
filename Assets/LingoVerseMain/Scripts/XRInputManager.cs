using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class XRInputManager : MonoBehaviour
{
    public InputActionReference leftSelectActionReference;
    public InputActionReference rightSelectActionReference;
    public InputActionReference leftPrimaryActionReference;
    public InputActionReference rightPrimaryActionReference;

    // Reference your custom actions here
    public InputActionReference aPressActionReference;
    public InputActionReference xPressActionReference;
    public InputActionReference yPressActionReference;

    private void Start()
    {
        leftSelectActionReference.action.started += ctx => OnLeftSelectButtonPress(ctx);
        leftPrimaryActionReference.action.started += ctx => OnLeftPrimaryButtonPress(ctx);
        rightSelectActionReference.action.started += ctx => OnRightSelectButtonPress(ctx);
        rightPrimaryActionReference.action.started += ctx => OnRightPrimaryButtonPress(ctx);
        
        xPressActionReference.action.started += OnXPress;
        yPressActionReference.action.started += OnYPress;
        
        aPressActionReference.action.started += OnAPress;
        aPressActionReference.action.canceled += OnAReleased;
    }

    private void OnEnable()
    {
        GameEventsManager.instance.inputEvents.onDisableAButton += DisableAButton;
        GameEventsManager.instance.inputEvents.onEnableAButton += EnableAButton;
        GameEventsManager.instance.sceneEvents.onSceneLoad += SceneChange;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.inputEvents.onDisableAButton -= DisableAButton;
        GameEventsManager.instance.inputEvents.onEnableAButton -= EnableAButton;
        GameEventsManager.instance.sceneEvents.onSceneLoad -= SceneChange;
    }

    private void SceneChange()
    {
        leftSelectActionReference.action.started -= ctx => OnLeftSelectButtonPress(ctx);
        leftPrimaryActionReference.action.started -= ctx => OnLeftPrimaryButtonPress(ctx);
        xPressActionReference.action.started -= OnXPress;
        yPressActionReference.action.started -= OnYPress;
        
        Debug.Log("Removing listeners");
        rightPrimaryActionReference.action.started -= ctx => OnRightPrimaryButtonPress(ctx);
        rightSelectActionReference.action.started -= ctx => OnRightSelectButtonPress(ctx);
        aPressActionReference.action.started -= OnAPress;
        aPressActionReference.action.canceled -= OnAReleased;
        
    }

    private void OnAPress(InputAction.CallbackContext ctx)
    {
        GameEventsManager.instance.inputEvents.APressed();
        Debug.Log("A Pressed");
    }

    private void OnAReleased(InputAction.CallbackContext ctx) {
        GameEventsManager.instance.inputEvents.AReleased();
        Debug.Log("A Released");
    }
    
    private void OnXPress(InputAction.CallbackContext ctx)
    {
        GameEventsManager.instance.inputEvents.XPressed();
        Debug.Log("X Pressed");
    }

    public void OnYPress(InputAction.CallbackContext ctx)
    {
        GameEventsManager.instance.inputEvents.YPressed();
        Debug.Log("Y Pressed");
    }

    // write a event that stops the player from pressing the button
    private void DisableAButton()
    {
        aPressActionReference.action.Disable();
    }

    private void EnableAButton()
    {
        aPressActionReference.action.Enable();
    }

    private void OnLeftSelectButtonPress(InputAction.CallbackContext context)
    {
        // Handle the left select button press.
        Debug.Log("Left Select Button Pressed");
    }

    private void OnLeftPrimaryButtonPress(InputAction.CallbackContext context)
    {
        // Handle the left primary button press.
        Debug.Log("Left Primary Button Pressed");
    }

    private void OnRightSelectButtonPress(InputAction.CallbackContext context)
    {
        // Handle the right select button press.
        GameEventsManager.instance.inputEvents.SelectPressed();
        Debug.Log("Right Select Button Pressed");
    }

    private void OnRightPrimaryButtonPress(InputAction.CallbackContext context)
    {
        // Handle the right primary button press.
        Debug.Log("Right Primary Button Pressed");
    }
}
