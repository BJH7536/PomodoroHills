using UnityEngine;
using Cinemachine;
using DG.Tweening;
using VInspector;

[DefaultExecutionOrder(-1)]
public class CameraManager : MonoBehaviour
{
    private static CameraManager instance;
    public static CameraManager Instance => instance;
    
    [Header("Control")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private BoxCollider cameraArea;

    private float zoomInOrthographicSize = 5.0f;
    private float zoomOutOrthographicSize = 10.0f;
    private float tweenDuration = 0.3f;

    private Vector3 velocity = Vector3.zero;
    private float smoothTime = 0.1f;
    
    [Header("Panning")]
    [SerializeField] private float panSpeed = 0.02f;

    private Tween currentZoomTween;

    [Header("Focus")]
    [SerializeField] private Transform pomo;
    private Vector3 DeltaToFocus;
    
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
        TouchManager.Instance.OnDragDelta += HandleDragDelta;
        // TouchManager.Instance.OnDragDelta += (vec)=> { DebugEx.Log($"OnDragDelta");};

        TouchManager.Instance.OnPinch += HandlePinch;
        // TouchManager.Instance.OnPinch += (vec)=> { DebugEx.Log($"OnPinch");};

        TouchManager.Instance.OnClick += HandleClick;
        // TouchManager.Instance.OnClick += (vec)=> { DebugEx.Log($"OnClick");};

        DeltaToFocus = virtualCamera.transform.position - pomo.position;
    }

    private void OnDisable()
    {
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.OnDragDelta -= HandleDragDelta;
            TouchManager.Instance.OnPinch -= HandlePinch;
            TouchManager.Instance.OnClick -= HandleClick;
        }
    }

    private void Update()
    {
        if ((currentZoomTween != null && currentZoomTween.IsActive()) || PlaceableManager.Instance.IsEdit)
            return;

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ZoomOut();
        }
    }

    private void HandleDragDelta(Vector2 delta)
    {
        if (TouchManager.Instance.IsPointerOverUIObject()) return;
        if (PlaceableManager.Instance.IsEdit) return;

        // 스크린 좌표에서 월드 좌표로 변환
        // 카메라의 로컬 축 기준으로 변환
        Vector3 localDelta = virtualCamera.transform.TransformDirection(new Vector3(delta.x, 0, delta.y * 1.2f) * panSpeed);
        Vector3 targetPosition = virtualCamera.transform.localPosition - localDelta;

        targetPosition = ClampPositionWithinConfiner(targetPosition);

        virtualCamera.transform.localPosition = Vector3.SmoothDamp(virtualCamera.transform.localPosition, targetPosition, ref velocity, smoothTime);
    }

    private void HandlePinch(float deltaMagnitudeDiff)
    {
        if (PlaceableManager.Instance.IsEdit) return;

        if (deltaMagnitudeDiff < 0)
        {
            ZoomIn();
        }
        else
        {
            ZoomOut();
        }
    }

    private void HandleClick(Vector2 position)
    {
        if (TouchManager.Instance.IsPointerOverUIObject()) return;
        if (PlaceableManager.Instance.IsEdit) return;

        // 마우스 휠 업으로 ZoomIn 시뮬레이션
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            ZoomIn();
        }
#endif
    }

    private void ZoomIn()
    {
        float endSize = zoomInOrthographicSize;
        if (currentZoomTween != null && currentZoomTween.IsActive())
        {
            currentZoomTween.Kill();
        }

        currentZoomTween = DOTween.To(() => virtualCamera.m_Lens.OrthographicSize, x =>
        {
            var lens = virtualCamera.m_Lens;
            lens.OrthographicSize = x;
            virtualCamera.m_Lens = lens;
        }, endSize, tweenDuration)
        .SetEase(Ease.Linear)
        .OnComplete(() => currentZoomTween = null);
    }

    private void ZoomOut()
    {
        float endSize = zoomOutOrthographicSize;
        if (currentZoomTween != null && currentZoomTween.IsActive())
        {
            currentZoomTween.Kill();
        }
        currentZoomTween = DOTween.To(() => virtualCamera.m_Lens.OrthographicSize, x =>
        {
            var lens = virtualCamera.m_Lens;
            lens.OrthographicSize = x;
            virtualCamera.m_Lens = lens;
        }, endSize, tweenDuration)
        .SetEase(Ease.Linear)
        .OnComplete(() => currentZoomTween = null);
    }

    private Vector3 ClampPositionWithinConfiner(Vector3 targetPosition)
    {
        if (cameraArea == null) return targetPosition;

        Vector3 closestPoint = cameraArea.ClosestPoint(targetPosition);

        return new Vector3(closestPoint.x, cameraArea.transform.position.y, closestPoint.z);
    }

    [Button]
    public void FocusToPomo()
    {
        Vector3 targetPosition = pomo.position + DeltaToFocus;
        
        targetPosition = ClampPositionWithinConfiner(targetPosition);

        virtualCamera.transform.DOMove(targetPosition, 0.5f, false);
    }

    public void EnableCameraPanning()
    {
        TouchManager.Instance.OnDragDelta += HandleDragDelta;
    }
    
    public void DisableCameraPanning()
    {
        TouchManager.Instance.OnDragDelta -= HandleDragDelta;
    }
}
