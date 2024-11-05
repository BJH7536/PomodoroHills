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
            PopupManager.Instance.ShowConfirmPopup("������ ����", $"{itemTableElement.name}�� �����Ͻðڽ��ϱ�?", async () =>
            {
                bool isPurchaseSuccessful = await StoreManager.Instance.TryToBuyStoreItem(id);

                if (isPurchaseSuccessful)
                {
                    // ���� ���� �˾� ǥ��
                    PopupManager.Instance.ShowAlertPopup("���� ����", $"{itemTableElement.name}�� ���������� �����߽��ϴ�.");
                }
                else
                {
                    // ���� ���� �˾� ǥ��
                    PopupManager.Instance.ShowAlertPopup("���� ����", $"{itemTableElement.name} ���ſ� �����߽��ϴ�. ����� ��ȭ�� �����ϴ�.");
                    
                }
            });
        });
    }
}
