using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class QuestPoint : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private QuestInfoScriptableObject questInfoForPoint;
    [SerializeField] private DialogueNode dialogueNodeStart;
    [SerializeField] private DialogueNode dialogueNodeInProgress;
    [SerializeField] private DialogueNode dialogueNodeEnd;

    [Header("Config")]
    [SerializeField] private bool startPoint;
    [SerializeField] private bool endPoint;

    [SerializeField] private Transform spawnPosition;
    [SerializeField] private Transform panelSpawnPosition;


    private bool playerInRange = false;
    private string questId;
    private QuestState currentQuestState;

    private bool dialogueInProgress = false;
    private string dialogueInProgressId;
    private bool dialoguePicking = false;

    private QuestIcon questIcon;

    private void Awake()
    {
        questId = questInfoForPoint.id;
        questIcon = GetComponentInChildren<QuestIcon>();
    }

    private void OnEnable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
        // when button pressed near npc... or maybe broadcast when button pressed to start quest
        // for now sam mam na button press... instead of other event ko activira quest
        GameEventsManager.instance.inputEvents.onAPressed += SubmitPressed;
        GameEventsManager.instance.dialogueEvents.onDialogueInProgress += SetDialogueInProgress;
        GameEventsManager.instance.dialogueEvents.onDialoguePicking += SetDialoguePicking;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
        GameEventsManager.instance.inputEvents.onAPressed -= SubmitPressed;
        GameEventsManager.instance.dialogueEvents.onDialogueInProgress -= SetDialogueInProgress;
        GameEventsManager.instance.dialogueEvents.onDialoguePicking -= SetDialoguePicking;
    }

    private void SubmitPressed()
    {
        if (!playerInRange)
            return;

        // Start / Finish a quest
        if (currentQuestState.Equals(QuestState.CAN_START) && startPoint && !dialoguePicking)
        {
            // Enable the panel
            GameEventsManager.instance.dialogueEvents.EnablePanel();
            //Move the player to the spawn position
            GameEventsManager.instance.movementEvents.TeleportPlayer(spawnPosition);
            // Move the dialogue panel to the spawn position
            GameEventsManager.instance.dialogueEvents.MovePanel(panelSpawnPosition);
            GameEventsManager.instance.movementEvents.DisableMovement();
            GameEventsManager.instance.movementEvents.DisableTeleportation();

            GameEventsManager.instance.dialogueEvents.StartDialogue(dialogueNodeStart.id, questId, true);
            dialogueInProgressId = dialogueNodeStart.id;
        }
        else if (currentQuestState.Equals(QuestState.CAN_START) && startPoint && dialogueInProgress)
        {
            GameEventsManager.instance.dialogueEvents.SelectAdvanceOrFinishDialogue(dialogueNodeStart.id, questId, true);
        }
        else if (currentQuestState.Equals(QuestState.IN_PROGRESS) && startPoint && dialogueInProgress)
        {
            GameEventsManager.instance.dialogueEvents.SelectAdvanceOrFinishDialogue(dialogueInProgressId, questId, true);
        }
        else if (currentQuestState.Equals(QuestState.IN_PROGRESS) && startPoint)
        {
            // Enable the panel
            GameEventsManager.instance.dialogueEvents.EnablePanel();
            //Move the player to the spawn position
            GameEventsManager.instance.movementEvents.TeleportPlayer(spawnPosition);
            // Move the dialogue panel to the spawn position
            GameEventsManager.instance.dialogueEvents.MovePanel(panelSpawnPosition);
            GameEventsManager.instance.movementEvents.DisableMovement();
            GameEventsManager.instance.movementEvents.DisableTeleportation();
            GameEventsManager.instance.dialogueEvents.StartDialogue(dialogueNodeInProgress.id, questId, true);
            dialogueInProgressId = dialogueNodeInProgress.id;
        }
        else if (currentQuestState.Equals(QuestState.FINISHED) && endPoint && dialogueInProgress)
        {
            GameEventsManager.instance.dialogueEvents.SelectAdvanceOrFinishDialogue(dialogueNodeEnd.id, questId, true);
        }
        else if (currentQuestState.Equals(QuestState.CAN_FINISH) && endPoint)
        {
            // Enable the panel
            GameEventsManager.instance.dialogueEvents.EnablePanel();
            //Move the player to the spawn position
            GameEventsManager.instance.movementEvents.TeleportPlayer(spawnPosition);
            // Move the dialogue panel to the spawn position
            GameEventsManager.instance.dialogueEvents.MovePanel(panelSpawnPosition);
            GameEventsManager.instance.movementEvents.DisableMovement();
            GameEventsManager.instance.movementEvents.DisableTeleportation();

            GameEventsManager.instance.dialogueEvents.StartDialogue(dialogueNodeEnd.id, questId, true);

            GameEventsManager.instance.questEvents.FinishQuest(questId);
            //get the dialogue point
            //dialogueId = GetComponent<DialoguePoint>().dialougeId;
            //DialoguePoint dialoguePoint = GetComponent<DialoguePoint>();
            //dialoguePoint.MovePanel();
            //dialoguePoint.MovePlayer();
            //GameEventsManager.instance.dialogueEvents.AdvanceDialogue(dialogueId);
        }
    }

    private void QuestStateChange(Quest quest)
    {
        if (quest.questInfo.id.Equals(questId))
        {
            currentQuestState = quest.state;
            questIcon.SetState(currentQuestState, startPoint, endPoint);
            Debug.Log("Quest with id: " + questId + " updated to state: " + currentQuestState);
        }
    }

    private void SetDialogueInProgress(string dialogueId, bool value, bool reset)
    {
        if (dialogueNodeStart.id.Equals(dialogueId))
        {
            if (reset)
            {
                GameEventsManager.instance.dialogueEvents.ResetDialogue(dialogueId);
            }
            dialogueInProgress = value;
        }
        else if (dialogueNodeInProgress.id.Equals(dialogueId))
        {
            dialogueInProgress = value;
            if (value == false && !currentQuestState.Equals(QuestState.CAN_FINISH))
                GameEventsManager.instance.dialogueEvents.ResetDialogue(dialogueId);
        }
        else if (dialogueNodeEnd.id.Equals(dialogueId))
        {
            dialogueInProgress = value;
        }
    }

    private void SetDialoguePicking(bool value)
    {
        dialoguePicking = value;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            Debug.Log("Player in range");
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            Debug.Log("Player out of range");
            playerInRange = false;
        }
    }
}
