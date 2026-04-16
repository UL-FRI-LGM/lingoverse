using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkChecker : MonoBehaviour
{
    public GameObject noInternetPanel;
    public TMP_Text noInternetText;

    // Check internet connection every 5 seconds
    private void Start()
    {
        InvokeRepeating("CheckInternetConnection", 0f, 5f);
    }

    // Check internet connection
    private void CheckInternetConnection()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            noInternetPanel.SetActive(true);
            noInternetText.text = "No internet connection available!";
        }
        else
        {
            noInternetPanel.SetActive(false);
        }
    }

}
