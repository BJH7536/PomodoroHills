using UnityEngine;

public class MainUI : MonoBehaviour
{
    // 인벤토리 팝업을 열기 위한 메서드
    public void OpenInventoryPopup()
    {
        PopupManager.Instance.ShowPopup<InventoryPopup>();
    }
    
    public void OpenTodoListPopup() 
    {
        PopupManager.Instance.ShowPopup<TodoListPopup>();
    }

    public void HideAllPopups()
    {
        PopupManager.Instance.HideAllPopups();
    }

    public void OpenGPGSAchievement()
    {
        // TODO 구글 업적 여는 기능 넣기
        // TODO 설정 팝업에서는 구글 로그인하는 기능 넣기
    }
    
    // 설정 팝업을 열기 위한 메서드
    public void OpenSettingsPopup() 
    {
        PopupManager.Instance.ShowPopup<SettingsPopup>();
    }
}
