using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DialougeGraph : EditorWindow
{
    private DialougeGraphView _graphView;
    private string _fileName = "New Dialouge";
    
    private DialogueContainer _dialogueContainer;
    
    private ObjectField _dialogueField;

    [MenuItem("Graph/Dialouge Graph")]
    public static void OpenDialougeGraphWindow()
    {
        var window = GetWindow<DialougeGraph>();
        window.titleContent = new GUIContent(text: "Dialouge Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
        GenerateMinimap();
    }

    private void GenerateMinimap()
    {
        var minimap = new MiniMap { anchored = true };
        minimap.SetPosition(new Rect(x: 10, y: 30, width: 200, height: 140));
        _graphView.Add(minimap);
    }

    private void ConstructGraphView()
    {
        _graphView = new DialougeGraphView(this)
        {
            name = "Dialouge Graph"
        };

        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();
        var fileName = new TextField(label: "Dialogue Name:");
        fileName.SetValueWithoutNotify(_fileName);
        fileName.MarkDirtyRepaint();
        fileName.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
        
        _dialogueField = new ObjectField(label: "Dialogue Container:");
        _dialogueField.allowSceneObjects = false;
        _dialogueField.objectType = typeof(DialogueContainer);
        _dialogueField.SetValueWithoutNotify(_dialogueContainer);
        _dialogueField.MarkDirtyRepaint();
        _dialogueField.RegisterValueChangedCallback(evt =>
        {
            _dialogueContainer = evt.newValue as DialogueContainer;
            if (_dialogueContainer != null)
                _fileName = _dialogueContainer.name;
            fileName.value = _fileName;
            fileName.MarkDirtyRepaint();
        });
        toolbar.Add(_dialogueField);
        toolbar.Add(new Button(() => RequestData(false)) { text = "Load" });
        toolbar.Add(new Button(() => RequestData(true)) { text = "Save" });



        toolbar.Add(fileName);
        toolbar.Add(new Button(() => CreateDialogue()) { text = "Create Dialogue" });

        rootVisualElement.Add(toolbar);
    }

    private void CreateDialogue()
    {
        // Create new input field to write name
        if (string.IsNullOrEmpty(_fileName))
        {
            EditorUtility.DisplayDialog(title: "Invalid file name!", message: "Please enter a valid file name.", ok: "OK");
            return;
        }

        var saveUtility = GraphSaveUtil.GetInstance(_graphView);
        _dialogueContainer = saveUtility.SaveGraph(_fileName);
        _dialogueField.MarkDirtyRepaint();
        _dialogueField.value = _dialogueContainer;
    }

    private void RequestData(bool save)
    {
        if (_dialogueContainer == null)
        {
            EditorUtility.DisplayDialog(title: "No Dialogue Container Selected", message: "Please select a valid Dialogue.", ok: "OK");
            return;
        }

        var saveUtility = GraphSaveUtil.GetInstance(_graphView);

        if (save)
        {
            _dialogueContainer = saveUtility.SaveGraph(_fileName);
            _dialogueField.MarkDirtyRepaint();
            _dialogueField.value = _dialogueContainer;
        }
        else
            saveUtility.LoadGraph(_dialogueContainer);
    }


    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }
}