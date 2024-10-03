using System;
using System.Collections.Generic;
using Ricimi;
using TMPro;
using TodoList;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MakeNewTodoItem : MonoBehaviour
{
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField descriptionInputField;
    [SerializeField] private Toggle termToggle;
    [SerializeField] private DatePickerCaller datePickerStart;
    [SerializeField] private DatePickerCaller datePickerEnd;
    [SerializeField] private Toggle timeBasedToggle;
    [SerializeField] private Toggle checkBasedToggle;
    [SerializeField] private TMP_InputField durationInputField;

    [SerializeField] private Toggle recurrenceNoneToggle;
    [SerializeField] private Toggle recurrenceDailyToggle;
    [SerializeField] private Toggle recurrenceWeeklyToggle;
    [SerializeField] private MakeNewTodoItem_SelectWeekdays recurrenceDays;

    [SerializeField] private TextSelectionSlider prioritySlider;
    
    private void OnEnable()
    {
        InitializeUI();
        //RegisterEventHandlers();
    }

    private void InitializeUI()
    {
        // 초기 UI 상태 설정
        nameInputField.text = string.Empty;
        descriptionInputField.text = string.Empty;
        termToggle.isOn = true;
        checkBasedToggle.isOn = false;
        timeBasedToggle.isOn = true;
        durationInputField.text = string.Empty;
        
        recurrenceWeeklyToggle.isOn = false;
        recurrenceDailyToggle.isOn = false;
        recurrenceNoneToggle.isOn = true;

        prioritySlider.InitializeToFirstValue();

        //UpdateUIBasedOnType();
        //UpdateUIBasedOnRecurrence();
    }
    
    public void OnCreateButtonClicked()
    {
        // 입력 데이터 수집 및 검증
        if (!ValidateInputs())
        {
            Debug.LogWarning("입력 값이 유효하지 않습니다.");
            return;
        }

        // 입력 데이터 수집
        string name = nameInputField.text.Trim();
        string description = descriptionInputField.text.Trim();
        
        DateTime startDate = datePickerStart.GetDateTime();
        DateTime endDate = datePickerEnd.GetDateTime();

        ItemType type = timeBasedToggle.isOn ? ItemType.TimeBased : ItemType.CheckBased;

        int durationInMinutes = 0;
        if (type == ItemType.TimeBased)
            durationInMinutes = int.Parse(durationInputField.text);
        
        Recurrence recurrence = Recurrence.None;
        if (recurrenceNoneToggle.isOn)
            recurrence = Recurrence.None;
        else if (recurrenceDailyToggle)
            recurrence = Recurrence.Daily;
        else if (recurrenceWeeklyToggle)
            recurrence = Recurrence.Weekly;
        
        List<DayOfWeek> recurrenceDays = null;
        if (recurrence == Recurrence.Weekly)
            recurrenceDays = this.recurrenceDays.GetSelectedRecurrenceDays();

        int priority = int.Parse(prioritySlider.GetCurrentValue());
        
        // 새로운 TodoItem 객체 생성
        TodoItem newTodoItem = new TodoItem(
            name: name,
            description: description,
            startDate: startDate,
            endDate: endDate,
            type: type,
            priority: priority,
            recurrence: recurrence,
            status: Status.Pending,
            durationInMinutes: durationInMinutes,
            recurrenceDays: recurrenceDays
        );

        // TodoManager에 추가
        TodoManager.Instance.AddTodoItem(newTodoItem);
        
        // 화면 닫기
        gameObject.SetActive(false);
    }
    
    private bool ValidateInputs()
    {
        // 필수 입력 필드 검증
        if (string.IsNullOrEmpty(nameInputField.text))
        {
            PopupManager.Instance.ShowErrorPopup("제목을 입력해주세요.");
            
            Debug.LogWarning("제목을 입력해주세요.");
            return false;
        }
        
        if (datePickerStart.GetDateTime() > datePickerEnd.GetDateTime())
        {
            PopupManager.Instance.ShowErrorPopup("시작일은 종료일보다 이전이어야 합니다.");
            
            Debug.LogWarning("시작일은 종료일보다 이전이어야 합니다.");
            return false;
        }
        
        if (timeBasedToggle.isOn)
        {
            if (string.IsNullOrEmpty(durationInputField.text) || !int.TryParse(durationInputField.text, out var duration))
            {
                PopupManager.Instance.ShowErrorPopup("소요 시간을 올바르게 입력해주세요.");
                
                Debug.LogWarning("소요 시간을 올바르게 입력해주세요.");
                return false;
            }
        }

        return true;
    }
}
