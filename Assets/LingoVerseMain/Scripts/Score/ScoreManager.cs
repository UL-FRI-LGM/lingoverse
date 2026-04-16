using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // Static score variables
    [SerializeField] private int startingScore = 0;
    [SerializeField] private bool loadScore = false;

    public int currentScore { get; private set; }

    private void Awake()
    {
        LoadScore();
    }

    private void OnEnable()
    {
        GameEventsManager.instance.scoreEvents.onScoreGain += UpdateScore;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.scoreEvents.onScoreGain -= UpdateScore;
    }

    private void Start()
    {
        GameEventsManager.instance.scoreEvents.ScoreChanged(currentScore);
    }

    private void UpdateScore(int amount)
    {
        currentScore += amount;
        GameEventsManager.instance.scoreEvents.ScoreChanged(currentScore);
    }

    private void OnApplicationQuit()
    {
        SaveScore();
    }

    private void SaveScore()
    {
        try
        {
            PlayerPrefs.SetInt("Score", currentScore);

            Debug.Log("Score saved " + currentScore);
        }
        catch (System.Exception e)
        {
            Debug.Log("Error saving score " + e.Message);
        }
        PlayerPrefs.Save();
    }

    public void LoadScore()
    {
        try
        {
            if (PlayerPrefs.HasKey("Score") && loadScore)
            {
                currentScore = PlayerPrefs.GetInt("Score");
            }
            else
            {
                currentScore = startingScore;
            }

            Debug.Log("Score loaded " + currentScore);
        }
        catch (System.Exception e)
        {
            Debug.Log("Error loading score " + e.Message);
        }
    }
}
