using System.Collections.Generic;
using PomodoroHills;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemTable", menuName = "PomodoroHills/ItemTable")]
public class ItemTable : ScriptableObject
{
    [SerializeField] private List<ItemTableElement> _items = new List<ItemTableElement>();
    
    private Dictionary<int, ItemTableElement> _itemDictionary = new Dictionary<int, ItemTableElement>();

    private void OnEnable()
    {
        // ScriptableObject가 로드될 때 Dictionary 초기화
        InitializeDictionary();
    }
    
    private void InitializeDictionary()
    {
        _itemDictionary = new Dictionary<int, ItemTableElement>();
        foreach (var item in _items)
        {
            if (!_itemDictionary.TryAdd(item.id, item))
            {
                DebugEx.LogWarning($"중복된 상점 아이템 id: {item.id} / id는 고유해야 합니다.");
            }
        }
    }
    
    public ItemTableElement GetItemInformById(int id)
    {
        if (_itemDictionary == null)
        {
            InitializeDictionary();  // 만약 Dictionary가 null이라면 다시 초기화
        }

        if (_itemDictionary != null && _itemDictionary.TryGetValue(id, out var item))
        {
            return item;
        }
        else
        {
            DebugEx.LogWarning($"id가 {id}인 ItemTableElement 을 찾을 수 없습니다.");
            return null;
        }
    }
}
