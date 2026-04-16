using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vosk;

public class VoskSpeechToText : MonoBehaviour
{
	[Tooltip("Location of the model, relative to the Streaming Assets folder.")]
	public string ModelPath = "vosk-model-small-de-0.15.zip";

	[Tooltip("The source of the microphone input.")]

	public VoiceProcessor VoiceProcessor;
	[Tooltip("The Max number of alternatives that will be processed.")]
	public int MaxAlternatives = 1;

	[Tooltip("Should the recognizer start when the application is launched?")]
	public bool AutoStart = false;

	[Tooltip("The phrases that will be detected. If left empty, all words will be detected.")]
	public List<string> KeyPhrases = new List<string>();

	//Cached version of the Vosk Model.
	private Model _model;

	//Cached version of the Vosk recognizer.
	private VoskRecognizer _recognizer;

	//Conditional flag to see if a recognizer has already been created.
	//TODO: Allow for runtime changes to the recognizer.
	private bool _recognizerReady;

	//Holds all of the audio data until the user stops talking.
	private readonly List<short> _buffer = new List<short>();

	//Called when the the state of the controller changes.
	public Action<string> OnStatusUpdated;

	//Called after the user is done speaking and vosk processes the audio.
	public Action<string> OnTranscriptionResult;

	//The absolute path to the decompressed model folder.
	private string _decompressedModelPath;

	//A string that contains the keywords in Json Array format
	private string _grammar = "";

	//Flag that is used to wait for the model file to decompress successfully.
	private bool _isDecompressing;

	//Flag that is used to wait for the the script to start successfully.
	private bool _isInitializing;

	//Flag that is used to check if Vosk was started.
	private bool _didInit;

	//Threading Logic

	// Flag to signal we are ending
	private bool _running = false;

	//Thread safe queue of microphone data.
	private readonly ConcurrentQueue<short[]> _threadedBufferQueue = new ConcurrentQueue<short[]>();

	//Thread safe queue of resuts
	private readonly ConcurrentQueue<string> _threadedResultQueue = new ConcurrentQueue<string>();


	public int audioBufferLength => _threadedBufferQueue.Count;
	public int resultBufferLength => _threadedResultQueue.Count;

	static readonly ProfilerMarker voskRecognizerCreateMarker = new ProfilerMarker("VoskRecognizer.Create");
	static readonly ProfilerMarker voskRecognizerReadMarker = new ProfilerMarker("VoskRecognizer.AcceptWaveform");

	//If Auto start is enabled, starts vosk speech to text.
	void Start()
	{
		StartVoskStt();
	}

	/// <summary>
	/// Start Vosk Speech to text
	/// </summary>
	/// <param name="keyPhrases">A list of keywords/phrases. Keywords need to exist in the models dictionary, so some words like "webview" are better detected as two more common words "web view".</param>
	/// <param name="modelPath">The path to the model folder relative to StreamingAssets. If the path has a .zip ending, it will be decompressed into the application data persistent folder.</param>
	public void StartVoskStt(List<string> keyPhrases = null, string modelPath = default)
	{
		if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
		{
			OnStatusUpdated?.Invoke($"Requesting Microphone access.");
			Permission.RequestUserPermission(Permission.Microphone);
		}

		if (_isInitializing)
		{
			OnStatusUpdated?.Invoke("Already Initializing");
			return;
		}
		if (_didInit)
		{
			OnStatusUpdated?.Invoke($"VOSK has already been initialized!");
			return;
		}

		if (!string.IsNullOrEmpty(modelPath))
		{
			ModelPath = modelPath;
		}

		if (keyPhrases != null)
		{
			KeyPhrases = keyPhrases;
		}
		StartCoroutine(DoStartVoskStt());
	}

