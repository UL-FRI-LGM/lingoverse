using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(VoiceProcessor)), RequireComponent(typeof(VoskSpeechToText))]
public class VoskVoiceManager : MonoBehaviour
{
    public static VoskVoiceManager _instance { get; private set; }

    public VoiceProcessor voskProcessor { get; private set; }
    public VoskSpeechToText voskSpeechToText { get; private set; }
    
    public DebugSubtitles debugSubtitles;

    private bool _detectionRuning = false;

    private string _buffer = "";

    private void Awake()
    {
        voskProcessor = GetComponent<VoiceProcessor>();
        voskSpeechToText = GetComponent<VoskSpeechToText>();
    }

    private void OnEnable()
    {
        GameEventsManager.instance.whisperEvents.toggleDetection += ToggleDetection;
        voskSpeechToText.OnStatusUpdated += LogSpeechToText;
        voskSpeechToText.OnTranscriptionResult += OnTranscribeReceived;
    }
    
    private void OnDisable()
    {
        GameEventsManager.instance.whisperEvents.toggleDetection -= ToggleDetection;
        voskSpeechToText.OnStatusUpdated -= LogSpeechToText;
        voskSpeechToText.OnTranscriptionResult -= OnTranscribeReceived;
    }

    private void ToggleDetection()
    {
        if (_detectionRuning)
        {
            _detectionRuning = false;
            StartCoroutine(WaitForTranscriptionQueue());
        }
        else
        {
            LogSpeechToText($"Clearing buffer:{_buffer.Length}");
            LogSpeechToText($"Audio Length: {voskSpeechToText.audioBufferLength}");
            LogSpeechToText($"Transcribe Buffer: {voskSpeechToText.resultBufferLength}");
            _buffer = "";
            voskSpeechToText.ClearBuffers();
            _detectionRuning = true;
            voskSpeechToText.ToggleDetection();
        }
    }
    
    // We should wait until all the _threadedResultQueue in VoskSpeechToText is empty
    private IEnumerator WaitForTranscriptionQueue()
    {
        
        // Wait for 0.5f second to capture remaining audio
        yield return new WaitForSeconds(2.0f);
        
        // Toggle Stop recording
        voskSpeechToText.ToggleDetection();

        yield return new WaitUntil(voskSpeechToText.IsAllAudioProcessed);
        
        // Stop Detection 
        voskSpeechToText.StopDetection();
        
        // Finish Interaction
        GameEventsManager.instance.whisperEvents.FullTranscriptionReceived(_buffer);
        GameEventsManager.instance.whisperEvents.InteractionComplete();
    }

    private void OnTranscribeReceived(string jsonText)
    {
        var result = new RecognitionResult(jsonText);
        var text = "";
        for (int i = 0; i < result.Phrases.Length; i++)
            text += result.Phrases[i].Text;
        if (!text.Equals(""))
        {
            _buffer += text;
            LogSpeechToText(_buffer);
            if (debugSubtitles != null)
                debugSubtitles.AddSegment(text);
        }
    }

    private void LogSpeechToText(string text)
    {
        Debug.LogFormat($"Status: {text}");
    }
}