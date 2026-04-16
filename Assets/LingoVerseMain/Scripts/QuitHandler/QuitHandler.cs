using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitHandler : MonoBehaviour
{
    public static QuitHandler instance { get; private set; }
    public bool canQuit = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            // Make sure this object isn't destroyed when loading a new scene
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
    }

    private void OnEnable()
    {
        //Application.wantsToQuit += HandleQuit;
    }

    private void OnDisable()
    {
        //Application.wantsToQuit -= HandleQuit;
    }

    public bool HandleQuit()
    {
        if (canQuit)
        {
            return true;
        }
        else
        {
            // If the player can't quit, show a message
            Debug.Log("Can't quit yet!");
            // Call api to post

            TimeTracker._instance.SaveData();
            return false;
        }
    }

    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
