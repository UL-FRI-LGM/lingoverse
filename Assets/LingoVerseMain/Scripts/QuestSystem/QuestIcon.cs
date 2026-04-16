using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestIcon : MonoBehaviour
{
    [Header("Icons")]
    [SerializeField] private GameObject requrementsNotMetToStartIcon;
    [SerializeField] private GameObject canStartIcon;
    [SerializeField] private GameObject requrementsNotMetToFinishIcon;
    [SerializeField] private GameObject canFinishIcon;

    public void SetState(QuestState state, bool startPoint, bool endPoint)
    {
        // set all icons to inactive
        requrementsNotMetToStartIcon.SetActive(false);
        canStartIcon.SetActive(false);
        requrementsNotMetToFinishIcon.SetActive(false);
        canFinishIcon.SetActive(false);

        // set based on state
        switch (state)
        {
            case QuestState.REQUIREMENTS_NOT_MET:
                if (startPoint)
                    requrementsNotMetToStartIcon.SetActive(true);
                break;
            case QuestState.CAN_START:
                if (startPoint) canStartIcon.SetActive(true); 
                break;
            case QuestState.IN_PROGRESS:
                if (endPoint) requrementsNotMetToFinishIcon.SetActive(true) ;
                break;
            case QuestState.CAN_FINISH:
                if (endPoint) canFinishIcon.SetActive(true);
                break;
            case QuestState.FINISHED:
                break;
            default:
                Debug.LogWarning("Quest State not recognized by the switch for quest icon: " + state);
                break;
        }
    }
}
