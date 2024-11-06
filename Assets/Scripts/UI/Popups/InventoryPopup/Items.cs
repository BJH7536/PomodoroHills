using System;
using UnityEngine;

namespace PomodoroHills
{
    public enum ItemType
    {
        Building,       // 건물
        Decoration,     // 장식물
        Seed,           // 씨앗
        Crop,           // 작물
    }
    
    [Serializable]
    public class ItemData           // 인벤토리의 아이템이 저장되는 형태
    {
        [SerializeField] public int id;                  // 아이템 고유 ID
        [SerializeField] public int amount;            // 아이템 수량
    }
    
    [Serializable]
    public class StoreItem          // 상점에서 파는 아이템의 항목
    {
        [SerializeField] public int id;
        [SerializeField] public int price;
    }
    
    [Serializable]
    public class StoreSellingItem   // 아이템을 상점에 팔 때의 가격
    {
        [SerializeField] public int id;
        [SerializeField] public int price;
    }
    
    [Serializable]
    public class ItemTableElement   // 아이템의 전체 정보를 저장하는 형태
    {
        [SerializeField] public int id;
        [SerializeField] public string name;
        [SerializeField] public Sprite image;
        [SerializeField] public Sprite image_noBackground;
        [SerializeField] public ItemType type;
    }
}

