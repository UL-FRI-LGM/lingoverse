using System;
using System.IO;
using UnityEngine;

[System.Serializable]
public class DataWrapper<T>
{
    public string playerId;
    public string prefKey;
    public string timeStamp;
    public T data;
    public bool postedToAPI;
}

public class DataManager : MonoBehaviour
{

    public static DataManager _instance { get; private set; }

    private string fileName;
    private string playTimeFileName;
    private StreamWriter fileWriter;
    private StreamWriter playTimeWriter;

    private void Start()
    {
        Debug.Log(Application.persistentDataPath);

        fileName = $"data_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.csv";
        fileWriter = new StreamWriter(Path.Combine(Application.persistentDataPath, fileName), append: true);
        fileWriter.AutoFlush = true;

        playTimeFileName = $"playtime_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.csv";
        playTimeWriter = new StreamWriter(Path.Combine(Application.persistentDataPath, playTimeFileName), append: true);
        playTimeWriter.AutoFlush = true;
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

        GameEventsManager.instance.apiEvents.onCompletedJson += OnCompletedJson;
        GameEventsManager.instance.apiEvents.onInDatabase += InDatabase;
        GameEventsManager.instance.apiEvents.onNotInDatabase += NotInDatabase;

        // Check for any unsent data and try to send it. Try to send it every 10 seconds.
        InvokeRepeating(nameof(SendUnsentData), 0f, 10f);
    }

    private void OnDestroy()
    {
        GameEventsManager.instance.apiEvents.onCompletedJson -= OnCompletedJson;
        GameEventsManager.instance.apiEvents.onInDatabase -= InDatabase;
        GameEventsManager.instance.apiEvents.onNotInDatabase -= NotInDatabase;
    }

    public void InDatabase(string key)
    {
        // Remove the key from the list of UnsentData
        string unsentDataKey = "UnsentData";
        if (PlayerPrefs.HasKey(unsentDataKey))
        {
            string unsentDataString = PlayerPrefs.GetString(unsentDataKey, "");
            string[] unsentDataArray = unsentDataString.Split("===");
            foreach (string data in unsentDataArray)
            {
                if (!string.IsNullOrEmpty(data))
                {
                    // Split by --
                    string[] dataSplit = data.Split("--");
                    // first elment is the key, second is the api path
                    string keyInList = dataSplit[0];

                    if (data == key)
                    {
                        unsentDataString = unsentDataString.Replace(data + "===", "");
                        PlayerPrefs.SetString(unsentDataKey, unsentDataString);
                        PlayerPrefs.Save();
                        break;
                    }
                }
            }
        }
    }   

    public void NotInDatabase(string apiPath, string key)
    {
        // get the data from the player prefs
        string data = PlayerPrefs.GetString(key);

        ApiManager._instance.TryPostRequest(data, apiPath, key);
    }

    // TODO: Add button to sync all player prefs

