using System;

public class InputEvents
{
    public event Action onAPressed;
    public void APressed()
    {
        onAPressed?.Invoke();
    }

    public event Action onAReleased;
    public void AReleased()
    {
        onAReleased?.Invoke();
    }

    public event Action onSelectPressed;
    public void SelectPressed()
    {
        onSelectPressed?.Invoke();
    }

    public event Action onDisableAButton;
    public void DisableAButton()
    {
        onDisableAButton?.Invoke();
    }

    public event Action onEnableAButton;
    public void EnableAButton()
    {
        onEnableAButton?.Invoke();
    }

    public event Action onXPressed;
    public void XPressed()
    {
        onXPressed?.Invoke();
    }

    public event Action onYPressed;
    public void YPressed()
    {
        onYPressed?.Invoke();
    }
}
