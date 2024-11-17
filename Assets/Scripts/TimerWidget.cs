using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerWidget : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Button button;
    
    private TimerManager _timerManager;

    private void Start()
    {
        _timerManager = TimerManager.Instance;
        _timerManager.OnTimeUpdated += UpdateTimeDisplay;
        
        button.onClick.AddListener(ShowTimerPopup);
    }

    private void UpdateTimeDisplay(int remainingTimeInSeconds)
    {
        if (remainingTimeInSeconds <= 0) remainingTimeInSeconds = 0;
        
        TimeSpan time = TimeSpan.FromSeconds(remainingTimeInSeconds);
        timeText.text = $"{time.Minutes:D2}:{time.Seconds:D2}";
    }

    private void ShowTimerPopup()
    {
        PopupManager.Instance.ShowPopup<TimerPopup>();
    }
}