	//Decompress model, load settings, start Vosk and optionally start the microphone
	private IEnumerator DoStartVoskStt()
	{
		_isInitializing = true;
		yield return WaitForMicrophoneInput();

		yield return Decompress();

		OnStatusUpdated?.Invoke("Loading Model from: " + _decompressedModelPath);
		_model = new Model(_decompressedModelPath);

		yield return null;

		OnStatusUpdated?.Invoke("Initialized");
		VoiceProcessor.OnFrameCaptured += VoiceProcessorOnOnFrameCaptured;
		VoiceProcessor.OnRecordingStart += (msg) => OnStatusUpdated?.Invoke($"Recording with {msg}");
		VoiceProcessor.OnRecordingStop += () => OnStatusUpdated?.Invoke("Stopped");
		VoiceProcessor.OnSilenceDetected += () => OnStatusUpdated?.Invoke("Silence Detected");
		
		_isInitializing = false;
		_didInit = true;
		
		//OnStatusUpdated += Debug.Log;
		
		// Start the Voice Processor Recording
		VoiceProcessor.StartRecording();
		
		// Start Voice Recognition 
		if (AutoStart) {
			VoiceProcessor.ToggleVoiceMinVolume();
			_running = true;
			StartCoroutine(RecognizeAudio());
			//Task.Run(ThreadedWork).ConfigureAwait(false);
		}
	}

	//Translates the KeyPhraseses into a json array and appends the `[unk]` keyword at the end to tell vosk to filter other phrases.
	private void UpdateGrammar()
	{
		if (KeyPhrases.Count == 0)
		{
			_grammar = "";
			return;
		}

		JSONArray keywords = new JSONArray();
		foreach (string keyphrase in KeyPhrases)
		{
			keywords.Add(new JSONString(keyphrase.ToLower()));
		}

		keywords.Add(new JSONString("[unk]"));

		_grammar = keywords.ToString();
	}

	//Decompress the model zip file or return the location of the decompressed files.
	private IEnumerator Decompress()
	{
		if (!Path.HasExtension(ModelPath)
			|| Directory.Exists(
				Path.Combine(Application.persistentDataPath, Path.GetFileNameWithoutExtension(ModelPath))))
		{
			OnStatusUpdated?.Invoke("Using existing decompressed model.");
			_decompressedModelPath =
				Path.Combine(Application.persistentDataPath, Path.GetFileNameWithoutExtension(ModelPath));
			Debug.Log(_decompressedModelPath);

			yield break;
		}

		OnStatusUpdated?.Invoke("Decompressing model...");
		string dataPath = Path.Combine(Application.streamingAssetsPath, ModelPath);
		
		Stream dataStream;
		// Read data from the streaming assets path. You cannot access the streaming assets directly on Android.
		if (dataPath.Contains("://"))
		{
			UnityWebRequest www = UnityWebRequest.Get(dataPath);
			www.SendWebRequest();
			while (!www.isDone)
			{
				yield return null;
			}
			dataStream = new MemoryStream(www.downloadHandler.data);
		}
		// Read the file directly on valid platforms.
		else
		{
			dataStream = File.OpenRead(dataPath);
		}

		//Read the Zip File
		var zipFile = ZipFile.Read(dataStream);

		//Listen for the zip file to complete extraction
		zipFile.ExtractProgress += ZipFileOnExtractProgress;

		//Update status text
		OnStatusUpdated?.Invoke("Reading Zip file");

		//Start Extraction
		zipFile.ExtractAll(Application.persistentDataPath);

		//Wait until it's complete
		while (_isDecompressing == false)
		{
			yield return null;
		}
		//Override path given in ZipFileOnExtractProgress to prevent crash
		_decompressedModelPath = Path.Combine(Application.persistentDataPath, Path.GetFileNameWithoutExtension(ModelPath));

		//Update status text
		OnStatusUpdated?.Invoke("Decompressing complete!");
		//Wait a second in case we need to initialize another object.
		yield return new WaitForSeconds(1);
		//Dispose the zipfile reader.
		zipFile.Dispose();
	}

	///The function that is called when the zip file extraction process is updated.
	private void ZipFileOnExtractProgress(object sender, ExtractProgressEventArgs e)
	{
		if (e.EventType == ZipProgressEventType.Extracting_AfterExtractAll)
		{
			_isDecompressing = true;
			_decompressedModelPath = e.ExtractLocation;
		}
	}

	//Wait until microphones are initialized
	private IEnumerator WaitForMicrophoneInput()
	{
		while (Microphone.devices.Length <= 0)
			yield return null;
	}

	public void ToggleDetection() {
		if (VoiceProcessor.IsRecording) {
			if (_running) {
				VoiceProcessor.ToggleVoiceMinVolume();
			}
			else {
				_running = true;
				VoiceProcessor.ToggleVoiceMinVolume();
				Task.Run(ThreadedWork).ConfigureAwait(false);
				OnStatusUpdated.Invoke($"[{GetType().Name}] Started Vosk Recognizer");
			}
		} else {
			Debug.LogError("[VoskSpeechToText] Vosk Processor not recording!");
		}
	}

