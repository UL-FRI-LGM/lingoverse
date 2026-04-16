using System;
using UnityEngine;

public class QuestEvents
{
    public Action<string> onStartQuest;
    public void StartQuest(string id)
    {
        onStartQuest?.Invoke(id);
    }

    public Action<string> onAdvanceQuest;
    public void AdvanceQuest(string id)
    {
        onAdvanceQuest?.Invoke(id);
    }

    public Action<string> onFinishQuest;
    public void FinishQuest(string id)
    {
        onFinishQuest?.Invoke(id);
    }

    public Action<Quest> onQuestStateChange;
    public void QuestStateChange(Quest quest)
    {
        onQuestStateChange?.Invoke(quest);
    }

    public Action<string, int, QuestStepState> onQuestStateStepChange;
    public void QuestStateStepChange(string id, int stepIndex, QuestStepState questStepState)
    {
        onQuestStateStepChange?.Invoke(id, stepIndex, questStepState);
    }

    public Action<string> onQuestStepChange;
    public void QuestStepChange(string id)
    {
        onQuestStepChange?.Invoke(id);
    }

}
