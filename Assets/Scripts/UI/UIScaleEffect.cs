using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIScaleEffect : MonoBehaviour
{
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void PlayScaleEffect()
    {
        // DOTween을 사용해 UI 요소를 살짝 키웠다가 돌아오는 효과를 줍니다.
        float duration = 0.1f; // 효과 지속 시간
        Vector3 targetScale = Vector3.one * 1.4f; // 살짝 커진 크기

        rectTransform.DOScale(targetScale, duration).SetEase(Ease.OutBack)
            .OnComplete(() => rectTransform.DOScale(Vector3.one, duration));
    }
}