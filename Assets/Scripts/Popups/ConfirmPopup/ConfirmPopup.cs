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

    private UnityAction onConfirmAction;

    public void Setup(string title, string message, UnityAction onConfirm)
    {
        if (!string.IsNullOrEmpty(title))
        {
            titleText.text = title;
        }

        if (!string.IsNullOrEmpty(message))
        {
            messageText.text = message;
        }

        onConfirmAction = onConfirm;

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
