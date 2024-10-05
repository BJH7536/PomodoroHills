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
        // �Էµ� �ð�(��)�� Ÿ�̸ӷ� ����
        if (int.TryParse(timeInputField.text.Trim(), out int seconds))
        {
            TimerManager.Instance.StartTimer(seconds);
        }
        else
        {
            Debug.LogWarning("��ȿ�� �ð��� �Է��ϼ���!");
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