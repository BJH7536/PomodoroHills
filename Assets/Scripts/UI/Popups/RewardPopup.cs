using Cysharp.Threading.Tasks;
using DG.Tweening;
using LeTai.Asset.TranslucentImage;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RewardPopup : Popup
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    [SerializeField] private TranslucentImage blur;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Transform panel;
    
    [SerializeField] private TMP_Text rewardGoldText;
    [SerializeField] private TMP_Text rewardGemText;
    
    private void OnEnable()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.DOFade(1, 0.5f);

        panel.localScale = Vector3.zero;
        panel.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }

    public void Setup(string title, string message, int rewardCoin, int rewardGem, TranslucentImageSource translucentImageSource)
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

        EconomyManager.Instance.AddCoinAsync(rewardCoin).Forget();
        EconomyManager.Instance.AddGemAsync(rewardGem).Forget();

        rewardGoldText.text = $"{rewardCoin:N0}";
        rewardGemText.text = $"{rewardGem:N0}";
        
        confirmButton.onClick.RemoveAllListeners(); // 기존 리스너 제거
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        
        cancelButton.onClick.RemoveAllListeners();  // 기존 리스너 제거
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    private void OnConfirmButtonClicked()
    {
        // 팝업 닫기
        PopupManager.Instance.HidePopup();
    }

    private void OnCancelButtonClicked()
    {
        // 취소 시 팝업 닫기
        PopupManager.Instance.HidePopup();
    }
}
