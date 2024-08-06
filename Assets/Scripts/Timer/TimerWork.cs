using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ricimi;
using Cinemachine;
using Gley.Jumpy;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class TimerWork : MonoBehaviour
{
    private static TimerWork _instance;

    public static TimerWork Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TimerWork>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("TimerWork");
                    _instance = go.AddComponent<TimerWork>();
                }
            }
            return _instance;
        }
    }


    // 실제로 시간이 작동하는 스크립트임

    private float timeToMain ;                  //집중 타이머 설정값
    private float timeToRest;                   //휴식 타이머 설정값
    private float timeLeft ;                    //타이머 잔여값       (집중주기와 휴식주기가 번갈아가면서 사용함)

    private int  cycleToDo = 1;                 //집중&휴식 타이머 목표 반복회수
    private int  cycleLeft = 1;                 //집중&휴식 타이머 잔여 반복회수
    private bool restCheck = false;             //잔여 타이머가 집중 타이머인지 휴식타이머인지
    private bool isStopped = true;              //타이머의 중단 여부 (ex)▶||
    private bool isWork = false;                //타이머의 작동 여부 (ex)▶■
    public bool IsStopped
    {
        get { return isStopped; }
        private set { IsStopped = value; }
    }
    public bool IsWork
    {
        get { return isWork; }
        private set { IsWork = value; }
    }
    public bool RestCheck
    {
        get { return restCheck; }
        private set { RestCheck = value; }
    }

    public ToDoListContent selectedToDo;        // 타이머의 실행결과를 받을 ToDoItem

    public TimerInputManager timerInputManager; 
    public TextMeshProUGUI textForTimer;        // 타이머 숫자 UI
    public GameObject timerSlide;               // 타이머 진척도를 표기할 원형 슬라이드 본체
    private CircularSilde circularSlide;        // 타이머 진척도를 표기할 원형 슬라이드 작동부

    public Button buttonPlay;                   // 새 타이머 주기를 시작하거나 중지/재개하는 버튼
    public Button buttonReset;                  // 현재 타이머 주기를 종료하고 초기화하는 버튼
    public RawImage mainImage;              //현재 상재 (주상태, 휴식상태)를 표시하기위한 임시수단입니다.
    public RawImage restImage;              //현재 상재 (주상태, 휴식상태)를 표시하기위한 임시수단입니다.

    public GameObject toDoListUI;
    public GameObject timerUI;
    public Button undoButton;

    void Awake()
    {
        if(_instance!=null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        circularSlide = timerSlide.GetComponent<CircularSilde>();
        buttonPlay.onClick.AddListener(PlayTimer);
        buttonReset.onClick.AddListener(ResetTimer);
    }

    void Start()
    {
        undoButton.onClick.AddListener(ReturnToDoListUI);
        TimerSet(5,5);
    }

    // Update is called once per frame
    void Update()
    {
        if(isWork && !isStopped)             //타이머가 중지상태가 아니고 실행된 상태이면 
        {
            if (timeLeft > 0){ 
                timeLeft -= Time.deltaTime; //타이머 시간 감소
                ReturnTimerResult();
                if (timeLeft < 0)           //잔여 시간이 잘못표시되지않게
                { timeLeft = 0; } 
                UpdateTimerText();          //타이머 텍스트 갱신
                UpdateTimerSlide();         //타이머 슬라이드 갱신
            }
            else if (!restCheck){ 
                timeLeft  = timeToRest; 
                restCheck = true;
                SetImage();
            }else if (cycleLeft > 1){       //잔여 반복회수 확인() 현재는 미사용
                cycleLeft--;
                timeLeft = timeToMain;
                restCheck = false;
                SetImage();
            }
            else
            {
                ResetTimer();               // 타이머 주기 종료 및 초기화
            }
        }   


    }






    void TimerSetFirst(float mainTime, float restTime, int cycleTime)
    {
        timeToMain = mainTime;          //집중시간 타이머의 시간을 설정
        timeToRest = restTime;      //집중시간 타이머의 시간을 설정
        TimerSet(mainTime, restTime); //타이머 시간, 상태 등 설정
        cycleLeft = cycleTime;      //반복할 횟수
        isWork = true;

    }                               //TimerSetFirst는 타이머의 목표 시간과 반복회수를 입력받아 설정하는 부분입니다.

    void TimerSet(float mainTime, float restTime)   //디버그용 타이머 설정 메서드
    {
        timeToMain = mainTime;
        timeToRest = restTime;
    }


    private void UpdateTimerSlide()                 //타이머 슬라이드 갱신
    {
        if (!restCheck)
            circularSlide.CallUpdateProgress(timeToMain - timeLeft, timeToMain);
        else if (restCheck)
            circularSlide.CallUpdateProgress(timeToRest - timeLeft, timeToRest);
    }

    private void UpdateTimerText()
    {
        int hours   = Mathf.FloorToInt(timeLeft / 3600);
        int minutes = Mathf.FloorToInt((timeLeft % 3600)/60);
        int seconds = Mathf.CeilToInt(timeLeft % 60);
        if (seconds == 60) { seconds--; }
        textForTimer.text = string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
    }   //ToDo: 종료시 000000으로 초기화도  만들자


    private void UpdateTimerTextEnd()
    {
        textForTimer.text = "00:00:00";
    }

    public void GetSubmit(float mainTime, float restTime) {     //타이머 설정 창에서 타이머 저장을 위한 메서드
        timeToMain = mainTime;
        timeToRest = restTime;
        isStopped = true;
        isWork = false;
        ResetTimer();
    }

    private void PlayTimer()        //타이머 실행 및 버튼 색 전환
    {
        if (!isWork) { ResetTimer(); isWork = true; buttonPlay.image.color = Color.green; }
        else if (!isStopped) { isStopped = true; buttonPlay.image.color = Color.white; }
        else { isStopped = false; buttonPlay.image.color = Color.green ; }
    }
    private void ResetTimer()       //타이머 초기화
    {
        isWork = false;
        isStopped = false;
        restCheck = false;
        timeLeft = timeToMain;
        cycleLeft = cycleToDo;

        ResetToDoTimeToInt();

        SetImage();
        buttonPlay.image.color = Color.white;

        UpdateTimerText();
        UpdateTimerSlide();
    }


    private void SetImage()         //타이머에 집중주기와 휴식주기의 아이콘 전환
    {
        if (restCheck)
        {
            mainImage.enabled = false;
            restImage.enabled = true;
        }
        else
        {
            restImage.enabled = false;
            mainImage.enabled = true;
        }
    }

    //아래의 코드는 상하관계인 ToDoList UI와의 이동(UI전환)을 제어하는 코드입니다.
    private void ReturnToDoListUI()
    {
        if (timerUI != null)
            timerUI.SetActive(false);
        if (toDoListUI != null)         //투두리스트 UI를 끄고타이머의 UI로 이동
            toDoListUI.SetActive(true);

    }



    //아래의 코드는 현재 선택된 ToDoListContent의 누적시간을 제어하는 코드입니다.
    private void ReturnTimerResult()                    //선택된 ToDoList에 타이머 시간 반환
    {
        if(selectedToDo != null && !restCheck)
        {
            selectedToDo.timePerformed +=Time.deltaTime;
            selectedToDo.SetTimeText();                 //리스트로 돌아가는 메서드로 옮길예정
        }
    }
    private void ResetToDoTimeToInt()                   //타이머종료시 잔류한 소수점 아래의 값을 삭제합니다.
    {   if(selectedToDo !=null)
        selectedToDo.timePerformed = (int)selectedToDo.timePerformed;
    }



}
