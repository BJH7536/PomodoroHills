using System;
using System.Collections.Generic;
using PomodoroHills;
using UnityEngine;

[CreateAssetMenu(fileName = "StoreSellingTable", menuName = "PomodoroHills/StoreSellingTable")]
public class StoreSellingTable : ScriptableObject
{
    [SerializeField] private List<StoreSellingItem> _storeSellingItems = new List<StoreSellingItem>();
    
    private Dictionary<int, StoreSellingItem> _storeSellingItemDictionary = new Dictionary<int, StoreSellingItem>();

    private void OnEnable()
    {
        // ScriptableObject�� �ε�� �� Dictionary �ʱ�ȭ
        InitializeDictionary();
    }
    
    private void InitializeDictionary()
    {
        _storeSellingItemDictionary = new Dictionary<int, StoreSellingItem>();
        foreach (var storeItem in _storeSellingItems)
        {
            if (!_storeSellingItemDictionary.TryAdd(storeItem.id, storeItem))
            {
                DebugEx.LogWarning($"�ߺ��� ���� ������ id: {storeItem.id} / id�� �����ؾ� �մϴ�.");
            }
        }
    }

    /// <summary>
    /// ������ ������ �� ���� ������ ã�� ���
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public int GetSellingPriceById(int id)
    {
        if (_storeSellingItemDictionary == null)
        {
            InitializeDictionary();  // ���� Dictionary�� null�̶�� �ٽ� �ʱ�ȭ
        }

        if (_storeSellingItemDictionary != null && _storeSellingItemDictionary.TryGetValue(id, out var storeSellingItem))
        {
            return storeSellingItem.price;
        }
        else
        {
            DebugEx.LogWarning($"id�� {id}�� StoreItem �� ã�� �� �����ϴ�.");
            return 0;
        }
    }
}
