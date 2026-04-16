using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEvents
{
    public Action onSceneLoad;
    public void SceneLoad()
    {
        onSceneLoad?.Invoke();
    }
}
