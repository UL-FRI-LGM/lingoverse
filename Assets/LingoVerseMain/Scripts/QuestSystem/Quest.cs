using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{
    public QuestInfoScriptableObject questInfo;

    // state info
    public QuestState state;
    private int currentQuestStepIndex;
    private QuestStepState[] questStepStates;


    public Quest(QuestInfoScriptableObject questInfo)
    {
        this.questInfo = questInfo;
        this.state = QuestState.REQUIREMENTS_NOT_MET;
        this.currentQuestStepIndex = 0;
        this.questStepStates = new QuestStepState[questInfo.questStepPrefabs.Length];
        for (int i = 0; i < questStepStates.Length; i++)
        {
            questStepStates[i] = new QuestStepState();
        }
    }

    public Quest(QuestInfoScriptableObject questInfo, QuestState questState, int currentQuestStepIndex, QuestStepState[] questStepStates)
    {
        this.questInfo = questInfo;
        this.state = questState;
        this.currentQuestStepIndex = currentQuestStepIndex;
        this.questStepStates = questStepStates;

        // if the quest step states and prefab lengths don't match, 
        // since its out of sync
        if (this.questStepStates.Length != questInfo.questStepPrefabs.Length)
        {
            Debug.LogWarning("Quest step states and prefab lengths don't match, since its out of sync. \nSomething has changed in QuestInfo and the saved data.\n Reset your data. QuestId " + questInfo.id);
        }
    }

    public void MoveToNextStep() 
    { 
        this.currentQuestStepIndex++; 
    }

    public bool CurrentStepExists()
    {
        return (currentQuestStepIndex < questInfo.questStepPrefabs.Length);
    }
        
    public void InstantiateCurrentQuestStep(Transform parentTransfrom)
    {
        GameObject questStepPrefab = GetCurrentQuestStepPrefab();
        if (questStepPrefab != null)
        {
            QuestStep questStep = Object.Instantiate<GameObject>(questStepPrefab, parentTransfrom)
                .GetComponent<QuestStep>();
            questStep.InitializeQuestStep(questInfo.id, currentQuestStepIndex, questStepStates[currentQuestStepIndex].state);
            GameEventsManager.instance.questEvents.QuestStepChange(questStep.name);
        }
    }

    private GameObject GetCurrentQuestStepPrefab()
    {
        GameObject questStepPrefab = null;
        if (CurrentStepExists())
        {
            questStepPrefab = questInfo.questStepPrefabs[currentQuestStepIndex];
        }
        else
        {
            Debug.LogWarning("Tried to get quest step prefab, but stepIndex was out of range.. No current step: QuestId " + questInfo.id + ", stepIndex " + currentQuestStepIndex);
        }
        return questStepPrefab;
    }

    public void StoreQuestStepState(QuestStepState questStepState, int stepIndex)
    {
        if (stepIndex < questStepStates.Length)
        {
            questStepStates[stepIndex] = questStepState;
        }
        else
        {
            Debug.LogWarning("Tried to store quest step state, but stepIndex was out of range.. No current step: QuestId " + questInfo.id + ", stepIndex " + stepIndex);
        }
    }

    public QuestData GetQuestData()
    {
        return new QuestData(state, currentQuestStepIndex, questStepStates);
    }
}
