using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueReseter : MonoBehaviour
{
    public GameObject resetButton;
    string dialogueId = "";

    private void Start()
    {
        if (dialogueId == "")
        {
            resetButton.SetActive(false);
        }
    }

    public void DialogueStart(string id)
    {
        dialogueId = id;
        resetButton.SetActive(true);
    }

    public void DialogueEnd()
    {
        dialogueId = "";
        resetButton.SetActive(false);
    }

    public void Press()
    {
        Debug.Log("Resetting dialogue " + dialogueId);
        if (dialogueId != "")
        {
            GameEventsManager.instance.dialogueEvents.DisablePanel();
            GameEventsManager.instance.dialogueEvents.ResetDialogue(dialogueId);
        }
    }

    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}