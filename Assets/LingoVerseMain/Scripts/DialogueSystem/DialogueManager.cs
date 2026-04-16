using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Whisper;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private bool loadDialogue;

    private Dictionary<string, Dialogue> dialogueMap;
    private Dictionary<string, int> timerDic = new Dictionary<string, int>();
    public GameObject panelContents;
    public TMP_Text panelText;
    public GameObject nextTextParent;
    public GameObject buttonsContainer;
    private List<GameObject> buttons = new List<GameObject>();
    private List<Button> buttonComponents = new List<Button>();
    public bool dialogueInProgress = false;
    public bool soundInProgress = false;
    private bool resetQuestDialogue = false;
    private bool questStarted = false;

    private int correctAnswers = 0;

    private Coroutine waitCoroutine;

    private bool resetDialogue = false;

    private string id;

    private Coroutine speechCoroutine;
    private Coroutine audioplayCoroutine;

    private string _whisperText = String.Empty;
    private bool _whisperActive = false;

    private void Awake()
    {
        dialogueMap = CreateDialogueMap();
        ResetPanel();
        resetQuestDialogue = false;
        resetDialogue = false;
        panelContents.SetActive(false);
        foreach (Transform child in buttonsContainer.transform)
        {
            buttons.Add(child.gameObject);
        }
    }

    private void Start()
    {
        InvokeRepeating("UpdateDialogueTimers", 1f, 1f);
        foreach (Dialogue dialogue in dialogueMap.Values)
        {
            // broadcast initial quest states
            GameEventsManager.instance.dialogueEvents.DialogueStateChange(dialogue);
        }
    }

    private void OnEnable()
    {
        GameEventsManager.instance.dialogueEvents.onStartDialogue += StartDialogue;
        GameEventsManager.instance.dialogueEvents.onAdvanceDialogue += AdvanceDialogue;
        GameEventsManager.instance.dialogueEvents.onFinishDialogue += FinishDialogue;
        GameEventsManager.instance.dialogueEvents.onSelectAdvanceOrFinishDialogue += AdvanceOrFinishDialogue;
        GameEventsManager.instance.dialogueEvents.onStartDialogueAnimation += StartDialogueAnimation;
        GameEventsManager.instance.dialogueEvents.movePanel += MovePanel;
        GameEventsManager.instance.dialogueEvents.onEnablePanel += ResetAndEnablePanel;
        GameEventsManager.instance.dialogueEvents.onDisablePanel += ResetAndDisablePanel;
        GameEventsManager.instance.dialogueEvents.onResetDialogue += ResetDialogue;
        GameEventsManager.instance.dialogueEvents.onAddQuestToTimer += AddDialogueToTimers;
        
        GameEventsManager.instance.whisperEvents.onSegmentReceived += OnNewWhisperSegment;
        //GameEventsManager.instance.whisperEvents.onFullTranscription += OnFinalTranscribeReceived;
        GameEventsManager.instance.whisperEvents.onFullTranscription += OnPartialTranscribe;
        GameEventsManager.instance.whisperEvents.onInteractionComplete += OnInteractionComplete;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.dialogueEvents.onStartDialogue -= StartDialogue;
        GameEventsManager.instance.dialogueEvents.onAdvanceDialogue -= AdvanceDialogue;
        GameEventsManager.instance.dialogueEvents.onFinishDialogue -= FinishDialogue;
        GameEventsManager.instance.dialogueEvents.onSelectAdvanceOrFinishDialogue -= AdvanceOrFinishDialogue;
        GameEventsManager.instance.dialogueEvents.onStartDialogueAnimation -= StartDialogueAnimation;
        GameEventsManager.instance.dialogueEvents.movePanel -= MovePanel;
        GameEventsManager.instance.dialogueEvents.onEnablePanel -= ResetAndEnablePanel;
        GameEventsManager.instance.dialogueEvents.onDisablePanel -= ResetAndDisablePanel;
        GameEventsManager.instance.dialogueEvents.onResetDialogue -= ResetDialogue;
        GameEventsManager.instance.dialogueEvents.onAddQuestToTimer -= AddDialogueToTimers;
        
        GameEventsManager.instance.whisperEvents.onSegmentReceived -= OnNewWhisperSegment;
        //GameEventsManager.instance.whisperEvents.onFullTranscription -= OnFinalTranscribeReceived;
        GameEventsManager.instance.whisperEvents.onFullTranscription -= OnPartialTranscribe;
        GameEventsManager.instance.whisperEvents.onInteractionComplete -= OnInteractionComplete;
    }

    private void UpdateDialogueTimers()
    {
        List<string> questIdsToRemove = new List<string>();

        foreach (string dialogueId in timerDic.Keys.ToList())
        {
            timerDic[dialogueId] -= 1;
            if (timerDic[dialogueId] <= 0)
            {
                questIdsToRemove.Add(dialogueId);
            }
        }

        foreach (string dialogueId in questIdsToRemove)
        {
            timerDic.Remove(dialogueId);
            Dialogue dialogue = GetDialougeById(dialogueId);
            dialogue.currentDialogueData = dialogue.dialogueData;
            dialogue.state = DialogueState.CAN_START;
            GameEventsManager.instance.dialogueEvents.DialogueStateChange(dialogue);
            ResetDialogue(dialogueId);
            Debug.Log("Reset dialogue " + dialogueId);
        }
    }

    public void AddDialogueToTimers(string dialougeId, int time)
    {
        if (timerDic.ContainsKey(dialougeId))
        {
            timerDic[dialougeId] = time;
        }
        else
        {
            timerDic.Add(dialougeId, time);
        }
    }

    private void StartDialogueAnimation(string text, TMP_Text panelText, string questId, float animationSpeed, bool isQuest = false)
    {
        dialogueInProgress = false;
        soundInProgress = false;
        audioplayCoroutine = StartCoroutine(AnimateLine(text, panelText, questId, animationSpeed, isQuest));
    }

    public void AudioFinishedCallback()
    {
        Debug.Log("Audio finished");
        soundInProgress = true;
    }

    IEnumerator AnimateLine(string text, TMP_Text panelText, string questId, float animationSpeed, bool isQuest = false)
    {
        DisableButtons();
        nextTextParent.SetActive(false);
        GameEventsManager.instance.inputEvents.DisableAButton();
        ResetPanel();
        yield return new WaitForSeconds(0.5f);
        //Play the audio
        // check if audio is not null
        bool hasAudio = false;
        Dialogue dialogue = GetDialougeById(id);
        hasAudio = dialogue.currentDialogueData.AudioClip != null;
        Coroutine soundCoroutine = null;
        if (hasAudio)
        {
            GameEventsManager.instance.audioEvents.Play(dialogue.currentDialogueData.AudioClip, AudioFinishedCallback);
            soundCoroutine = StartCoroutine(WaitForSoundToFinish(dialogue.currentDialogueData.AudioClip.length));
            // calculateThe animation speed based on the audio length and the text length
            animationSpeed = dialogue.currentDialogueData.AudioClip.length / dialogue.currentDialogueData.DialogueText.Length;
        }

        // replace [Name] with the player name
        if (text.Contains("[Name]"))
        {
            text = text.Replace("[Name]", PlayerManager.instance.GetPlayerName());
        }

        panelText.text = "";
        foreach (char c in text)
        {
            panelText.text += c;
            yield return new WaitForSeconds(animationSpeed);
        }

        // wait until the soundCoroutine is finished
        if (hasAudio)
        {
            yield return soundCoroutine;
        }

        SetDialogueState(id, questId, questStarted, isQuest);
        GameEventsManager.instance.inputEvents.EnableAButton();
        Debug.Log("Finished animating");
        dialogueInProgress = false;
        
        // check the state of the dialogue and if its not in the picking state, show the next text button
        if (!dialogue.state.Equals(DialogueState.IN_PROGRESS_MULTIPLE_CHOICE) && !dialogue.state.Equals(DialogueState.IN_PROGRESS_WHISPER))
            nextTextParent.SetActive(true);
    }

    private void MovePanel(Transform transform)
    {
        panelContents.transform.position = transform.position;
        panelContents.transform.rotation = transform.rotation;
    }

    private void ResetAndEnablePanel()
    {
        ResetPanel();
        DisableButtons();
        panelContents.SetActive(true);
    }

    private void ResetAndDisablePanel()
    {
        ResetPanel();
        DisableButtons();
        panelContents.SetActive(false);
    }
    private void DisableButtons()
    {
        for (int i = 0; i < buttonComponents.Count; i++)
        {
            int index = i;
            buttonComponents[index].onClick.RemoveAllListeners();
            buttons[index].gameObject.SetActive(false);
        }
        buttonComponents.Clear();
    }

    IEnumerator WaitForSoundToFinish(float audioLength)
    {
        yield return new WaitForSeconds(audioLength);
        Debug.Log("Sound finished");
    }

    private void ChangeDialogueState(string id, DialogueState state)
    {
        Dialogue dialogue = GetDialougeById(id);
        dialogue.state = state;
        var type = dialogue.dialogueData.dialogueType;
        var numNextNodes = dialogue.nextNodes.Count;
        Debug.Log($"Dialogue {id} {type} {numNextNodes} state changed to {state}");
        GameEventsManager.instance.dialogueEvents.DialogueStateChange(dialogue);
    }

    IEnumerator Wait(float seconds, string id, bool isQuest)
    {
        yield return new WaitForSeconds(seconds);

        Debug.Log("Waited for " + seconds + " seconds");
        // Finish the dialogue
        FinishDialogue(id, isQuest);
    }

    // Function that checks the dialouge nodes and sets the state
    private void SetDialogueState(string id, string questId, bool afterQuest = false, bool isQuest = false)
    {
        Dialogue dialogue = GetDialougeById(id);
        // check if there are 1 or more next quests
        int numberOfNextNodes = dialogue.nextNodes.Count;
        if (numberOfNextNodes == 0) {
            ChangeDialogueState(dialogue.dialogueData.NodeGUID, DialogueState.CAN_FINISH_DIALOGUE);
            //Wait N seconds
            waitCoroutine = StartCoroutine(Wait(2f, id, isQuest));
        }
        else if (numberOfNextNodes == 1) {
            ChangeDialogueState(dialogue.dialogueData.NodeGUID, DialogueState.IN_PROGRESS);
            Debug.Log("Next choice " + dialogue.currentDialogueData.DialogueText);
        }
        else if (dialogue.currentDialogueData.dialogueType.Equals(DialogueType.MultiChoice)) {
            ChangeDialogueState(dialogue.dialogueData.NodeGUID, DialogueState.IN_PROGRESS_MULTIPLE_CHOICE);
            Debug.Log("Multiple choices " + dialogue.options.Count);
            
            //// disable a button
            GameEventsManager.instance.dialogueEvents.DialoguePicking(true);
            
            // Save the buttons to a list and randomize them
            List<DialogueOptionsData> randomisedNodes = new List<DialogueOptionsData>();

            for (int i = 0; i < dialogue.options.Count; i++)
            {
                // If the node is first, set isCorrectAnswer to true
                if (i == 0 || dialogue.dialogueData.allCorrect)
                    dialogue.options[i].SetIsCorrectAnswer(true);
                // Get the dialogue node at the index
                randomisedNodes.Add(dialogue.options[i]);
            }
            // randomize the buttons
            randomisedNodes = randomisedNodes.OrderBy(x => UnityEngine.Random.value).ToList();
            // show the buttons
            for (int i = 0; i < dialogue.options.Count; i++)
            {
                int index = i;
                // get the tmp text from the button
                TMP_Text buttonText = buttons[index].GetComponentInChildren<TMP_Text>();
                // set the text to the dialogue text
                buttonText.text = randomisedNodes[index].text;
                if (buttonText.text.Contains("[Name]"))
                {
                    buttonText.text = buttonText.text.Replace("[Name]", PlayerManager.instance.GetPlayerName());
                }
                // set the button to active
                buttons[index].SetActive(true);
                // Find the button component on the button
                Button button = buttons[index].GetComponentInChildren<Button>();
                // add the listener to the button
                button.onClick.AddListener(() => OnButtonClicked(id, randomisedNodes[index], isQuest, questId));
                buttonComponents.Add(button);
            }
        }
        else if (dialogue.currentDialogueData.dialogueType.Equals(DialogueType.MultiWhisper)) {
            ChangeDialogueState(dialogue.dialogueData.NodeGUID, DialogueState.IN_PROGRESS_WHISPER);
            
            // Disable A button for Quests
            GameEventsManager.instance.dialogueEvents.DialoguePicking(true);
            panelText.text = $"{dialogue.currentDialogueData.DialogueText}\nHold A to record.";
            
            // Save the buttons to a list
            List<DialogueOptionsData> optionsData = new List<DialogueOptionsData>();
            for (int i = 0; i < dialogue.options.Count; i++)
            {
                // If the node is first, set isCorrectAnswer to true
                if (i == 0 || dialogue.dialogueData.allCorrect)
                    dialogue.options[i].SetIsCorrectAnswer(true);
                // Get the dialogue node at the index
                optionsData.Add(dialogue.options[i]);
            }
            // Try Again Button
            TMP_Text buttonText = buttons[0].GetComponentInChildren<TMP_Text>();
            buttonText.text = "Repeat";
            buttons[0].SetActive(false);
            Debug.Log(buttons);
            Button button = buttons[0].GetComponentInChildren<Button>();
            button.onClick.AddListener(() =>
            {
                // We must hide buttons
                buttons[0].SetActive(false);
                buttons[1].SetActive(false);
                
                // We must reset the previous Whisper Text
                _whisperText = String.Empty;
                panelText.text = $"{dialogue.currentDialogueData.DialogueText}\nHold A to record.";
                speechCoroutine = StartCoroutine(OnSpeechToText(id));
            });
            buttonComponents.Add(button);
            
            // Procceed Button
            buttonText = buttons[1].GetComponentInChildren<TMP_Text>();
            buttonText.text = "Continue";
            buttons[1].SetActive(false);
            button = buttons[1].GetComponentInChildren<Button>();
            button.onClick.AddListener(() => {
                OnAdvanceWhisper(id, optionsData, isQuest, questId);
            });
            buttonComponents.Add(button);
            
            speechCoroutine = StartCoroutine(OnSpeechToText(id));
        }
    }

    private void StartDialogue(string id, string questId, bool isQuest = false)
    {
        this.id = id;
        //create a method to reset everything to defaultzs
        //todo: maybe set the player to a certain position and rotation for the dialogue
        //todo check ce vec nodes
        // show the panel
        resetQuestDialogue = false;
        resetDialogue = false;
        panelContents.SetActive(true);
        GameEventsManager.instance.dialogueEvents.EnableRestartPanel(id);
        if (isQuest)
            GameEventsManager.instance.dialogueEvents.DialogueInProgress(id, true, false);
        ResetPanel();
        ShowDialogueText(id, questId, isQuest: isQuest);
    }

    private void ShowDialogueText(string id, string questId, bool isQuest = false)
    {
        Dialogue dialogue = GetDialougeById(id);
        if (dialogue.state.Equals(DialogueState.CAN_START))
        {
            dialogue.InstantiateCurrentDialogue(panelText, questId, isQuest);
        }
        else if (dialogue.state.Equals(DialogueState.IN_PROGRESS))
        {
            dialogue.currentDialogueData = dialogue.nextNodes.First();
            dialogue.InstantiateCurrentDialogue(panelText, questId, isQuest);
        }
        else if (dialogue.state.Equals(DialogueState.CAN_FINISH_DIALOGUE))
        {
            if (dialogue.nextNodes.Count == 1)
            {
                panelContents.SetActive(true);
                dialogue.currentDialogueData = dialogue.nextNodes[0];
                dialogue.InstantiateCurrentDialogue(panelText, questId, isQuest);
            }
        }
    }

    private void OnButtonClicked(string id, DialogueOptionsData dialogueData, bool isQuest, string questId)
    {
        Dialogue dialogue = GetDialougeById(id);
        dialogue.currentDialogueData = dialogueData.nextDialogue;
        dialogue.nextNodes = new List<DialogueNodeData> { dialogueData.nextDialogue };
        //keep track of correctly clicked buttons for the score
        // disable the buttons
        DisableButtons();
        buttonComponents.Clear();
        Debug.Log("Button clicked with text " + dialogueData.text);
        SetDialogueState(id, questId);
        // print the text with the correct answer

        // todo: if wrong answer was selected, allow the user to repeat the dialogue
        if (isQuest)
        {
            if (dialogueData.GetIsCorrectAnswer())
            {
                GameEventsManager.instance.questEvents.StartQuest(questId);
                ChangeDialogueState(id, DialogueState.CAN_FINISH_DIALOGUE);
                AdvanceDialogue(id, questId, isQuest);
                resetQuestDialogue = false;
            }
            else
            {
                ChangeDialogueState(id, DialogueState.CAN_FINISH_DIALOGUE);
                AdvanceDialogue(id, questId, isQuest);
                resetQuestDialogue = true;
            }
        }
        else
        {
            Debug.Log("Dialogue is: " + dialogueData.text + " and the answer is: " + dialogueData.GetIsCorrectAnswer() + " and the exp is: " + dialogue.exp);
            if (dialogueData.GetIsCorrectAnswer())
            {
                ClaimRewards(dialogue);
                correctAnswers++;
                resetDialogue = false;
            }
            else
            {
                // Need to reset the dialogue
                // TODO: reset the dialogue after it is finished
                resetDialogue = true;
            }
            if (dialogue.state.Equals(DialogueState.IN_PROGRESS))
            {
                AdvanceDialogue(id, questId);
            }
            else if (dialogue.state.Equals(DialogueState.CAN_FINISH_DIALOGUE))
            {
                FinishDialogue(id, isQuest);
                //enable movement and teleportation
                GameEventsManager.instance.movementEvents.EnableMovement();
                GameEventsManager.instance.movementEvents.EnableTeleportation();
            }
            GameEventsManager.instance.dialogueEvents.DialoguePicking(false);
        }

    }

    private void OnAdvanceWhisper(string id, List<DialogueOptionsData> dialogueOptions, bool isQuest, string questId)
    {
        // Clear Buttons
        DisableButtons();
        buttonComponents.Clear();
        
        // Remove all special characters.
        Regex rgx = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        
        // Trim leading and trailing spaces
        _whisperText = rgx.Replace(_whisperText, "");
        _whisperText = _whisperText.Trim();
        
        string optionText = "";
        Dialogue dialogue = GetDialougeById(id);
        bool isCorrect = false;
        for (int i = 0; i < dialogueOptions.Count; i++)
        {
            optionText = dialogueOptions[i].text;
            // Remove if [Name] is in the option text
            if (optionText.Contains("[Name]"))
            {
                // We should do it like so in case the name was misunderstood
                optionText = optionText.Replace("[Name]", "");
            }
            optionText = rgx.Replace(optionText, "");
            optionText = optionText.Trim();
            
            
            Debug.Log($"Option {i}: {optionText}");
            
            // Either try to match the option or if option empty AUTO CORRECT
            if (Regex.IsMatch(_whisperText, optionText, RegexOptions.IgnoreCase) || optionText.Equals(""))
            {
                dialogue.currentDialogueData = dialogueOptions[i].nextDialogue;
                dialogue.nextNodes = new List<DialogueNodeData> { dialogueOptions[i].nextDialogue };
                Debug.Log("Correct answer");
                resetDialogue = !dialogueOptions[i].GetIsCorrectAnswer();
                isCorrect = dialogueOptions[i].GetIsCorrectAnswer();
                resetQuestDialogue = isQuest && !isCorrect;
                break;
            }
        }
        SetDialogueState(id, questId, false, isQuest);
        
        // If Is Quest we do not claim rewards but start quest
        if (isQuest)
        {
            if (isCorrect)
                GameEventsManager.instance.questEvents.StartQuest(questId);
            ChangeDialogueState(id, DialogueState.CAN_FINISH_DIALOGUE);
            AdvanceDialogue(id, questId, isQuest);
        }
        else
        {
            Debug.Log("Dialogue is: " + _whisperText + " and the answer is: " + optionText + " and the exp is: " + dialogue.exp);
            if (isCorrect)
            {
                ClaimRewards(dialogue);
                correctAnswers++;
            }
            if (dialogue.state.Equals(DialogueState.IN_PROGRESS))
            {
                AdvanceDialogue(id, questId);
            } else if (dialogue.state.Equals(DialogueState.CAN_FINISH_DIALOGUE))
            {
                FinishDialogue(id, isQuest);
                GameEventsManager.instance.movementEvents.EnableMovement();
                GameEventsManager.instance.movementEvents.EnableTeleportation();
            }
            GameEventsManager.instance.dialogueEvents.DialoguePicking(false);
        }
        _whisperText = String.Empty;
    }
    
    private IEnumerator OnSpeechToText(string id)
    {
        // Connect the Toggle Detection action to Press A button
        GameEventsManager.instance.inputEvents.onAPressed += StartDetection;
        GameEventsManager.instance.inputEvents.onAReleased += StopDetection;
        
        // Wait for the OnWhisperReceived event to be called and then check if the whisper was correct
        _whisperActive = true;
        yield return new WaitUntil(() => !_whisperActive);
        
        // Disconnect the Toggle Detection events 
        GameEventsManager.instance.inputEvents.onAPressed -= StartDetection;
        GameEventsManager.instance.inputEvents.onAReleased -= StopDetection;
        
        GameEventsManager.instance.audioEvents.ResumeBackgroundMusic();
        Debug.Log("Whisper received: " + _whisperText);
        Dialogue dialogue = GetDialougeById(id);
        panelText.text = $"{dialogue.currentDialogueData.DialogueText}\nReceived: {_whisperText}";
        
        // Enable the Reset and Continue Buttons
        buttons[0].SetActive(true);
        buttons[1].SetActive(true);
        Debug.Log("Enabling buttons");
    }
    
    private void AdvanceOrFinishDialogue(string id, string questId, bool isQuest = false)
    {
        Dialogue dialogue = GetDialougeById(id);
        if (dialogue.state.Equals(DialogueState.IN_PROGRESS))
        {
            AdvanceDialogue(id, questId, isQuest);
        }
        else if (dialogue.state.Equals(DialogueState.CAN_FINISH_DIALOGUE))
        {
            FinishDialogue(id, isQuest);
            //enable movement and teleportation
            GameEventsManager.instance.movementEvents.EnableMovement();
            GameEventsManager.instance.movementEvents.EnableTeleportation();
        }
    }

    private void AdvanceDialogue(string id, string questId, bool isQuest = false)
    {
        if (dialogueInProgress)
            return;

        ShowDialogueText(id, questId, isQuest: isQuest);
    }

    private void ResetPanel()
    {
        panelText.text = "";
    }

    [Serializable]
    public class DialogueKeyValuePair
    {
        public string key;
        public int score;
    }

    private void FinishDialogue(string id, bool isQuest = false)
    {
        // todo test with multiple quests active and finished etc
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
        }
        Dialogue dialogue = GetDialougeById(id);
        ResetPanel();
        panelContents.SetActive(false);
        //ClaimRewards(dialogue);
        ChangeDialogueState(dialogue.dialogueData.NodeGUID, DialogueState.FINISHED);
        if (isQuest)
        {
            GameEventsManager.instance.dialogueEvents.DialogueInProgress(id, false, resetQuestDialogue);
            GameEventsManager.instance.dialogueEvents.DialoguePicking(false);
        }
        else
        {
            // Set the state of dialogue to finished
            if (resetDialogue) // TODO : tuki je bil ce ima option restartat, sam majo vsi
            {
                ResetDialogue(id);
            }
            else
            {
                dialogue.completed = true;
                // update the icon
                GameEventsManager.instance.dialogueEvents.IconStateChange(DialogueState.FINISHED, id);

            }

        }
        GameEventsManager.instance.dialogueEvents.DisableRestartPanel();
        //Enable movement and teleportation
        GameEventsManager.instance.movementEvents.EnableMovement();
        GameEventsManager.instance.movementEvents.EnableTeleportation();
        // Hide the panel
        panelContents.SetActive(false);
        ResetPanel();

        // Save the score
        // Also add the path for api when its implemented to DialogueScore
        // For key use "Score" and the dialogue id and timpstamp 

        // Create a json with key value pairs for the score
        DialogueKeyValuePair dialogueKeyValuePair = new DialogueKeyValuePair();
        string key = "DiagScore-" + id + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        dialogueKeyValuePair.key = key;
        dialogueKeyValuePair.score = correctAnswers;

        if (DataManager._instance != null)
        {
            Debug.Log("Saving score: " + dialogueKeyValuePair.score + " with key: " + dialogueKeyValuePair.key);
        
            DataManager._instance.SaveData(key, dialogueKeyValuePair, "dialogue");
        }
        correctAnswers = 0;
    }

    private void ResetDialogue(string id)
    {
        ChangeDialogueState(id, DialogueState.CAN_START);
        GameEventsManager.instance.dialogueEvents.DisableRestartPanel();
        Dialogue dialogue = GetDialougeById(id);
     
        // Stop the audio coroutine
        GameEventsManager.instance.audioEvents.Stop();
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
        }
        if (audioplayCoroutine != null)
        {
            StopCoroutine(audioplayCoroutine);
        }
        
        // We must reenable the A button
        GameEventsManager.instance.inputEvents.EnableAButton();
        
        // reset the icon
        GameEventsManager.instance.dialogueEvents.IconStateChange(DialogueState.CAN_START, id);
        dialogue.currentDialogueData = dialogue.dialogueData;
        dialogue.state = DialogueState.CAN_START;
        correctAnswers = 0;

        dialogueInProgress = false;
        // Enable movement and teleportation
        GameEventsManager.instance.movementEvents.EnableMovement();
        GameEventsManager.instance.movementEvents.EnableTeleportation();
        
        // Stop VOSK Detection if it is active
        if (_whisperActive)
        {
            // Remove the Listeners 
            GameEventsManager.instance.inputEvents.onAPressed -= StartDetection;
            GameEventsManager.instance.inputEvents.onAReleased -= StopDetection;
            
            StopDetection();
            StopCoroutine(speechCoroutine);
        }
        
        // Disable Buttons
        DisableButtons();
        buttonComponents.Clear();
    }

    private Dictionary<string, Dialogue> CreateDialogueMap()
    {
        // Load all quests from resources
        List<DialogueContainer> allDialogues = Resources.LoadAll<DialogueContainer>("Dialogues").ToList();

        Dictionary<string, Dialogue> idToQuestMap = new Dictionary<string, Dialogue>();
        foreach (DialogueContainer diagContainer in allDialogues)
        {
            
            DialogueNodeData startNode = diagContainer.GetStartNode();
            if (idToQuestMap.ContainsKey(startNode.NodeGUID))
            {
                Debug.LogWarning($"Duplicate Node ID {startNode.NodeGUID} found in {diagContainer.name} and {idToQuestMap[startNode.NodeGUID].dialogueContainer.name}");
            }
            else
            {
                idToQuestMap.Add(startNode.NodeGUID, LoadDialogue(startNode, diagContainer));
            }
        }

        return idToQuestMap;
    }

    private void ClaimRewards(Dialogue dialogue)
    {
        GameEventsManager.instance.scoreEvents.ScoreGained(dialogue.exp);
        Debug.Log("Earned: " + dialogue.exp + " in quest: " + dialogue.currentDialogueData.NodeGUID);
    }

    private Dialogue GetDialougeById(string id)
    {
        if (dialogueMap.ContainsKey(id)) {
            return dialogueMap[id];
        }
        else {
            Debug.LogError("ID not found in dialogue map " + id);
            return null;
        }
    }

    private Dialogue LoadDialogue(DialogueNodeData dialogueNode, DialogueContainer dialogueContainer)
    {
        Dialogue dialogue = null;
        try
        {
            //if (PlayerPrefs.HasKey(dialogueNode.NodeGUID) && loadDialogue)
            //{
            //    string serializedData = PlayerPrefs.GetString(dialogueNode.NodeGUID);
            //    DialogueData dialogueData = JsonUtility.FromJson<DialogueData>(serializedData);
            //    dialogue = new Dialogue(dialogueNode, dialogueData.state, dialogueData.currentNode, dialogueData.completed);
            //}
            //else
            //{
            //    dialogue = new Dialogue(dialogueNode);
            //}
            dialogue = new Dialogue(dialogueNode, dialogueContainer);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load dialogue " + dialogueNode.NodeGUID + ": " + e);
        }
        return dialogue;
    }
    
    
    // For Vosk Integration model
    private void StartDetection()
    {
        GameEventsManager.instance.audioEvents.PauseBackgroundMusic();
        _whisperActive = true;
        _whisperText = "";
        GameEventsManager.instance.whisperEvents.ToggleDetection();
        Dialogue dialogue = GetDialougeById(id);
        panelText.text = $"{dialogue.currentDialogueData.DialogueText}\nListening ...";
    }
    
    
    
    // For VOSK integration model
    private void StopDetection()
    {
        GameEventsManager.instance.whisperEvents.ToggleDetection();

    }

    // For whisper integration
    private void OnNewWhisperSegment(WhisperSegment segment) {
        Debug.Log($"[DialogueManager] New Segment: `{segment.Text}`");
        _whisperText = segment.Text;
    }

    private void OnPartialTranscribe(string text)
    {
        Debug.Log($"[DialogueManager] Received: `{text}`");
        _whisperText += text;
    }
    
    // For Wit.ai model and VOSK
    private void OnFinalTranscribeReceived(string text)
    {
        Debug.Log($"[DialogueManager] Received: `{text}`");
        _whisperText = text;
        
    }

    // For Wit.ai model
    private void OnInteractionComplete()
    {
        Dialogue dialogue = GetDialougeById(id);
        panelText.text = $"{dialogue.currentDialogueData.DialogueText}\nReceived: {_whisperText}";
        _whisperActive = false;
    }
}
