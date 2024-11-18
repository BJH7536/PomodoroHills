using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LofiMusicPlayList : MonoBehaviour
{
    [SerializeField] private List<AudioClip> playlist = new();
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int currentTrackIndex = 0;

    private Coroutine trackCoroutine;

    private void Start()
    {
        PlayRandomTrack();

        SoundManager.Instance.OnChangeMasterSoundVolume += ChangeAudioSourceVolume;
        SoundManager.Instance.OnChangeMusicSoundVolume += ChangeAudioSourceVolume;

        audioSource.loop = false; // 루프 비활성화
    }

    void ChangeAudioSourceVolume(float value)
    {
        audioSource.volume = SoundManager.Instance.MasterSoundVolume * SoundManager.Instance.MusicSoundVolume;
    }

    /// <summary>
    /// 현재 트랙을 재생합니다.
    /// </summary>
    public void Play()
    {
        if (playlist == null || playlist.Count == 0 || audioSource == null)
        {
            DebugEx.LogWarning("Playlist is empty or AudioSource is not initialized.");
            return;
        }

        if (audioSource.isPlaying)
            audioSource.Stop();

        if (trackCoroutine != null)
        {
            StopCoroutine(trackCoroutine);
        }

        audioSource.clip = playlist[currentTrackIndex];
        audioSource.time = 0f;
        audioSource.volume = SoundManager.Instance.MusicSoundVolume * SoundManager.Instance.MasterSoundVolume;
        audioSource.Play();

        DebugEx.Log($"Playing track: {playlist[currentTrackIndex].name}");

        trackCoroutine = StartCoroutine(WaitForTrackEnd());
    }

    /// <summary>
    /// 재생/일시정지를 토글합니다.
    /// </summary>
    public void TogglePause()
    {
        if (audioSource == null || audioSource.clip == null)
        {
            DebugEx.LogWarning("No track is currently loaded in the AudioSource.");
            return;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            DebugEx.Log("Track paused.");

            if (trackCoroutine != null)
            {
                StopCoroutine(trackCoroutine);
                trackCoroutine = null;
            }
        }
        else
        {
            audioSource.UnPause();
            DebugEx.Log("Track resumed.");

            if (trackCoroutine != null)
            {
                StopCoroutine(trackCoroutine);
            }

            trackCoroutine = StartCoroutine(WaitForTrackEnd());
        }
    }

    /// <summary>
    /// 랜덤 트랙을 재생합니다.
    /// </summary>
    public void PlayRandomTrack()
    {
        if (playlist == null || playlist.Count == 0)
        {
            DebugEx.LogWarning("Playlist is empty.");
            return;
        }

        int randomIndex;
        if (playlist.Count == 1)
        {
            randomIndex = 0;
        }
        else
        {
            do
            {
                randomIndex = Random.Range(0, playlist.Count);
            } while (randomIndex == currentTrackIndex);
        }

        currentTrackIndex = randomIndex;
        Play();

        DebugEx.Log($"Random track selected: {playlist[currentTrackIndex].name}");
    }

    /// <summary>
    /// 현재 트랙을 처음부터 다시 재생합니다.
    /// </summary>
    public void RestartCurrentTrack()
    {
        if (audioSource == null || audioSource.clip == null)
        {
            DebugEx.LogWarning("No track is currently loaded in the AudioSource.");
            return;
        }

        if (trackCoroutine != null)
        {
            StopCoroutine(trackCoroutine);
            trackCoroutine = null;
        }

        audioSource.time = 0;
        audioSource.Play();
        DebugEx.Log("Track restarted.");

        trackCoroutine = StartCoroutine(WaitForTrackEnd());
    }

    /// <summary>
    /// 트랙이 끝날 때까지 대기 후 랜덤 트랙 재생
    /// </summary>
    private IEnumerator WaitForTrackEnd()
    {
        while (audioSource.isPlaying)
        {
            yield return null;
        }

        OnTrackFinished();
    }

    /// <summary>
    /// 트랙 재생이 끝났을 때 호출되는 함수
    /// </summary>
    private void OnTrackFinished()
    {
        DebugEx.Log("Track finished. Playing next random track.");
        PlayRandomTrack();
    }
}
