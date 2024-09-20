using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using VInspector;

public class TodoListPopup : Popup
{
    [Tab("Tweening")]
    [SerializeField] private float originScale = 0.7f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private Ease Ease = Ease.OutBack;
    
    [Tab("Other")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private string TitleBase = "Todo List";
    [SerializeField] private GameObject panel;
    [SerializeField] public CanvasGroup canvasGroup; // 투명도를 조정할 UI 그룹


    private void OnEnable()
    {
        panel.transform.localScale = Vector3.one * originScale;
        panel.transform.DOScale(Vector3.one, duration).SetEase(Ease);

        titleText.text = $"{TitleBase} - {DateTime.Today:yyyy.MM.dd}";
    }
    
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.A))
        // {
        //     FadeOutUI().Forget();
        // } else if (Input.GetKeyDown(KeyCode.D))
        // {
        //     FadeInUI();
        // }
    }

    // 투명도를 1에서 0으로 서서히 줄이기
    public async UniTaskVoid FadeOutUI()
    {
        // 첫 번째 트윈: 투명도를 1에서 0으로 서서히 줄이기
        await canvasGroup.DOFade(0f, 1f).ToUniTask();
        canvasGroup.DOFade(1f, 1f);
    }

    // 투명도를 0에서 1로 서서히 늘리기
    public void FadeInUI()
    {
        canvasGroup.DOFade(1f, 1f);  // 1초 동안 알파 값을 1로 변경
    }

    public void Close()
    {
        PopupManager.Instance.HidePopup();
    }
    
}
