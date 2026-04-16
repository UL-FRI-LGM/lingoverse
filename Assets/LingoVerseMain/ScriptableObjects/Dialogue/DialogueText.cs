using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueText")]
public class DialogueText : ScriptableObject
{
    public string text;
    public string textEn;
    public AudioClip textAudio;
    public bool isCorrectAnswer;
}
