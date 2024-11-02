using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class UIUtillity
{
    /// <summary>
    /// 버튼의 모든 기능을 제거하고 하나의 액션만을 바인딩
    /// </summary>
    /// <param name="buttonClickedEvent"></param>
    /// <param name="call"></param>
    public static void RemoveAllAndAddListener(this Button.ButtonClickedEvent buttonClickedEvent, UnityAction call)
    {
        buttonClickedEvent.RemoveAllListeners();
        buttonClickedEvent.AddListener(call);
    }
    
    public static void RemoveAllAndAddListeners(this Button.ButtonClickedEvent buttonClickedEvent, params UnityAction[] calls)
    {
        buttonClickedEvent.RemoveAllListeners();
        foreach (var call in calls)
            if (call != null)
                buttonClickedEvent.AddListener(call);
    }
    
    public static void RemoveAllAndAddListener(this ScrollRect.ScrollRectEvent scrollRectEvent, UnityAction<Vector2> call)
    {
        scrollRectEvent.RemoveAllListeners();
        scrollRectEvent.AddListener(call);
    }
}


public static class DebugEx  
{  
    public enum LogLevel  
    {  
        None,  
        Error,  
        Warning,  
        All  
    }  
  
    public static LogLevel CurrentLogLevel = LogLevel.All;  
  
    public static void Log(object message, Object context = null)  
    {
#if UNITY_EDITOR  
        if (CurrentLogLevel >= LogLevel.All)  
        {
            Debug.Log(message, context);  
        }
#endif  
    }  
  
    public static void LogWarning(object message, Object context = null)  
    {
#if UNITY_EDITOR  
        if (CurrentLogLevel >= LogLevel.Warning)  
        {
            Debug.LogWarning(message, context);  
        }
#endif  
    }  
  
    public static void LogError(object message, Object context = null)  
    {
#if UNITY_EDITOR 
        if (CurrentLogLevel >= LogLevel.Error)  
        {
            Debug.LogError(message, context);  
        }
#endif 
    }  
}