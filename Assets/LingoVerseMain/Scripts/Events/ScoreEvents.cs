using System;

public class ScoreEvents 
{
    public Action<int> onScoreGain;
    public void ScoreGained(int amount)
    {
        onScoreGain?.Invoke(amount);
    }

    public Action<int> onScoreChange;
    public void ScoreChanged(int amount)
    {
        onScoreChange?.Invoke(amount);
    }
}
