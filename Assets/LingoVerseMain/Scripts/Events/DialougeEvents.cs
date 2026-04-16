using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueEvents
{
    public Action<string, string, bool> onStartDialogue;
    public void StartDialogue(string id, string questId, bool isQuest = false)
    {
        onStartDialogue?.Invoke(id, questId, isQuest);
    }

    public Action<string, string, bool> onAdvanceDialogue;
    public void AdvanceDialogue(string id, string questId, bool isQuest = false)
    {
        onAdvanceDialogue?.Invoke(id, questId, isQuest);
    }

    public Action<string, bool> onFinishDialogue;
    public void FinishDialogue(string id, bool isQuest = false)
    {
        onFinishDialogue?.Invoke(id, isQuest);
    }

    public Action<string, string, bool> onSelectAdvanceOrFinishDialogue;
    public void SelectAdvanceOrFinishDialogue(string id, string questId, bool isQuest = false)
    {
        onSelectAdvanceOrFinishDialogue?.Invoke(id, questId, isQuest);
    }

    public Action<Dialogue> onDialogueStateChange;
    public void DialogueStateChange(Dialogue dialogue)
    {
        onDialogueStateChange?.Invoke(dialogue);
    }

    public Action<string, TMP_Text, string, float, bool> onStartDialogueAnimation;
    public void StartDialogueAnimation(string text, TMP_Text panelText, string questId, float animationSpeed = 0.1f, bool isQuest = false)
    {
        onStartDialogueAnimation?.Invoke(text, panelText, questId, animationSpeed, isQuest);
    }

    public Action<Transform> movePanel;
    public void MovePanel(Transform transform)
    {
        movePanel?.Invoke(transform);
    }

    public Action onEnablePanel;
    public void EnablePanel()
    {
        onEnablePanel?.Invoke();
    }

    public Action onDisablePanel;
    public void DisablePanel()
    {
        onDisablePanel?.Invoke();
    }

    public Action<string, bool, bool> onDialogueInProgress;
    public void DialogueInProgress(string id, bool value, bool reset)
    {
        onDialogueInProgress?.Invoke(id, value, reset);
    }

    public Action<bool> onDialoguePicking;
    public void DialoguePicking(bool value)
    {
        onDialoguePicking?.Invoke(value);
    }

    public Action<string> onResetDialogue;
    public void ResetDialogue(string id)
    {
        onResetDialogue?.Invoke(id);
    }

    public Action<DialogueState, string> onIconStateChange;
    public void IconStateChange(DialogueState state, string id)
    {
        onIconStateChange?.Invoke(state, id);
    }

    public Action<string, int> onAddQuestToTimer;
    public void AddQuestToTimer(string id, int time)
    {
        onAddQuestToTimer?.Invoke(id, time);
    }

    public Action<string> onEnableRestartPanel;
    public void EnableRestartPanel(string id)
    {
        onEnableRestartPanel?.Invoke(id);
    }

    public Action onDisableRestartPanel;
    public void DisableRestartPanel()
    {
        onDisableRestartPanel?.Invoke();
    }
}