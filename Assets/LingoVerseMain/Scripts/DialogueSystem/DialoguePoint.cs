using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class DialoguePoint : MonoBehaviour
{
    [Header("Start Node")]
    [SerializeField] private DialogueContainer startNode;

    //add the spawn position to place the player at the start of the dialogue
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private Transform panelSpawnPosition;

    private bool playerInRange = false;
    public string dialougeId;
    private DialogueState currentDialogueState;
    public QuestInfoScriptableObject questInfoForPoint;
    public bool repeatable = false;

    private DialogueIcon dialogueIcon;

    private void Awake()
    {
        if (startNode == null)
        {
            Debug.LogWarning($"Start Node not set for dialogue point: {gameObject.name}. Disabling Script");
            // Disable the script if the start node is not set
            enabled = false;
            return;
        }
        dialougeId = startNode.NodeLinks.First().TargetNodeGUID;
        dialogueIcon = GetComponentInChildren<DialogueIcon>();
    }

    private void OnEnable()
    {
        GameEventsManager.instance.inputEvents.onAPressed += SubmitPressed;
        GameEventsManager.instance.dialogueEvents.onDialogueStateChange += DialogueStateChange;
        GameEventsManager.instance.dialogueEvents.onIconStateChange += ChangeIconState;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.inputEvents.onAPressed -= SubmitPressed;
        GameEventsManager.instance.dialogueEvents.onDialogueStateChange -= DialogueStateChange;
        GameEventsManager.instance.dialogueEvents.onIconStateChange -= ChangeIconState;
    }

    private void SubmitPressed()
    {
        Debug.Log("Submit Pressed DIalogue Point");
        Debug.Log("Player in range: " + playerInRange);
        if (!playerInRange || currentDialogueState.Equals(DialogueState.WAITING_FOR_QUEST_FINISH))
            return;

        // todo kle itak ce je repeatable in finished ne bo slo... this needs to change when i add repeatable dialogue
        if (!repeatable && currentDialogueState.Equals(DialogueState.FINISHED))
            return;


        // Start / Finish a quest
        if (currentDialogueState.Equals(DialogueState.CAN_START))
        {
            //Move the player to the spawn position
            //z();
            // Move the dialogue panel to the spawn position
            MovePanel();
            GameEventsManager.instance.dialogueEvents.StartDialogue(dialougeId, "");
            //disable movement and teleportation
            //GameEventsManager.instance.movementEvents.DisableMovement();
            //GameEventsManager.instance.movementEvents.DisableTeleportation();
        }
        else if (currentDialogueState.Equals(DialogueState.IN_PROGRESS))
        {
            GameEventsManager.instance.dialogueEvents.AdvanceDialogue(dialougeId, "");
        }
        else if (currentDialogueState.Equals(DialogueState.IN_PROGRESS_WHISPER) || currentDialogueState.Equals(DialogueState.IN_PROGRESS_MULTIPLE_CHOICE))
        {
            // Waiting for either Correct Button Pressed or Whisper to finish
            return;
        }
        else if (currentDialogueState.Equals(DialogueState.CAN_FINISH_DIALOGUE))
        {
            GameEventsManager.instance.dialogueEvents.FinishDialogue(dialougeId);
            //enable movement and teleportation
            GameEventsManager.instance.movementEvents.EnableMovement();
            GameEventsManager.instance.movementEvents.EnableTeleportation();

            // Add timer to enable after some time
            // if repatable and finished, reset to can start
            if (repeatable && currentDialogueState.Equals(DialogueState.FINISHED))
            {
                GameEventsManager.instance.dialogueEvents.AddQuestToTimer(dialougeId, 5);
            }
        }

    }

    private void ChangeIconState(DialogueState state, string id)
    {
        if (id.Equals(dialougeId))
        {
            dialogueIcon.SetState(state);
        }
    }
    public void MovePlayer()
    {
        //Move the player to the spawn position and rotation
        if (spawnPosition == null)
        {
            Debug.LogWarning("Spawn position not set for dialogue point: " + gameObject.name);
            return;
        }
        GameEventsManager.instance.movementEvents.TeleportPlayer(spawnPosition);
    }

    public void MovePanel()
    {
        //Move the dialogue panel to the spawn position and rotation
        if (panelSpawnPosition == null)
        {
            Debug.LogWarning("Panel spawn position not set for dialogue point: " + gameObject.name);
            return;
        }
        GameEventsManager.instance.dialogueEvents.MovePanel(panelSpawnPosition);
    }

    private void DialogueStateChange(Dialogue dialogue)
    {
        if (dialogue.dialogueData.NodeGUID.Equals(dialougeId))
        {
            currentDialogueState = dialogue.state;
            Debug.Log("Dialogue with id: " + dialougeId + " updated to state: " + currentDialogueState);
        }

    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            playerInRange = false;
            // Close and reset the dialogue panel
            GameEventsManager.instance.dialogueEvents.DisablePanel();
            GameEventsManager.instance.dialogueEvents.ResetDialogue(dialougeId);
        }
    }
}
