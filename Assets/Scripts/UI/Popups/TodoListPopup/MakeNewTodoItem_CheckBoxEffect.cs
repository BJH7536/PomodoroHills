using TMPro;
using UnityEngine;

public class MakeNewTodoItem_CheckBoxEffect : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_InputField _inputField;

    public void MakeEffectToTarget(bool isOn)
    {
        if (isOn)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
            }
            if (_inputField != null) _inputField.interactable = true;
        }
        else
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0.5f;
                _canvasGroup.interactable = false;
            }
            if (_inputField != null) _inputField.interactable = false;
        }
        
    }
}
