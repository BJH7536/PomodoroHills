using UnityEngine;

public class TestPopupOpener : MonoBehaviour
{
    // 인벤토리 팝업을 열기 위한 메서드
    public void OpenInventoryPopup()
    {
        PopupManager.Instance.ShowPopup<InventoryPopup>();
    }

    // 현재 활성화된 팝업을 닫기 위한 메서드
    public void CloseCurrentPopup()
    {
        PopupManager.Instance.HidePopup();
    }
    
    // 설정 팝업을 열기 위한 메서드
    public void OpenSettingsPopup() 
    {
        PopupManager.Instance.ShowPopup<SettingsPopup>();
    }
    
    public void OpenTodoListPopup() 
    {
        PopupManager.Instance.ShowPopup<TodoListPopup>();
    }
    
}