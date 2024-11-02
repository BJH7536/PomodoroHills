using System;
using System.IO;
using Cysharp.Threading.Tasks;
using DataManagement;
using TodoSystem;
using UnityEngine;
using ItemType = DataManagement.ItemType;

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
            DebugEx.LogError($"DebugExController: AddItem 호출 중 오류 발생: {ex.Message}");
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
            DebugEx.LogWarning("DeleteItem 호출 시 인벤토리가 비어있습니다.");
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
            DebugEx.LogError($"DebugExController: DeleteItem 호출 중 오류 발생: {ex.Message}");
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

    public void DeleteAllData()
    {
        // persistentDataPath 경로 아래의 모든 파일과 폴더 삭제
        string path = Application.persistentDataPath;
        
        if (Directory.Exists(path))
        {
            // 모든 파일 삭제
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                File.Delete(file);
            }

            // 모든 디렉토리 삭제
            string[] directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                Directory.Delete(directory, true);
            }
        }
        
        // PlayerPrefs 초기화
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Todo항목들 불러오기 
        TodoManager.Instance.LoadTodoList();
        
        DebugEx.Log("모든 데이터가 초기화되었습니다.");
    }

    public void ShowAlertPopup()
    {
        PopupManager.Instance.ShowAlertPopup("대충 제목", "대충 내용");
    }
    
    public void ShowTimerInformation() {
        
        DebugEx.Log($"현재의 타이머 세션 종류는 : {TimerManager.Instance.CurrentSessionType}\n" +
                    $"현재의 타이머 상태는 : {TimerManager.Instance.CurrentTimerState}\n" +
                    $"현재의 타이머의 남은 사이클 수는 {TimerManager.Instance.remainingCycleCount}\n" +
                    $"마지막 사이클은 {TimerManager.Instance.lastCycleTime}분 \n" +
                    $"타이머의 남은 초단위 시간은 : {TimerManager.Instance.RemainingTimeInSeconds}\n" +
                    $"타이머와 연동된 Todo항목은 : \n[{TimerManager.Instance.CurrentTodoItem}]\n");
    }

    public void Add100Gold()
    {
        EconomyManager.Instance.AddCoinAsync(100).Forget();
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
        PopupManager.Instance.HandleBackButton();
    }
}
