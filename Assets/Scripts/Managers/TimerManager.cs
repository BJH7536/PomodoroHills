using System;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using TodoSystem;
using UnityEngine;
using UnityEngine.Events;

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
    public event Action<int> OnTimeUpdated;
    
    // 타이머가 완료되었을 때 호출되는 이벤트, 유니티 엔진에서 쉽게 연결할 수 있도록 UnityEvent로.
    [SerializeField] public UnityEvent OnTimerCompleted;
    
    // 상태가 변할 때 호출되는 이벤트.
    public event Action<TimerState> OnTimerStateChanged;
    
    #endregion

    #region Fields

    private CancellationTokenSource _cancellationTokenSource;
    
    public SessionType CurrentSessionType { get; private set; }
    
    private TimerState _currentTimerState = TimerState.Stopped;
    public TimerState CurrentTimerState
    {
        get => _currentTimerState;
        private set
        {
            if (_currentTimerState != value)
            {
                _currentTimerState = value;
                OnTimerStateChanged?.Invoke(_currentTimerState);  // 상태 변경 시 이벤트 호출
            }
        }
    }
    
    private TodoItem currentTodoItem = null;           // 타이머에 연결된 OnTodoItemLink
    public TodoItem CurrentTodoItem => currentTodoItem;
    
    private int _remainingTimeInSeconds;        // 남은 시간 (초 단위)
    public int RemainingTimeInSeconds => _remainingTimeInSeconds;
    
    public int focusMinute = 25;
    public int relaxMinute = 5;
    
    public int remainingCycleCount;             // 남은 사이클 수
    public int lastCycleTime;                   // 마지막 사이클 시간
    
    private const string TimerRemainsKey = "TimeRemains";
    private const string TimeStampKey = "TimeStamp";
    private const string TimerSessionKey = "CurrentSessionType";
    
    private const string CurrentTodoItemId = "CurrentTodoItemId";
    private const string FocusMinuteKey = "FocusMinute";
    private const string RelaxMinuteKey = "RelaxMinute";
    
    private const string RemainingCycleCountKey = "RemainingCycleCount";
    private const string LastCycleTimeKey = "LastCycleTime";
    
    #endregion

    public void PrepareNextSession(PanelCircularTimer panelCircularTimer)
    {
        // 다음 세션이 휴식 세션이면 UI를 휴식 상태로 준비하고, 아니면 집중 상태로 준비한다.
        if (CurrentSessionType == SessionType.Focus)
        {
            panelCircularTimer.PrepareRelaxSession(relaxMinute);
        }
        else if (CurrentSessionType == SessionType.Relax)
        {
            panelCircularTimer.PrepareFocusSession(focusMinute);
        }
    }
    
    #region TimerManagement

    /// <summary>
    /// TodoItem을 타이머와 연동
    /// </summary>
    /// <param name="todoItem">연동할 OnTodoItem</param>
    public void LinkTodoItem(TodoItem todoItem)
    {
        currentTodoItem = todoItem;
        
        DebugEx.Log($"{nameof(TimerManager)} : TodoItem '{todoItem.Name}'이(가) 타이머에 연동되었습니다.");
    }

    /// <summary>
    /// OnTodoItem 연동을 해제하는 기능
    /// </summary>
    public void UnlinkTodoItem()
    {
        currentTodoItem = null;
        
        DebugEx.Log($"{nameof(TimerManager)} : TodoItem 연동이 해제되었습니다.");
    }
    
    /// <summary>
    /// 타이머 시간 설정 및 시작
    /// </summary>
    /// <param name="durationInSeconds"></param>
    /// <param name="sessionType">
    /// </param>
    public void StartTimer(int durationInSeconds, SessionType sessionType)
    {
        // 타이머가 실행 중인 경우, 중복 시작을 방지
        if (CurrentTimerState == TimerState.Running)
        {
            DebugEx.LogWarning("타이머가 이미 실행 중입니다!");
            return;
        }

        // 남은 시간 설정
        _remainingTimeInSeconds = durationInSeconds;
        // 타이머 실행 상태로 전환
        CurrentTimerState = TimerState.Running;
        // 현재 세션의 종류 (집중, 휴식) 지정
        CurrentSessionType = sessionType;
        
        _cancellationTokenSource = new CancellationTokenSource();
        UpdateTimer(_cancellationTokenSource.Token).Forget();
    }

    /// <summary>
    /// 타이머 일시정지
    /// </summary>
    public void PauseTimer()
    {
        if (CurrentTimerState == TimerState.Running)
        {
            CurrentTimerState = TimerState.Paused;
            _cancellationTokenSource?.Cancel();
            SaveTimerState();
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
            CurrentTimerState = TimerState.Running;
            _cancellationTokenSource = new CancellationTokenSource();
            UpdateTimer(_cancellationTokenSource.Token).Forget();
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
            _cancellationTokenSource?.Cancel();
            NotificationManager.Instance.CancelAllNotifications(); // 알림 취소
            
            OnTimeUpdated?.Invoke(_remainingTimeInSeconds);
            
            DebugEx.Log("타이머가 취소되었습니다.");
        }
    }

    /// <summary>
    /// 타이머 상태 갱신
    /// </summary>
    private async UniTaskVoid UpdateTimer(CancellationToken token)
    {
        while (_remainingTimeInSeconds > 0 && CurrentTimerState == TimerState.Running && !token.IsCancellationRequested)
        {
            OnTimeUpdated?.Invoke(_remainingTimeInSeconds);
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
            _remainingTimeInSeconds--;
            
            // 매 60초마다 TodoItem에 진행 상황을 기록
            if (_remainingTimeInSeconds % 60 == 0 && currentTodoItem != null && CurrentSessionType == SessionType.Focus)
            {
                currentTodoItem.AddProgress(DateTime.Today, 1); // 1분 단위로 기록
            }
        }

        if (_remainingTimeInSeconds <= 0)
        {
            CurrentTimerState = TimerState.Completed;
            
            OnTimeUpdated?.Invoke(_remainingTimeInSeconds);
            OnTimerCompleted?.Invoke();
            
            // 타이머 완료 시 예약된 알림 취소
            NotificationManager.Instance.CancelAllNotifications();
        }
    }
    
    #endregion

    #region Save & Restore Timer

    /// <summary>
    /// 타이머 상태 저장
    /// </summary>
    private void SaveTimerState()
    {
        if (CurrentTimerState != TimerState.Running) return;
        
        PlayerPrefs.SetInt(TimerRemainsKey, _remainingTimeInSeconds);                               // 타이머의 남은 시간을 저장
        PlayerPrefs.SetString(TimerSessionKey, CurrentSessionType.ToString());                      // 타이머의 세션 정보를 저장
        PlayerPrefs.SetString(TimeStampKey, DateTime.Now.ToString(CultureInfo.CurrentCulture));     // 현재 시각을 저장
        PlayerPrefs.SetInt(FocusMinuteKey, focusMinute);
        PlayerPrefs.SetInt(RelaxMinuteKey, relaxMinute);
        PlayerPrefs.SetInt(RemainingCycleCountKey, remainingCycleCount);                            // 남은 사이클 수 저장
        PlayerPrefs.SetInt(LastCycleTimeKey, lastCycleTime);   
        
        if (currentTodoItem != null)
        {
            PlayerPrefs.SetString(CurrentTodoItemId, currentTodoItem.Id);               // TodoItem의 id를 저장
        }
        
        PlayerPrefs.Save();
        DebugEx.Log("타이머 상태가 저장되었습니다.");
    }
    
    /// <summary>
    /// 타이머 상태 수복
    /// </summary>
    // private void RestoreTimerState()
    // {
    //     if (!PlayerPrefs.HasKey(TimerRemainsKey)) return;
    //
    //     // AlertPopup이 필요한가? 타이머가 수복된 후 남은 시간이 없다면 필요하다.
    //     bool needAlert = false;
    //     
    //     // 복원 절차 수행
    //     int savedTime = PlayerPrefs.GetInt(TimerRemainsKey);
    //     DebugEx.Log($"savedTime : {savedTime}");
    //     
    //     DateTime savedTimeStamp = DateTime.Parse(PlayerPrefs.GetString(TimeStampKey));
    //     TimeSpan elapsed = DateTime.Now - savedTimeStamp;
    //     DebugEx.Log($"elapsed : {elapsed}");
    //     
    //     int newRemainingSeconds = savedTime - (int)elapsed.TotalSeconds;       // 복원 결과로 도출된 남은 시간 (초단위)
    //     DebugEx.Log($"newRemainingSeconds : {newRemainingSeconds}");
    //     
    //     // 기존 타이머 작업 취소 및 초기화
    //     if (CurrentTimerState == TimerState.Running)
    //     {
    //         _cancellationTokenSource?.Cancel();                 // 기존 타이머 작업 취소
    //         CurrentTimerState = TimerState.Paused;              // 타이머 상태 초기화
    //     }
    //     
    //     // 세션 타입 복원
    //     if (PlayerPrefs.HasKey(TimerSessionKey))
    //     {
    //         string sessionTypeStr = PlayerPrefs.GetString(TimerSessionKey);
    //         if (Enum.TryParse(sessionTypeStr, out SessionType sessionType))
    //         {
    //             CurrentSessionType = sessionType;
    //             DebugEx.Log($"복원된 세션 타입: {CurrentSessionType}");
    //         }
    //         else
    //         {
    //             DebugEx.LogWarning("세션 타입을 복원하는 데 실패했습니다.");
    //         }
    //     }
    //
    //     // focusMinute relaxMinute 복원
    //     if (PlayerPrefs.HasKey(FocusMinuteKey) && PlayerPrefs.HasKey(RelaxMinuteKey))
    //     {
    //         focusMinute = PlayerPrefs.GetInt(FocusMinuteKey);
    //         relaxMinute = PlayerPrefs.GetInt(RelaxMinuteKey);
    //         DebugEx.Log($"복원된 집중 시간: {focusMinute}, 휴식 시간: {relaxMinute}");
    //     }
    //     
    //     // 남은 사이클 수와 마지막 사이클 시간 복원
    //     if (PlayerPrefs.HasKey(RemainingCycleCountKey) && PlayerPrefs.HasKey(LastCycleTimeKey))
    //     {
    //         remainingCycleCount = PlayerPrefs.GetInt(RemainingCycleCountKey);
    //         lastCycleTime = PlayerPrefs.GetInt(LastCycleTimeKey);
    //         DebugEx.Log($"복원된 남은 사이클 수: {remainingCycleCount}, 마지막 사이클 시간: {lastCycleTime}");
    //     }
    //     
    //     // 연동된 OnTodoItem 복원
    //     if (PlayerPrefs.HasKey(CurrentTodoItemId))
    //     {
    //         string currentTodoItemId = PlayerPrefs.GetString(CurrentTodoItemId);
    //         TodoItem linkedTodoItem = TodoManager.Instance.FindTodoItemById(currentTodoItemId);
    //         if (linkedTodoItem != null)
    //         {
    //             LinkTodoItem(linkedTodoItem);
    //             
    //             // 진척도 갱신 - 현재 세션이 집중 세션일 경우에만
    //             if (CurrentSessionType == SessionType.Focus)
    //             {
    //                 int progressToAdd = (savedTime >= (int)elapsed.TotalSeconds) ? savedTime - newRemainingSeconds : (int)Math.Floor(elapsed.TotalMinutes);
    //                 currentTodoItem.AddProgress(DateTime.Today, progressToAdd);
    //                 DebugEx.Log($"진척도 {progressToAdd}분 추가됨");
    //             }
    //         }
    //     }
    //     
    //     // 복원했는데 남은 시간이 있을 때 
    //     if (newRemainingSeconds > 0)
    //     {
    //         // 새로운 남은 시간으로 타이머 재개
    //         _remainingTimeInSeconds = newRemainingSeconds;
    //         _cancellationTokenSource = new CancellationTokenSource();  // 새 CancellationTokenSource 생성
    //         CurrentTimerState = TimerState.Running;
    //     
    //         // UI 즉시 업데이트
    //         OnTimeUpdated?.Invoke(_remainingTimeInSeconds);
    //         
    //         // 타이머 재개
    //         UpdateTimer(_cancellationTokenSource.Token).Forget();
    //         DebugEx.Log("타이머가 복원되어 재개되었습니다.");
    //     }
    //     // 복원했는데 남은 시간이 없을 때
    //     else
    //     {
    //         // 남은 시간이 없으면 타이머 완료 처리
    //         needAlert = true;
    //         
    //         _remainingTimeInSeconds = 0;
    //         
    //         // TODO : 남은 시간이 없으면 이 항목이 지금 하나의 세션만 지나친건지, 아니면 마지막 세션을 지나친건지 구분해야 한다.
    //         // 하나의 세션만 지나친거면 Completed가 되는게 맞고
    //         // 마지막 세션을 지나친거면
    //         
    //         CurrentTimerState = TimerState.Completed;
    //         OnTimeUpdated?.Invoke(_remainingTimeInSeconds);     // UI 즉시 업데이트
    //         OnTimerCompleted?.Invoke();                         // 타이머 완료 처리
    //         DebugEx.Log("타이머가 복원되었으나 이미 만료되었습니다.");
    //     }
    //     
    //     // 복원 후 조치
    //     
    //     
    //     // 남은 시간이 있든없든 어쨋든 TimerPopup은 열어야하고
    //     PanelCircularTimer panelCircularTimer = PopupManager.Instance.ShowPopup<TimerPopup>().panelCircularTimer;
    //     
    //     // 세션이 다 지났으면 AlertPopup띄워주고
    //     // AlertPopup도 한번만 띄우게 
    //     if (needAlert)
    //     {
    //         string message = CurrentSessionType switch
    //         {
    //             SessionType.Focus => "집중 세션 종료",
    //             SessionType.Relax => "휴식 세션 종료",
    //             _ => ""
    //         };
    //
    //         PopupManager.Instance.ShowAlertPopup("세션 종료", message);
    //         
    //         // TODO : 이 다음 세션 장전
    //         // TODO : playButton 하나 띄워놓고, 누르면 다음 세션이 시작하도록
    //         
    //         // 이 다음 세션 장전 및 플레이 버튼 준비
    //         PrepareNextSession(panelCircularTimer);
    //     }
    //     // 세션이 다 안지났으면 플레이버튼은 숨기고 일시정지랑 취소 버튼 띄워주고
    //     else
    //     {
    //         panelCircularTimer.HidePlayButton();
    //         panelCircularTimer.ShowPauseAndCancelButton();
    //     }
    //     
    //     // 저장한 타이머 상태 데이터 삭제 (AlertPopup도 한번만 띄우게)
    //     PlayerPrefs.DeleteKey(TimerRemainsKey);
    // }
    
    private void RestoreTimerState()
    {
        if (!PlayerPrefs.HasKey(TimerRemainsKey)) return;

        RestoreSavedValues();
        var elapsed = CalculateElapsedTime();
        int newRemainingSeconds = CalculateNewRemainingTime(elapsed);

        HandlePreviousRunningState();
        RestoreTodoItemProgress(elapsed);

        if (newRemainingSeconds > 0)
        {
            ResumeTimerWithRemainingTime(newRemainingSeconds);
        }
        else
        {
            // TODO : 남은 시간이 없으면 이 항목이 지금 하나의 세션만 지나친건지, 아니면 마지막 세션을 지나친건지 구분해야 한다.
            // TODO : 마지막 세션을 지나친건가?
            DebugEx.Log($"{remainingCycleCount}");
            
            CompleteTimer();
        }
        
        ShowTimerPopup(newRemainingSeconds <= 0);
        DeleteStoredTimerState();
    }

    private void RestoreSavedValues()
    {
        // 세션 타입 복원
        if (PlayerPrefs.HasKey(TimerSessionKey))
        {
            string sessionTypeStr = PlayerPrefs.GetString(TimerSessionKey);
            if (Enum.TryParse(sessionTypeStr, out SessionType sessionType))
            {
                CurrentSessionType = sessionType;
                DebugEx.Log($"복원된 세션 타입: {CurrentSessionType}");
            }
            else
            {
                DebugEx.LogWarning("세션 타입을 복원하는 데 실패했습니다.");
            }
        }

        // focusMinute relaxMinute 복원
        if (PlayerPrefs.HasKey(FocusMinuteKey) && PlayerPrefs.HasKey(RelaxMinuteKey))
        {
            focusMinute = PlayerPrefs.GetInt(FocusMinuteKey);
            relaxMinute = PlayerPrefs.GetInt(RelaxMinuteKey);
            DebugEx.Log($"복원된 집중 시간: {focusMinute}, 휴식 시간: {relaxMinute}");
        }

        // 남은 사이클 수와 마지막 사이클 시간 복원
        if (PlayerPrefs.HasKey(RemainingCycleCountKey) && PlayerPrefs.HasKey(LastCycleTimeKey))
        {
            remainingCycleCount = PlayerPrefs.GetInt(RemainingCycleCountKey);
            lastCycleTime = PlayerPrefs.GetInt(LastCycleTimeKey);
            DebugEx.Log($"복원된 남은 사이클 수: {remainingCycleCount}, 마지막 사이클 시간: {lastCycleTime}");
        }
    }

    private TimeSpan CalculateElapsedTime()
    {
        DateTime savedTimeStamp = DateTime.Parse(PlayerPrefs.GetString(TimeStampKey));
        TimeSpan elapsed = DateTime.Now - savedTimeStamp;
        DebugEx.Log($"elapsed : {elapsed}");
        return elapsed;
    }

    private int CalculateNewRemainingTime(TimeSpan elapsed)
    {
        int savedTime = PlayerPrefs.GetInt(TimerRemainsKey);
        DebugEx.Log($"savedTime : {savedTime}");
        int newRemainingSeconds = savedTime - (int)elapsed.TotalSeconds;
        DebugEx.Log($"newRemainingSeconds : {newRemainingSeconds}");
        return newRemainingSeconds;
    }

    private void HandlePreviousRunningState()
    {
        // 기존 타이머 작업 취소 및 초기화
        if (CurrentTimerState == TimerState.Running)
        {
            _cancellationTokenSource?.Cancel(); // 기존 타이머 작업 취소
            CurrentTimerState = TimerState.Paused; // 타이머 상태 초기화
        }
    }

    private void RestoreTodoItemProgress(TimeSpan elapsed)
    {
        if (!PlayerPrefs.HasKey(CurrentTodoItemId)) return;

        string currentTodoItemId = PlayerPrefs.GetString(CurrentTodoItemId);
        TodoItem linkedTodoItem = TodoManager.Instance.FindTodoItemById(currentTodoItemId);
        if (linkedTodoItem != null)
        {
            LinkTodoItem(linkedTodoItem);

            if (CurrentSessionType == SessionType.Focus)
            {
                int progressToAdd = (int)Math.Min(elapsed.TotalMinutes, focusMinute);
                currentTodoItem.AddProgress(DateTime.Today, progressToAdd);
                DebugEx.Log($"진척도 추가: {progressToAdd}분");
            }
        }
    }

    private void ResumeTimerWithRemainingTime(int newRemainingSeconds)
    {
        _remainingTimeInSeconds = newRemainingSeconds;
        _cancellationTokenSource = new CancellationTokenSource(); // 새 CancellationTokenSource 생성
        CurrentTimerState = TimerState.Running;

        // UI 즉시 업데이트
        OnTimeUpdated?.Invoke(_remainingTimeInSeconds);

        // 타이머 재개
        UpdateTimer(_cancellationTokenSource.Token).Forget();
        DebugEx.Log("타이머가 복원되어 재개되었습니다.");
    }

    private void CompleteTimer()
    {
        _remainingTimeInSeconds = 0;
        CurrentTimerState = TimerState.Completed;
        OnTimeUpdated?.Invoke(_remainingTimeInSeconds); // UI 즉시 업데이트
        OnTimerCompleted?.Invoke(); // 타이머 완료 처리
        DebugEx.Log("타이머가 복원되었으나 이미 만료되었습니다.");
    }

    private void ShowTimerPopup(bool isTimerComplete)
    {
        PanelCircularTimer panelCircularTimer = PopupManager.Instance.ShowPopup<TimerPopup>().panelCircularTimer;

        if (isTimerComplete)
        {
            string message = CurrentSessionType switch
            {
                SessionType.Focus => "집중 세션 종료",
                SessionType.Relax => "휴식 세션 종료",
                _ => ""
            };

            PopupManager.Instance.ShowAlertPopup("세션 종료", message);
            PrepareNextSession(panelCircularTimer);
        }
        else
        {
            panelCircularTimer.HidePlayButton();
            panelCircularTimer.ShowPauseAndCancelButton();
        }
    }

    private void DeleteStoredTimerState()
    {
        PlayerPrefs.DeleteKey(TimerRemainsKey);
    }

    
    #endregion
    
    #region Application Lifecycle Methods

    public void Start()
    {
        OnTimerCompleted.AddListener(()=> DebugEx.Log("타이머가 완료되었습니다."));
        //OnTimerCompleted.AddListener(UnlinkTodoItem);
    }

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
    }
    
    public void OnApplicationPause(bool isPaused)
    {
        DebugEx.Log($"앱 일시정지됨 {isPaused}");

        // 게임이 일시정지되면
        if (isPaused)
        {
            // 타이머가 동작중인 경우에는 
            if (CurrentTimerState == TimerState.Running)
            {
                // 남은 시간과 타임스탬프를 저장
                SaveTimerState();  
                // 알림 예약
                string title = "", message = "";
                
                switch (CurrentSessionType)
                {
                    case SessionType.Focus:
                        title = "집중 세션 종료";
                        message = "집중 세션이 종료되었습니다.";
                        break;
                    case SessionType.Relax:
                        title = "휴식 세션 종료";
                        message = "휴식 세션이 종료되었습니다.";
                        break;
                }
                
                NotificationManager.Instance.ScheduleNotification(_remainingTimeInSeconds, title, message);
            }
        }
        // 게임이 재개되면 (켜지는 경우 포함)
        else
        {
            // 알림 취소
            NotificationManager.Instance.CancelAllNotifications();
            // 타이머 복구
            RestoreTimerState();
        }
    }
    
    public void OnApplicationQuit()
    {
        // 게임이 종료될 때 타이머가 동작 중이면
        // 타이머 수복을 위한 상태 저장과 함께 알림을 예약
        if (CurrentTimerState == TimerState.Running)
        {
            SaveTimerState();  // 타이머 상태와 함께 현재 시각 저장
            
            // 알림 예약
            string title = "", message = "";
                
            switch (CurrentSessionType)
            {
                case SessionType.Focus:
                    title = "집중 세션 종료";
                    message = "집중 세션이 종료되었습니다.";
                    break;
                case SessionType.Relax:
                    title = "휴식 세션 종료";
                    message = "휴식 세션이 종료되었습니다.";
                    break;
            }
            
            NotificationManager.Instance.ScheduleNotification(_remainingTimeInSeconds, title, message);
        }
    }
    
    #endregion
}

public enum SessionType
{
    Focus,
    Relax
}

public enum TimerState
{
    Stopped,        // 타이머가 완전히 멈춘 상태 (세션이 전혀 없는 상태)
    Running,        // 타이머가 동작하는 상태 (세션이 진행 중)
    Paused,         // 타이머 일시정지
    Initializing,   // 타이머가 초기화 중인 상태 (세션 준비, UI 초기화)
    Completed       // 세션이 완료된 상태
}