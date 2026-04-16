using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtil
{
    private DialougeGraphView targetGraphView;
    private DialogueContainer containerCache;

    private List<Edge> edges => targetGraphView.edges.ToList();
    private List<DialougeEditorNode> nodes => targetGraphView.nodes.ToList().Cast<DialougeEditorNode>().ToList();

    public static GraphSaveUtil GetInstance(DialougeGraphView targetGraphView)
    {
        return new GraphSaveUtil
        {
            targetGraphView = targetGraphView
        };
    }

    public DialogueContainer SaveGraph(string fileName)
    {
        if (!edges.Any() && nodes.Count == 1) return null;
        
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
        
        // Check if DialogueContainer exists
        DialogueContainer dialougeContainer = Resources.Load<DialogueContainer>($"Dialogues/{fileName}");
        bool isExistingContainer = dialougeContainer != null;

        if (!isExistingContainer) {
            dialougeContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        }
        else {
            // Clear all existing data of the container to recreate it on save
            dialougeContainer.NodeLinks.Clear();
            dialougeContainer.DialogueNodeData.Clear();
        }
        
        // Make sure there is no entry point with the same GUID
        DialougeEditorNode startNode = null;
        foreach (var edge in edges)
        {
            Debug.Assert(edge.output.node != null, $"{edge} has a null output node");
            var outputNode = edge.output.node as DialougeEditorNode;
            if (outputNode.entryPoint)
            {
                // Start node is not the entry point but the first Node
                startNode = edge.input.node as DialougeEditorNode;
            }
        }
        
        if (startNode != null)
        {
            // Check all DialougeEditorNodes for the same GUID
            List<DialogueContainer> allDialogues = Resources.LoadAll<DialogueContainer>("Dialogues").ToList();
            foreach (DialogueContainer diagCheck in allDialogues)
            {
                
                if (diagCheck.name.Equals(dialougeContainer.name)) continue;
                DialogueNodeData sNode = diagCheck.GetStartNode();
                if (sNode.NodeGUID.Equals(startNode.guid))
                {
                    Debug.Log("Found same GUID");
                    startNode.guid = Guid.NewGuid().ToString();
                }
            }
        }
        

        var connectedPorts = edges.Where(x => x.input.node != null).ToArray();
        foreach ( var connectedPort in connectedPorts)
        {
            var outputNode = connectedPort.output.node as DialougeEditorNode; 
            var inputNode = connectedPort.input.node as DialougeEditorNode;
            
            if (outputNode == null || inputNode == null) continue;
            
            dialougeContainer.NodeLinks.Add(new NodeLinkData
            {
                BaseNodeGUID = outputNode.guid,
                PortName = connectedPort.output.portName,
                TargetNodeGUID = inputNode.guid
            });
        }

        // Refresh
        foreach (var dialougeNode in nodes.Where(node => !node.entryPoint))
        {
            dialougeContainer.DialogueNodeData.Add(new DialogueNodeData
            (
                dialougeNode.guid,
                dialougeNode.dialougeText,
                dialougeNode.GetPosition().position,
                dialougeNode.aud,
                dialougeNode.allCorrect,
                dialougeNode.dialougeType
            ));
        }

        if (isExistingContainer) {
            Debug.Log("Saving existing container");
            EditorUtility.SetDirty(dialougeContainer);
            AssetDatabase.SaveAssetIfDirty(dialougeContainer);
        }
        else {
            Debug.Log("Creating new container");
            AssetDatabase.CreateAsset(dialougeContainer, $"Assets/Resources/Dialogues/{fileName}.asset");
        }
        
        return Resources.Load<DialogueContainer>($"Dialogues/{fileName}");
    }

    public DialogueContainer LoadGraph(string fileName)
    {
        containerCache = Resources.Load<DialogueContainer>($"Dialogues/{fileName}");

        if (containerCache == null)
        {
            EditorUtility.DisplayDialog(title: "File not found!", message: "Target dialouge graph file does not exist!", ok: "OK");
            return null;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
        return containerCache;
    }

    public void LoadGraph(DialogueContainer dialougeContainer)
    {
        containerCache = dialougeContainer;
        if (containerCache == null)
        {
            EditorUtility.DisplayDialog(title: "Invalid container", message: "Dialogue Container does not exist!", ok: "OK");
            return;
        }
        ClearGraph();
        CreateNodes();
        ConnectNodes();
    }

    private void ClearGraph()
    {
        var firstNode = nodes.Find(x => x.entryPoint);
        
        // Nothing to Clear
        if (firstNode == null) return;
        
        if (containerCache.NodeLinks.Count > 0)
            firstNode.guid = containerCache.NodeLinks[0].BaseNodeGUID;

        foreach (var dialougeNode in nodes)
        {
            if (dialougeNode.entryPoint) continue;

            edges.Where(x => x.input.node == dialougeNode).ToList()
                .ForEach(edge => targetGraphView.RemoveElement(edge));
            targetGraphView.RemoveElement(dialougeNode);
        }
    }

    private void CreateNodes()
    {
        foreach (var nodeData in containerCache.DialogueNodeData)
        {
            DialougeEditorNode tempNode = targetGraphView.GenerateDialougeEditorNode(nodeData.DialogueText,
                nodeData.AudioClip, Vector2.zero, nodeData.allCorrect, nodeData.dialogueType);
            tempNode.guid = nodeData.NodeGUID;
            targetGraphView.AddElement(tempNode);
            List<NodeLinkData> nodePorts = containerCache.NodeLinks.Where(x => x.BaseNodeGUID == nodeData.NodeGUID).ToList();
            nodePorts.ForEach(x => targetGraphView.AddChoicePort(tempNode, x.PortName));
        }
    }

    private void ConnectNodes()
    {
        for (var i = 0; i < nodes.Count; i++)
        {
            var connections = containerCache.NodeLinks.Where(x => x.BaseNodeGUID == nodes[i].guid).ToList();
            for (var j = 0; j < connections.Count; j++)
            {
                var targetNodeGuid = connections[j].TargetNodeGUID;
                var targetNode = nodes.First(x => x.guid == targetNodeGuid);
                LinkNodes(nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);
                targetNode.SetPosition(new Rect(containerCache.DialogueNodeData.First(x => x.NodeGUID == targetNodeGuid).Position, targetGraphView.defaultNodeSize));
            }
        }
    }

    private void LinkNodes(Port output, Port input)
    {
        var tempEdge = new Edge
        {
            output = output,
            input = input
        };

        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);

        targetGraphView.Add(tempEdge);
    }
}
