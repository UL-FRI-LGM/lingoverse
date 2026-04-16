using System;
using System.Collections;
using UnityEngine;


// Disabling all Networking in this script
public class ApiManager : MonoBehaviour
{
    public static ApiManager _instance { get; private set; }
    private string baseUrl = "";

    public bool isSendingData = false;

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

    // Check internet connection
    public bool IsInternetAvailable() {
        return false;
        //return Application.internetReachability != NetworkReachability.NotReachable;
    }

    // If connected to the internet, make a POST request
    public void TryPostRequest(string jsonData, string apiContainer, string jsonKey)
    {
        if (IsInternetAvailable())
        {
            //Debug.Log("Sending POST request to " + apiContainer);
            SendPostRequest(jsonData, apiContainer, jsonKey);
        }
        else
        {
            Debug.Log("No internet connection available!");
        }
    }

    public void TryGet(string path, string key)
    {
        if (IsInternetAvailable())
        {
            Debug.Log("Sending GET request to " + path);
            GetRequest(path, key);
        }
        else
        {
            Debug.Log("No internet connection available!");
        }
    }

    private void GetRequest(string path, string key)
    {
        StartCoroutine(GetRequestCoroutine(path, key));
    }

    private IEnumerator GetRequestCoroutine(string path, string key)
    {
        /*string escapedUrl = Uri.EscapeUriString(baseUrl + "/" + path + "/id/" + key);
        var request = UnityWebRequest.Get(escapedUrl);
        // print the url to the console
        Debug.Log("Sending GET request to " + escapedUrl);
        yield return request.SendWebRequest();

        // if response is empty array
        if (request != null)
        {
            if (request.downloadHandler.text == "[]")
            {
                Debug.Log("Empty response from API");
                GameEventsManager.instance.apiEvents.NotInDatabase(path, key);
            }
            else
            {
                Debug.Log("GET successful! Response: " + request.downloadHandler.text);
                GameEventsManager.instance.apiEvents.InDatabase(key+"--"+path);
            }
        }
        else
        {
            Debug.LogError("Error sending request: " + request.error);
        }*/
        yield return null;
    }

    private void SendPostRequest(string jsonPayload, string apiContainer, string jsonKey)
    {
        StartCoroutine(PostRequest(jsonPayload, apiContainer, jsonKey));
    }

    private IEnumerator PostRequest(string json, string apiContainer, string jsonKey)
    {
        /*var request = new UnityWebRequest(baseUrl + "/" + apiContainer, "POST");
        Debug.Log("Sending POST request to " + baseUrl + "/" + apiContainer);
        Debug.Log(json);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error sending request: " + request.error);
        }
        else
        {
            Debug.Log("POST successful! Response: " + request.downloadHandler.text);

            // Mark the data as posted to the API
            GameEventsManager.instance.apiEvents.CompletedJson(jsonKey);
        }

        //QuitHandler.instance.QuitApplication();
        */
        yield return null;
    }
}
