using UnityEngine;

/// <summary>
/// 모든 팝업의 기본 클래스입니다.
/// 팝업의 열림과 닫힘을 관리합니다.
/// </summary>
public class Popup : MonoBehaviour
{
    /// <summary>
    /// 팝업을 활성화합니다.
    /// </summary>
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 팝업을 비활성화합니다.
    /// </summary>
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 팝업이 닫힐 때 추가 동작을 처리할 수 있는 메서드입니다.
    /// 필요에 따라 재정의하여 사용합니다.
    /// </summary>
    public virtual void OnClose()
    {
        // 필요한 경우 재정의하여 추가적인 동작을 처리
    }

    public void Close()
    {
        PopupManager.Instance.HidePopup();
    }
}