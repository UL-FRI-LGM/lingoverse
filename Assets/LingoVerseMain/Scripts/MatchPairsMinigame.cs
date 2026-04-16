using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public class MatchPairsMinigame : MonoBehaviour
{
    public MatchPairGameObject matchPairGameObject;
    public List<GameObject> spawnPointsL = new List<GameObject>();
    public List<GameObject> spawnPointsR = new List<GameObject>();

    [Tooltip("This is applied only to the pickable objects, and not the final position.")]
    public bool randomiseSpawnPositions = true;

    public GameObject textPanel;
    public GameObject toggleButton;

    public GameObject finalPedistalsParent;

    public GameObject walls;

    public bool isGameRunning = false;
    private bool buttonEnabled = true;

    public float countdownTime = 3;
    private float stopwatchStartTime;
    private bool countdownStarted = false;

    private float currentTime = 0.0f;
    public TMP_Text timerPanelText;

    private List<MatchPair> pairsL = new();
    private List<MatchPair> pairsR = new();

    private List<GameObject> itemPanels = new List<GameObject>();

    List<XRSocketInteractor> xRSocketInteractors = new ();

    private List<GameObject> spawnedMovableObjects = new ();


    public void Start()
    {
        // go through final pedistal parent and get all its children of type XRSocketInteractor
        xRSocketInteractors.AddRange(finalPedistalsParent.GetComponentsInChildren<XRSocketInteractor>());

        pairsL = new List<MatchPair>(matchPairGameObject.pairs);
        pairsR = new List<MatchPair>(matchPairGameObject.pairs);
        if (walls != null)
            walls.gameObject.SetActive(false);
       
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int rand = Random.Range(i, list.Count);
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    public void ToggleButton()
    {
        if (!buttonEnabled)
            return;
        if (!isGameRunning)
        {
            StartGame();
        }
        else
        {
            StopGame(true);
        }

    }

    public void Update()
    {
        if (countdownStarted && Time.time >= stopwatchStartTime)
        {
            isGameRunning = true;
            currentTime = 0.0f;
            countdownStarted = false;
            // Make all objects movable
            foreach (GameObject go in spawnedMovableObjects)
            {
                go.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>().enabled = true;
            }
        }
        if (isGameRunning)
        {
            currentTime += Time.deltaTime;
            UpdateTimerText();
        }
    }

    public void StartGame()
    {
        Debug.Log("Start Game");
        SpawnObjects();
        if (walls != null)
            walls.gameObject.SetActive(true);
        // Create a countdown to begin
        buttonEnabled = false;
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        // Set the button text to Stop
        toggleButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stop Game!";
        //Hide the button
        toggleButton.SetActive(false);
        countdownStarted = true;
        stopwatchStartTime = Time.time + 1.0f + countdownTime;

        // countdown loop
        float remainingTime = countdownTime;
        while (remainingTime > 0)
        {
            timerPanelText.text = remainingTime.ToString();
            yield return new WaitForSeconds(1.0f);
            remainingTime -= 1.0f;
        }
        timerPanelText.text = "Go!";
        yield return new WaitForSeconds(1.0f);
        buttonEnabled = true;
        // Show the button
        toggleButton.SetActive(true);
    }

    private void SpawnObjects()
    {
        // Spawn objects
        if (spawnedMovableObjects.Count > 0)
        {
            for (int i = 0; i < spawnedMovableObjects.Count; i++)
            {
                Destroy(spawnedMovableObjects[i]);
            }
            spawnedMovableObjects.Clear();
        }
        
        // shuffle pairsL 
        if (randomiseSpawnPositions)
            Shuffle(pairsL);

        for (int i = 0; i < pairsL.Count; i++)
        {
            GameObject go = Instantiate(pairsL[i].objectOne, spawnPointsL[i].transform.position, Quaternion.identity);
            go.name = pairsL[i].pairName;
            
            if (!go.TryGetComponent(out UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable))
            {
                interactable = go.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                Transform attach = go.transform;

                GameObject attachObj = go.GetNamedChild("GrabPoint");
                if (attachObj != null)
                    attach = attachObj.transform;
                interactable.attachTransform = attach;
                interactable.enabled = false;
            }
            spawnedMovableObjects.Add(go);
            if (pairsR[i].objectTwo.gameObject != null)
            {
                Instantiate(pairsR[i].objectTwo.GetGameObject(), spawnPointsR[i].transform.position, Quaternion.identity);
            }
            else
            {
                // instantiate the panel with text on it
                GameObject pairPanel = Instantiate(textPanel, spawnPointsR[i].transform.position, Quaternion.identity);
                TextMeshProUGUI text = pairPanel.GetComponentInChildren<TextMeshProUGUI>();
                text.text = pairsR[i].objectTwo.text;
                itemPanels.Add(pairPanel);
                //Debug.Log($"index: {i}, text {pairsR[i].objectTwo.text}");
            }

        }
        
    }

    private void FixedUpdate()
    {
        if (isGameRunning)
        {
            // Check if all xrsocketinteractors have an object in them
            bool allInteractorsHaveObjects = true;
            for (int i = 0; i < xRSocketInteractors.Count; i++)
            {
                if (xRSocketInteractors[i].GetOldestInteractableSelected() == null)
                {
                    allInteractorsHaveObjects = false;
                    break;
                }
            }
            if (allInteractorsHaveObjects)
            {
                StopGame();
            }
        }
    }
    [System.Serializable]
    public class GameKeyValuePair
    {
        public string key;
        public int score;
        public float time;
        public bool endedManually;
        public List<string> pairs = new List<string>();
    }
    public void StopGame(bool endedManual = false)
    {
        if (walls != null)
            walls.gameObject.SetActive(false);
        
        // Set the button text to Start Game
        toggleButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start Game!";

        isGameRunning = false;
        int score = 0;
        GameKeyValuePair gameScoreObj = new GameKeyValuePair();

        for (int i = 0; i < xRSocketInteractors.Count; i++)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable objName = xRSocketInteractors[i].GetOldestInteractableSelected();
            if (objName != null)
                gameScoreObj.pairs.Add(objName.transform.name + "-" + pairsR[i].pairName);
            else
                gameScoreObj.pairs.Add("null-" + pairsR[i].pairName);

            if (objName != null && objName.transform.name.Equals(pairsR[i].pairName))
                score++;

        }

        // Create a json with key value pairs for the score
        string key = "Score-" + matchPairGameObject.name + "-" + System.DateTime.Now.ToString("yyyyMMddHHmmss");
        gameScoreObj.key = key;
        gameScoreObj.score = score;
        gameScoreObj.time = currentTime;
        gameScoreObj.endedManually = endedManual;


        Debug.Log("Saving score: " + gameScoreObj.score + " with key: " + gameScoreObj.key + " and time: " + gameScoreObj.time);
        
        if (DataManager._instance != null)
            DataManager._instance.SaveData(key, gameScoreObj, "minigame");

        // Remove spawned objects
        foreach (GameObject go in spawnedMovableObjects)
        {
            Destroy(go);
        }
        countdownStarted = false;

        UpdateTimerText("Score: " + score + "/" + pairsL.Count);

        // Remove spawned panels
        foreach (GameObject go in itemPanels)
        {
            Destroy(go);
        }
        itemPanels.Clear();

        // Set variable to canStart after 2 seconds
        StartCoroutine(ResetGame());
    }

    IEnumerator ResetGame()
    {
        yield return new WaitForSeconds(2.0f);
        buttonEnabled = true;
    }

    private void UpdateTimerText(string str)
    {
        UpdateTimerText();
        timerPanelText.text += "\n" + str;
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        int milliseconds = Mathf.FloorToInt((currentTime * 1000) % 1000);

        string time = string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);

        timerPanelText.text = time;
        //Debug.Log(time);
    }
}
