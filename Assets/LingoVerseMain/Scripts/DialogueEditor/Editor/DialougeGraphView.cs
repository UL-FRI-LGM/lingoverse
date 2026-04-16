using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Piper;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class DialougeGraphView : GraphView
{
    public readonly Vector2 defaultNodeSize = new Vector2(x: 800, y: 400);

    private EditorWindow _window;
    private NodeSearchWindow searchWindow;

    private Node selectedNode;

    private DialoguePiper _dialoguePiper;

    private static List<Color> nodeColors = new()
    {
        new (0, 0.5f, 0),
        new (0, 0, 0.5f),
    };

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach((port) => 
        { 
            if(startPort != port && startPort.node != port.node)
                compatiblePorts.Add(port);
        });

        return compatiblePorts;
    }

    public DialougeGraphView(EditorWindow editorWindow) {
        _window = editorWindow;
        styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/DialogueGraph"));
        SetupZoom(0.25f, 2.0f);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        
        var grid = new GridBackground();
        Insert(index: 0, grid);
        grid.StretchToParentSize();

        AddElement(GenerateEntryPointNode());
        AddSearchWindow(editorWindow);
        
        // Set the Required Graph Elements for Copy/Paste/Cut actions
        // TODO: Implement Copy/Paste/Cut actions
        this.serializeGraphElements = SerializeGraphElements;
        this.canPasteSerializedData = AllowPaste;
        this.unserializeAndPaste = OnPaste;
    }
    
    private class CopyNodeData
    {
        public Vector2 position;
        public string dialougeText;
        public bool entryPoint;
        public string audioClipPath;
        public bool allCorrect;
        public DialogueType dialougeType;
    }

    public new string SerializeGraphElements(IEnumerable<GraphElement> elements) {
        var serializedData = new List<string>();
        Vector2 anchorNode = Vector2.zero;
        int index = 0;
        foreach (var element in elements.OfType<DialougeEditorNode>()) {
            var pos = anchorNode - element.GetPosition().position;
            if (index == 0) {
                pos = Vector2.zero;
                anchorNode = element.GetPosition().position;
            }
            CopyNodeData nodeData = new CopyNodeData {
                position = pos,
                dialougeText = element.dialougeText,
                entryPoint = element.entryPoint,
                audioClipPath = AssetDatabase.GetAssetPath(element.aud),
                allCorrect = element.allCorrect,
                dialougeType = element.dialougeType
            };
            serializedData.Add(JsonUtility.ToJson(nodeData));
            index++;
        }

        var output = string.Join("\\n", serializedData);
        Debug.Log(output);
        return output;
    }
    
    public bool AllowPaste(string serializedData)
    {
        return !string.IsNullOrEmpty(serializedData);
    }

    public void OnPaste(string operationName, string data)
    {
        var nodeDataArray = data.Split("\\n");
        
        // Make all pastes be relative to the anchor first node
        var localMousePos = contentViewContainer.WorldToLocal(Mouse.current.position.ReadValue());
        foreach (var nodeDataJson in nodeDataArray)
        {
            if (string.IsNullOrWhiteSpace(nodeDataJson))
                continue;
            var nodeData = JsonUtility.FromJson<CopyNodeData>(nodeDataJson);
            var newNode = GenerateDialougeEditorNode(nodeData.dialougeText,
                AssetDatabase.LoadAssetAtPath<AudioClip>(nodeData.audioClipPath), localMousePos - nodeData.position,
                nodeData.allCorrect,
                nodeData.dialougeType);
            newNode.entryPoint = nodeData.entryPoint;
            AddElement(newNode);
        }
    }

    private void AddSearchWindow(EditorWindow editorWindow)
    {
        searchWindow =ScriptableObject.CreateInstance<NodeSearchWindow>();
        searchWindow.Init(editorWindow, this);
        nodeCreationRequest = context =>
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
    }

    private Port GeneratePort(DialougeEditorNode editorNode, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return editorNode.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }

    private DialougeEditorNode GenerateEntryPointNode()
    {
        var node = new DialougeEditorNode()
        {
            title = "Start",
            guid = Guid.NewGuid().ToString(),
            dialougeText = "ENTRYPOINT",
            entryPoint = true
        };

        var generatedPort = GeneratePort(node, Direction.Output);
        generatedPort.portName = "Next";

        node.outputContainer.Add(generatedPort);

        node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100, 200, 100, 150));

        return node;
    }

    public void CreateNode(string nodeName, Vector2 mousePos, DialogueType type)
    {
        AddElement(GenerateDialougeEditorNode(nodeName, null, mousePos, false, type));
    }
    
    public void AddChoicePort(DialougeEditorNode dialougeEditorNode, string overriddenPortName = "X", bool readOnly = false)
    {
        var generatedPort = GeneratePort(dialougeEditorNode, Direction.Output);

        var oldLabel = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(oldLabel);

        var outputPortCount = dialougeEditorNode.outputContainer.Query(name: "connector").ToList().Count();
        generatedPort.portName = $"Choice {outputPortCount}";

        var choicePortName = overriddenPortName.Equals("X") ? $"Choice {outputPortCount}" : overriddenPortName;
        
        var textField = new TextField
        {
            name = string.Empty,
            value = choicePortName
        };

        // Set textField to be small
        textField.style.flexDirection = FlexDirection.Row;
        textField.style.maxWidth = 150;
        
        // Set the text field to be read only
        if (readOnly)
            textField.SetEnabled(false);

        textField.RegisterValueChangedCallback(val => generatedPort.portName = val.newValue);
        generatedPort.contentContainer.Add(new Label("  "));
        generatedPort.contentContainer.Add(textField);
        var deleteButton = new Button(() => RemovePort(dialougeEditorNode, generatedPort))
        {
            text = "X"
        };
        if (readOnly)
            deleteButton.SetEnabled(false);

        generatedPort.contentContainer.Add(deleteButton);
        generatedPort.portName = choicePortName;
        dialougeEditorNode.outputContainer.Add(generatedPort);
        
        dialougeEditorNode.RefreshPorts();
        dialougeEditorNode.RefreshExpandedState();
    }

    public void RemovePort(DialougeEditorNode editorNode, Port port)
    {
        var targetEdge = edges.ToList()
                .Where(x => x.output.portName == port.portName && x.output.node == port.node);
        if (targetEdge.Any())
        {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(targetEdge.First());
        }

        editorNode.outputContainer.Remove(port);
        editorNode.RefreshPorts();
        editorNode.RefreshExpandedState();
    }

    public DialougeEditorNode GenerateDialougeEditorNode(string nodeName, AudioClip audioClip, Vector2 position, 
        bool allCorrect, DialogueType diagType)
    {
        var node = new DialougeEditorNode
        {
            title = nodeName,
            guid = System.Guid.NewGuid().ToString(),
            dialougeType = diagType,
            dialougeText = nodeName,
            aud = audioClip,
            allCorrect = allCorrect
        };
        node.styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/Node"));
        var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        node.inputContainer.Add(inputPort);
        node.RefreshExpandedState();
        node.RefreshPorts();

        var button = new Button(() => { AddChoicePort(node); });
        button.text = "New Choice";
        node.titleContainer.Add(button);

        var textLabel = new Label("Text Dialogue");
        node.mainContainer.Add(textLabel);
        var textField = new TextField(string.Empty);
        textField.style.maxWidth = new StyleLength(Length.Percent(100));
        textField.multiline = true;
        textField.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
        textField.RegisterValueChangedCallback(e =>
        {
            node.dialougeText = e.newValue;
            node.title = $"{node.dialougeType.ToString()}:{e.newValue.Substring(0, Mathf.Min(e.newValue.Length, 3))}...";
            textField.style.whiteSpace = WhiteSpace.Normal;
        });
        textField.SetValueWithoutNotify(node.title);
        node.mainContainer.Add(textField);
        
        List<string> choices = new List<string>();
        foreach (var dType in Enum.GetValues(typeof(DialogueType)).Cast<DialogueType>())
            choices.Add(dType.ToString());
        
        // Set node title color based on diag type
        int choiceIndex = (int)node.dialougeType;
        node.titleContainer.style.backgroundColor = nodeColors[choiceIndex];
        
        var dropDown = new DropdownField("Dialogue Type", choices, choiceIndex);
        dropDown.RegisterValueChangedCallback(e =>
        {
            if (Enum.TryParse(e.newValue, out DialogueType newType))
            {
                node.dialougeType = newType;
                node.titleContainer.style.backgroundColor = nodeColors[(int)node.dialougeType];
            }
        });
        node.mainContainer.Add(dropDown);
        
        // Add label for audio file
        var audioLabel = new Label("Audio File");
        node.mainContainer.Add(audioLabel);
            
        // Add a field for adding an audio file
        var audioField = new ObjectField
        {
            objectType = typeof(AudioClip),
            allowSceneObjects = false,
            value = null
        };

        audioField.RegisterValueChangedCallback(e =>
        {
            node.aud = e.newValue as AudioClip;
            audioField.MarkDirtyRepaint();
        });
        audioField.SetValueWithoutNotify(node.aud);
        node.mainContainer.Add(audioField);
        
        // Create label for Running TTS and listening to AudioFile
        var ttsLabel = new Label("Text-To-Speech");
        node.mainContainer.Add(ttsLabel);

        var ttsContainer = new VisualElement();
        
        List<string> ttsChoices = new List<string>();
        foreach (var dType in Enum.GetValues(typeof(PiperVoices)).Cast<PiperVoices>())
            ttsChoices.Add(dType.ToString());
        
        var ttsSelectModel = new DropdownField("Voice", ttsChoices, 0);
        ttsContainer.Add(ttsSelectModel);
        var ttsButton = new Button();
        ttsButton.clicked += () => {
            if (PiperWrapper.active)
                return;
            
            if (Enum.TryParse(ttsSelectModel.value, out PiperVoices voice))
                _dialoguePiper = new DialoguePiper((int) voice);

            Debug.Log("Starting task");
            var ttsRequest = _dialoguePiper.TextToSpeechAsync(node.dialougeText);
            ttsRequest.ContinueWith((t) => {
                Debug.Log($"Status: {t.Status}");
            });
        };
        ttsButton.text = "Create Audio";
        ttsContainer.Add(ttsButton);
        node.mainContainer.Add(ttsContainer);

        Toggle allCorrectToggle = new Toggle("All Choices Correct") { value = allCorrect };
        allCorrectToggle.RegisterValueChangedCallback(e =>
        {
            node.allCorrect = e.newValue as bool? ?? false;
        });
        allCorrectToggle.SetValueWithoutNotify(node.allCorrect); 
        node.mainContainer.Add(allCorrectToggle);

        node.AddToClassList("main-container");

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(position, defaultNodeSize));

        return node;
    }
}
