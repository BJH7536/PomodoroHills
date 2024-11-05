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
        // ScriptableObject가 로드될 때 Dictionary 초기화
        InitializeDictionary();
    }
    
    private void InitializeDictionary()
    {
        _storeItemDictionary = new Dictionary<int, StoreItem>();
        foreach (var storeItem in _storeItems)
        {
            if (!_storeItemDictionary.TryAdd(storeItem.id, storeItem))
            {
                DebugEx.LogWarning($"중복된 상점 아이템 id: {storeItem.id} / id는 고유해야 합니다.");
            }
        }
    }

    public int GetPriceById(int id)
    {
        if (_storeItemDictionary == null)
        {
            InitializeDictionary();  // 만약 Dictionary가 null이라면 다시 초기화
        }

        if (_storeItemDictionary != null && _storeItemDictionary.TryGetValue(id, out var storeItem))
        {
            return storeItem.price;
        }
        else
        {
            DebugEx.LogWarning($"id가 {id}인 StoreItem 을 찾을 수 없습니다.");
            return 0;
        }
    }
}