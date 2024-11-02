using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DatePickerCaller : MonoBehaviour
{
    [SerializeField] private TMP_Text callerText;

    private DateTime _dateTime;

    private void OnEnable()
    {
        SetDateTime(DateTime.Today);
    }
    
    public void OpenDatePicker()
    {
        //todoListPopupDatePicker.Open(this, _dateTime);
        
        PopupManager.Instance.ShowPopup<TodoListPopup_DatePicker>().Open(this, _dateTime);
    }
    
    public void SetDateTime(DateTime dateTime)
    {
        _dateTime = dateTime;
        callerText.text = $"{dateTime:yyyy.MM.dd}";
    }

    public DateTime GetDateTime()
    {
        return _dateTime;
    }
}
