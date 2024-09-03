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
        // BGM�p��AudioSource���쐬
        bgmSource = CreateAudioSource("BGMSource");

        // SE�p��AudioSource���쐬
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

    // TODO: BGM�̃t�F�[�h�C���E�t�F�[�h�A�E�g�@�\����������

    // TODO: �����SE�`�����l���̉��ʂ��ʂɒ�������@�\��ǉ�����

    // TODO: �T�E���h�̃v�����[�h�@�\���������A�������g�p�ʂ��œK������

    // TODO: �Đ�����BGM��SE�̈ꎞ��~�ƍĊJ�@�\��ǉ�����

    // TODO: �Q�[�����̃C�x���g�Ɋ�Â��ăT�E���h�𓮓I�ɕύX����@�\����������

    // TODO: �T�E���h�ݒ���v���C���[�̐ݒ�Ɋ�Â��ĕۑ��E�ǂݍ��݂���@�\��ǉ�����

    // TODO: �V�X�e�����ʂ̕ύX�ɉ����āA�Q�[�����̉��ʂ�������������@�\��ǉ�����
}