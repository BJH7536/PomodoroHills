using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PomodoroHills
{
    /// <summary>
    /// 개별 아이템의 UI를 관리하는 클래스입니다.
    /// </summary>
    public class InventoryPopup_ItemUI : MonoBehaviour
    {
        [SerializeField] private ItemTable ItemTable;
        
        [SerializeField] private TMP_Text itemNameText;     // 아이템 이름 텍스트
        [SerializeField] private TMP_Text itemQuantityText; // 아이템 수량 텍스트
        [SerializeField] private Image itemIconImage;       // 아이템 아이콘 이미지

        [SerializeField] private LongPressDetector LongPressDetector;
        
        [SerializeField] public ItemData Item;
        
        // 아이템 고유 ID를 저장하기 위한 필드
        public int itemID { get; private set; }

        /// <summary>
        /// 아이템 데이터를 기반으로 UI를 설정합니다.
        /// </summary>
        /// <param name="item">설정할 아이템 데이터</param>
        public void Setup(ItemData item)
        {
            if (item == null)
            {
                DebugEx.LogWarning("InventoryPopup_ItemUI Setup 호출 시 null 아이템이 전달되었습니다.");
                return;
            }

            // 아이템 ID 설정
            itemID = item.id;

            Item = item;

            var itemTableElement = ItemTable.GetItemInformById(itemID);
            
            
            if (itemNameText != null)
                itemNameText.text = itemTableElement.name;

            if (itemQuantityText != null)
                itemQuantityText.text = $"{Item.amount}";

            itemIconImage.sprite = itemTableElement.image;

            if (itemTableElement.type == ItemType.Crop)
            {
                LongPressDetector.onLongPress.RemoveAllAndAddListener(() =>
                {
                    InventoryPopup_ChooseAmountForItemSell chooseAmount = PopupManager.Instance.ShowPopup<InventoryPopup_ChooseAmountForItemSell>();
                    chooseAmount.SetUp(item.id, Item.amount);
                });
            }
        }

        /// <summary>
        /// UI를 초기화하여 풀에 반환되기 전에 상태를 리셋합니다.
        /// </summary>
        public void ResetUI()
        {
            itemID = 0;

            if (itemNameText != null)
                itemNameText.text = string.Empty;

            if (itemQuantityText != null)
                itemQuantityText.text = string.Empty;

            if (itemIconImage != null)
                itemIconImage.sprite = null;
        }

        public void RefreshUI()
        {
            Setup(Item);
        }
        
        public ItemType GetItemType()
        {
            return (ItemType)(itemID / 100);
        }
    }
}
