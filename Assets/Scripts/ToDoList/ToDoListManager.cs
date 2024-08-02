using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ToDoListManager : MonoBehaviour
{
    public TMP_InputField inputField;   //TMP
    public GameObject todoItemPrefab;   //toDoItem 프리팹
    public Transform content;           //Scroll View Content 오브젝트
    public Button addItemButton;


    public GameObject toDoListUI;
    public GameObject timerUI;

    private void Start()
    {
        addItemButton.onClick.AddListener(AddToDoItem);
    }

    public void AddToDoItem()           //toDoList에 새 할일분야를 추가하는 기능
    {
        if (!string.IsNullOrWhiteSpace(inputField.text)){
            GameObject newItem = Instantiate(todoItemPrefab, content);
            ToDoListContent newItemContent = newItem.GetComponent<ToDoListContent>();
            newItemContent.contentText.text = inputField.text;
            inputField.text = "";

            if (timerUI != null)                //ON/OFF해야할 UI에 대한 정보 저장
                newItemContent.timerUI = timerUI;
            if (toDoListUI != null)
                newItemContent.toDoListUI = toDoListUI;   
        }
    }





}
