using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwipeUpUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Scrollbar scrollBar;
    [SerializeField] private RectTransform mainUI;
    [SerializeField] private float swipeThreshold = 0.6f;        // 페이지가 열리고, 닫히는 기준 값
    [SerializeField] private float tweeningDuration = 0.5f;
    [SerializeField] private float scaler = 0.88f;
    [SerializeField] private bool touching = false;               // 터치 중 
    [SerializeField] private bool tweening = false;               // 트위닝 중
    
    public void OnPointerDown(PointerEventData eventData)
    {
        touching = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        touching = false;
    }
    
    public void CheckSwipeUIOpen(float value)
    {
        float easedValue = EaseOutSine(value);
        float scale = Mathf.Lerp(1, scaler, easedValue);
        mainUI.localScale = Vector3.one * scale;
        
        // 터치 중이거나, 트위닝 중이면 아무것도 안하고
        if (touching || tweening) return;
        
        // 터치도 하고있지 않고,
        // 트위닝도 하고 있지 않으면
        // 지금 스크롤 바 값과 임계값의 대소비교를 통해 열거나 닫기를 수행
        
        if(value <= swipeThreshold) OpenMainUI();
        else CloseMainUI();
    }
    
    private float EaseOutSine(float x)
    {
        return Mathf.Sin((x * Mathf.PI) / 2);
    }

    private void OpenMainUI()
    {
        tweening = true;
        DOTween.To(() => scrollBar.value, x => scrollBar.value = x, 0, tweeningDuration)
            .SetEase(Ease.OutSine)
            .SetAutoKill(false)
            .SetRecyclable(true)
            .OnComplete(() => tweening = false);
    }

    private void CloseMainUI()
    {
        tweening = true;
        DOTween.To(() => scrollBar.value, x => scrollBar.value = x, 1, tweeningDuration)
            .SetEase(Ease.InSine)
            .SetAutoKill(false)
            .SetRecyclable(true)
            .OnComplete(() => tweening = false);
    }
}
