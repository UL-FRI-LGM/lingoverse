using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNodeData
{
    public string NodeGUID;
    public string DialogueText;
    public Vector2 Position;
    public AudioClip AudioClip;
    public bool allCorrect = false;
    public DialogueType dialogueType;

    public DialogueNodeData(string nodeGUID, string dialogueText, Vector2 position, AudioClip audioClip,
        bool allCorrect, DialogueType dialogueType) {
        NodeGUID = nodeGUID;
        DialogueText = dialogueText;
        Position = position;
        AudioClip = audioClip;
        this.allCorrect = allCorrect;
        this.dialogueType = dialogueType;
    }

    public DialogueNodeData(DialogueNodeData dialogueNodeData)
    {
        NodeGUID = dialogueNodeData.NodeGUID;
        DialogueText = dialogueNodeData.DialogueText;
        Position = dialogueNodeData.Position;
        AudioClip = dialogueNodeData.AudioClip;
        allCorrect = dialogueNodeData.allCorrect;
        dialogueType = dialogueNodeData.dialogueType;
    }
}
