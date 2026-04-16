using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using Whisper;
using Whisper.Utils;
using Debug = UnityEngine.Debug;

/// <summary>
/// Modeled after <see href="https://github.com/Macoron/whisper.unity/blob/master/Assets/Samples/2%20-%20Microphone/MicrophoneDemo.cs">LINK</see>
/// </summary>
[RequireComponent(typeof(WhisperManager)), RequireComponent(typeof(MicrophoneRecord))]
public class WhisperVoiceManager : MonoBehaviour
{
    [SerializeField, InspectorName("Max record time in seconds.")]
    public float maxRecordTimeInSeconds = 10f;
    
    public static WhisperVoiceManager _instance { get; private set; }
    
    public WhisperManager whisper;
    public MicrophoneRecord microphoneRecord;
    
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
        GameEventsManager.instance.whisperEvents.onRecordStart += SubmitPressed;
        GameEventsManager.instance.whisperEvents.onRecordStop += OnButtonReleased;
    }
    
    private void OnDisable() {
        GameEventsManager.instance.whisperEvents.onRecordStart -= SubmitPressed;
        GameEventsManager.instance.whisperEvents.onRecordStop -= OnButtonReleased;
    }

    private void Start() {
        whisper = GetComponent<WhisperManager>();
        microphoneRecord = GetComponent<MicrophoneRecord>();
        microphoneRecord.OnRecordStop += OnRecordStop;
        whisper.OnNewSegment += OnSegmentReceived;
    }

    private void OnDestroy() {
        microphoneRecord.OnRecordStop -= OnRecordStop;
        whisper.OnNewSegment -= OnSegmentReceived;
    }

    private async void OnRecordStop(AudioChunk recordedAudio) {
        var sw = new Stopwatch();
        sw.Start();
        var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
        if (res == null) {
            Debug.LogError("Failed to get text");
            return;
        }

        var time = sw.ElapsedMilliseconds;
        var rate = recordedAudio.Length / (time * 1e-3f);
        Debug.Log($"Language: {res.Language}");
        Debug.Log($"Time: {time} ms\nRate: {rate}x");
    }

    private void OnButtonReleased()
    {
        if (microphoneRecord.IsRecording)
        {
            if (debugSubtitles != null)
                debugSubtitles.AddSegment("Processing ...");
            microphoneRecord.StopRecord();
        }
    }

    private void SubmitPressed() {
        if (!microphoneRecord.IsRecording)
        {
            microphoneRecord.StartRecord();
            if (debugSubtitles != null)
                debugSubtitles.AddSegment("Rec ...");
        }
    }

    // Trying to run Mic Record in Coroutine to alleviate Mic Start Lag.
    private IEnumerator MicRecordCoroutine()
    {
        // Test for Mic latency
        AudioSource audioSource = new AudioSource();
        audioSource.clip = Microphone.Start("", true, 1, 44100);
        yield return null;
    }
    
    private void OnSegmentReceived(WhisperSegment segment) {
        if (!streamSegments)
            return;
        if (debugSubtitles != null)
            debugSubtitles.AddSegment(segment.Text);
        _buffer += segment.Text;
        
        // Trigger event for all listeners
        GameEventsManager.instance.whisperEvents.SegmentReceived(segment);
    }
}
