using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DatePickerCaller : MonoBehaviour
{
    [SerializeField] private DatePickerUI _datePickerUI;
    [SerializeField] private TMP_Text callerText;

    private DateTime _dateTime;

    private void OnEnable()
    {
        SetDateTime(DateTime.Today);
    }
    
    public void OpenDatePicker()
    {
        _datePickerUI.Open(this, _dateTime);
    }
    
    public void SetDateTime(DateTime dateTime)
    {
        _dateTime = dateTime;
        callerText.text = $"{dateTime:yyyy.MM.dd}";
    }

    public DateTime GetDateTime(DateTime dateTime)
    {
        return _dateTime;
    }
}
