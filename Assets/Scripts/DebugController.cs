using System;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TodoSystem;
using UnityEngine;

public class DebugExController  : MonoBehaviour
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

    public void OpenAlertPopup()
    {
        PopupManager.Instance.ShowAlertPopup("제목", "내용");
    }
    
    public void Add1000Gold()
    {
        EconomyManager.Instance.AddCoinAsync(1000).Forget();
    }
    
    public void Subtract100Gold()
    {
        EconomyManager.Instance.SpendCoinAsync(100).Forget();
    }
    
    public void Add100Gem()
    {
        EconomyManager.Instance.AddGemAsync(100).Forget();
    }
    
    public void Subtract100Gem()
    {
        EconomyManager.Instance.SpendGemAsync(100).Forget();
    }

    public void AndroidBackButton()
    {
        PopupManager.Instance.HandleAndroidBackButton();
    }
}
