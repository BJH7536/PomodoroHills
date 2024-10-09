using System;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

// MVC 패턴 활용
// Model (TimerManager) : 타이머의 상태 및 로직 관리. 핵심 로직 처리
// View (TimerPopup, TimerWidget) : 타이머의 상태를 사용자에게 보여주는 UI 부분.
// Controller (UI) : 사용자의 입력을 받아 TimerManager에게 타이머를 시작, 일시정지, 재개, 취소 등의 명령을 내린다.
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

    #endregion

    #region Fields

    private int _remainingTimeInSeconds;        // 남은 시간 (초 단위)
    private bool _isTimerRunning;               // 타이머가 실행 중인지 여부
    
    private CancellationTokenSource _cancellationTokenSource;
    
    private const string TimerPrefsKey = "TimerData";
    private const string TimeStampKey = "TimeStamp";

    #endregion

    #region TimerManagement
    
    /// <summary>
    /// 타이머 시간 설정 및 시작
    /// </summary>
    /// <param name="durationInSeconds"></param>
    public void StartTimer(int durationInSeconds)
    {
        // 타이머가 실행 중인 경우, 중복 시작을 방지
        if (_isTimerRunning)
        {
            Debug.LogWarning("타이머가 이미 실행 중입니다!");
            return;
        }

        // 남은 시간 설정
        _remainingTimeInSeconds = durationInSeconds;
        // 타이머 실행 상태로 전환
        _isTimerRunning = true;

        _cancellationTokenSource = new CancellationTokenSource();
        UpdateTimer(_cancellationTokenSource.Token).Forget();
    }

    /// <summary>
    /// 타이머 일시정지
    /// </summary>
    public void PauseTimer()
    {
        if (_isTimerRunning)
        {
            _isTimerRunning = false;
            _cancellationTokenSource?.Cancel();
            SaveTimerState();
            NotificationManager.Instance.CancelAllNotifications();      // 알림 취소
            Debug.Log("타이머가 일시정지되었습니다.");
        }
    }

    /// <summary>
    /// 타이머 재개
    /// </summary>
    public void ResumeTimer()
    {
        if (!_isTimerRunning && _remainingTimeInSeconds > 0)
        {
            _isTimerRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            UpdateTimer(_cancellationTokenSource.Token).Forget();
            NotificationManager.Instance.ScheduleNotification(_remainingTimeInSeconds, "타이머 완료", "설정하신 타이머가 완료되었습니다."); // 알림 예약
            Debug.Log("타이머가 재개되었습니다.");
        }
    }

    /// <summary>
    /// 타이머 취소
    /// </summary>
    public void CancelTimer()
    {
        if (_isTimerRunning || _remainingTimeInSeconds > 0)
        {
            _isTimerRunning = false;
            _remainingTimeInSeconds = 0;
            _cancellationTokenSource?.Cancel();
            NotificationManager.Instance.CancelAllNotifications(); // 알림 취소
            
            OnTimeUpdated?.Invoke(_remainingTimeInSeconds);
            
            Debug.Log("타이머가 취소되었습니다.");
        }
    }

    /// <summary>
    /// 타이머 상태 갱신
    /// </summary>
    private async UniTaskVoid UpdateTimer(CancellationToken token)
    {
        while (_remainingTimeInSeconds > 0 && _isTimerRunning && !token.IsCancellationRequested)
        {
            OnTimeUpdated?.Invoke(_remainingTimeInSeconds);
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
            _remainingTimeInSeconds--;
        }

        if (_remainingTimeInSeconds <= 0)
        {
            _isTimerRunning = false;
            
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
        if (!_isTimerRunning) return;
        
        PlayerPrefs.SetInt(TimerPrefsKey, _remainingTimeInSeconds);
        PlayerPrefs.SetString(TimeStampKey, DateTime.Now.ToString(CultureInfo.CurrentCulture));
        PlayerPrefs.Save();
        Debug.Log("타이머 상태가 저장되었습니다.");
    }
    
    /// <summary>
    /// 타이머 상태 복원
    /// </summary>
    private void RestoreTimerState()
    {
        if (!PlayerPrefs.HasKey(TimerPrefsKey)) return;

        int savedTime = PlayerPrefs.GetInt(TimerPrefsKey);
        DateTime savedTimeStamp = DateTime.Parse(PlayerPrefs.GetString(TimeStampKey));
        TimeSpan elapsed = DateTime.Now - savedTimeStamp;

        int newRemainingTime = savedTime - (int)elapsed.TotalSeconds;

        // 기존 타이머 작업 취소 및 초기화
        if (_isTimerRunning)
        {
            _cancellationTokenSource?.Cancel();  // 기존 타이머 작업 취소
            _isTimerRunning = false;             // 타이머 상태 초기화
        }

        if (newRemainingTime > 0)
        {
            // 새로운 남은 시간으로 타이머 재개
            _remainingTimeInSeconds = newRemainingTime;
            _cancellationTokenSource = new CancellationTokenSource();  // 새 CancellationTokenSource 생성
            _isTimerRunning = true;
        
            // UI 즉시 업데이트
            OnTimeUpdated?.Invoke(_remainingTimeInSeconds);

            // 타이머 재개
            UpdateTimer(_cancellationTokenSource.Token).Forget();
            Debug.Log("타이머가 복원되어 재개되었습니다.");
        }
        else
        {
            // 남은 시간이 없으면 타이머 완료 처리
            _remainingTimeInSeconds = 0;
            _isTimerRunning = false;
            OnTimeUpdated?.Invoke(_remainingTimeInSeconds);  // UI 즉시 업데이트
            OnTimerCompleted?.Invoke();  // 타이머 완료 처리
            Debug.Log("타이머가 복원되었으나 이미 만료되었습니다.");
        }
    }

    #endregion
    
    #region Application Lifecycle Methods

    public void Start()
    {
        OnTimerCompleted.AddListener(()=> Debug.Log("타이머가 완료되었습니다."));
    }

    /// <summary>
    /// 게임 오브젝트가 활성화될 때 호출됩니다.
    /// 싱글톤 인스턴스를 설정하고, 데이터를 로드합니다.
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
        Debug.Log($"앱 일시정지됨 {isPaused}");

        // 게임이 일시정지되면
        if (isPaused)
        {
            // 타이머가 동작중인 경우에는 
            if (_isTimerRunning)
            {
                // 남은 시간과 타임스탬프를 저장
                SaveTimerState();  
                // 알림 예약
                NotificationManager.Instance.ScheduleNotification(_remainingTimeInSeconds, "타이머 완료", "설정하신 타이머가 완료되었습니다.");
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
        // 게임이 종료될 때 타이머가 동작 중이면 알림 예약
        if (_isTimerRunning)
        {
            SaveTimerState();  // 타이머 상태와 함께 현재 시각 저장
            NotificationManager.Instance.ScheduleNotification(_remainingTimeInSeconds, "타이머 완료", "설정하신 타이머가 완료되었습니다.");
        }
    }
    
    #endregion
}