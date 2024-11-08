// using GooglePlayGames;

using GooglePlayGames;
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
        PlayGamesPlatform.Instance.ShowAchievementsUI();
    }
    
    // 설정 팝업을 열기 위한 메서드
    public void OpenSettingsPopup() 
    {
        PopupManager.Instance.ShowPopup<SettingsPopup>();
    }
}
