using System;

public class PlayerEvents 
{
    public Action onGetPlayerName;
    public void GetPlayerName()
    {
        onGetPlayerName?.Invoke();
    }

    public Action<string> onSendPlayerName;
    public void SendPlayerName(string playerName)
    {
        onSendPlayerName?.Invoke(playerName);
    }
}