	public void StopDetection() {
		OnStatusUpdated?.Invoke($"[{GetType().Name}] Stopped Vosk Recognizer");
		_running = false;
	}

	//Calls the On Phrase Recognized event on the Unity Thread
	void Update()
	{
		if (_threadedResultQueue.TryDequeue(out string voiceResult))
		{
		    OnTranscriptionResult?.Invoke(voiceResult);
		}
	}

	//Callback from the voice processor when new audio is detected
	private void VoiceProcessorOnOnFrameCaptured(short[] samples)
	{	
		//Debug.Log("Frame captured "+samples.Length);
		//OnStatusUpdated?.Invoke("Frame Captured "+samples.Length);
        _threadedBufferQueue.Enqueue(samples);
	}
	
	// Feds the audio logic into the Vosk Recognizer but with Coroutine
	private IEnumerator RecognizeAudio() {
		// Initialize the Voice Recognizer
		voskRecognizerCreateMarker.Begin();
		if (!_recognizerReady)
		{
			UpdateGrammar();

			//Only detect defined keywords if they are specified.
			if (string.IsNullOrEmpty(_grammar))
			{
				_recognizer = new VoskRecognizer(_model, 16000.0f);
			}
			else
			{
				_recognizer = new VoskRecognizer(_model, 16000.0f, _grammar);
			}

			_recognizer.SetMaxAlternatives(MaxAlternatives);
			_recognizerReady = true;
			
			OnStatusUpdated?.Invoke("Recognizer ready");
		}

		voskRecognizerCreateMarker.End();

		voskRecognizerReadMarker.Begin();

		while (_running)
		{
			if (_threadedBufferQueue.TryDequeue(out short[] voiceResult))
			{
				if (_recognizer.AcceptWaveform(voiceResult, voiceResult.Length))
				{
					var resString = _recognizer.Result();
					var result = new RecognitionResult(resString);
					var text = "";
					for (int i = 0; i < result.Phrases.Length; i++)
						text += result.Phrases[i].Text;
					Debug.Log("[VoskSpeechToText] Recognized THE FOLLLOWING =====================>"+text);
					_threadedResultQueue.Enqueue(resString);
				}
			}
			else
			{
				// Wait for some data
				yield return new WaitForSeconds(0.01f);
			}
		}
		voskRecognizerReadMarker.End();
		
	}

	//Feeds the audio logic into the vosk recorgnizer
	private async Task ThreadedWork()
	{
		// Initialize the Voice Recognizer
		voskRecognizerCreateMarker.Begin();
		if (!_recognizerReady)
		{
			UpdateGrammar();

			//Only detect defined keywords if they are specified.
			if (string.IsNullOrEmpty(_grammar))
			{
				_recognizer = new VoskRecognizer(_model, 16000.0f);
			}
			else
			{
				_recognizer = new VoskRecognizer(_model, 16000.0f, _grammar);
			}

			_recognizer.SetMaxAlternatives(MaxAlternatives);
			_recognizerReady = true;
			
			OnStatusUpdated?.Invoke("Recognizer ready");
		}

		voskRecognizerCreateMarker.End();

		voskRecognizerReadMarker.Begin();

		while (_running)
		{
			if (_threadedBufferQueue.TryDequeue(out short[] voiceResult))
			{
				if (_recognizer.AcceptWaveform(voiceResult, voiceResult.Length))
				{
					var resString = _recognizer.Result();
					var result = new RecognitionResult(resString);
					var text = "";
					for (int i = 0; i < result.Phrases.Length; i++)
						text += result.Phrases[i].Text;
					Debug.Log("[VoskSpeechToText] Recognized THE FOLLLOWING =====================>"+text);
					_threadedResultQueue.Enqueue(resString);
				}
			}
			else
			{
				// Wait for some data
				await Task.Delay(50);
			}
		}
		voskRecognizerReadMarker.End();
	}
	
	public bool IsAllAudioProcessed()
	{
		return _threadedBufferQueue.Count == 0 && _threadedResultQueue.Count == 0;
	}

	private void OnApplicationQuit() {
		StopDetection();
	}
	
	private void OnDestroy() {
		StopDetection();
	}

	public void ClearBuffers()
	{
		_threadedBufferQueue.Clear();
		_threadedResultQueue.Clear();
	}
}
