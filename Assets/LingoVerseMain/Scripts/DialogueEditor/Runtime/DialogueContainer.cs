using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueContainer : ScriptableObject
{
    public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
    public List<DialogueNodeData> DialogueNodeData = new List<DialogueNodeData>();


    public DialogueNodeData GetStartNode()
    {
        DialogueNodeData startNode;

        foreach (var nodelink in NodeLinks)
        {
            if (nodelink.PortName.Equals("Next"))
            {
                string currentGUID = nodelink.TargetNodeGUID;
                startNode = DialogueNodeData.Find(x => x.NodeGUID == currentGUID);
                return startNode;
            }
        }
        Debug.LogError("Start node not found in the dialogue container " + name);
        return null;
    }
}
