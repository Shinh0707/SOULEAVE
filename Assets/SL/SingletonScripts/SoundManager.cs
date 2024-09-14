using UnityEngine;
using System.Collections.Generic;

public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    [System.Serializable]
    public class Sound
    {
        public int id;
        public AudioClip clip;
        public bool isBGM;
    }

    [SerializeField] private Sound[] sounds;
    [SerializeField] private int seChannelCount = 4;
    private Dictionary<int, AudioClip> soundDictionary = new Dictionary<int, AudioClip>();
    private AudioSource bgmSource;
    private List<AudioSource> seSourcesPool = new List<AudioSource>();

    protected override void Awake()
    {
        base.Awake();
        InitializeSoundDictionary();
        CreateAudioSources();
    }

    private void InitializeSoundDictionary()
    {
        foreach (Sound s in sounds)
        {
            soundDictionary[s.id] = s.clip;
        }
    }

    private void CreateAudioSources()
    {
        // BGM用のAudioSourceを作成
        bgmSource = CreateAudioSource("BGMSource");

        // SE用のAudioSourceを作成
        for (int i = 0; i < seChannelCount; i++)
        {
            seSourcesPool.Add(CreateAudioSource($"SESource_{i}"));
        }
    }

    private AudioSource CreateAudioSource(string name)
    {
        GameObject sourceObj = new GameObject(name);
        sourceObj.transform.SetParent(this.transform);
        return sourceObj.AddComponent<AudioSource>();
    }

    public void PlaySE(int soundId)
    {
        if (soundDictionary.TryGetValue(soundId, out AudioClip clip))
        {
            AudioSource availableSource = GetAvailableSESource();
            if (availableSource != null)
            {
                availableSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("No available SE audio source.");
            }
        }
        else
        {
            Debug.LogWarning("Sound not found: " + soundId);
        }
    }

    private AudioSource GetAvailableSESource()
    {
        return seSourcesPool.Find(source => !source.isPlaying);
    }

    public void PlayBGM(int musicId)
    {
        if (soundDictionary.TryGetValue(musicId, out AudioClip clip))
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning("Music not found: " + musicId);
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
    }

    public void SetSEVolume(float volume)
    {
        foreach (var source in seSourcesPool)
        {
            source.volume = volume;
        }
    }

    // TODO: BGMのフェードイン・フェードアウト機能を実装する

    // TODO: 特定のSEチャンネルの音量を個別に調整する機能を追加する

    // TODO: サウンドのプリロード機能を実装し、メモリ使用量を最適化する

    // TODO: 再生中のBGMやSEの一時停止と再開機能を追加する

    // TODO: ゲーム内のイベントに基づいてサウンドを動的に変更する機能を実装する

    // TODO: サウンド設定をプレイヤーの設定に基づいて保存・読み込みする機能を追加する

    // TODO: システム音量の変更に応じて、ゲーム内の音量を自動調整する機能を追加する
}