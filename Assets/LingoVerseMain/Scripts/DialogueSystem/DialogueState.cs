using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueState
{
    CAN_START,
    IN_PROGRESS,
    IN_PROGRESS_MULTIPLE_CHOICE,
    IN_PROGRESS_WHISPER,
    CAN_FINISH_DIALOGUE,
    WAITING_FOR_QUEST_FINISH,
    FINISHED
}
