using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Piper;

using UnityEngine;

public class DialoguePiper
{
	public float scaleSpeed = 1.0f;
	public float scalePitch = 1.0f;
	public float scaleGlottal = 0.8f;

	public string espeakNgRelativePath = "espeak-ng-data";

	private int sampleRate = 22050;
	private string modelPath = "";
	private string modelLang = "";
	private string modelName = "";
	
	private Unity.InferenceEngine.Model runtimeModel;
	private Unity.InferenceEngine.Worker worker;

	private Unity.InferenceEngine.BackendType _backendType = Unity.InferenceEngine.BackendType.GPUCompute;

	public DialoguePiper(int modelIndex) {
		// Init functions
		string espeakPath = Path.Combine(Application.streamingAssetsPath, espeakNgRelativePath);
		PiperWrapper.InitPiper(espeakPath);

		modelPath = ModelVoice.GetModelPath(modelIndex);
		sampleRate = ModelVoice.GetSampleRate(modelIndex);
		modelLang = ModelVoice.GetVoiceLang(modelIndex);
		modelName = ModelVoice.GetName(modelIndex);

		modelPath = Path.Combine(Application.streamingAssetsPath, modelPath);
		runtimeModel = Unity.InferenceEngine.ModelLoader.Load(modelPath);
		worker = new Unity.InferenceEngine.Worker(runtimeModel, _backendType);
	}

	~DialoguePiper() {
		PiperWrapper.FreePiper();
		if (worker != null)  {
			worker.Dispose();
		}
	}
	

	public async Task<AudioClip> TextToSpeechAsync(string text) {
		var phonemeResult = PiperWrapper.ProcessText(text, modelLang);
		var allSamples = new List<float>();

		for (int s = 0; s < phonemeResult.Sentences.Length; s++) {
			
			var sentence = phonemeResult.Sentences[s];
			int[] phonemeIds = sentence.PhonemesIds;
			
			// 入力テンソル作成
			using var inputTensor = new Unity.InferenceEngine.Tensor<int>(new Unity.InferenceEngine.TensorShape(1, phonemeIds.Length), phonemeIds);
			using var inputLengthsTensor = new Unity.InferenceEngine.Tensor<int>(new Unity.InferenceEngine.TensorShape(1), new int[] { phonemeIds.Length });
			using var scalesTensor = new Unity.InferenceEngine.Tensor<float>(
				new Unity.InferenceEngine.TensorShape(3),
				new float[] { scaleSpeed, scalePitch, scaleGlottal }
			);

			string inputName        = runtimeModel.inputs[0].name;
			string inputLengthsName = runtimeModel.inputs[1].name;
			string scalesName       = runtimeModel.inputs[2].name;

			worker.SetInput(inputName,         inputTensor);
			worker.SetInput(inputLengthsName,  inputLengthsTensor);
			worker.SetInput(scalesName,        scalesTensor);

			worker.Schedule();
			
			// 4-1. ScheduleIterable
			var enumerator = worker.ScheduleIterable();
			while (enumerator.MoveNext())
			{
				await Task.Yield();
			}

			// 4-2. 出力を取得
			Unity.InferenceEngine.Tensor<float> outputTensor = worker.PeekOutput() as Unity.InferenceEngine.Tensor<float>;
			System.Diagnostics.Debug.Assert(outputTensor != null, nameof(outputTensor) + " != null");
			float[] sentenceSamples = outputTensor.DownloadToArray();
			allSamples.AddRange(sentenceSamples);
		}
		
		// Update TTSCounter based on the most recent file index
		Regex regex = new Regex(@$"^TTS_{modelName}_(\d+)\.wav$");
		var files = Directory.GetFiles(Path.Combine(Application.dataPath, "Audio Text-To-Speech"))
			.Select(Path.GetFileName)
			.Select(n => regex.Match(n))
			.Where(match => match.Success)
			.Select(match => int.Parse(match.Groups[1].Value))
			.ToList();
		
		Debug.Log($"Found {files.Count} existing TTS files for model {modelName}.");
		Debug.Log($"Files are indexed: {string.Join(", ", files)}.");
		
		int fIndex = files.Count > 0 ? files.Count : 0;
		string fname = $"TTS_{modelName}_{fIndex}";

		AudioClip clip = AudioClip.Create(fname, allSamples.Count, 1, sampleRate, false);
		clip.SetData(allSamples.ToArray(), 0);
		SaveTTS(clip);
		Debug.Log("Clip created.");
		worker?.Dispose();
		PiperWrapper.FreePiper();
		return clip;
	}

	public void SaveTTS(AudioClip recordedClip) {
		if (recordedClip != null) {
			Debug.Log($"Saving the recorded clip {recordedClip.name}");
			string filePath = Path.Combine(Application.dataPath, $"Audio Text-To-Speech/{recordedClip.name}.wav");
			WavUtility.Save(filePath, recordedClip);
			Debug.Log("Recording saved as " + filePath);
		}
		else {
			Debug.Log("No recording data.");
		}
	}
}