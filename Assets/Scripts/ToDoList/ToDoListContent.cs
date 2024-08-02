using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToDoListContent : MonoBehaviour
{
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI timeText;
    public Button playButton;
    public Button deleteButton;

    public float timePerformed;             //타이머와 함께 작동한 시간(누적)
    

    private void Start()                            //삭제버튼 클릭 시 ToDoItem 삭제
    {
        deleteButton.onClick.AddListener(Delete);
        playButton.onClick.AddListener(SelectToDo);
        SetTimeText();
    }
    private void Delete()
    {       
        Destroy(gameObject);                        //ToDoItem삭제
    }
    public void SetTimeText()                       //시간 텍스트 갱신 메서드
    {
        int hours = Mathf.FloorToInt(timePerformed / 3600);
        int minutes = Mathf.FloorToInt((timePerformed % 3600) / 60);
        int seconds = Mathf.FloorToInt(timePerformed % 60);
        if (seconds == 60) { seconds--; }
        timeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
    }
    public void SelectToDo()
    {
        TimerWork.Instance.selectedToDo = GetComponent<ToDoListContent>();
    }

}
