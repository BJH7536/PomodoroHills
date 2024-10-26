using System;
using TMPro;
using TodoSystem;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TodoListPopup에서 TodoItem을 목록 중 하나의 UI항목으로 시각화하기 위한 스크립트.
/// TodoItem의 UI버전.
/// </summary>
public class TodoListPopup_TodoItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image checkMark;
    [SerializeField] private TMP_Text percentageText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text periodText;

    [SerializeField] private GameObject recurrenceDaily;
    [SerializeField] private GameObject recurrenceWeekly;

    private TodoItemUIPool _todoItemUIPool;
    private TodoItem _todoItem;                         // 실제 OnTodoItemLink 객체 
    
    public void SetUp(TodoItem todoItem, TodoItemUIPool pool)
    {
        _todoItem = todoItem;

        _todoItemUIPool = pool;
        
        nameText.text = _todoItem.Name;
        checkMark.enabled = _todoItem.IsTodayTaskCompleted(DateTime.Today);
        percentageText.text = $"{_todoItem.GetProgressPercentage():F2}%";
        
        descriptionText.text = _todoItem.Description;

        DateTime? startDate = todoItem.GetStartDateTime();
        DateTime? endDate = todoItem.GetEndDateTime();

        if (startDate == endDate)
        {
            periodText.text = $"{todoItem.GetStartDateTime()?.Date:yy.MM.dd}";
        }
        else
        {
            periodText.text = $"{todoItem.GetStartDateTime()?.Date:yy.MM.dd}~\n" + 
                              $"{todoItem.GetEndDateTime()?.Date:yy.MM.dd}";
        }
        
        recurrenceDaily.SetActive(false);
        recurrenceWeekly.SetActive(false);

        if (_todoItem.Recurrence == Recurrence.Daily)
        {
            recurrenceDaily.SetActive(true);
        }
        else if (_todoItem.Recurrence == Recurrence.Weekly)
        {
            recurrenceWeekly.SetActive(true);

            // 모든 요일 버튼을 비활성화
            for (int i = 0; i < recurrenceWeekly.transform.childCount; i++)
            {
                recurrenceWeekly.transform.GetChild(i).gameObject.SetActive(false);
            }

            // 설정된 요일만 활성화
            foreach (var day in _todoItem.RecurrenceDays)
            {
                recurrenceWeekly.transform.GetChild((int)day).gameObject.SetActive(true);
            }
        }
    }

    private void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(ShowTimerPopup);
    }

    public void ShowTimerPopup()
    {
        // Todo항목을 짧게 터치하면 타이머에 연결하는 기능 추가 필요
        TimerManager.Instance.LinkTodoItem(_todoItem);              // 이 ui 요소를 터치하면 TimerManager에 todoItem 객체를 연결시키고
        PopupManager.Instance.ShowPopup<TimerPopup>();              // TimerPopup을 띄운다
    }

    public void ShowDeleteConfirmPopup()
    {
        // 롱프레스가 감지되면 삭제 확인 팝업 호출
        PopupManager.Instance.ShowConfirmPopup(
            title: "삭제 확인",
            message: $"'{_todoItem.Name}' 할 일을 삭제하시겠습니까?",
            confirmAction: DeleteTodoItem
        );
    }

    private void DeleteTodoItem()
    {
        // OnTodoItemLink 삭제 로직
        TodoManager.Instance.DeleteTodoItem(_todoItem);
        
        // UI에서 해당 OnTodoItemLink 삭제
        _todoItemUIPool.ReturnTodoItemUI(gameObject);
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    public void UpdateProgressUI()
    {
        checkMark.enabled = _todoItem.IsTodayTaskCompleted(DateTime.Today);
        percentageText.text = $"{_todoItem.GetProgressPercentage():F2}%";
    }
}
