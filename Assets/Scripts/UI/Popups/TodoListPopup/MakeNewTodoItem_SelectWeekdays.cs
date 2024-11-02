using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MakeNewTodoItem_SelectWeekdays : MonoBehaviour
{
    [SerializeField] private List<Toggle> days;

    private void OnEnable()
    {
        DisableAllDays();
    }

    public void DisableAllDays()
    {
        foreach (var day in days)
        {
            day.isOn = false;
        }
    }
    
    public List<DayOfWeek> GetSelectedRecurrenceDays()
    {
        List<DayOfWeek> selectedDays = new List<DayOfWeek>();
        foreach (Toggle toggle in days)
        {
            if (toggle.isOn)
            {
                selectedDays.Add((DayOfWeek)toggle.transform.GetSiblingIndex());
            }
        }
        return selectedDays;
    }
}
