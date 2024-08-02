using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ToDoListContent : MonoBehaviour
{
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI timeText;
    public Button playButton;
    public Button deleteButton;

    public GameObject toDoListUI;
    public GameObject timerUI;

    public float timePerformed;             //타이머와 함께 작동한 시간(누적)
    //timePerformed = before + timermax -timerleft로 수정

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
        //기존 선택 버튼 색상 변경
        if (TimerWork.Instance.selectedToDo != null)
        {
            TimerWork.Instance.selectedToDo.playButton.image.color = Color.white;
        }
        TimerWork.Instance.selectedToDo = GetComponent<ToDoListContent>();
        TimerWork.Instance.selectedToDo.playButton.image.color = Color.green;

        if (toDoListUI != null)         //투두리스트 UI를 끄고타이머의 UI로 이동
            toDoListUI.SetActive(false);
        if (timerUI != null)
            timerUI.SetActive(true);
}

}
