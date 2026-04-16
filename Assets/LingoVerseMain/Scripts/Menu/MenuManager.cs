using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject aboutPanel;
    public GameObject mainPanel;
    public GameObject gameModePanel;
    public GameObject voiceLevelsPanel;
    public GameObject noVoiceLevelsPanel;

    public void Start() {
        settingsPanel.SetActive(false);
        aboutPanel.SetActive(false);
        mainPanel.SetActive(mainPanel.activeSelf);
        voiceLevelsPanel.SetActive(false);
        noVoiceLevelsPanel.SetActive(false);
        gameModePanel.SetActive(false);
        Debug.Log("MenuManager");
    }

    public void GoToScene(int sceneIndex)
    {
        GameEventsManager.instance.sceneEvents.SceneLoad();
        SceneTransitionManager.singleton.GoToSceneAsync(sceneIndex);
    }

    public void ShowVoiceLevels()
    {
        // Hide the main panel
        mainPanel.SetActive(false);
        // Hide the About panel
        aboutPanel.SetActive(false);
        // Hide the Settings panel
        settingsPanel.SetActive(false);
        gameModePanel.SetActive(false);
        voiceLevelsPanel.SetActive(!voiceLevelsPanel.activeSelf);
    }

    public void ShowNoVoiceLevels() {
        // Hide the main panel
        mainPanel.SetActive(false);
        // Hide the About panel
        aboutPanel.SetActive(false);
        // Hide the Settings panel
        settingsPanel.SetActive(false);
        gameModePanel.SetActive(false);
        noVoiceLevelsPanel.SetActive(!noVoiceLevelsPanel.activeSelf);
    }

    public void ShowGameMode() {
        mainPanel.SetActive(false);
        aboutPanel.SetActive(false);
        settingsPanel.SetActive(false);
        gameModePanel.SetActive(!gameModePanel.activeSelf);
    }

    public void ShowMainPanel()
    {
        voiceLevelsPanel.SetActive(false);
        aboutPanel.SetActive(false);
        settingsPanel.SetActive(false);
        noVoiceLevelsPanel.SetActive(false);
        gameModePanel.SetActive(false);
        mainPanel.SetActive(!mainPanel.activeSelf);
    }

    public void Tutorial()
    {
        // Unsubscribe from all events
        //UnsubscribeFromEvents();
        GameEventsManager.instance.sceneEvents.SceneLoad();
        SceneTransitionManager.singleton.GoToSceneAsync(1);
    }

    public void Settings()
    {
        // Hide the About panel
        aboutPanel.SetActive(false);
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void About()
    {
        // Hide the Settings panel
        settingsPanel.SetActive(false);
        aboutPanel.SetActive(!aboutPanel.activeSelf);
    }

    public void QuitGame()
    {
        // This will quit the application when running as a standalone build.
        // It won't work in the Unity Editor.
        Application.Quit();
    }

    private void UnsubscribeFromEvents()
    {
        // Unsubscribe from all events
        GameEventsManager.instance.movementEvents = null;
        GameEventsManager.instance.questEvents = null;
        GameEventsManager.instance.itemEvents = null;
        GameEventsManager.instance.npcEvents = null;
        GameEventsManager.instance.inputEvents = null;
        GameEventsManager.instance.dialogueEvents = null;
        GameEventsManager.instance.audioEvents = null;
        GameEventsManager.instance.scoreEvents = null;
        GameEventsManager.instance.playerEvents = null;
    }
}