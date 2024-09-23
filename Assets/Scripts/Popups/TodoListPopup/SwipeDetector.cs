using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool isSwiping = false;

    // 스와이프를 감지할 최소 거리를 설정
    public float swipeThreshold = 50f;

    // 터치가 시작되었을 때 호출
    public void OnPointerDown(PointerEventData eventData)
    {
        // 터치 시작 위치 기록
        startTouchPosition = eventData.position;
        isSwiping = true;
    }

    // 터치가 끝났을 때 호출
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isSwiping)
        {
            // 터치 끝 위치 기록
            endTouchPosition = eventData.position;
            DetectSwipeDirection();
            isSwiping = false;
        }
    }

    // 스와이프 방향을 감지하는 함수
    private void DetectSwipeDirection()
    {
        float verticalDistance = endTouchPosition.y - startTouchPosition.y;

        if (Mathf.Abs(verticalDistance) > swipeThreshold)
        {
            if (verticalDistance > 0)
            {
                // 위로 스와이프 감지
                OnSwipeUp();
            }
            else
            {
                // 아래로 스와이프 감지
                OnSwipeDown();
            }
        }
    }

    // 위로 스와이프 동작이 감지되었을 때 실행되는 함수
    private void OnSwipeUp()
    {
        Debug.Log("위로 스와이프 감지됨");
        // 위로 스와이프 동작 처리
    }

    // 아래로 스와이프 동작이 감지되었을 때 실행되는 함수
    private void OnSwipeDown()
    {
        Debug.Log("아래로 스와이프 감지됨");
        // 아래로 스와이프 동작 처리
    }
}