    public void SendUnsentData()
    {
        if (_instance != null)
        {
            string unsentDataKey = "UnsentData";
            if (PlayerPrefs.HasKey(unsentDataKey))
            {
                string unsentDataString = PlayerPrefs.GetString(unsentDataKey, "");
                string[] unsentDataArray = unsentDataString.Split("===");
                foreach (string data in unsentDataArray)
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        // Split by --
                        string[] dataSplit = data.Split("--");
                        // first elment is the key, second is the api path
                        string key = dataSplit[0];
                        string apiPath = dataSplit[1];

                        // Check if the data has already been posted to the API
                        ApiManager._instance.TryGet(apiPath, key);
                    }
                }
            }
        }
    }

    public void OnCompletedJson(string json)
    {
        // Remove the data from the list of UnsentData
        string unsentDataKey = "UnsentData";
        if (PlayerPrefs.HasKey(unsentDataKey))
        {
            string unsentDataString = PlayerPrefs.GetString(unsentDataKey, "");
            string[] unsentDataArray = unsentDataString.Split("===");
            foreach (string data in unsentDataArray)
            {
                if (!string.IsNullOrEmpty(data))
                {
                    // Split by --
                    string[] dataSplit = data.Split("--");
                    // first elment is the key, second is the api path
                    string key = dataSplit[0];
                    string apiPath = dataSplit[1];

                    string jsonString = PlayerPrefs.GetString(key);

                    if (jsonString.Contains(json))
                    {
                        unsentDataString = unsentDataString.Replace(data + "===", "");
                        PlayerPrefs.SetString(unsentDataKey, unsentDataString);
                        PlayerPrefs.Save();
                        break;
                    }
                }
            }
        }
    }

    public void SaveDataNoApi<T>(string key, T data)
    {
        key = PlayerManager.instance.GetPlayerId() + "-" + key;
        DataWrapper<T> wrapper = new DataWrapper<T>
        {
            playerId = key,
            prefKey = PlayerManager.instance.GetPlayerId(),
            timeStamp = DateTime.Now.ToString("yyyy/MM/dd/HH:mm:ss"),
            data = data,
            postedToAPI = false
        };

        string jsonString = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(key, jsonString);
        // dEbug.Log("Saving data: " + jsonString);
        Debug.Log("Saving data: " + jsonString);

        // Save json string to player prefs in list of UnsentData
        string unsentDataKey = "UnsentData";
        if (PlayerPrefs.HasKey(unsentDataKey))
        {
            string unsentDataString = PlayerPrefs.GetString(unsentDataKey, "");
            string key2 = key + "--time===";

            // check if the key is already in the list
            if (!unsentDataString.Contains(key2))
                unsentDataString += key + "--time===";


            PlayerPrefs.SetString(unsentDataKey, unsentDataString);
        }
        else
        {
            PlayerPrefs.SetString(unsentDataKey, key + "--time===");
        }
        PlayerPrefs.Save();
        playTimeWriter.Write(jsonString + "\n");
    }
    
    public void SaveData<T>(string key, T data, string apiPath, bool posted = false)
    {
        key = PlayerManager.instance.GetPlayerId() + "-" + key;
        DataWrapper<T> wrapper = new DataWrapper<T>
        {
            playerId = key,
            prefKey = PlayerManager.instance.GetPlayerId(),
            timeStamp = DateTime.Now.ToString("yyyy/MM/dd/HH:mm:ss"),
            data = data,
            postedToAPI = posted
        };

        string jsonString = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(key, jsonString);
        // dEbug.Log("Saving data: " + jsonString);
        Debug.Log("Saving data: " + jsonString);

        // Save json string to player prefs in list of UnsentData
        string unsentDataKey = "UnsentData";
        if (PlayerPrefs.HasKey(unsentDataKey))
        {
            string unsentDataString = PlayerPrefs.GetString(unsentDataKey, "");
            string key2 = key + "--time===";

            // check if the key is already in the list
            if (!unsentDataString.Contains(key2))
                unsentDataString += key + "--time===";
            PlayerPrefs.SetString(unsentDataKey, unsentDataString);
        }
        else
        {
            PlayerPrefs.SetString(unsentDataKey, key + "--" + apiPath + "===");
        }
        PlayerPrefs.Save();
        fileWriter.Write(jsonString + "\n");

        //// Save the data to the API
        ApiManager._instance.TryPostRequest(jsonString, apiPath, key); 

    }

    public DataWrapper<T> LoadData<T>(string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            string jsonString = PlayerPrefs.GetString(key);
            return JsonUtility.FromJson<DataWrapper<T>>(jsonString);
        }
        return null;
    }

    public string LoadJsonData(string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            return PlayerPrefs.GetString(key);
        }
        return null;
    }

    public void MarkDataAsPosted(string key)
    {
        // Dynamically load data and mark it as posted (since the data type T is unknown here).
        string jsonString = PlayerPrefs.GetString(key);
        if (!string.IsNullOrEmpty(jsonString))
        {
            DataWrapper<object> dummyWrapper = JsonUtility.FromJson<DataWrapper<object>>(jsonString);
            dummyWrapper.postedToAPI = true;
            jsonString = JsonUtility.ToJson(dummyWrapper);
            PlayerPrefs.SetString(key, jsonString);
            PlayerPrefs.Save();
        }
    }
}
