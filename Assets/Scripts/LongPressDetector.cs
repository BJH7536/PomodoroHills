using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LongPressDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float longPressDuration = 1.0f;  // 롱프레스를 인식하는 시간 (초)
    private bool isPressing = false;
    private float pressTime = 0f;

    // UnityEvent로 롱프레스 시 호출할 이벤트를 에디터에서 지정할 수 있음
    public UnityEvent onLongPress;

    private void Update()
    {
        if (isPressing)
        {
            pressTime += Time.deltaTime;

            // 롱프레스 시간이 지나면 이벤트 호출
            if (pressTime >= longPressDuration)
            {
                isPressing = false;  // 중복 호출 방지
                onLongPress?.Invoke();  // 롱프레스 이벤트 호출
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 버튼이 눌리면 타이머 시작
        isPressing = true;
        pressTime = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 버튼이 떼어지면 타이머 종료
        isPressing = false;
        pressTime = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 포인터가 영역을 벗어나면 롱프레스 취소
        isPressing = false;
        pressTime = 0f;
    }
}