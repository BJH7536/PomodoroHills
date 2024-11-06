using UnityEngine;

public class MainUI : MonoBehaviour
{
    // �κ��丮 �˾��� ���� ���� �޼���
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
        // TODO ���� ���� ���� ��� �ֱ�
        // TODO ���� �˾������� ���� �α����ϴ� ��� �ֱ�
    }
    
    // ���� �˾��� ���� ���� �޼���
    public void OpenSettingsPopup() 
    {
        PopupManager.Instance.ShowPopup<SettingsPopup>();
    }
}
