using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// SRP을 만족하기 위한 TimerRestorationService.
/// TimerManager에서 주로 사용된다.
/// </summary>
public static class TimerRestorationService
{
    #region PlayerPrefs keys

    private static readonly string TimeStampKey = "TimeStamp";
    private static readonly string TimerRemainsKey = "TimeRemains";
    private static readonly string TimerStateKey = "TimerState";
    private static readonly string PrevTimerStateKey = "PrevTimerState";
    
    private static readonly string RemainingCycleKey = "RemainingCycleCount";
    private static readonly string LastCycleKey = "LastCycleTime";

    private static readonly string CurrentTodoItemId = "CurrentTodoItemId";
    private static readonly string FocusMinuteKey = "FocusMinute";
    private static readonly string RelaxMinuteKey = "RelaxMinute";
    
    #endregion
    
    /// <summary>
    /// <para>
    /// 타이머의 현 상태를 저장하는 기능. TimerManager의 필드들을 활용해 타이머 데이터를 PlayerPrefs로 저장한다.
    /// </para>
    /// <para>
    /// 저장하는 정보 :
    /// 현재 타이머의 남은 시간 (초단위) / 현재 세션의 종류 / 저장이 이루어지는 시각 / 집중 시간 (분) / 휴식 시간 (분)
    /// / TodoItem의 id
    /// </para>
    /// </summary>
    public static void SaveTimerState(TimerData timerData)
    {
        PlayerPrefs.SetString(TimeStampKey, DateTime.Now.ToString(CultureInfo.CurrentCulture));         // 현재 시각을 저장
        PlayerPrefs.SetInt(TimerRemainsKey, timerData.RemainingTimeInSeconds);                          // 타이머의 남은 시간을 저장
        PlayerPrefs.SetInt(TimerStateKey, (int) timerData.CurrentTimerState);                           // 타이머의 상태를 저장
        PlayerPrefs.SetInt(PrevTimerStateKey, (int) timerData.PrevTimerState);                          // 타이머의 이전 상태를 저장
        
        PlayerPrefs.SetInt(FocusMinuteKey, timerData.FocusMinute);
        PlayerPrefs.SetInt(RelaxMinuteKey, timerData.RelaxMinute);
        
        PlayerPrefs.SetInt(RemainingCycleKey, timerData.RemainingCycleCount);
        PlayerPrefs.SetInt(LastCycleKey, timerData.LastCycleTime);
        
        if (TimerManager.Instance.TodoItemInitialized)
        {
            PlayerPrefs.SetString(CurrentTodoItemId, TimerManager.Instance.CurrentTodoItem.Id);         // TodoItem의 id를 저장
        }
        
        PlayerPrefs.Save();
        DebugEx.Log("타이머 상태가 저장되었습니다.");
    }
    
    /// <summary>
    /// 타이머의 현 상태를 복원하는 기능. 
    /// </summary>
    public static TimerData RestoreTimerState()
    {
        if (!PlayerPrefs.HasKey(TimerRemainsKey)) return null;

        TimerData timerData = new TimerData();

        // TimeStamp 복원
        if (PlayerPrefs.HasKey(TimeStampKey))
        {
            string timeStampStr = PlayerPrefs.GetString(TimeStampKey);
            if (DateTime.TryParse(timeStampStr, out DateTime timeStamp))
            {
                timerData.TimeStamp = timeStamp;
            }
        }

        // RemainingTimeInSeconds 복원
        timerData.RemainingTimeInSeconds = PlayerPrefs.GetInt(TimerRemainsKey);

        // 타이머 상태 복원
        timerData.CurrentTimerState = (TimerState) PlayerPrefs.GetInt(TimerStateKey);
        timerData.PrevTimerState = (TimerState) PlayerPrefs.GetInt(PrevTimerStateKey);
        
        // 사이클 정보 복원
        if (PlayerPrefs.HasKey(RemainingCycleKey))
            timerData.RemainingCycleCount = PlayerPrefs.GetInt(RemainingCycleKey);

        if (PlayerPrefs.HasKey(LastCycleKey))
            timerData.LastCycleTime = PlayerPrefs.GetInt(LastCycleKey);
        
        // FocusMinute, RelaxMinute 복원
        if (PlayerPrefs.HasKey(FocusMinuteKey))
            timerData.FocusMinute = PlayerPrefs.GetInt(FocusMinuteKey);

        if (PlayerPrefs.HasKey(RelaxMinuteKey))
            timerData.RelaxMinute = PlayerPrefs.GetInt(RelaxMinuteKey);
        
        // CurrentTodoItemId 복원
        if (PlayerPrefs.HasKey(CurrentTodoItemId))
            timerData.CurrentTodoItemId = PlayerPrefs.GetString(CurrentTodoItemId);

        return timerData;
    }
    
    public static void DeleteStoredTimerState()
    {
        PlayerPrefs.DeleteKey(TimerRemainsKey);
        PlayerPrefs.DeleteKey(TimeStampKey);
        PlayerPrefs.DeleteKey(TimerStateKey);
        PlayerPrefs.DeleteKey(FocusMinuteKey);
        PlayerPrefs.DeleteKey(RelaxMinuteKey);
        PlayerPrefs.DeleteKey(CurrentTodoItemId);
    }
}

[Serializable]
public class TimerData
{
    public DateTime TimeStamp;
    public int RemainingTimeInSeconds;
    public TimerState CurrentTimerState;
    public TimerState PrevTimerState;

    public int RemainingCycleCount;
    public int LastCycleTime;
    
    public int FocusMinute;
    public int RelaxMinute;

    public string CurrentTodoItemId;
    
    public override string ToString()
    {
        return $"TimerData: \n" +
               $"[TimeStamp: {TimeStamp}, RemainingTimeInSeconds: {RemainingTimeInSeconds}, \n" +
               $" CurrentTimerState {CurrentTimerState}, PrevTimerState {PrevTimerState}\n" +
               $" RemainingCycleCount: {RemainingCycleCount}, LastCycleTime : {LastCycleTime},\n" +
               $" FocusMinute: {FocusMinute}, RelaxMinute: {RelaxMinute}, \n" +
               $" CurrentTodoItemId: {CurrentTodoItemId ?? "N/A"}] \n";
    }
}
