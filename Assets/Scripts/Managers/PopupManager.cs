using System;
using System.Collections.Generic;
using DG.Tweening;
using LeTai.Asset.TranslucentImage;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 팝업을 관리하는 클래스입니다.
/// 팝업의 열림과 닫힘을 제어하며, 팝업 인스턴스의 재사용을 통해 성능을 최적화합니다.
/// </summary>
public class PopupManager : MonoBehaviour
{
    /// <summary>
    /// PopupManager의 단일 인스턴스를 유지하기 위한 변수입니다.
    /// </summary>
    private static PopupManager instance;

    /// <summary>
    /// PopupManager의 싱글톤 인스턴스에 접근하기 위한 프로퍼티입니다.
    /// </summary>
    public static PopupManager Instance => instance;

    /// <summary>
    /// 팝업이 생성될 부모 캔버스입니다.
    /// </summary>
    [SerializeField] private Transform popupCanvas;

    /// <summary>
    /// 현재 활성화된 팝업들을 관리하는 스택입니다.
    /// </summary>
    private Stack<Popup> activePopups = new Stack<Popup>();

    /// <summary>
    /// 팝업 타입별로 비활성화된 팝업 인스턴스를 저장하는 객체 풀입니다.
    /// </summary>
    private Dictionary<Type, Stack<Popup>> popupPool = new Dictionary<Type, Stack<Popup>>();

    [SerializeField] private TranslucentImageSource _translucentImageSource;

    [SerializeField] private PomoPopupImages pomoPopupImages;
    
    /// <summary>
    /// 게임 오브젝트가 활성화될 때 호출됩니다.
    /// 싱글톤 인스턴스를 설정하고, 팝업 캔버스를 초기화합니다.
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

        // PopupCanvas 초기화
        InitializePopupCanvas();

