using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI timerText;           // Ÿ�̸Ӹ� ǥ���� UI Text ���
    [SerializeField] private float timer;                        // ��� �ð� ���� ����
    [SerializeField] private bool isRunning;                     // Ÿ�̸� �۵� ����

    void Start()
    {
        timer = 0f;             // Ÿ�̸� �ʱ�ȭ
        isRunning = false;      // Ÿ�̸� �ʱ� ���¸� ������ ����
    }

    void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime; // Ÿ�̸� ������Ʈ
            UpdateTimerText();       // Ÿ�̸� �ؽ�Ʈ ������Ʈ
        }
    }

    void UpdateTimerText()
    {
        int hours = Mathf.FloorToInt(timer / 3600f);      //�� ���
        int minutes = Mathf.FloorToInt(timer / 60f);    // �� ���
        int seconds = Mathf.FloorToInt(timer % 60f);    // �� ���

        // Ÿ�̸� ���ڿ� ����ȭ �� UI �ؽ�Ʈ ������Ʈ
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    // Ÿ�̸� ����/���� ��� �Լ�
    public void ToggleTimer(bool isOn)
    {
        isRunning = isOn;
    }

    // Ÿ�̸� �Ͻ����� �Լ�
    public void PauseTimer()
    {
        isRunning = false;
    }

    // Ÿ�̸� �簳 �Լ�
    public void ResumeTimer()
    {
        isRunning = true;
    }

}
