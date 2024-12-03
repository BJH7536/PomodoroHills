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
        if (Instance == null)           //싱글톤 선언
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
    [SerializeField] private float masterSoundVolume = 1.0f;             // 마스터 볼륨

    public float MasterSoundVolume
    {
        get => masterSoundVolume;
        set
        {
            masterSoundVolume = value;
            OnChangeMasterSoundVolume?.Invoke(masterSoundVolume);
        }
    }
    
    [SerializeField] private float musicSoundVolume = 1.0f;              // 음악
    
    public float MusicSoundVolume
    {
        get => musicSoundVolume;
        set
        {
            musicSoundVolume = value;
            OnChangeMusicSoundVolume?.Invoke(musicSoundVolume);
        }
    }
    
    [SerializeField] private float sfxSoundVolume = 1.0f;            // 효과음
    
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
        // 배경음 AudioSource 세팅
        if (backgroundAudioSource == null)
        {
            GameObject BGM = new GameObject("@BGM");

            BGM.transform.SetParent(transform);
            backgroundAudioSource = BGM.AddComponent<AudioSource>();
            backgroundAudioSource.loop = true;
            
            DontDestroyOnLoad(BGM);
        }

        // 효과음 오디오 소스 풀 초기화
        for (int i = 0; i < InitialPoolSize; i++)
        {
            CreateNewAudioSource();
        }

        // 마스터 볼륨이 변할 때
        OnChangeMasterSoundVolume += ChangeMasterSoundVolume;
    }

    void ChangeMasterSoundVolume(float value)
    {
        
    }

    /// <summary>
    /// AudioClip 버전의 오디오 플레이 함수. 
    /// </summary>
    /// <param name="audioClip">AudioClip</param>
    /// <param name="type">Clip의 종류</param>
    /// <param name="volume">재생 볼륨</param>
    /// <param name="pitch">재생 속도</param>
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
    /// 효과음을 재생하는 함수
    /// </summary>
    private async UniTaskVoid PlayEffect(AudioClip audioClip, float volume, float pitch, Vector3 pos)
    {
        AudioSource audioSource = GetAudioSourceFromPool();

        audioSource.transform.position = pos;
        
        audioSource.clip = audioClip;
        audioSource.volume = volume * SfxSoundVolume * MasterSoundVolume;
        audioSource.pitch = pitch;
        audioSource.Play();

        await UniTask.Delay((int)(audioClip.length * 1000)); // 클립 길이만큼 대기

        ReturnAudioSourceToPool(audioSource);
    }

    /// <summary>
    /// 오디오 소스 풀에서 오디오 소스를 가져오는 함수
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
    /// 오디오 소스를 풀로 반환하는 함수
    /// </summary>
    private void ReturnAudioSourceToPool(AudioSource audioSource)
    {
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.gameObject.SetActive(false);
        effectAudioSourcePool.Enqueue(audioSource);
    }

    /// <summary>
    /// 새로운 오디오 소스를 생성하여 풀에 추가하는 함수
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