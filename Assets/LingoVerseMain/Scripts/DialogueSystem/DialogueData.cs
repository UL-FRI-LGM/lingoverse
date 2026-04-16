using System;

[Serializable]
public class DialogueData
{
    public DialogueState state;
    public DialogueNode currentNode;
    public bool completed;
    // Add if corretly answered??? Todo

    public DialogueData(DialogueState state, bool completed, DialogueNode currentNode)
    {
        this.state = state;
        this.completed = completed;
        this.currentNode = currentNode;
    }
}
