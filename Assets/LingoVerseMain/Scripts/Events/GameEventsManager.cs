using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager instance { get; private set; }

    // Add all the possible events
    public MovementEvents movementEvents;
    public QuestEvents questEvents;
    public ItemEvents itemEvents;
    public NPCEvents npcEvents;
    public InputEvents inputEvents;
    public DialogueEvents dialogueEvents;
    public AudioEvents audioEvents;
    public ScoreEvents scoreEvents;
    public PlayerEvents playerEvents;
    public SceneEvents sceneEvents;
    public ApiEvents apiEvents;
    
    // New Whisper Events
    public WhisperEvents whisperEvents;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one instance of Game Events Manager");
        }
        instance = this;

        // init events
        movementEvents = new MovementEvents();
        questEvents = new QuestEvents();
        itemEvents = new ItemEvents();
        npcEvents = new NPCEvents();
        inputEvents = new InputEvents();
        dialogueEvents = new DialogueEvents();
        audioEvents = new AudioEvents();
        scoreEvents = new ScoreEvents();
        playerEvents = new PlayerEvents();
        sceneEvents = new SceneEvents();
        apiEvents = new ApiEvents();
        whisperEvents = new WhisperEvents();
    }
}
