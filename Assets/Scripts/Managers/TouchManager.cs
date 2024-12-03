using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-1)]
public class TouchManager : MonoBehaviour
{
    public static TouchManager Instance { get; private set; }

    // 입력 이벤트
    public event Action<Vector2> OnClick;
    public event Action<Vector2> OnDoubleClick;
    public event Action<Vector2> OnDragStart;
    public event Action<Vector2> OnDrag;                // 현재 위치
    public event Action<Vector2> OnDragDelta;           // 이동한 거리(delta)
    public event Action OnDragEnd;
    public event Action<float> OnPinch;                 // 핀치 줌

    // 설정 가능한 매개변수
    [SerializeField] private float doubleClickMaxThreshold = 0.3f;
    [SerializeField] private float dragThreshold = 5f;          // 드래그로 인식할 최소 거리 (픽셀 단위)
    [SerializeField] private float pinchThreshold = 5f;         // 핀치로 인식할 최소 거리

    private bool isDragging = false;
    private Vector2 startDragPosition;
    private Vector2 lastDragPosition;
    private float lastClickTime = 0f;
    private float clickStartTime = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    private void HandleMouseInput()
    {
        if (IsPointerOverUIObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            clickStartTime = Time.time;
            startDragPosition = Input.mousePosition;
            lastDragPosition = startDragPosition;
            isDragging = false;
        }
        else if (Input.GetMouseButton(0))
        {
            float distance = Vector2.Distance(startDragPosition, Input.mousePosition);
            
            if (!isDragging && distance > dragThreshold)
            {
                isDragging = true;
                OnDragStart?.Invoke(Input.mousePosition);
            }

            if (isDragging)
            {
                Vector2 currentPosition = Input.mousePosition;
                Vector2 delta = currentPosition - lastDragPosition;
                OnDrag?.Invoke(currentPosition);
                OnDragDelta?.Invoke(delta);
                lastDragPosition = currentPosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;
                OnDragEnd?.Invoke();
            }
            else
            {
                float timeSinceLastClick = Time.time - lastClickTime;
                if (timeSinceLastClick <= doubleClickMaxThreshold)
                {
                    OnDoubleClick?.Invoke(Input.mousePosition);
                    lastClickTime = 0f;
                }
                else
                {
                    OnClick?.Invoke(Input.mousePosition);
                    lastClickTime = Time.time;
                }
            }
        }

        // 마우스 휠로 핀치 줌 시뮬레이션
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0f)
        {
            OnPinch?.Invoke(-scroll * 100f); // 스크롤 방향과 크기를 조정하세요.
        }
    }

    private void HandleTouchInput()
    {
        if (IsPointerOverUIObject()) return;

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    clickStartTime = Time.time;
                    startDragPosition = touch.position;
                    lastDragPosition = startDragPosition;
                    isDragging = false;
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    float distance = Vector2.Distance(startDragPosition, touch.position);
                    DebugEx.Log($"Drag Distance : {distance}");

                    if (!isDragging && distance > dragThreshold)
                    {
                        isDragging = true;
                        OnDragStart?.Invoke(touch.position);
                    }

                    if (isDragging)
                    {
                        Vector2 currentPosition = touch.position;
                        Vector2 delta = currentPosition - lastDragPosition;
                        OnDrag?.Invoke(currentPosition);
                        OnDragDelta?.Invoke(delta);
                        lastDragPosition = currentPosition;
                    }
                    break;
                case TouchPhase.Ended:
                    if (isDragging)
                    {
                        isDragging = false;
                        OnDragEnd?.Invoke();
                    }
                    else
                    {
                        float timeSinceLastClick = Time.time - lastClickTime;
                        if (timeSinceLastClick <= doubleClickMaxThreshold)
                        {
                            OnDoubleClick?.Invoke(touch.position);
                            lastClickTime = 0f;
                        }
                        else
                        {
                            OnClick?.Invoke(touch.position);
                            lastClickTime = Time.time;
                        }
                    }
                    break;
            }
        }
        else if (Input.touchCount == 2)
        {
            // 핀치 제스처 처리
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            DebugEx.Log($"Pinch distance : {deltaMagnitudeDiff}");
            if (Mathf.Abs(deltaMagnitudeDiff) > pinchThreshold)
            {
                OnPinch?.Invoke(deltaMagnitudeDiff);
            }
        }
    }

    public bool IsPointerOverUIObject()
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
#if UNITY_EDITOR || UNITY_STANDALONE
        eventDataCurrentPosition.position = Input.mousePosition;
#else
        if (Input.touchCount > 0)
            eventDataCurrentPosition.position = Input.GetTouch(0).position;
        else
            return false;
#endif

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        // 레이어가 5인 오브젝트만 검사
        foreach (var result in results)
        {
            if (result.gameObject.layer == 5)
            {
                return true;
            }
        }
        return false;
    }
}
