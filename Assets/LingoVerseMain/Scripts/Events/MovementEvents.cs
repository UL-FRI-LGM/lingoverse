using System;
using UnityEngine;

public class MovementEvents
{
    public Action onDisableMovement;
    public void DisableMovement()
    {
        onDisableMovement?.Invoke();
    }

    public Action onEnableMovement;
    public void EnableMovement()
    {
        onEnableMovement?.Invoke();
    }

    public Action onDisableTeleportation;
    public void DisableTeleportation()
    {
        onDisableTeleportation?.Invoke();
    }

    public Action onEnableTeleportation;
    public void EnableTeleportation()
    {
        onEnableTeleportation?.Invoke();
    }

    public Action<Transform> onTeleportPlayer;
    public void TeleportPlayer(Transform transform)
    {
        onTeleportPlayer?.Invoke(transform);
    }

    public Action<int> onSetRotationType;
    public void SetRotationType(int value)
    {
        onSetRotationType?.Invoke(value);
    }

    public Action<float> onSetWalkSpeed;
    public void SetWalkSpeed(float value)
    {
        onSetWalkSpeed?.Invoke(value);
    }
}
