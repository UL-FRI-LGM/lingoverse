using System;
using UnityEngine;

public class AudioEvents
{
    public Action<AudioClip, Action> onPlay;
    public void Play(AudioClip audio, Action action=null)
    {
        onPlay?.Invoke(audio, action);
    }

    public Action<AudioClip> onPlayLoop;
    public void PlayLoop(AudioClip audio)
    {
        onPlayLoop?.Invoke(audio);
    }

    public Action onStop;
    public void Stop()
    {
        onStop?.Invoke();
    }
    
    public Action pauseBackgroundMusic;
    public void PauseBackgroundMusic()
    {
        pauseBackgroundMusic?.Invoke();
    }
    
    public Action resumeBackgroundMusic;
    public void ResumeBackgroundMusic()
    {
        resumeBackgroundMusic?.Invoke();
    }
}
