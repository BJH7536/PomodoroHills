using DG.Tweening;
using TMPro;
using UnityEngine;

public class ErrorPopup : Popup
{
    [SerializeField] private TMP_Text errorMessageText;
    [SerializeField] private CanvasGroup canvasGroup;

    // 에러 메시지를 설정하는 메서드
    public void SetErrorMessage(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
        }
    }

    private void OnEnable()
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        
        Vanish();
    }

    private void Vanish()
    {
        // 시퀀스 생성
        Sequence sequence = DOTween.Sequence();
        
        // 3초간 유지
        sequence.AppendInterval(3.0f);

        // 3초 동안 서서히 alpha 값을 0으로 감소
        sequence.Append(canvasGroup.DOFade(0.0f, 3.0f));

        // 애니메이션 완료 후 팝업 숨김
        sequence.OnComplete(() =>
        {
            PopupManager.Instance.HidePopup();
        });
    }
}