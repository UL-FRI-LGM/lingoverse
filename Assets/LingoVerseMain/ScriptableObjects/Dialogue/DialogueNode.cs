using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueNode")]
public class DialogueNode : ScriptableObject
{
    [field: SerializeField] public string id { get; private set; }

    [Header("General")]
    public DialogueText dialogueText;
    public bool canRestartOnFail = false;

    [Header("Next Steps")]
    public List<DialogueNode> nodes;

    [Header("Reward")]
    public int exp = 0;
    
    [Header("Type")]
    public DialogueType dialogueType = DialogueType.MultiChoice;


    private void OnValidate()
    {

#if UNITY_EDITOR
        id = this.name;
        UnityEditor.EditorUtility.SetDirty(this);
#endif

    }
}
