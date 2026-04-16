using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Unity.VisualScripting;
using System.Collections;

public class TimeTracker : MonoBehaviour
{
    [System.Serializable]
    public class SceneTimes
    {
        public string scene;
        public float value;
    }

    [System.Serializable]
    public class SerializableDictionary
    {
        public List<SceneTimes> keyValuePairs = new List<SceneTimes>();

        // Method to add data from your dictionary to the serializable list
        public void AddData(string key, float value)
        {
            keyValuePairs.Add(new SceneTimes { scene = key, value = value });
        }

        // Method to convert the serializable list to a dictionary
        public Dictionary<string, float> ToDictionary()
        {
            Dictionary<string, float> dictionary = new Dictionary<string, float>();

            foreach (var pair in keyValuePairs)
            {
                dictionary[pair.scene] = pair.value;
            }

            return dictionary;
        }

        public bool ContainsKey(string key)
        {
            return keyValuePairs.Any(item => item.scene == key);
        }

        // A method to add or update a key-value pair
        public void AddOrUpdate(string key, float value)
        {
            var existingItem = keyValuePairs.FirstOrDefault(item => item.scene == key);

            if (existingItem != null)
            {
                existingItem.value = value;
            }
            else
            {
                keyValuePairs.Add(new SceneTimes { scene = key, value = value });
            }
        }

        // Method to convert a dictionary to the serializable list
        public void FromDictionary(Dictionary<string, float> dictionary)
        {
            keyValuePairs.Clear();

            foreach (var pair in dictionary)
            {
                keyValuePairs.Add(new SceneTimes { scene = pair.Key, value = pair.Value });
            }
        }
    }

    [System.Serializable]
    public class Session
    {
        public string startTime;
        public string endTime;
        public float totalDuration;
        public SerializableDictionary timeInScenes = new SerializableDictionary();
    }

    public static TimeTracker _instance;

    private float sessionStartTime;
    private float sceneStartTime;
    private bool isFirstTime = true;
    private Session currentSession;
    [SerializeField]
    private string currentSceneName;
    private string currentSceneNameOg;
    [SerializeField]
    private string currentSessionKey;
    private Dictionary<string, int> sceneVisitCounts = new Dictionary<string, int>();

    private bool isPaused = false;

    public float timeSpentInScene;
    private bool timerActive = false;

