using DG.Tweening;
using UnityEngine;

/// <summary>
/// 로딩 UI를 제어하는 클래스입니다.
/// 로딩 시작 시 패널을 표시하고, 로딩 완료 시 패널을 숨깁니다.
/// </summary>
public class LoadingUI : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel; // 로딩 패널 참조

    private Tween _rotationTween;
    
    /// <summary>
    /// 로딩 패널을 표시합니다.
    /// </summary>
    public void ShowLoading()
    {
        loadingPanel.SetActive(true);
        
        StartLoadingAnimation();
    }

    /// <summary>
    /// 로딩 패널을 숨깁니다.
    /// </summary>
    public void HideLoading()
    {
        loadingPanel.SetActive(false);
        
        StopLoadingAnimation();
    }
    
    // Tween 객체를 저장하기 위한 변수

    // 회전 시작 메서드
    private void StartLoadingAnimation()
    {
        // Z축 기준으로 무한 회전. 360도 회전하며 시간이 지나면 다시 시작하는 방식으로 무한히 반복.
        _rotationTween = loadingPanel.transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart) // 무한 루프 설정
            .SetEase(Ease.Linear); // 일정한 속도로 회전
    }

    // 회전 멈추기 메서드
    private void StopLoadingAnimation()
    {
        // 트윈이 진행 중일 때만 중지
        if (_rotationTween != null && _rotationTween.IsActive())
        {
            _rotationTween.Kill(); // 회전 트윈을 중지 및 제거
        }
    }
}