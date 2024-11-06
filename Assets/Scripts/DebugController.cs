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
    
    public async void DeleteAllData()
    {
        // 각 매니저의 캐시된 데이터 초기화
        ResetInventoryManager();
        ResetEconomyManager();
        ResetTodoManager();
        ResetPlaceableManager();
        
        // 데이터가 저장된 경로 가져오기
        string dataPath = Application.persistentDataPath;

        // 삭제할 데이터 파일 목록
        string[] dataFiles = new string[]
        {
            Path.Combine(dataPath, "inventoryData.json"),
            Path.Combine(dataPath, "currencyData.json"),
            Path.Combine(dataPath, "todoList.json"),
            Path.Combine(dataPath, "placeables.json")
        };

        // 각 파일 삭제
        foreach (string filePath in dataFiles)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                DebugEx.Log($"파일 삭제됨: {filePath}");
            }
            else
            {
                DebugEx.Log($"파일을 찾을 수 없음: {filePath}");
            }
        }

        // PlayerPrefs 데이터 초기화
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        DebugEx.Log("모든 PlayerPrefs 데이터가 초기화되었습니다.");

        DebugEx.Log("모든 매니저의 데이터가 초기화되었습니다.");

        // 매니저들이 데이터를 다시 로드하도록 합니다.
        await LoadAllManagers();

        DebugEx.Log("모든 매니저의 데이터가 다시 로드되었습니다.");
    }

    private static void ResetInventoryManager()
    {
        if (PomodoroHills.InventoryManager.Instance != null)
        {
            PomodoroHills.InventoryManager.Instance.ClearCachedItems();
            DebugEx.Log("InventoryManager의 캐시된 아이템이 초기화되었습니다.");
        }
    }

    private static void ResetEconomyManager()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.ResetCurrency();
            DebugEx.Log("EconomyManager의 재화가 초기화되었습니다.");
        }
    }

    private static void ResetTodoManager()
    {
        if (TodoSystem.TodoManager.Instance != null)
        {
            TodoSystem.TodoManager.Instance.ClearTodoList();
            DebugEx.Log("TodoManager의 TodoList가 초기화되었습니다.");
        }
    }

    private static void ResetPlaceableManager()
    {
        if (PlaceableManager.Instance != null)
        {
            PlaceableManager.Instance.ClearPlaceables();
            DebugEx.Log("PlaceableManager의 배치된 오브젝트가 초기화되었습니다.");
        }
    }

    private static async UniTask LoadAllManagers()
    {
        // 각 매니저의 데이터를 로드하여, 데이터가 없을 경우 샘플 데이터를 생성하도록 합니다.
        if (PomodoroHills.InventoryManager.Instance != null)
        {
            await PomodoroHills.InventoryManager.Instance.LoadItemsAsync();
            DebugEx.Log("InventoryManager의 데이터가 로드되었습니다.");
        }

        if (EconomyManager.Instance != null)
        {
            await EconomyManager.Instance.LoadCurrencyAsync();
            DebugEx.Log("EconomyManager의 데이터가 로드되었습니다.");
        }

        if (TodoSystem.TodoManager.Instance != null)
        {
            TodoSystem.TodoManager.Instance.LoadTodoList();
            DebugEx.Log("TodoManager의 데이터가 로드되었습니다.");
        }

        if (PlaceableManager.Instance != null)
        {
            PlaceableManager.Instance.LoadPlaceables();
            DebugEx.Log("PlaceableManager의 데이터가 로드되었습니다.");
        }
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
