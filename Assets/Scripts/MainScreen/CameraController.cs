using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private BoxCollider CameraArea;

    private float zoomInOrthographicSize = 5.0f;
    private float zoomOutOrthographicSize = 10.0f;
    private float tweenDuration = 0.3f;

    private Vector3 previousTouchPosition;
    private Vector3 velocity = Vector3.zero;
    private float smoothTime = 0.1f;
    
    [SerializeField] private bool isPanning = false;
    [SerializeField] private float panSpeed = 8f;
    [SerializeField] private float zoomOutThreshold = 40;
    [SerializeField] private float zoomInThreshold = 40;

    private Tween currentZoomTween;
    
    private void LateUpdate()
    {
        if ((currentZoomTween != null && currentZoomTween.IsActive()) || PlaceableManager.Instance.isEdit)
            return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ZoomOut();
        }
    }
    
    private void HandleMouseInput()
    {
        if(IsMouseOnUI()) return;
        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            ZoomIn();
        }
        else if (scroll < 0f)
        {
            ZoomOut();
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            isPanning = true;
            previousTouchPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(1) && isPanning)
        {
            Vector3 currentTouchPosition = Input.mousePosition;
            Vector3 delta = currentTouchPosition - previousTouchPosition;

            // 카메라의 로컬 축 기준으로 변환
            Vector3 localDelta = virtualCamera.transform.TransformDirection(new Vector3(delta.x, 0, delta.y) * panSpeed);
            Vector3 targetPosition = virtualCamera.transform.localPosition - localDelta;

            // Confiner 경계로 위치 제한
            targetPosition = ClampPositionWithinConfiner(targetPosition);

            // 위치 적용
            virtualCamera.transform.localPosition = Vector3.SmoothDamp(virtualCamera.transform.localPosition, targetPosition, ref velocity, smoothTime);

            previousTouchPosition = currentTouchPosition;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isPanning = false;
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 2)
        {
            if(IsMouseOnUI()) return;
            
            Touch firstTouch = Input.GetTouch(0);
            Touch secondTouch = Input.GetTouch(1);

            if (firstTouch.phase == TouchPhase.Began || secondTouch.phase == TouchPhase.Began)
            {
                isPanning = false;
            }

            if (DetectPinchOut())
            {
                ZoomIn();
            }
            else if (DetectPinchIn())
            {
                ZoomOut();
            }
        }
        else if (Input.touchCount == 1)
        {
            if(IsMouseOnUI()) return;
            
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isPanning = true;
                previousTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved && isPanning)
            {
                Vector3 currentTouchPosition = touch.position;
                Vector3 delta = currentTouchPosition - previousTouchPosition;

                // 카메라 로컬 축 기준으로 변환
                Vector3 localDelta = virtualCamera.transform.TransformDirection(new Vector3(delta.x, 0, delta.y) * panSpeed);
                Vector3 targetPosition = virtualCamera.transform.localPosition - localDelta;

                // Confiner 경계로 위치 제한
                targetPosition = ClampPositionWithinConfiner(targetPosition);

                // 위치 적용
                virtualCamera.transform.localPosition = Vector3.SmoothDamp(virtualCamera.transform.localPosition, targetPosition, ref velocity, smoothTime);

                previousTouchPosition = currentTouchPosition;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isPanning = false;
            }
        }
        else
        {
            isPanning = false;
        }
    }
    
    private void ZoomIn()
    {
        float endSize = zoomInOrthographicSize;
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
        currentZoomTween = DOTween.To(() => virtualCamera.m_Lens.OrthographicSize, x =>
        {
            var lens = virtualCamera.m_Lens;
            lens.OrthographicSize = x;
            virtualCamera.m_Lens = lens;
        }, endSize, tweenDuration)
        .SetEase(Ease.Linear)
        .OnComplete(() => currentZoomTween = null);
    }

    private bool DetectPinchOut()
    {
        if (Input.touchCount != 2) return false;

        Touch firstTouch = Input.GetTouch(0);
        Touch secondTouch = Input.GetTouch(1);

        Vector2 firstTouchPreviousPosition = firstTouch.position - firstTouch.deltaPosition;
        Vector2 secondTouchPreviousPosition = secondTouch.position - secondTouch.deltaPosition;

        float previousPositionDistance = (firstTouchPreviousPosition - secondTouchPreviousPosition).magnitude;
        float currentPositionDistance = (firstTouch.position - secondTouch.position).magnitude;

        return currentPositionDistance > previousPositionDistance + zoomInThreshold;
    }

    private bool DetectPinchIn()
    {
        if (Input.touchCount != 2) return false;

        Touch firstTouch = Input.GetTouch(0);
        Touch secondTouch = Input.GetTouch(1);

        Vector2 firstTouchPreviousPosition = firstTouch.position - firstTouch.deltaPosition;
        Vector2 secondTouchPreviousPosition = secondTouch.position - secondTouch.deltaPosition;

        float previousPositionDistance = (firstTouchPreviousPosition - secondTouchPreviousPosition).magnitude;
        float currentPositionDistance = (firstTouch.position - secondTouch.position).magnitude;

        return currentPositionDistance < previousPositionDistance - zoomOutThreshold;
    }

    private Vector3 ClampPositionWithinConfiner(Vector3 targetPosition)
    {
        // CameraArea에서 가장 가까운 경계 점 계산
        if (CameraArea == null) return targetPosition;

        Vector3 closestPoint = CameraArea.ClosestPoint(targetPosition);

        // Y축 높이는 그대로 유지, XZ 평면만 제한
        return new Vector3(closestPoint.x, CameraArea.transform.position.y, closestPoint.z);
    }
    
    private bool IsMouseOnUI()      //마우스가 UI 위에 있는지 검증합니다. 오브젝트 조작과 UI 조작이 겹칠 경우의 조작을 유효하게 하기 위한 절차입니다.
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            position = Input.mousePosition
#else
            position = Input.GetTouch(0).position
#endif
        };
        
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
    
        if (raycastResults.Count > 0)
        {
            return raycastResults[0].gameObject.layer == 5;
        }
        return false;
    }

}
