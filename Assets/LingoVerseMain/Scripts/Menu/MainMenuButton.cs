using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuButton : MonoBehaviour
{
    // Go to the main menu
    public void GoToMainMenu()
    {
        //// Unsubscribe from all events
        //GameEventsManager.instance.movementEvents = null;
        //GameEventsManager.instance.questEvents = null;
        //GameEventsManager.instance.itemEvents = null;
        //GameEventsManager.instance.npcEvents = null;
        //GameEventsManager.instance.inputEvents = null;
        //GameEventsManager.instance.dialogueEvents = null;
        //GameEventsManager.instance.audioEvents = null;
        //GameEventsManager.instance.scoreEvents = null;
        //GameEventsManager.instance.playerEvents = null;

        // Load the main menu scene
        GameEventsManager.instance.sceneEvents.SceneLoad();
        SceneTransitionManager.singleton.GoToSceneAsync(0);
    }
}
