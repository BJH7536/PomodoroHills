using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MakeNewTodoItemCaller : MonoBehaviour
{
    [SerializeField] private RectTransform myRectTransform;
    [SerializeField] private GameObject MakeNewTodoItem;

    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] private ContentSizeFitter contentSizeFitter;
    
    private RectTransform targetRectT;
    private Vector2 oriAMin;
    private Vector2 oriAMax;
    private Vector2 oriAP;
    
    private void Awake()
    {
        targetRectT = MakeNewTodoItem.GetComponent<RectTransform>();
        
        oriAMin = targetRectT.anchorMin;
        oriAMax = targetRectT.anchorMax;
        oriAP = targetRectT.anchoredPosition;
    }

    public void OnEnable()
    {
        MakeNewTodoItem.SetActive(false);
    }

    public async void OpenMakeNewTodoItem()
    {
        MakeNewTodoItem.SetActive(true);
        
        verticalLayoutGroup.enabled = true;
        contentSizeFitter.enabled = true;
        
        targetRectT.anchorMin = Vector2.right;
        targetRectT.anchorMax = Vector2.right;
        targetRectT.anchoredPosition = myRectTransform.anchoredPosition;

        var canvasGroup = targetRectT.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        
        // Tween들이 끝날 때까지 대기
        await UniTask.WhenAll(
            canvasGroup.DOFade(1, 0.5f).SetEase(Ease.InExpo).ToUniTask(),
            targetRectT.DOAnchorMin(oriAMin, 0.5f).ToUniTask(),
            targetRectT.DOAnchorMax(oriAMax, 0.5f).ToUniTask(),
            targetRectT.DOAnchorPos(oriAP, 0.5f, true).ToUniTask()
        );

        // Tween이 끝난 후 VerticalLayoutGroup과 ContentSizeFitter 비활성화
        verticalLayoutGroup.enabled = false;
        contentSizeFitter.enabled = false;
    }
    
}
