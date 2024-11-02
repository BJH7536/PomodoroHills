using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerPopup : Popup
{

    [SerializeField] public PanelCircularTimer panelCircularTimer;
    
    public void Close()
    {
        PopupManager.Instance.HidePopup();
    }

    public override void OnClose()
    {
        TodoListPopup todoListPopup = FindObjectOfType<TodoListPopup>();
        if (todoListPopup != null)
        {
            todoListPopup.UpdateAllTodoItemsUI();
        }
    }
}
