using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_InputField timeInputField;
    [SerializeField] private Button startButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button cancelButton;
    
    private void Start()
    {

        startButton.onClick.AddListener(StartTimer);
        pauseButton.onClick.AddListener(PauseTimer);
        resumeButton.onClick.AddListener(ResumeTimer);
        cancelButton.onClick.AddListener(CancelTimer);

        TimerManager.Instance.OnTimeUpdated += UpdateTimeDisplay;
        TimerManager.Instance.OnTimerCompleted.AddListener(OnTimerCompleted);
    }

    private void StartTimer()
    {
        // 입력된 시간(초)을 타이머로 설정
        if (int.TryParse(timeInputField.text.Trim(), out int seconds))
        {
            TimerManager.Instance.StartTimer(seconds);
        }
        else
        {
            Debug.LogWarning("유효한 시간을 입력하세요!");
        }
    }

    private void PauseTimer()
    {
        TimerManager.Instance.PauseTimer();
    }

    private void ResumeTimer()
    {
        TimerManager.Instance.ResumeTimer();
    }

    private void CancelTimer()
    {
        TimerManager.Instance.CancelTimer();
        timeText.text = "00:00";
    }

    private void UpdateTimeDisplay(int remainingTimeInSeconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(remainingTimeInSeconds);
        timeText.text = $"{time.Minutes:D2}:{time.Seconds:D2}";
    }

    private void OnTimerCompleted()
    {
        timeText.text = "Completed!";
    }
}