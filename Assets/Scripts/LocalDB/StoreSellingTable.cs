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
        // ScriptableObject가 로드될 때 Dictionary 초기화
        InitializeDictionary();
    }
    
    private void InitializeDictionary()
    {
        _storeSellingItemDictionary = new Dictionary<int, StoreSellingItem>();
        foreach (var storeItem in _storeSellingItems)
        {
            if (!_storeSellingItemDictionary.TryAdd(storeItem.id, storeItem))
            {
                DebugEx.LogWarning($"중복된 상점 아이템 id: {storeItem.id} / id는 고유해야 합니다.");
            }
        }
    }

    /// <summary>
    /// 물건을 상점에 팔 때의 가격을 찾는 기능
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public int GetSellingPriceById(int id)
    {
        if (_storeSellingItemDictionary == null)
        {
            InitializeDictionary();  // 만약 Dictionary가 null이라면 다시 초기화
        }

        if (_storeSellingItemDictionary != null && _storeSellingItemDictionary.TryGetValue(id, out var storeSellingItem))
        {
            return storeSellingItem.price;
        }
        else
        {
            DebugEx.LogWarning($"id가 {id}인 StoreItem 을 찾을 수 없습니다.");
            return 0;
        }
    }
}
