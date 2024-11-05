using System;
using System.Collections.Generic;
using PomodoroHills;
using UnityEngine;


[CreateAssetMenu(fileName = "StoreTable", menuName = "PomodoroHills/StoreTable")]
public class StoreTable : ScriptableObject
{
    [SerializeField] private List<StoreItem> _storeItems = new List<StoreItem>();
    
    private Dictionary<int, StoreItem> _storeItemDictionary = new Dictionary<int, StoreItem>();

    private void OnEnable()
    {
        // ScriptableObject�� �ε�� �� Dictionary �ʱ�ȭ
        InitializeDictionary();
    }
    
    private void InitializeDictionary()
    {
        _storeItemDictionary = new Dictionary<int, StoreItem>();
        foreach (var storeItem in _storeItems)
        {
            if (!_storeItemDictionary.TryAdd(storeItem.id, storeItem))
            {
                DebugEx.LogWarning($"�ߺ��� ���� ������ id: {storeItem.id} / id�� �����ؾ� �մϴ�.");
            }
        }
    }

    public int GetPriceById(int id)
    {
        if (_storeItemDictionary == null)
        {
            InitializeDictionary();  // ���� Dictionary�� null�̶�� �ٽ� �ʱ�ȭ
        }

        if (_storeItemDictionary != null && _storeItemDictionary.TryGetValue(id, out var storeItem))
        {
            return storeItem.price;
        }
        else
        {
            DebugEx.LogWarning($"id�� {id}�� StoreItem �� ã�� �� �����ϴ�.");
            return 0;
        }
    }
}