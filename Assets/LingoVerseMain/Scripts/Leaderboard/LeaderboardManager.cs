using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Linq;
using TMPro;

[System.Serializable]
public class PlayerData
{
    public string _id { get; set; }
    public string playerId { get; set; }
    public string playerNickname { get; set; }
    public int score { get; set; }
    public int __v { get; set; }
}

public class LeaderboardManager : MonoBehaviour
{
    private const string apiUrl = "http://localhost:3000/players";
    private WaitForSeconds apiCallInterval = new WaitForSeconds(5f); // 1 minute interval
    private List<PlayerData> playerList = new List<PlayerData>();
    public GameObject playerTextParent;
    private List<TMP_Text> texts = new List<TMP_Text>();

    private void Start()
    {
        StartCoroutine(CallAPI());
        texts = playerTextParent.GetComponentsInChildren<TMP_Text>().ToList();
    }

    private IEnumerator CallAPI()
    {
        while (true)
        {
            yield return apiCallInterval;
            // Call the API using UnityWebRequest
            UnityWebRequest request = UnityWebRequest.Get(apiUrl);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse the JSON response into a list of PlayerData objects
                string jsonResponse = request.downloadHandler.text;
                playerList = JsonConvert.DeserializeObject<List<PlayerData>>(jsonResponse);

                // Update the leaderboard
                UpdateLeaderboard();
            }
            else
            {
                Debug.LogError("API Request Failed: " + request.error);
            }

            request.Dispose();
        }
    }

    private void UpdateLeaderboard()
    {
        // Sort the playerlist by score
        playerList = playerList.OrderByDescending(player => player.score).ToList();

        for (int i = 0; i < texts.Count; i++)
        {
            if (i < playerList.Count)
            {
                var place = i + 1;
                var player = playerList[i];
                var playerNick = player.playerNickname;
                var score = player.score;
                texts[i].text = string.Format("{0}. {1} - {2}", place, playerNick, score.ToString());
            }
            else
            {
                texts[i].text = string.Empty;
            }

        }
    }
}
