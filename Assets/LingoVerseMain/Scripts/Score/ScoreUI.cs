using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreUI;

    private void OnEnable()
    {
        GameEventsManager.instance.scoreEvents.onScoreChange += UpdateScoreUI;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.scoreEvents.onScoreChange -= UpdateScoreUI;
    }

    private void UpdateScoreUI(int score)
    {
        scoreUI.text = score.ToString();
    }

}
