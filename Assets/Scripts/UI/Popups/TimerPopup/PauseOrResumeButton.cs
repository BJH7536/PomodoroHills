using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseOrResumeButton : MonoBehaviour
{
    [SerializeField] public Button pauseButton;
    [SerializeField] public Button resumeButton;

    private void OnEnable()
    {
        switch (TimerManager.Instance.CurrentTimerState)
        {
            case TimerState.Running:
                ShowPauseButton();
                break;
            case TimerState.Paused:
                ShowResumeButton();
                break;
        }
    }

    public void ShowPauseButton()
    {
        pauseButton.transform.parent.gameObject.SetActive(true);
        resumeButton.transform.parent.gameObject.SetActive(false);
    }
    
    public void ShowResumeButton()
    {
        pauseButton.transform.parent.gameObject.SetActive(false);
        resumeButton.transform.parent.gameObject.SetActive(true);
    }
}
