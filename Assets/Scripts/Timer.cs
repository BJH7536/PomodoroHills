using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI timerText;           // 타이머를 표시할 UI Text 요소
    [SerializeField] private float timer;                        // 경과 시간 저장 변수
    [SerializeField] private bool isRunning;                     // 타이머 작동 여부

    void Start()
    {
        timer = 0f;             // 타이머 초기화
        isRunning = false;      // 타이머 초기 상태를 정지로 설정
    }

    void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime; // 타이머 업데이트
            UpdateTimerText();       // 타이머 텍스트 업데이트
        }
    }

    void UpdateTimerText()
    {
        int hours = Mathf.FloorToInt(timer / 3600f);      //시 계산
        int minutes = Mathf.FloorToInt(timer / 60f);    // 분 계산
        int seconds = Mathf.FloorToInt(timer % 60f);    // 초 계산

        // 타이머 문자열 형식화 및 UI 텍스트 업데이트
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    // 타이머 시작/정지 토글 함수
    public void ToggleTimer(bool isOn)
    {
        isRunning = isOn;
    }

    // 타이머 일시정지 함수
    public void PauseTimer()
    {
        isRunning = false;
    }

    // 타이머 재개 함수
    public void ResumeTimer()
    {
        isRunning = true;
    }

}
