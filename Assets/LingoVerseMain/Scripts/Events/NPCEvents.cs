using System;

public class NPCEvents
{
    public event Action<DialogueNode> onQuestStarted;
    public void QuestStarted(DialogueNode dialogueData)
    {
        onQuestStarted?.Invoke(dialogueData);
    }
}
