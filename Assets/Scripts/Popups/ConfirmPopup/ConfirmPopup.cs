using System;
using DG.Tweening;
using LeTai.Asset.TranslucentImage;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmPopup : Popup
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    [SerializeField] private TranslucentImage blur;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Transform panel;
    
    private UnityAction onConfirmAction;

    private void OnEnable()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.DOFade(1, 0.5f);

        panel.localScale = Vector3.zero;
        panel.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }

    public void Setup(string title, string message, UnityAction ConfirmAction, TranslucentImageSource translucentImageSource)
    {
        if (!string.IsNullOrEmpty(title))
        {
            titleText.text = title;
        }

        if (!string.IsNullOrEmpty(message))
        {
            messageText.text = message;
        }

        blur.source = translucentImageSource;
        
        onConfirmAction = ConfirmAction;

        confirmButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        
        cancelButton.onClick.RemoveAllListeners();  // 기존 리스너 제거
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    private void OnConfirmButtonClicked()
    {
        onConfirmAction?.Invoke();  // 확인 작업 수행
        
        // 팝업 닫기
        PopupManager.Instance.HidePopup();
    }

    private void OnCancelButtonClicked()
    {
        // 취소 시 팝업 닫기
        PopupManager.Instance.HidePopup();
    }
    
}
