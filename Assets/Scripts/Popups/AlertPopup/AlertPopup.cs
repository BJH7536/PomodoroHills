using System;
using DG.Tweening;
using LeTai.Asset.TranslucentImage;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AlertPopup : Popup
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    
    [SerializeField] private TranslucentImage blur;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Transform panel;

    [SerializeField] private Button touchCover;

    private void Awake()
    {
        touchCover.onClick.RemoveAllAndAddListener(()=> PopupManager.Instance.HidePopup());
    }

    private void OnEnable()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.DOFade(1, 0.5f);

        panel.localScale = Vector3.zero;
        panel.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }
    
    public void Setup(string title, string message, TranslucentImageSource translucentImageSource)
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
    }

    private void Close()
    {
        PopupManager.Instance.HidePopup();
    }
}
