using System;
using TMPro;
using UnityEngine;

public class TimerWidget : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;

    private TimerManager _timerManager;

    private void Start()
    {
        _timerManager = TimerManager.Instance;
        _timerManager.OnTimeUpdated += UpdateTimeDisplay;
    }

    private void UpdateTimeDisplay(int remainingTimeInSeconds)
    {
        if (remainingTimeInSeconds <= 0) remainingTimeInSeconds = 0;
        
        TimeSpan time = TimeSpan.FromSeconds(remainingTimeInSeconds);
        timeText.text = $"{time.Minutes:D2}:{time.Seconds:D2}";
    }
}