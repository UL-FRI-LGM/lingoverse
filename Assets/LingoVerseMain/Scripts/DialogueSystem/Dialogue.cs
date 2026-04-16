using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;

public class Dialogue
{
    public DialogueNodeData dialogueData;
    public DialogueContainer dialogueContainer;

    public DialogueState state;
    public bool completed;
    public DialogueNodeData currentDialogueData; // check if needed
    public float animateSpeed = 0.05f;
    public int exp = 10;
    public List<DialogueNodeData> nextNodes = new List<DialogueNodeData>();
    public List<DialogueOptionsData> options = new List<DialogueOptionsData>();
    public DialogueType dialogueType = DialogueType.MultiChoice;

    public Dialogue(DialogueNodeData data)
    {
        this.dialogueData = data;
        this.state = DialogueState.CAN_START;
        this.currentDialogueData = data;
    }

    public Dialogue(DialogueNodeData data, DialogueContainer dialogueContainer)
    {
        this.dialogueData = data;
        this.state = DialogueState.CAN_START;
        this.currentDialogueData = data;
        this.dialogueContainer = dialogueContainer;
    }

    public Dialogue(DialogueNodeData data, DialogueState state, DialogueNodeData currentNode, bool isCompleted)
    {
        this.dialogueData = data;
        this.state = state;
        this.currentDialogueData = currentNode;
        this.completed = isCompleted;
    }

    private List<DialogueNodeData> GetNextNodes()
    {
        List<DialogueNodeData> nextNodes = new List<DialogueNodeData>();
        string currentGUID = currentDialogueData.NodeGUID;
        foreach (NodeLinkData nodeLink in dialogueContainer.NodeLinks.FindAll(x => x.BaseNodeGUID == currentGUID))
        {
            nextNodes.Add(dialogueContainer.DialogueNodeData.Find(x => x.NodeGUID == nodeLink.TargetNodeGUID));
        }
        return nextNodes;
    }

    private List<DialogueOptionsData> GetOptions()
    {
        string currentGUID = currentDialogueData.NodeGUID;
        List<NodeLinkData> options = new List<NodeLinkData>();
        options = dialogueContainer.NodeLinks.FindAll(x => x.BaseNodeGUID == currentGUID);

        List<DialogueOptionsData> dialogueOptions = new List<DialogueOptionsData>();
        
        foreach (NodeLinkData option in options)
        {
            DialogueNodeData nextDialogue = dialogueContainer.DialogueNodeData.Find(x => x.NodeGUID == option.TargetNodeGUID);
            DialogueOptionsData dialogueOption = new DialogueOptionsData(option.PortName, nextDialogue);
            dialogueOptions.Add(dialogueOption);
        }

        return dialogueOptions;
    }

    public void InstantiateCurrentDialogue(TMP_Text panelText, string questId, bool isQuest = false)
    {
        // Also event for playing the audio
        this.nextNodes = GetNextNodes();
        this.options = GetOptions();
        GameEventsManager.instance.dialogueEvents.StartDialogueAnimation(currentDialogueData.DialogueText, panelText, questId, animateSpeed, isQuest: isQuest);
    }

    public DialogueNodeData GetDialogueData()
    {
        if (!state.Equals(DialogueState.FINISHED))
            state = DialogueState.CAN_START;
        return new DialogueNodeData(currentDialogueData);
    }
}
