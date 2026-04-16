using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueIcon : MonoBehaviour
{
    [Header("Icons")]
    [SerializeField] private GameObject canStartIcon;

    public void SetState(DialogueState state)
    {
        // set all icons to inactive
        canStartIcon.SetActive(false);

        // set based on state
        switch (state)
        {
            case DialogueState.CAN_START:
                canStartIcon.SetActive(true);
                break;
            case DialogueState.FINISHED:
                break;
            default:
                Debug.LogWarning("Dialogue State not recognized by the switch for dialogue icon: " + state);
                break;
        }
    }
}
