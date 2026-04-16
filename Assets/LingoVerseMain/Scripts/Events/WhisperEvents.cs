using System;
using UnityEngine;
using Whisper;

public class WhisperEvents
{
	public Action<WhisperSegment> onSegmentReceived;
	public void SegmentReceived(WhisperSegment segment)
	{
		onSegmentReceived?.Invoke(segment);
	}

	public Action<int> onProgress;
	public void Progress(int progress) {
		onProgress?.Invoke(progress);
	}

	public Action toggleDetection;

	public void ToggleDetection()
	{
		toggleDetection?.Invoke();
	}

	public Action onRecordStart;
	public void RecordStart() {
		onRecordStart?.Invoke();
	}
	
	public Action onRecordStop;
	public void RecordStop() {
		onRecordStop?.Invoke();
	}
	
	// Designed to work with local Whisper, Wit.ai, VOSK
	public Action<string> onFullTranscription;
	public void FullTranscriptionReceived(string message)
	{
		onFullTranscription?.Invoke(message);	
	}
	
	// Using new Wit.ai 
	public Action onInteractionComplete;

	public void InteractionComplete()
	{
		onInteractionComplete?.Invoke();
	}
}