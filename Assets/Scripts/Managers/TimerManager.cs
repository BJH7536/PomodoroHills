using System;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using TodoSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

// MVC 패턴 활용
// Model (TimerManager) : 타이머의 상태 및 로직 관리. 핵심 로직 처리
// View (TimerPopup(PanelCircularTimer), TimerWidget) : 타이머의 상태를 사용자에게 보여주는 UI 부분.
// Controller (TimerPopup(PanelCircularTimer)) : 사용자의 입력을 받아 TimerManager에게 타이머를 시작, 일시정지, 재개, 취소 등의 명령을 내린다.
public class TimerManager : MonoBehaviour
{
    #region Singleton

    /// <summary>
    /// 싱글톤 인스턴스
    /// </summary>
    private static TimerManager instance;

    /// <summary>
    /// 싱글톤 인스턴스에 접근하기 위한 프로퍼티
    /// </summary>
    public static TimerManager Instance => instance;
    
    #endregion

    #region Events

    // 타이머가 남은 시간이 갱신될 때 호출되는 이벤트, 매 초마다 실행될 것을 고려해 가벼운 Action으로.
    public Action<int> OnTimeUpdated;
    
    // 타이머가 완료되었을 때 호출되는 이벤트, 유니티 엔진에서 쉽게 연결할 수 있도록 UnityEvent로.
    [SerializeField] public UnityEvent OnSessionCompleted;
    
    // 전체 포모도로가 완료되었을 때 호출되는 이벤트, 유니티 엔진에서 쉽게 연결할 수 있도록 UnityEvent로.
    [SerializeField] public UnityEvent OnPpomodoroCompleted;
    
    // 상태가 변할 때 호출되는 이벤트.
    public event Action<TimerState> OnTimerStateChanged;
    
    #endregion

    #region Fields

    public CancellationTokenSource CancellationTokenSource;
    
    public TimerState PrevTimerState = TimerState.NeedToBeInitialized;
    
    public TimerState _currentTimerState = TimerState.NeedToBeInitialized;
    public TimerState CurrentTimerState
    {
        get => _currentTimerState;
        set
        {
            PrevTimerState = _currentTimerState;
            
            if (_currentTimerState != value)
            {
                _currentTimerState = value;
                OnTimerStateChanged?.Invoke(_currentTimerState);  // 상태 변경 시 이벤트 호출
            }
        }
    }
    
    private TodoItem currentTodoItem = null;           // 타이머에 연결된 OnTodoItemLink
    public TodoItem CurrentTodoItem => currentTodoItem;

    public bool TodoItemInitialized = false;
    
    private int _remainingTimeInSeconds;        // 남은 시간 (초 단위)
    public int RemainingTimeInSeconds
    {
        get => _remainingTimeInSeconds;
        set => _remainingTimeInSeconds = value;
    }

    public int focusMinute = 25;
    public int relaxMinute = 5;
    
    public int remainingCycleCount;             // 남은 사이클 수
    public int lastCycleTime;                   // 마지막 사이클 시간

    #endregion

    #region Application Lifecycle Methods
    
