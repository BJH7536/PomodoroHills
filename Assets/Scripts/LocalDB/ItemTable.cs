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
        // ScriptableObject�� �ε�� �� Dictionary �ʱ�ȭ
        InitializeDictionary();
    }
    
    private void InitializeDictionary()
    {
        _itemDictionary = new Dictionary<int, ItemTableElement>();
        foreach (var item in _items)
        {
            if (!_itemDictionary.TryAdd(item.id, item))
            {
                DebugEx.LogWarning($"�ߺ��� ���� ������ id: {item.id} / id�� �����ؾ� �մϴ�.");
            }
        }
    }
    
    public ItemTableElement GetItemInformById(int id)
    {
        if (_itemDictionary == null)
        {
            InitializeDictionary();  // ���� Dictionary�� null�̶�� �ٽ� �ʱ�ȭ
        }

        if (_itemDictionary != null && _itemDictionary.TryGetValue(id, out var item))
        {
            return item;
        }
        else
        {
            DebugEx.LogWarning($"id�� {id}�� ItemTableElement �� ã�� �� �����ϴ�.");
            return null;
        }
    }
}
