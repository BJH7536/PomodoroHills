using System;
using TMPro;
using TodoList;
using UnityEngine;
using UnityEngine.UI;

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
    private TodoItem _todoItem;
    
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
    
    public void ShowDeleteConfirmPopup()
    {
        // 롱프레스가 감지되면 삭제 확인 팝업 호출
        PopupManager.Instance.ShowConfirmPopup(
            title: "삭제 확인",
            message: $"'{_todoItem.Name}' 할 일을 삭제하시겠습니까?",
            onConfirm: DeleteTodoItem
        );
    }

    private void DeleteTodoItem()
    {
        // TodoItem 삭제 로직
        TodoManager.Instance.DeleteTodoItem(_todoItem);
        
        _todoItemUIPool.ReturnTodoItemUI(gameObject);
        
        // UI에서 해당 TodoItem 삭제
        // Destroy(gameObject);  
    }
    
}
