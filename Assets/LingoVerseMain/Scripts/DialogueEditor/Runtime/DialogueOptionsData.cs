using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueOptionsData
{
    public DialogueNodeData nextDialogue;
    public string text;
    public bool isCorrectAnswer = false;

    public DialogueOptionsData(string text, DialogueNodeData nextDialogue)
    {
        this.text = text;
        this.nextDialogue = nextDialogue;
    }

    public void SetIsCorrectAnswer(bool isCorrectAnswer)
    {
        this.isCorrectAnswer = isCorrectAnswer;
    }

    public bool GetIsCorrectAnswer()
    {
        return isCorrectAnswer;
    }
}
