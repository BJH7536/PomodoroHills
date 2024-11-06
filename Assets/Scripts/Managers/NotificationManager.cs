using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Notifications;
using UnityEngine;
using Unity.Notifications.Android;
using UnityEngine.Android;

[DefaultExecutionOrder(-1)]
public class NotificationManager : MonoBehaviour
{
    [SerializeField] Color notificationColor;
    
    #region Singleton

    private static NotificationManager instance;
    public static NotificationManager Instance => instance;

    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeNotifications();
    }

    /// <summary>
    /// 알림 시스템 초기화
    /// </summary>
    private void InitializeNotifications()
    {
        #if UNITY_ANDROID
            InitializeAndroidNotifications();
        #elif UNITY_IOS
            InitializeiOSNotifications();
        #endif
    }

    
    #region Notification Scheduling

    /// <summary>
    /// 알림 예약
    /// </summary>
    public void ScheduleNotification(int remainingTimeInSeconds, string title, string message)
    {
        #if UNITY_ANDROID
            ScheduleAndroidNotification(remainingTimeInSeconds, title, message);
        #elif UNITY_IOS
            ScheduleiOSNotification(remainingTimeInSeconds, title, message);
        #endif
    }

    /// <summary>
    /// 알림 취소
    /// </summary>
    public void CancelAllNotifications()
    {
        #if UNITY_ANDROID
            CancelAllAndroidNotifications();
        #elif UNITY_IOS
            CancelAlliOSNotifications();
        #endif
    }

    #endregion

    #region Android Notification 

    private const string AndroidChannelId = "timer_channel";
    private int? _androidVersion;
    
    /// <summary>
    /// Android 알림 권한 런타임 확보 및 채널 설정
    /// </summary>
    private async void InitializeAndroidNotifications()
    {
        // 안드로이드 Notification 권한 요청
        await RequestNotificationPermissionAsync();
        
        var channel = new AndroidNotificationChannel()
        {
            Id = AndroidChannelId,
            Name = "Timer Notifications",
            Importance = Importance.High,       // Importance.High : heads-up notification
            Description = "알림 타이머 채널",
        };
        
        // 알림 채널 등록
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
        DebugEx.Log("[Android] 알림 채널이 초기화되었습니다.");
    }
    
    /// <summary>
    /// Android Notification 권한 요청
    /// </summary>
    private async UniTask RequestNotificationPermissionAsync()
    {
        string notificationPermission = "android.permission.POST_NOTIFICATIONS";

        // Android 13(API 33) 이상에서만 권한 요청 필요
        if (Application.platform == RuntimePlatform.Android && AndroidVersionIsAtLeast33())
        {
            // POST_NOTIFICATIONS 권한이 없으면 요청
            if (!Permission.HasUserAuthorizedPermission(notificationPermission))
            {
                var tcs = new UniTaskCompletionSource<bool>();
                var callbacks = new PermissionCallbacks();

                // 권한 승인 시 호출되는 콜백
                callbacks.PermissionGranted += permission =>
                {
                    HandlePermissionGranted(permission);
                    tcs.TrySetResult(true);
                };

                // 권한 거부 시 호출되는 콜백
                callbacks.PermissionDenied += permission =>
                {
                    HandlePermissionDenied(permission);
                    tcs.TrySetResult(false);
                };

                // 권한이 거부되고 "다시 묻지 않기"를 선택했을 때 호출되는 콜백
                callbacks.PermissionDeniedAndDontAskAgain += permission =>
                {
                    HandlePermissionDeniedPermanently(permission);
                    tcs.TrySetResult(false);
                };

                // 권한 요청
                Permission.RequestUserPermission(notificationPermission, callbacks);

                // 권한 요청 결과를 비동기로 대기
                await tcs.Task;
            }
            else
            {
                // 이미 권한이 있는 경우 처리
                HandlePermissionGranted(notificationPermission);
            }
        }
    }

    /// <summary>
    /// 권한이 승인된 경우 처리
    /// </summary>
    private void HandlePermissionGranted(string permissionName)
    {
        DebugEx.Log($"{permissionName} 권한이 승인됨");
        // 알림 관련 작업을 계속 진행
    }

    /// <summary>
    /// 권한이 거부된 경우 처리
    /// </summary>
    private void HandlePermissionDenied(string permissionName)
    {
        DebugEx.Log($"{permissionName} 권한이 거부되었습니다. 알림 기능을 사용할 수 없습니다.");
        // 알림 관련 기능 비활성화 또는 다른 대체 기능 제공
    }

    /// <summary>
    /// 권한이 영구적으로 거부된 경우 처리
    /// </summary>
    private void HandlePermissionDeniedPermanently(string permissionName)
    {
        DebugEx.Log($"{permissionName} 권한이 영구적으로 거부되었습니다. 설정에서 수동으로 권한을 활성화해야 합니다.");
        // 사용자가 설정에서 권한을 수동으로 활성화하도록 안내
    }
    
    /// <summary>
    /// 안드로이드 버전이 33이상인지 확인
    /// </summary>
    /// <returns></returns>
    private bool AndroidVersionIsAtLeast33()
    {
        if (_androidVersion == null)
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                _androidVersion = version.GetStatic<int>("SDK_INT");
            }
        }
        return _androidVersion >= 33;
    }
    
    /// <summary>
    /// Android 알림 생성 및 예약
    /// </summary>
    /// <param name="remainingTimeInSeconds"></param>
    /// <param name="title"></param>
    /// <param name="message"></param>
    private void ScheduleAndroidNotification(int remainingTimeInSeconds, string title, string message)
    {
        // 노티 설정
        var notification = new AndroidNotification
        {
            Title = title,
            Text = message,
            SmallIcon = "timer",
            //LargeIcon = "timer",
            FireTime = DateTime.Now.AddSeconds(remainingTimeInSeconds),
        };
        
        notification.Color = notificationColor;

        // 알림 예약
        AndroidNotificationCenter.SendNotification(notification, AndroidChannelId);

        DebugEx.Log($"[Android] 알림이 {remainingTimeInSeconds}초 후에 예약되었습니다.");
    }

    /// <summary>
    /// Android에서 모든 예약된 및 표시된 알림 취소
    /// </summary>
    private void CancelAllAndroidNotifications()
    {
        // 예약된 알림 취소
        AndroidNotificationCenter.CancelAllScheduledNotifications();
        // 이미 표시된 알림 제거
        AndroidNotificationCenter.CancelAllDisplayedNotifications();
        DebugEx.Log("[Android] 모든 알림이 취소되었습니다.");
    }

    #endregion
}
