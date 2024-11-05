using System;
using Cysharp.Threading.Tasks;
using PomodoroHills;
using UnityEngine;
using UnityEngine.Serialization;
using VInspector;

public class InventoryPopup_ChooseAmountForItemSell : Popup
{
    [Tab("information")] 
    [SerializeField] private ItemTable ItemTable;
    [SerializeField] private ScrollSystem amountScrollSystem;
    
    public int ChosenAmount = 1;

    private int itemIdToSell;
    
    public void SetUp(int itemIdForSell, int maxAmount)
    {
        itemIdToSell = itemIdForSell;
        amountScrollSystem.Setup(0, maxAmount, OnAmountChanged, 1);
        
        ChosenAmount = 1;
    }

    public void OnAmountChanged(int value)
    {
        ChosenAmount = value;
    }

    public void Confirm()
    {
        PopupManager.Instance.HidePopup();
        
        if (ChosenAmount == 0)
        {
            PopupManager.Instance.ShowAlertPopup("판매 불가", "0개의 아이템은\n\n판매할 수 없습니다.");
            return;
        }
        
        string name = ItemTable.GetItemInformById(itemIdToSell).name;
        
        PopupManager.Instance.ShowConfirmPopup("아이템 판매", $"{ChosenAmount}개의 {name}을(를)\n정말로 판매하시겠습니까?", () =>
        {
            StoreManager.Instance.SellItem(itemIdToSell, ChosenAmount).Forget();
        });
    }
    
    public void Close()
    {
        PopupManager.Instance.HidePopup();
    }
    
}