    /// <summary>
    /// 싱글톤 인스턴스 설정.
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 오브젝트가 파괴되지 않도록 설정
            
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 존재하면 중복 오브젝트를 파괴
            return;
        }

        OnTimerStateChanged += (state) =>
        {
            if(state == TimerState.PpomodoroEnd)
                OnPpomodoroCompleted?.Invoke();
        };
        
        OnPpomodoroCompleted.AddListener(() =>
        {
            string title = "할 일을 모두 완수했어요!";
            string message = $"대단해요!\n총 {currentTodoItem.DailyTargetDurationInMinutes}분동안 집중했어요!";
            int rewardCoin = currentTodoItem.DailyTargetDurationInMinutes * 100;
            int rewardGem = currentTodoItem.DailyTargetDurationInMinutes * 1;

            PopupManager.Instance.ShowRewardPopup(title, message, rewardCoin, rewardGem);
            
            CurrentTimerState = TimerState.NeedToBeInitialized;
        });
    }
    
    public void Start()
    {
        OnSessionCompleted.AddListener(WhenSessionCompleted);
    }
    
    public void OnApplicationPause(bool isPaused)
    {
        DebugEx.Log($"앱 일시정지됨 {isPaused}");

        // 게임이 일시정지되면
        if (isPaused)
        {
            // 타이머가 동작중인 경우에는 
            if (CurrentTimerState is TimerState.FocusSessionRunning or TimerState.RelaxSessionRunning)
            {
                // 타이머 상태 저장
                TimerData timerData = GetCurrentTimerData();
                TimerRestorationService.SaveTimerState(timerData);
                
                ScheduleNotification();
            }
        }
        // 게임이 재개되면 (켜지는 경우 포함)
        else
        {
            // 알림 취소
            NotificationManager.Instance.CancelAllNotifications();
            
            // 타이머 복원
            TimerData restoredData = TimerRestorationService.RestoreTimerState();
            if (restoredData != null)
            {
                RestoreTimerState(restoredData);
            }
        }
    }
    
    public void OnApplicationQuit()
    {
        // 게임이 종료될 때 타이머가 동작 중이면
        // 타이머 수복을 위한 상태 저장과 함께 알림을 예약
        if (CurrentTimerState is TimerState.FocusSessionRunning or TimerState.RelaxSessionRunning)
        {
            // 타이머 상태 저장
            TimerData timerData = GetCurrentTimerData();
            TimerRestorationService.SaveTimerState(timerData);

            // 알림 예약
            ScheduleNotification();
        }
    }
    
    #endregion
    
    #region TimerManagement

    /// <summary>
    /// TodoItem을 타이머와 연동
    /// </summary>
    /// <param name="todoItem">연동할 OnTodoItem</param>
    public void LinkTodoItem(TodoItem todoItem)
    {
        currentTodoItem = todoItem;

        TodoItemInitialized = true;
        
        DebugEx.Log($"{nameof(TimerManager)} : TodoItem '{todoItem.Name}'이(가) 타이머에 연동되었습니다.");
    }

    /// <summary>
    /// OnTodoItem 연동을 해제하는 기능
    /// </summary>
    public void UnlinkTodoItem()
    {
        currentTodoItem = null;
        
        TodoItemInitialized = false;
        
        DebugEx.Log($"{nameof(TimerManager)} : TodoItem 연동이 해제되었습니다.");
    }

    /// <summary>
    /// 타이머 시간 설정 및 시작
    /// </summary>
    /// <param name="durationInSeconds"></param>
    /// <param name="state"></param>
    public void StartTimer(int durationInSeconds, TimerState state)
    {
        // 타이머가 실행 중인 경우, 중복 시작을 방지
        // if (CurrentTimerState == state)
        // {
        //     DebugEx.LogWarning("타이머가 이미 실행 중입니다!");
        //     return;
        // }

        // 남은 시간 설정
        _remainingTimeInSeconds = durationInSeconds;
        // 타이머 상태 전환
        CurrentTimerState = state;
        
        CancellationTokenSource = new CancellationTokenSource();
        UpdateTimer(CancellationTokenSource.Token).Forget();
    }
    
    /// <summary>
    /// 타이머 일시정지
    /// </summary>
    public void PauseTimer()
    {
        if (CurrentTimerState is TimerState.FocusSessionRunning or TimerState.RelaxSessionRunning)
        {
            CurrentTimerState = TimerState.Paused;
            CancellationTokenSource?.Cancel();

            TimerData timerData = GetCurrentTimerData();
            TimerRestorationService.SaveTimerState(timerData);
            
            NotificationManager.Instance.CancelAllNotifications();      // 알림 취소
            DebugEx.Log("타이머가 일시정지되었습니다.");
        }
    }

    /// <summary>
    /// 타이머 재개
    /// </summary>
    public void ResumeTimer()
    {
        if (CurrentTimerState == TimerState.Paused && _remainingTimeInSeconds > 0)
        {
            CurrentTimerState = PrevTimerState;
            CancellationTokenSource = new CancellationTokenSource();
            UpdateTimer(CancellationTokenSource.Token).Forget();
            NotificationManager.Instance.ScheduleNotification(_remainingTimeInSeconds, "타이머 완료", "설정하신 타이머가 완료되었습니다."); // 알림 예약
            DebugEx.Log("타이머가 재개되었습니다.");
        }
    }

    /// <summary>
    /// 타이머 취소
    /// </summary>
    public void CancelTimer()
    {
        if (CurrentTimerState == TimerState.Stopped || _remainingTimeInSeconds > 0)
        {
            CurrentTimerState = TimerState.Stopped;
            _remainingTimeInSeconds = 0;
            CancellationTokenSource?.Cancel();
            NotificationManager.Instance.CancelAllNotifications(); // 알림 취소
            
            OnTimeUpdated?.Invoke(_remainingTimeInSeconds);
            
            DebugEx.Log("타이머가 취소되었습니다.");
        }
    }

    /// <summary>
    /// 타이머 상태 갱신
    /// </summary>
    public async UniTaskVoid UpdateTimer(CancellationToken token)
    {
        while (_remainingTimeInSeconds > 0 && !token.IsCancellationRequested && CurrentTimerState is TimerState.FocusSessionRunning or TimerState.RelaxSessionRunning)
        {
            OnTimeUpdated?.Invoke(_remainingTimeInSeconds);
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
            _remainingTimeInSeconds--;
            
            // 매 60초마다 TodoItem에 진행 상황을 기록
            if (_remainingTimeInSeconds % 60 == 0 && currentTodoItem != null && CurrentTimerState is TimerState.FocusSessionRunning)
            {
                currentTodoItem.AddProgress(DateTime.Today, 1); // 1분 단위로 기록
            }
        }

        if (_remainingTimeInSeconds <= 0)
        {
            OnTimeUpdated?.Invoke(_remainingTimeInSeconds);
            OnSessionCompleted?.Invoke();
        }
    }
    
    private TimerData GetCurrentTimerData()
    {
        TimerData timerData = new TimerData
        {
            TimeStamp = DateTime.Now,
            RemainingTimeInSeconds = _remainingTimeInSeconds,
            CurrentTimerState = CurrentTimerState,
            PrevTimerState = PrevTimerState,
            RemainingCycleCount = remainingCycleCount,
            LastCycleTime = lastCycleTime,
            FocusMinute = focusMinute,
            RelaxMinute = relaxMinute,
            CurrentTodoItemId = TodoItemInitialized ? CurrentTodoItem.Id : null
        };

        return timerData;
    }
    
    private void RestoreTimerState(TimerData timerData)
    {
        // 경과 시간 계산
        TimeSpan elapsed = DateTime.Now - timerData.TimeStamp;
        int newRemainingSeconds = timerData.RemainingTimeInSeconds - (int)elapsed.TotalSeconds;

        // 타이머 상태 복원
        _remainingTimeInSeconds = newRemainingSeconds > 0 ? newRemainingSeconds : 0;
        PrevTimerState = timerData.PrevTimerState;
        _currentTimerState = timerData.CurrentTimerState;
        
        remainingCycleCount = timerData.RemainingCycleCount;
        lastCycleTime = timerData.LastCycleTime;
        
        focusMinute = timerData.FocusMinute;
        relaxMinute = timerData.RelaxMinute;

        // TodoItem 복원
        if (!string.IsNullOrEmpty(timerData.CurrentTodoItemId))
        {
            TodoItem linkedTodoItem = TodoManager.Instance.FindTodoItemById(timerData.CurrentTodoItemId);
            if (linkedTodoItem != null)
            {
                LinkTodoItem(linkedTodoItem);

                if (CurrentTimerState == TimerState.FocusSessionRunning)
                {
                    int progressToAdd = (int)Math.Min(elapsed.TotalMinutes, focusMinute);
                    currentTodoItem.AddProgress(DateTime.Today, progressToAdd);
                    DebugEx.Log($"진척도 추가: {progressToAdd}분");
                }
            }
        }

        // 복원 후의 타이머 상태는?
        DebugEx.Log($"<color='green'>Timer Data After Restoration: {GetCurrentTimerData()}</color>");
        
        // 타이머 팝업 띄우기
        PopupManager.Instance.ShowPopup<TimerPopup>();
        
        // 타이머가 아직 그 세션에서 벗어나지 않을 때
        if (RemainingTimeInSeconds > 0)
        {
            // 타이머 재개
            ResumeTimerWithRemainingTime(_remainingTimeInSeconds);
        }
        // 타이머가 그 세션에서 벗어날 때
        else
        {
            // 경우의 수를 나열해보자, 방금 끝난 세션 CurrentTimerState 이 뭐였을까?
            // *마지막 깍두기 세션은 있을수도, 없을 수도 있다. 
            //  (= lastCycleTime > 0, lastCycleTime == 0)
            
            if (CurrentTimerState == TimerState.FocusSessionRunning)        // 게임을 끌 때 집중 세션을 돌고있었더라면
            {
                if (remainingCycleCount == 1)                               // 마지막 사이클이었더라면
                {
                    CurrentTimerState = TimerState.PpomodoroEnd;                // 뽀모도로 엔드
                }
                else                                                        // 마지막 사이클이 아니었더라면
                {
                    CurrentTimerState = TimerState.ReadyForRelaxSession;        // 휴식 세션을 준비시킨다.
                }
            }        
            else if (CurrentTimerState == TimerState.RelaxSessionRunning)   // 게임을 끌 때 휴식 세션을 돌고있었더라면
            {
                remainingCycleCount--;                                  // 남은 사이클을 하나 차감하고
                
                if (remainingCycleCount == 1 && lastCycleTime > 0)           // 이번에 시작할 사이클이 깍두기라면
                {
                    focusMinute = lastCycleTime;                                    // 집중 시간을 깍두기로 바꾸고
                }
                
                CurrentTimerState = TimerState.ReadyForFocusSession;         // 집중 세션을 준비시킨다.
            }
            
            // OnTimeUpdated?.Invoke(_remainingTimeInSeconds);
            // OnSessionCompleted?.Invoke();
            
            //WhenSessionCompleted();
            
            DebugEx.Log("타이머가 복원되었으나 이미 만료되었습니다.");
        }
        
        // 저장된 타이머 상태 삭제
        // TODO : 나중엔 주석 지우기
        TimerRestorationService.DeleteStoredTimerState();
    }
    
    private void ResumeTimerWithRemainingTime(int newRemainingSeconds)
    {
        _remainingTimeInSeconds = newRemainingSeconds;
        CancellationTokenSource = new CancellationTokenSource();

        OnTimeUpdated?.Invoke(_remainingTimeInSeconds);

        // 타이머 재개
        UpdateTimer(CancellationTokenSource.Token).Forget();
        DebugEx.Log("타이머가 복원되어 재개되었습니다.");
    }
    
    /// <summary>
    /// 타이머의 세션이 끝났을 때.
    /// </summary>
    private void WhenSessionCompleted()
    {
        DebugEx.Log($"<color='red'> {nameof(WhenSessionCompleted)} </color>");        
        
        if (CurrentTimerState == TimerState.FocusSessionRunning)        // 집중 세션을 돌고있었더라면
        {
            if (remainingCycleCount == 1)                               // 마지막 사이클이었더라면
            {
                CurrentTimerState = TimerState.PpomodoroEnd;                // 뽀모도로 엔드
            }
            else                                                        // 마지막 사이클이 아니었더라면
            {
                CurrentTimerState = TimerState.RelaxSessionRunning;        // 휴식 세션을 시작시킨다.
            }
        }        
        else if (CurrentTimerState == TimerState.RelaxSessionRunning)   // 휴식 세션을 돌고있었더라면
        {
            remainingCycleCount--;                                  // 남은 사이클을 하나 차감하고
                
            if (remainingCycleCount == 1 && lastCycleTime > 0)           // 이번에 시작할 사이클이 깍두기라면
            {
                focusMinute = lastCycleTime;                                    // 집중 시간을 깍두기로 바꾸고
            }
                
            CurrentTimerState = TimerState.FocusSessionRunning;         // 집중 세션을 시작시킨다.
        }
        
        DebugEx.Log($"세션이 끝남. ");
        DebugEx.Log($"남은 사이클은 {remainingCycleCount} 사이클.");
        DebugEx.Log($"마지막 사이클은 {lastCycleTime}분 ");

        if(RemainingTimeInSeconds != 0)
            OnTimeUpdated?.Invoke(RemainingTimeInSeconds);
    }
    
    #endregion
    
    private void ScheduleNotification()
    {
        string title = string.Empty;
        if (CurrentTimerState == TimerState.FocusSessionRunning)
        {
            title = "집중 세션 종료";
        } 
        else if (CurrentTimerState == TimerState.RelaxSessionRunning)
        {
            title = "휴식 세션 종료";
        }
        
        string message = "설정하신 타이머가 완료되었습니다.";
        NotificationManager.Instance.ScheduleNotification(_remainingTimeInSeconds, title, message);
    }
}

public enum TimerState
{
    NeedToBeInitialized,        // 집중 세션과 휴식 세션을 정해야 하는 상태
    ReadyForFocusSession,       // 집중 세션을 준비하는 상태
    ReadyForRelaxSession,       // 휴식 세션을 준비하는 상태
    FocusSessionRunning,        // 집중 세션이 진행중인 상태 
    RelaxSessionRunning,        // 휴식 세션이 진행중인 상태 
    Paused,                     // 타이머 일시정지
    Stopped,                    // 타이머가 완전히 멈춘 상태 (세션이 전혀 없는 상태)
    PpomodoroEnd,                  // 전체 타이머가 끝난 상태
}