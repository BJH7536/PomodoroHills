using System;

namespace DataManagement
{
    [Serializable]
    public class Item
    {
        public string ItemID;         // 아이템 고유 ID
        public string Name;           // 아이템 이름
        public ItemType Type;         // 아이템 종류
        public int Quantity;          // 아이템 수량
        // 필요에 따라 추가적인 속성들 (예: 설명, 이미지 경로 등)
    }

    public enum ItemType
    {
        Building,       // 건물
        Decoration,     // 장식물
        Seed,           // 씨앗
        Crop,           // 작물
    }
}