        // DOTween 초기화
        DOTween.Init(true, true, LogBehaviour.ErrorsOnly).SetCapacity(200, 50);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            HandleBackButton();
        }
    }

    /// <summary>
    /// PopupCanvas를 초기화하는 메서드입니다.
    /// 기존에 존재하는 "PopupCanvas"를 찾거나, 새로 생성하여 설정합니다.
    /// </summary>
    private void InitializePopupCanvas()
    {
        if (popupCanvas != null) return;

        int referenceWidth = 1080; // 기준 해상도 너비
        int referenceHeight = 1920; // 기준 해상도 높이

        // 씬 내에서 "PopupCanvas"라는 이름의 캔버스를 찾습니다.
        Canvas foundCanvas = GameObject.Find("PopupCanvas")?.GetComponent<Canvas>();
        if (foundCanvas != null)
        {
            popupCanvas = foundCanvas.transform;
            return;
        }

        // "PopupCanvas"가 존재하지 않으면 새로 생성합니다.
        GameObject newCanvas = new GameObject("PopupCanvas");
        newCanvas.layer = LayerMask.NameToLayer("UI"); // UI 레이어로 설정

        // Canvas 컴포넌트 추가 및 설정
        Canvas canvasComponent = newCanvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceCamera; // 화면 공간 오버레이 모드
        canvasComponent.worldCamera = Camera.main;
        canvasComponent.sortingOrder = 1000;
        
        // CanvasScaler 컴포넌트 추가 및 설정
        CanvasScaler canvasScaler = newCanvas.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; // 화면 크기에 따라 스케일 조정
        canvasScaler.referenceResolution = new Vector2(referenceWidth, referenceHeight); // 기준 해상도 설정
        canvasScaler.matchWidthOrHeight = 0.5f;
        
        // GraphicRaycaster 컴포넌트 추가
        newCanvas.AddComponent<GraphicRaycaster>();

        // 팝업 캔버스를 설정
        popupCanvas = newCanvas.transform;
    }

    /// <summary>
    /// 특정 타입의 팝업을 표시하는 메서드입니다.
    /// 동일한 타입의 팝업이 이미 활성화되어 있으면 새로 생성하지 않습니다.
    /// </summary>
    /// <typeparam name="T">표시할 팝업의 타입</typeparam>
    /// <returns>생성된 팝업 인스턴스</returns>
    public T ShowPopup<T>(bool blur = false) where T : Popup
    {
        Type popupType = typeof(T);
        T popup = null;

        // 이미 같은 타입의 팝업이 활성화되어 있는지 확인
        foreach (Popup activePopup in activePopups)
        {
            if (activePopup.GetType() == popupType)
            {
                // 같은 타입의 팝업이 이미 활성화되어 있다면 아무 작업도 하지 않음
                return null;
            }
        }

        // 객체 풀에서 사용 가능한 팝업이 있는지 확인
        if (popupPool.TryGetValue(popupType, out Stack<Popup> pool) && pool.Count > 0)
        {
            popup = (T)pool.Pop();
            popup.gameObject.SetActive(true); // 팝업 활성화
        }
        else
        {
            // 객체 풀이 비어있으면 팝업을 새로 생성
            popup = PopupFactory.CreatePopup<T>();
            if (popup != null)
            {
                if(blur)
                    popup.transform.SetParent(null, false); 
                else
                    popup.transform.SetParent(popupCanvas, false); // PopupCanvas를 부모로 설정
            }
        }

        if (popup != null)
        {
            popup.transform.SetAsLastSibling(); // 팝업을 부모 계층에서 가장 마지막 자식으로 이동시킴
            popup.Show(); // 팝업 표시
            activePopups.Push(popup); // 활성 팝업 스택에 추가
        }

        return popup;
    }

    /// <summary>
    /// 현재 활성화된 팝업을 닫는 메서드입니다.
    /// 가장 최근에 열린 팝업을 닫고 객체 풀에 반환한다.
    /// </summary>
    public bool HidePopup()
    {
        if (activePopups.Count == 0) return false;      // 열려있는 팝업이 없었기에, 팝업을 끄는 데 실패했다.

        Popup popupToClose = activePopups.Pop(); // 활성 팝업 스택에서 팝업을 꺼냄
        popupToClose.OnClose(); // 팝업이 닫힐 때 추가 동작 수행
        popupToClose.Hide(); // 팝업 비활성화

        // 객체 풀에 팝업 반환
        Type popupType = popupToClose.GetType();
        if (!popupPool.ContainsKey(popupType))
        {
            popupPool[popupType] = new Stack<Popup>();
        }
        popupPool[popupType].Push(popupToClose); // 팝업을 풀에 추가
        
        return true;        // 열려있는 팝업이 있었고, 그래서 팝업을 끌 수 있었다.
    }

    public ErrorPopup ShowErrorPopup(string message)
    {
        ErrorPopup errorPopup = ShowPopup<ErrorPopup>();

        if (errorPopup != null)
        {
            errorPopup.SetErrorMessage(message);
            return errorPopup;
        }

        return null;
    }

    public ConfirmPopup ShowConfirmPopup(string title, string message, UnityAction confirmAction)
    {
        ConfirmPopup confirmPopup = ShowPopup<ConfirmPopup>(true);
        
        if (confirmPopup != null)
        {
            confirmPopup.Setup(title, message, confirmAction, _translucentImageSource);
            return confirmPopup;
        }

        return null;
    }

    public AlertPopup ShowAlertPopup(string title, string message)
    {
        AlertPopup confirmPopup = ShowPopup<AlertPopup>(true);
        
        if (confirmPopup != null)
        {
            confirmPopup.Setup(title, message, _translucentImageSource);
            return confirmPopup;
        }

        return null;
    }
    
    public void HandleBackButton()
    {
        // 팝업을 끄는 데 실패했다면,
        // 게임 종료를 묻는 팝업이 켜진다.
        if (!HidePopup())
        {
            ShowPopup<QuitGamePopup>(true);
        }
    }
}
