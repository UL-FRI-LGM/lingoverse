using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSwitcher : MonoBehaviour
{
    public void SwitchScene(int sceneIndex)
    {
        GameEventsManager.instance.sceneEvents.SceneLoad();
        SceneTransitionManager.singleton.GoToSceneAsync(sceneIndex);
    }
}
