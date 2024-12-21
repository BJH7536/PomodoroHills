using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LongPressDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float longPressDuration = 1.0f;      // 롱프레스를 인식하는 시간 (초)
    public bool isPressing = false;
    public bool longPressTriggered = false;    // 롱프레스 이벤트의 발생 여부 추적
    private float pressTime = 0f;
    
    public UnityEvent onLongPress;           // 롱프레스 시작 시 호출되는 이벤트
    public UnityEvent onLongPressEnd;        // 롱프레스 종료 시 호출되는 이벤트

    private void Update()
    {
        if (isPressing)
        {
            pressTime += Time.deltaTime;

            // 롱프레스 시간이 지나면 이벤트 호출
            if (!longPressTriggered && pressTime >= longPressDuration)
            {
                longPressTriggered = true;
                onLongPress?.Invoke();  // 롱프레스 이벤트 호출
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 버튼이 눌리면 타이머 시작
        isPressing = true;
        longPressTriggered = false;
        pressTime = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 버튼이 떼어지면 타이머 종료
        if (longPressTriggered)
        {
            onLongPressEnd?.Invoke();  // 롱프레스 종료 이벤트 호출
        }
        Reset();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 롱프레스가 아직 트리거되지 않았을 때만 롱프레스를 취소
        if (!longPressTriggered)
        {
            Reset();
        }
        // 롱프레스가 이미 시작된 경우에는 아무 작업도 하지 않음
    }
    
    private void Reset()
    {
        isPressing = false;
        longPressTriggered = false;
        pressTime = 0f;
    }
}