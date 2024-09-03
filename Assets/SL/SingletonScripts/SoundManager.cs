using UnityEngine;
using System.Collections.Generic;

public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }

    [SerializeField] private Sound[] sounds;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource effectsSource;

    private Dictionary<string, AudioClip> soundDictionary = new Dictionary<string, AudioClip>();

    protected override void Awake()
    {
        base.Awake();
        foreach (Sound s in sounds)
        {
            soundDictionary[s.name] = s.clip;
        }
    }

    public void PlaySound(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            effectsSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Sound not found: " + soundName);
        }
    }

    public void PlayMusic(string musicName)
    {
        if (soundDictionary.TryGetValue(musicName, out AudioClip clip))
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Music not found: " + musicName);
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetEffectsVolume(float volume)
    {
        effectsSource.volume = volume;
    }
}