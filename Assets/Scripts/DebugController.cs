using System;
using DataManagement;
using UnityEngine;

public class DebugController  : MonoBehaviour
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
    
    private int itemCounter = 1; // 테스트용 아이템 카운터
    
    public async void AddItem()
    {
        // 임의의 아이템 타입 선택
        ItemType randomType = GetRandomItemType();

        // 새로운 아이템 생성
        DataManagement.Item newItem = new DataManagement.Item
        {
            ItemID = Guid.NewGuid().ToString(),
            Name = $"Item {itemCounter}",
            Type = randomType,
            Quantity = UnityEngine.Random.Range(1, 10)
        };
        itemCounter++;

        try
        {
            // DataManager를 통해 아이템 추가
            await DataManager.Instance.AddItemAsync(newItem);
        }
        catch (Exception ex)
        {
            Debug.LogError($"DebugController: AddItem 호출 중 오류 발생: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete Item 버튼이 클릭될 때 호출되는 메서드입니다.
    /// </summary>
    public async void DeleteItem()
    {
        // 현재 아이템 리스트 가져오기
        var currentItems = DataManager.Instance.GetItems();

        if (currentItems.Count == 0)
        {
            Debug.LogWarning("DeleteItem 호출 시 인벤토리가 비어있습니다.");
            return;
        }

        // 마지막 아이템 삭제
        var lastItem = currentItems[^1];
        try
        {
            await DataManager.Instance.DeleteItemAsync(lastItem.ItemID);
        }
        catch (Exception ex)
        {
            Debug.LogError($"DebugController: DeleteItem 호출 중 오류 발생: {ex.Message}");
        }
    }

    /// <summary>
    /// 임의의 아이템 타입을 반환하는 메서드입니다.
    /// </summary>
    /// <returns>임의의 ItemType</returns>
    private ItemType GetRandomItemType()
    {
        Array values = Enum.GetValues(typeof(ItemType));
        return (ItemType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }
}
