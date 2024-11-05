using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PomodoroHills;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    private static StoreManager instance;
    
    public static StoreManager Instance => instance;

    public ItemTable ItemTable;
    public StoreTable StoreTable;
    public StoreSellingTable StoreSellingTable;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 오브젝트가 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 존재하면 중복 오브젝트를 파괴
            return;
        }
    }

    public async UniTask<bool> TryToBuyStoreItem(int id)
    {
        int price = StoreTable.GetPriceById(id);
        string name = ItemTable.GetItemInformById(id).name;
        
        // 코인 사용 시도
        bool isPurchaseSuccessful = await EconomyManager.Instance.SpendCoinAsync(price);

        if (isPurchaseSuccessful)
        {
            // 구매 성공 시 로직
            DebugEx.Log($"{name} 구매에 성공함 !");
            
            ItemData newItemData = new ItemData()
            {
                id = id,
                amount = 1,
            };
            
            PomodoroHills.InventoryManager.Instance.AddItemAsync(newItemData).Forget();
            
            return true;
        }
        else
        {
            // 구매 실패 시 로직
            DebugEx.Log($"{name} 구매에 실패함 ㅠㅠ");
            
            return false;
        }
    }

    public async UniTask<bool> SellItem(int id, int amount)
    {
        // TODO 아이템 파는 로직 개발하기
        if (amount <= 0)
        {
            DebugEx.LogWarning("SellItem: 판매 수량은 0보다 커야 합니다.");
            return false;
        }

        // 아이템 정보 가져오기
        ItemData itemToSell = PomodoroHills.InventoryManager.Instance.GetItems().Find(item => item.id == id);
        if (itemToSell == null)
        {
            DebugEx.LogWarning($"SellItem: ID {id}에 해당하는 아이템이 인벤토리에 없습니다.");
            PopupManager.Instance.ShowAlertPopup("판매 실패", "판매하려는 아이템이 인벤토리에 없습니다.");
            return false;
        }

        if (itemToSell.amount < amount)
        {
            DebugEx.LogWarning($"SellItem: 판매하려는 수량이 보유한 수량보다 많습니다. 현재 수량: {itemToSell.amount}, 판매 시도 수량: {amount}");
            PopupManager.Instance.ShowAlertPopup("판매 실패", "보유한 아이템 수량이 부족합니다.");
            return false;
        }

        // 판매 가격 계산 (StoreSellingTable을 통해 아이템의 판매 가격을 가져온다고 가정)
        int pricePerItem = StoreSellingTable.GetSellingPriceById(id);
        int totalSaleAmount = pricePerItem * amount;

        // 인벤토리에서 아이템 삭제
        await PomodoroHills.InventoryManager.Instance.DeleteItemAsync(id, amount);
        // 판매 수익 추가
        await EconomyManager.Instance.AddGemAsync(totalSaleAmount);
        
        DebugEx.Log($"{itemToSell.id} 을(를) {amount}개 판매하여 {totalSaleAmount} 젬을 얻었습니다.");

        // 판매 성공 팝업 표시
        PopupManager.Instance.ShowAlertPopup("판매 성공", $"{ItemTable.GetItemInformById(itemToSell.id).name} 을(를) {amount}개를 판매하여\n\n{totalSaleAmount} 젬을 얻었습니다.");

        return true;
        
        
        //PomodoroHills.InventoryManager.Instance.DeleteItemAsync(item.id, )
    }
}
