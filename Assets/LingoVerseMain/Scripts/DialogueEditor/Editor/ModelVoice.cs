using UnityEngine;

public static class ModelVoice
{
	private static string[] model_name = {
		"ArturSL",
		"ManEN",
		"WomanEN",
		"EvaDE",
		"ThorstenDE",
		"ManFR",
		"WomanFR"
	};
	
	private static string[] model_paths = {
		"sl_SI-artur-medium.sentis",
		"en_US-hfc_male-medium.sentis",
		"en_US-ljspeech-high.sentis",
		"de_DE-eva_k-x_low.sentis",
		"de_DE-thorsten-high.sentis",
		"fr_FR-tom-medium.sentis"
	};

	private static int[] sample_rates = {
		22050,
		22050,
		22050,
		16000,
		22050,
		44100,
		22050
	};

	private static string[] voice_lang = {
		"sl",
		"en",
		"en",
		"de",
		"de",
		"fr",
		"fr"
	};

	public static string GetModelPath(int voice) {
		return model_paths[voice];
	}

	public static int GetSampleRate(int voice) {
		return sample_rates[voice];
	}

	public static string GetVoiceLang(int voice) {
		return voice_lang[voice];
	}

	public static string GetName(int voice) {
		return model_name[voice];
	}
}

public enum PiperVoices
{
	ArturSL,
	ManEN,
	WomanEN,
	EvaDE,
	ThorstenDE,
	ManFR,
	WomanFR
}