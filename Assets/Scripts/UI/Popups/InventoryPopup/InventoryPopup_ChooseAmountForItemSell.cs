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
            PopupManager.Instance.ShowAlertPopup("�Ǹ� �Ұ�", "0���� ��������\n\n�Ǹ��� �� �����ϴ�.");
            return;
        }
        
        string name = ItemTable.GetItemInformById(itemIdToSell).name;
        
        PopupManager.Instance.ShowConfirmPopup("������ �Ǹ�", $"{ChosenAmount}���� {name}��(��)\n������ �Ǹ��Ͻðڽ��ϱ�?", () =>
        {
            StoreManager.Instance.SellItem(itemIdToSell, ChosenAmount).Forget();
        });
    }
    
    public void Close()
    {
        PopupManager.Instance.HidePopup();
    }
    
}