    private static TimeTracker Instance
    {
        get
        {
            if (_instance == null)
            {
                // Attempt to find an existing instance in the scene
                _instance = FindObjectOfType<TimeTracker>();

                if (_instance == null)
                {
                    // If no instance exists, create a new one
                    GameObject gameObject = new GameObject("TimeTracker");
                    _instance = gameObject.AddComponent<TimeTracker>();
                }

                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            // Make sure this object isn't destroyed when loading a new scene
            DontDestroyOnLoad(this.gameObject);
            _instance = this;
        }

    }

    private void Start()
    {
        PlayerPrefs.DeleteKey("a");
        // Hook into the scene changed event
        SceneManager.activeSceneChanged += OnSceneChanged;
        // Start the session and scene timers
        BeginNewSession();
        BeginNewScene(SceneManager.GetActiveScene().name);

        // Save the session data every 5 seconds
        InvokeRepeating(nameof(SaveAndUpdateData), 5f, 5f);

    }

    private void SaveAndUpdateData()
    {
        // Get current session and set the time
        currentSession.timeInScenes.AddOrUpdate(currentSceneName, timeSpentInScene);

        // Update the playerprefs
        currentSession.totalDuration = currentSession.timeInScenes.ToDictionary().Values.Sum();
        if (DataManager._instance != null)
            DataManager._instance.SaveDataNoApi(currentSessionKey, currentSession);
    }

    private void BeginNewSession()
    {
        sessionStartTime = Time.realtimeSinceStartup;
        currentSession = new Session
        {
            startTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        // Generate a unique session key
        currentSessionKey = GenerateSessionKey();
    }

    private void Update()
    {
        if (timerActive)
        {
            timeSpentInScene += Time.deltaTime;
            //Debug.Log("Time spent in scene " + currentSceneName + ": " + timeSpentInScene);
        }
    }

    private void BeginNewScene(string sceneName)
    {
        // Create a coroutine to update the time spent in the previous scene
        timerActive = true;
        timeSpentInScene = 0f;

        sceneStartTime = Time.realtimeSinceStartup;
        currentSceneNameOg = sceneName;
        currentSceneName = sceneName;
        if (sceneVisitCounts.ContainsKey(sceneName))
        {
            sceneVisitCounts[sceneName]++;
        }
        else
        {
            sceneVisitCounts.Add(sceneName, 1);
        }

        currentSceneName = sceneName + "_" + sceneVisitCounts[sceneName];

        if (!currentSession.timeInScenes.ContainsKey(currentSceneName))
        {
            currentSession.timeInScenes.AddOrUpdate(currentSceneName, 0f);
        }
    }

    private void UpdateSceneTime()
    {
        timerActive = false;
        //float timeSpentInScene = Time.realtimeSinceStartup - sceneStartTime;
        Debug.Log("Time spent in scene " + currentSceneName + ": " + timeSpentInScene);
        currentSession.timeInScenes.AddOrUpdate(currentSceneName, timeSpentInScene);
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        // Update time of the old scene
        UpdateSceneTime();

        // Start timing the new scene
        BeginNewScene(newScene.name);
    }

    private void UpdateSession()
    {
        SaveData();

        // Begin a new session
        BeginNewSession();
    }

    public void SaveData()
    {
        UpdateSceneTime();

        currentSession.endTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //currentSession.totalDuration = Time.realtimeSinceStartup - sessionStartTime;

        // Sum up the time spent in each scene
        currentSession.totalDuration = currentSession.timeInScenes.ToDictionary().Values.Sum();

        // Save session data, assuming you have the DataManager code from before
        Debug.Log("Saving session data");

        //Serialize the session data
        if (DataManager._instance != null)
            DataManager._instance.SaveData(currentSessionKey, currentSession, "time");
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }

    private void OnApplicationSuspend()
    {
        SaveData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        //SimulateThingy();
        if (isFirstTime)
        {
            isFirstTime = false;
            return;
        }
        if (pauseStatus)
        {
            if (isPaused)
                return;
            UpdateSceneTime();
            isPaused = true;
            if (PlayerPrefs.HasKey("a"))
            {
                string a = PlayerPrefs.GetString("a") + "updateSceneTime(" + pauseStatus + ")";
                PlayerPrefs.SetString("a", a);
            }
            else
            {
                PlayerPrefs.SetString("a", "updateSceneTime(" + pauseStatus + ")");
            }
            PlayerPrefs.Save();
        }
        else
        {
            if (!isPaused)
                return;
            // Start timing the new scene
            BeginNewScene(currentSceneNameOg);
            isPaused = false;
            if (PlayerPrefs.HasKey("a"))
            {
                string a = PlayerPrefs.GetString("a") + "beginNewSession(" + pauseStatus + ")";
                PlayerPrefs.SetString("a", a);
            }
            else
            {
                PlayerPrefs.SetString("a", "beginNewSession(" + pauseStatus + ")");
            }
            PlayerPrefs.Save();
        }
    }

    public void SimulateThingy()
    {
        if (isFirstTime)
        {
            isFirstTime = false;
            return;
            // Todo: also make a check if this is needed to be reset when changing scenes
        }
        UpdateSession();
    }

    private void OnDestroy()
    {
        // Unhook from the scene changed event to avoid any potential issues
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }
    private string GenerateSessionKey()
    {
        return "Session-" + System.DateTime.Now.ToString("yyyyMMddHHmmssfff");
    }

}
