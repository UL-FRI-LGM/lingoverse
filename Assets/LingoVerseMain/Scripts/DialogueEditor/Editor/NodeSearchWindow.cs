using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private DialougeGraphView _graphView;
    private EditorWindow _window;

    public void Init(EditorWindow editorWindow, DialougeGraphView graphView)
    {
        this._window = editorWindow;
        this._graphView = graphView;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
        
        var diagNode = CreateInstance<DialogueNode>();
        diagNode.dialogueType = DialogueType.MultiChoice;
        var whisperNode = CreateInstance<DialogueNode>();
        whisperNode.dialogueType = DialogueType.MultiWhisper;
        
        var tree = new List<SearchTreeEntry>()
        {
            new SearchTreeGroupEntry(new GUIContent("Create Dialogue Node"), 0),
            new (new GUIContent("Dialogue Node"))
            {
                userData = diagNode, level = 1
            },
            new (new GUIContent("Whisper Node")) {
                userData = whisperNode, level = 1
            }
        };

        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
    {
        var worldMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent,
            context.screenMousePosition - _window.position.position);
        var localMousePosition = _graphView.contentViewContainer.WorldToLocal(worldMousePosition);
        
        if (searchTreeEntry.userData is DialogueNode) {
            DialogueNode newNode = searchTreeEntry.userData as DialogueNode;
            if (newNode != null)
                switch (newNode.dialogueType) {
                    case DialogueType.MultiChoice:
                        _graphView.CreateNode("Dialogue Node", localMousePosition, DialogueType.MultiChoice);
                        return true;
                    case DialogueType.MultiWhisper:
                        _graphView.CreateNode("Whisper Node", localMousePosition, DialogueType.MultiWhisper);
                        return true;
                }
        }
        return false;
    }
}
