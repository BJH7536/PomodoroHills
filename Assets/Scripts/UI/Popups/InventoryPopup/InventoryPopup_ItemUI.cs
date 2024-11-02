using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DataManagement
{
    /// <summary>
    /// 개별 아이템의 UI를 관리하는 클래스입니다.
    /// </summary>
    public class InventoryPopup_ItemUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text itemNameText;     // 아이템 이름 텍스트
        [SerializeField] private TMP_Text itemQuantityText; // 아이템 수량 텍스트
        [SerializeField] private Image itemIconImage;       // 아이템 아이콘 이미지

        private Item _item;
        
        // 아이템 고유 ID를 저장하기 위한 필드
        public string itemID { get; private set; }

        /// <summary>
        /// 아이템 데이터를 기반으로 UI를 설정합니다.
        /// </summary>
        /// <param name="item">설정할 아이템 데이터</param>
        public void Setup(Item item)
        {
            if (item == null)
            {
                DebugEx.LogWarning("InventoryPopup_ItemUI Setup 호출 시 null 아이템이 전달되었습니다.");
                return;
            }

            // 아이템 ID 설정
            itemID = item.ItemID;

            _item = item;
            
            if (itemNameText != null)
                itemNameText.text = item.Name;

            if (itemQuantityText != null)
                itemQuantityText.text = $"x{item.Quantity}";

            // 아이템 아이콘 설정 (예: Resources에서 이미지 로드)
            if (itemIconImage != null)
            {
                // 예시: 아이템 타입에 따라 다른 이미지를 로드
                string iconPath = $"Sprites/Icons/{item.Type.ToString()}";
                Sprite iconSprite = Resources.Load<Sprite>(iconPath);
                if (iconSprite != null)
                    itemIconImage.sprite = iconSprite;
                else
                    DebugEx.LogWarning($"아이템 아이콘을 찾을 수 없습니다: {iconPath}");
            }
        }

        /// <summary>
        /// UI를 초기화하여 풀에 반환되기 전에 상태를 리셋합니다.
        /// </summary>
        public void ResetUI()
        {
            itemID = string.Empty;

            if (itemNameText != null)
                itemNameText.text = string.Empty;

            if (itemQuantityText != null)
                itemQuantityText.text = string.Empty;

            if (itemIconImage != null)
                itemIconImage.sprite = null;
        }

        public ItemType GetItemType()
        {
            return _item.Type;
        }
    }
}
