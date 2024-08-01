using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchOut : MonoBehaviour
{
    [SerializeField] private CameraManager CameraManager;
    
    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            CameraManager.ZoomOut();
        }
        
        DetectCameraZoomOut();
    }

    private void DetectCameraZoomOut()
    {
        if (Input.touchCount != 2) return; 
        
        Touch firstTouch = Input.GetTouch(0);
        Touch secondTouch = Input.GetTouch(1);

        // 직전 프레임의 터치 위치를 구하기 위해 "현재 위치 - 위치 변화량"을 계산
        Vector2 firstTouchPreviousPosition = firstTouch.position - firstTouch.deltaPosition;
        Vector2 secondTouchPreviousPosition = secondTouch.position - secondTouch.deltaPosition;

        // 직전 프레임에서 터치 위치 거리 값
        float previousPositionDistance = (firstTouchPreviousPosition - secondTouchPreviousPosition).magnitude;
        float currentPositionDistance = (firstTouch.position - secondTouch.position).magnitude;

        if (currentPositionDistance < previousPositionDistance)
        {
            CameraManager.ZoomOut();
        }

    }
}
