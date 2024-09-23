using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckboxButtonController : MonoBehaviour
{
    [SerializeField] private List<Button> buttons;
    [SerializeField] private List<TMP_Text> texts;

    public void OnToggled(bool toggled)
    {
        if (toggled)
        {
            foreach (var b in buttons)
            {
                b.interactable = true;
                b.GetComponent<DatePickerCaller>().SetDateTime(DateTime.Today);
            }

            foreach (var t in texts)
            {
                t.alpha = 1f;
            }
        }
        else
        {
            foreach (var b in buttons)
            {
                b.interactable = false;
                b.GetComponent<DatePickerCaller>().SetDateTime(DateTime.Today);
            }

            foreach (var t in texts)
            {
                t.alpha = 0.5f;
                t.text = $"{DateTime.Today:yyyy.MM.dd}";
            }
        }
    }

}
