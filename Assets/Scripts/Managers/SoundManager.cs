using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using VInspector;

[DefaultExecutionOrder(-1)]
public class SoundManager : MonoBehaviour
{
    #region Singleton
    
    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)           //�̱��� ����
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion
    
    [Tab("Volumes")]
    [SerializeField] private float masterSoundVolume = 1.0f;             // ������ ����

    public float MasterSoundVolume
    {
        get => masterSoundVolume;
        set
        {
            masterSoundVolume = value;
            OnChangeMasterSoundVolume?.Invoke(masterSoundVolume);
        }
    }
    
    [SerializeField] private float musicSoundVolume = 1.0f;              // ����
    
    public float MusicSoundVolume
    {
        get => musicSoundVolume;
        set
        {
            musicSoundVolume = value;
            OnChangeMusicSoundVolume?.Invoke(musicSoundVolume);
        }
    }
    
    [SerializeField] private float sfxSoundVolume = 1.0f;            // ȿ����
    
    public float SfxSoundVolume
    {
        get => sfxSoundVolume;
        set
        {
            sfxSoundVolume = value;
            OnChangeSfxSoundVolume?.Invoke(sfxSoundVolume);
        }
    }
    

    public Action<float> OnChangeMasterSoundVolume;
    public Action<float> OnChangeMusicSoundVolume;
    public Action<float> OnChangeSfxSoundVolume;
    
    [Tab("Pooling")]
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private GameObject audioSourcePrefab;
    [SerializeField] private int InitialPoolSize = 10;
    
    private Queue<AudioSource> effectAudioSourcePool = new();

    private void Start()
    {
        // ����� AudioSource ����
        if (backgroundAudioSource == null)
        {
            GameObject BGM = new GameObject("@BGM");

            BGM.transform.SetParent(transform);
            backgroundAudioSource = BGM.AddComponent<AudioSource>();
            backgroundAudioSource.loop = true;
            
            DontDestroyOnLoad(BGM);
        }

        // ȿ���� ����� �ҽ� Ǯ �ʱ�ȭ
        for (int i = 0; i < InitialPoolSize; i++)
        {
            CreateNewAudioSource();
        }

        // ������ ������ ���� ��
        OnChangeMasterSoundVolume += ChangeMasterSoundVolume;
    }

    void ChangeMasterSoundVolume(float value)
    {
        
    }

    /// <summary>
    /// AudioClip ������ ����� �÷��� �Լ�. 
    /// </summary>
    /// <param name="audioClip">AudioClip</param>
    /// <param name="type">Clip�� ����</param>
    /// <param name="volume">��� ����</param>
    /// <param name="pitch">��� �ӵ�</param>
    /// <param name="pos"></param>
    public void Play(AudioClip audioClip, SoundType type = SoundType.Effect, float volume = 1.0f, float pitch = 1.0f, Vector3 pos = default)
    {
        if (audioClip == null)
            return;
        
        if (type == SoundType.BGM)
        {
            if(backgroundAudioSource.isPlaying)
                backgroundAudioSource.Stop();
            
            backgroundAudioSource.pitch = pitch;
            backgroundAudioSource.volume = volume * MusicSoundVolume * MasterSoundVolume;
            backgroundAudioSource.clip = audioClip;
            backgroundAudioSource.Play();

            DebugEx.Log($"GlobalSoundVolume is {MasterSoundVolume}");
        }
        else if (type == SoundType.Effect)
        {
            PlayEffect(audioClip, volume, pitch, pos).Forget();
        }
    }
    
    /// <summary>
    /// ȿ������ ����ϴ� �Լ�
    /// </summary>
    private async UniTaskVoid PlayEffect(AudioClip audioClip, float volume, float pitch, Vector3 pos)
    {
        AudioSource audioSource = GetAudioSourceFromPool();

        audioSource.transform.position = pos;
        
        audioSource.clip = audioClip;
        audioSource.volume = volume * SfxSoundVolume * MasterSoundVolume;
        audioSource.pitch = pitch;
        audioSource.Play();

        await UniTask.Delay((int)(audioClip.length * 1000)); // Ŭ�� ���̸�ŭ ���

        ReturnAudioSourceToPool(audioSource);
    }

    /// <summary>
    /// ����� �ҽ� Ǯ���� ����� �ҽ��� �������� �Լ�
    /// </summary>
    private AudioSource GetAudioSourceFromPool()
    {
        if (effectAudioSourcePool.Count > 0)
        {
            AudioSource audioSource = effectAudioSourcePool.Dequeue();
            audioSource.gameObject.SetActive(true);
            return audioSource;
        }
        else
        {
            return CreateNewAudioSource();
        }
    }

    /// <summary>
    /// ����� �ҽ��� Ǯ�� ��ȯ�ϴ� �Լ�
    /// </summary>
    private void ReturnAudioSourceToPool(AudioSource audioSource)
    {
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.gameObject.SetActive(false);
        effectAudioSourcePool.Enqueue(audioSource);
    }

    /// <summary>
    /// ���ο� ����� �ҽ��� �����Ͽ� Ǯ�� �߰��ϴ� �Լ�
    /// </summary>
    private AudioSource CreateNewAudioSource()
    {
        GameObject obj = Instantiate(audioSourcePrefab, transform);
        AudioSource newAudioSource = obj.GetComponent<AudioSource>();
        obj.SetActive(false);
        effectAudioSourcePool.Enqueue(newAudioSource);
        return newAudioSource;
    }
}

public enum SoundType
{
    BGM,
    Effect,
}