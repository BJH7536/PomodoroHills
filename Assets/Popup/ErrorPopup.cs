using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorPopup : Popup
{
    [SerializeField] private TMP_Text errorMessageText;

    // 에러 메시지를 설정하는 메서드
    public void SetErrorMessage(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
        }
    }
    
}