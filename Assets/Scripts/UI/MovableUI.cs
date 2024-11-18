using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(LongPressDetector))]
public class MovableUI : MonoBehaviour, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform rectTransform;               // UI 요소의 RectTransform
    [SerializeField] private Canvas canvas;                             // UI가 속한 Canvas (좌표 변환에 필요)
    [SerializeField] private LongPressDetector longPressDetector;
    
    private bool isMovable = false; // 드래그 가능 여부

    private float originXPos;
    
    void Awake()
    {
        longPressDetector.onLongPress.AddListener(() =>
        {
            isMovable = true;                                       // 롱프레스가 감지되면 이동 가능 상태 활성화

            rectTransform.DOScale(0.7f, 0.2f).SetEase(Ease.OutExpo);
            
            longPressDetector.GetComponent<Button>().interactable = false;
            
            DebugEx.Log($"MovableUI is now movable");
        });

        originXPos = rectTransform.anchoredPosition.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isMovable)
        {
            // 터치 위치를 UI 로컬 좌표로 변환하여 이동
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out var localPointerPosition);

            rectTransform.localPosition = localPointerPosition;
        }
    }
    
    public async void OnPointerUp(PointerEventData eventData)
    {
        if (!isMovable) return;

        isMovable = false;
        rectTransform.DOScale(1, 0.2f).SetEase(Ease.OutExpo);

        // 가까운 가장자리로 이동
        await SnapToEdgeAsync();

        longPressDetector.GetComponent<Button>().interactable = true;
        DebugEx.Log("UI 이동 완료");
    }

    private async UniTask SnapToEdgeAsync()
    {
        // 부모 RectTransform의 너비 계산
        RectTransform parentRect = rectTransform.parent as RectTransform;
        float parentWidth = parentRect.rect.width;
        
        // 현재 위치 가져오기
        float currentX = rectTransform.anchoredPosition.x;

        // 화면 중앙 기준 가까운 가장자리로 이동 결정
        // float targetX = currentX < 0 ? -parentWidth / 2 : parentWidth / 2;
        float targetX = (currentX < -(parentWidth / 2)) ? -parentWidth - originXPos : originXPos;
    
        DebugEx.Log($"targetX {targetX}");
        
        // DOTween을 사용해 부드럽게 이동
        await rectTransform.DOAnchorPosX(targetX, 0.2f).SetEase(Ease.OutExpo).ToUniTask();
    }
}