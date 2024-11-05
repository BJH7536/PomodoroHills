using Cysharp.Threading.Tasks;
using PomodoroHills;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorePopup_StoreItem : MonoBehaviour
{
    [SerializeField] private ItemTable ItemTable;
    
    [SerializeField] private int id;

    [SerializeField] private Image image;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;

    [SerializeField] private Button button;

    private void OnEnable()
    {
        SetUp();
    }
    
    void SetUp()
    {
        ItemTableElement itemTableElement = ItemTable.GetItemInformById(id);
        
        int price = StoreManager.Instance.StoreTable.GetPriceById(id);
        
        image.sprite = itemTableElement.image;
        nameText.text = itemTableElement.name;
        priceText.text = $"{price:N0}";
        
        button.onClick.RemoveAllAndAddListener(() =>
        {
            PopupManager.Instance.ShowConfirmPopup("아이템 구매", $"{itemTableElement.name}을 구매하시겠습니까?", async () =>
            {
                bool isPurchaseSuccessful = await StoreManager.Instance.TryToBuyStoreItem(id);

                if (isPurchaseSuccessful)
                {
                    // 구매 성공 팝업 표시
                    PopupManager.Instance.ShowAlertPopup("구매 성공", $"{itemTableElement.name}을 성공적으로 구매했습니다.");
                }
                else
                {
                    // 구매 실패 팝업 표시
                    PopupManager.Instance.ShowAlertPopup("구매 실패", $"{itemTableElement.name} 구매에 실패했습니다. 충분한 재화가 없습니다.");
                    
                }
            });
        });
    }
}
