using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public MenuManager gameStartMenu;
    public static PlayerManager instance;
    public string playerName = "Janez/Metka.";
    public string playerId;
    public GameObject namePanel;
    public TMP_Text textFieldName;
    public TMP_Text placeholderText;
    public GameObject keyboard;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start() {
        playerName = "";
        if (PlayerPrefs.HasKey("playerName")) {
            // Open The Name Panel, but fill with existing name
            playerName = PlayerPrefs.GetString("playerName");
            Debug.Log("Current Player Name: " + playerName);
        }

        if (SceneManager.GetActiveScene().name.Contains("MainScene")) {
            Debug.Log("PlayerManager");
            if (placeholderText != null)
                placeholderText.text = playerName;
            if (gameStartMenu != null)
                gameStartMenu.mainPanel.SetActive(false);
            if (namePanel != null)
                namePanel.SetActive(true);
        }
    }

    public string GetPlayerId() {
        return playerId;
    }

    public string GetPlayerName()
    {
        if (playerName == null || playerName == "")
        {
            Debug.LogWarning("PlayerManager: playerName is null or empty");
            return null;
        }
        return playerName;
    }

    public void SetPlayerName()
    {
        playerName = textFieldName.text;
        if (playerName.Equals("") || playerName == null || playerName.Length <= 1) {
            playerName = placeholderText.text;
        }
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("PlayerManager: playerName is null or empty");
            return;
        }

        playerId = Guid.NewGuid().ToString();
        PlayerPrefs.SetString("playerId", playerId);
        PlayerPrefs.SetString("playerName", playerName);
        PlayerPrefs.Save();
        Debug.Log($"PlayerManager: playerName set to '{playerName}'");
        if (keyboard != null)
            keyboard.SetActive(false);
        if (namePanel != null)
            namePanel.SetActive(false);
        if (gameStartMenu != null)
            gameStartMenu.mainPanel.SetActive(true);
    }
}
