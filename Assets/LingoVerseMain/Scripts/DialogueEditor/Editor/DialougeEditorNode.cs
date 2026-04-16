using Unity.Collections;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEditor.Rendering;

public class DialougeEditorNode : Node
{
    public DialogueNodeData dialogueNodeData;
    
    public string guid;
    public bool entryPoint = false;
    public string dialougeText;
    public AudioClip aud;
    public bool allCorrect = false;
    public DialogueType dialougeType;

    public override void OnSelected() {
        base.OnSelected();

        InspectorNode viewOnlyNode = ScriptableObject.CreateInstance<InspectorNode>();
        viewOnlyNode.guid = guid;
        viewOnlyNode.entryPoint = entryPoint;
        viewOnlyNode.dialougeText = dialougeText;
        viewOnlyNode.aud = aud;
        viewOnlyNode.allCorrect = allCorrect;
        viewOnlyNode.dialougeType = dialougeType;
        Selection.SetActiveObjectWithContext(viewOnlyNode, viewOnlyNode);
    }

    // Scriptable Version of the Node, but read only
    public class InspectorNode : ScriptableObject
    {
        public string guid;
        public string dialougeText;
        public bool entryPoint;
        public AudioClip aud;
        public bool allCorrect;
        public DialogueType dialougeType;
    }
    
    
    
}
