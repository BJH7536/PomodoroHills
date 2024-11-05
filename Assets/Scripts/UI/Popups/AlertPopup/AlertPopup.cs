using System;
using Cysharp.Threading.Tasks;
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
    
    private async void OnEnable()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.DOFade(1, 0.5f);

        panel.localScale = Vector3.zero;
        panel.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        

        touchCover.onClick.RemoveAllListeners();
        await UniTask.Delay(500);
        touchCover.onClick.AddListener(()=> PopupManager.Instance.HidePopup());
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
