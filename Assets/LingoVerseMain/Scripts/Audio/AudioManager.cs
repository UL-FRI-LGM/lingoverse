using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0, 1)]
    public float volume = 1;
    [Range(-3, 3)]
    public float pitch = 1;
    public bool loop = false;
    public AudioSource source;

    public Sound()
    {
        volume = 1;
        pitch = 1;
        loop = false;
    }
}
public class AudioManager : MonoBehaviour
{
    public Sound current;
    public List<AudioClip> backgroundMusicTracks;
    public List<AudioClip> cityNoises;
    private AudioSource cityNoisesSource;
    public float transitionTimeBetweenTracks = 3f; // 5 seconds by default
    private AudioSource musicSource1;
    private AudioSource musicSource2;
    private AudioSource voiceSource;
    private bool isFirstAudioSourcePlaying;

    private Coroutine voiceCoroutine;
    
    private void Start()
    {
        // Voice Source
        voiceSource = gameObject.AddComponent<AudioSource>();
        
        // Initialize both AudioSources
        musicSource1 = gameObject.AddComponent<AudioSource>();
        musicSource2 = gameObject.AddComponent<AudioSource>();
        isFirstAudioSourcePlaying = true;

        //Get the songs from the resources folder
        backgroundMusicTracks = new List<AudioClip>(Resources.LoadAll<AudioClip>("Songs"));
        // Get the city noises from the resources folder
        cityNoises = new List<AudioClip>(Resources.LoadAll<AudioClip>("CityNoises"));
        // Set the volume of the background music
        musicSource1.volume = 0.03f;
        musicSource2.volume = 0.03f;

        // Start the city noises in a loop as soon as the game starts
        cityNoisesSource = gameObject.AddComponent<AudioSource>();
        cityNoisesSource.volume = 0.03f;
        // Select a random clip
        cityNoisesSource.clip = cityNoises[UnityEngine.Random.Range(0, cityNoises.Count)];
        cityNoisesSource.loop = true;
        cityNoisesSource.Play();

        // Start playing background music
        PlayRandomBackgroundMusic();
    }


    private void OnEnable()
    {
        GameEventsManager.instance.audioEvents.onPlay += Play;
        GameEventsManager.instance.audioEvents.onStop += Stop;
        GameEventsManager.instance.audioEvents.pauseBackgroundMusic += PauseBackgroundMusic;
        GameEventsManager.instance.audioEvents.resumeBackgroundMusic += ResumeBackgroundMusic;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.audioEvents.onPlay -= Play;
        GameEventsManager.instance.audioEvents.onStop -= Stop;
        GameEventsManager.instance.audioEvents.pauseBackgroundMusic -= PauseBackgroundMusic;
        GameEventsManager.instance.audioEvents.resumeBackgroundMusic -= ResumeBackgroundMusic;
    }

    private void PlayRandomBackgroundMusic()
    {
        if (backgroundMusicTracks.Count == 0) return;

        AudioSource activeSource = isFirstAudioSourcePlaying ? musicSource1 : musicSource2;

        int randomIndex = UnityEngine.Random.Range(0, backgroundMusicTracks.Count);
        AudioClip nextClip = backgroundMusicTracks[randomIndex];

        // Play the next song immediately
        activeSource.clip = nextClip;
        activeSource.Play();

        float timeToWait = nextClip.length - transitionTimeBetweenTracks;

        // Start the crossfade process near the end of the current clip
        StartCoroutine(StartCrossfade(timeToWait));
    }

    private IEnumerator StartCrossfade(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        StartCoroutine(UpdateMusicWithCrossFade(transitionTimeBetweenTracks));
    }

    private IEnumerator UpdateMusicWithCrossFade(float transitionTime)
    {
        float startVolume = 0.02f;
        float endVolume = 0;

        AudioSource activeSource = isFirstAudioSourcePlaying ? musicSource1 : musicSource2;
        AudioSource inactiveSource = isFirstAudioSourcePlaying ? musicSource2 : musicSource1;

        inactiveSource.clip = backgroundMusicTracks[UnityEngine.Random.Range(0, backgroundMusicTracks.Count)];
        inactiveSource.Play();

        // Fade out the active source while fading in the inactive source
        for (float i = 0; i < transitionTime; i += Time.deltaTime)
        {
            activeSource.volume = Mathf.Lerp(startVolume, endVolume, i / transitionTime);
            inactiveSource.volume = Mathf.Lerp(endVolume, startVolume, i / transitionTime);
            yield return null;
        }

        activeSource.volume = 0;
        inactiveSource.volume = 0.02f;

        // Swap the source
        isFirstAudioSourcePlaying = !isFirstAudioSourcePlaying;

        // Schedule the next song
        float timeToWait = inactiveSource.clip.length - transitionTimeBetweenTracks;
        StartCoroutine(StartCrossfade(timeToWait));
    }


    // crate a function to play the sound
    public void Play(AudioClip audio, Action onAudioComplete)
    {
        if (audio == null)
        {
            Debug.LogWarning("Audio clip is null");
            return;
        }
        // convert the audio clip to a sound
        Sound sound = new Sound();
        sound.clip = audio;
        sound.name = audio.name;
        sound.source = voiceSource;
        sound.source.clip = sound.clip;
        sound.source.Play();
        voiceCoroutine = StartCoroutine(OnAudioComplete(sound, onAudioComplete));
        current = sound;
    }

    IEnumerator OnAudioComplete(Sound sound, Action onAudioComplete)
    {
        yield return new WaitUntil(() => !sound.source.isPlaying);
        onAudioComplete?.Invoke();
    }

    public void Stop()
    {
        if (current == null) return;
        if (voiceCoroutine != null)
        {
            StopCoroutine(voiceCoroutine);
        }
        if (current.source != null)
        {
            current.source.Stop();
        }
    }
    
    public void PauseBackgroundMusic()
    {
        AudioSource activeSource = isFirstAudioSourcePlaying ? musicSource1 : musicSource2;
        activeSource.Pause();
    }
    
    public void ResumeBackgroundMusic()
    {
        AudioSource activeSource = isFirstAudioSourcePlaying ? musicSource1 : musicSource2;
        activeSource.UnPause();
    }
}