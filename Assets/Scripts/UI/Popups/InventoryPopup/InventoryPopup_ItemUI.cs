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
        [SerializeField] private ItemUIPool itemUIPool; // ItemUIPool 참조

        [SerializeField] private TMP_Text itemNameText;     // 아이템 이름 텍스트
        [SerializeField] private TMP_Text itemQuantityText; // 아이템 수량 텍스트
        [SerializeField] private Image itemIconImage;       // 아이템 아이콘 이미지

        [SerializeField] private Button button;    
        [SerializeField] private LongPressDetector LongPressDetector;
        
        [SerializeField] public ItemData Item;
        
        // 아이템 고유 ID를 저장하기 위한 필드
        public int itemID { get; private set; }

        /// <summary>
        /// 아이템 데이터를 기반으로 UI를 설정합니다.
        /// </summary>
        /// <param name="item">설정할 아이템 데이터</param>
        public void Setup(ItemData item, ItemUIPool pool)
        {
            if (item == null)
            {
                DebugEx.LogWarning("InventoryPopup_ItemUI Setup 호출 시 null 아이템이 전달되었습니다.");
                return;
            }

            // 아이템 ID 설정
            itemID = item.id;
            
            itemUIPool = pool;
            
            Item = item;

            var itemTableElement = ItemTable.GetItemInformById(item.id);
            
            if (itemNameText != null)
                itemNameText.text = itemTableElement.name;

            if (itemQuantityText != null)
                itemQuantityText.text = $"{Item.amount}";

            itemIconImage.sprite = itemTableElement.image;
            
            // TODO 건물이거나 장식품인 경우에만 짧은 터치로 건물 짓는 기능 연결하기
            if (itemTableElement.type is ItemType.Building or ItemType.Decoration)
            {
                button.onClick.RemoveAllAndAddListener(() =>
                {
                    PopupManager.Instance.HidePopup();
                    InteractionManager.Instance.UnpackPlaceable(item.id);
                });
            }
            
            // 작물인 경우에만 롱프레스로 팔기 기능 연계
            if (itemTableElement.type == ItemType.Crop)
            {
                LongPressDetector.onLongPress.RemoveAllAndAddListener(() =>
                {
                    InventoryPopup_ChooseAmountForItemSell chooseAmount = PopupManager.Instance.ShowPopup<InventoryPopup_ChooseAmountForItemSell>();
                    chooseAmount.SetUp(item.id, Item.amount);
                });
            }
        }
        
        public void RefreshUI()
        {
            if(Item.amount != 0)
                Setup(Item, itemUIPool);
            else
                itemUIPool.ReturnItemUI(gameObject, GetItemType());
        }
        
        public ItemType GetItemType()
        {
            return (ItemType)(itemID / 100);
        }
    }
}
