using System.Collections;
using System.Net;
using Meta.WitAi.Requests;
using Oculus.Voice.Dictation;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MetaVoiceManager : MonoBehaviour
{
    public static MetaVoiceManager _instance { get; private set; }
    
    public AppDictationExperience appDictationExperience;
    
    public bool streamSegments = true;
    public DebugSubtitles debugSubtitles;

    private string _buffer;


    private void Awake() {
        // if (_instance != null && _instance != this) {
        //     Destroy(this.gameObject);
        // }
        // else {
        //     // Make sure this object isn't destroyed when loading a new scene
        //     DontDestroyOnLoad(this.gameObject);
        //     _instance = this;
        // }
    }

    private void OnEnable() {
        //GameEventsManager.instance.whisperEvents.onRecordStart += SubmitPressed;
        GameEventsManager.instance.whisperEvents.toggleDetection += SubmitPressed;
        appDictationExperience.TranscriptionEvents.OnFullTranscription.AddListener(OnFullTranscribeReceived);
        appDictationExperience.DictationEvents.OnComplete.AddListener(OnInteractionComplete);
    }
    
    private void OnDisable() {
        GameEventsManager.instance.whisperEvents.toggleDetection -= SubmitPressed;
        appDictationExperience.TranscriptionEvents.OnFullTranscription.RemoveListener(OnFullTranscribeReceived);
        appDictationExperience.DictationEvents.OnComplete.RemoveListener(OnInteractionComplete);
    }


    private void SubmitPressed()
    {
        // If previous Interaction exists it should be canceled.
        if (appDictationExperience.MicActive)
            appDictationExperience.Deactivate();
        Debug.Log("[MetaVoiceManager] Start recording.");
        appDictationExperience.Activate();
    }

    public void OnInteractionComplete(VoiceServiceRequest voiceServiceRequest)
    {
        if (voiceServiceRequest.Results.StatusCode == (int)HttpStatusCode.OK)
        {
            Debug.Log("[MetaVoiceManager] Interaction completed successfully.");
            GameEventsManager.instance.whisperEvents.InteractionComplete();
        }
        else
        {
            Debug.Log(voiceServiceRequest.Results.StatusCode);
            Debug.Log(voiceServiceRequest.Results.Message);
            appDictationExperience.Deactivate();
        }
    }

    public void OnFullTranscribeReceived(string text)
    {
        Debug.Log("OnFullTranscribeReceived: "+text);
        _buffer += text;
        if (debugSubtitles != null)
            debugSubtitles.AddSegment(text);
        GameEventsManager.instance.whisperEvents.FullTranscriptionReceived(text);
        appDictationExperience.Deactivate();
    }

    private void OnMicStoppedListening()
    {
        Debug.Log("[MetaVoiceManager] Recording stopped.");
    }
}
