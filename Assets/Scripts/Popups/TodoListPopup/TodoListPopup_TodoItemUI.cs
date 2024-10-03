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

            // ��� ���� ��ư�� ��Ȱ��ȭ
            for (int i = 0; i < recurrenceWeekly.transform.childCount; i++)
            {
                recurrenceWeekly.transform.GetChild(i).gameObject.SetActive(false);
            }

            // ������ ���ϸ� Ȱ��ȭ
            foreach (var day in _todoItem.RecurrenceDays)
            {
                recurrenceWeekly.transform.GetChild((int)day).gameObject.SetActive(true);
            }
        }
    }
    
    public void ShowDeleteConfirmPopup()
    {
        // ���������� �����Ǹ� ���� Ȯ�� �˾� ȣ��
        PopupManager.Instance.ShowConfirmPopup(
            title: "���� Ȯ��",
            message: $"'{_todoItem.Name}' �� ���� �����Ͻðڽ��ϱ�?",
            onConfirm: DeleteTodoItem
        );
    }

    private void DeleteTodoItem()
    {
        // TodoItem ���� ����
        TodoManager.Instance.DeleteTodoItem(_todoItem);
        
        _todoItemUIPool.ReturnTodoItemUI(gameObject);
        
        // UI���� �ش� TodoItem ����
        // Destroy(gameObject);  
    }
    
}
