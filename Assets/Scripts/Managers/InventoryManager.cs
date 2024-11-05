using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PomodoroHills
{
    /// <summary>
    /// 아이템 데이터를 로컬 저장소에 저장하고 불러오는 기능을 제공하는 클래스입니다.
    /// 싱글톤 패턴을 사용하여 전역에서 접근 가능합니다.
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        /// <summary>
        /// DataManager의 단일 인스턴스를 유지하기 위한 변수입니다.
        /// </summary>
        private static InventoryManager instance;

        /// <summary>
        /// DataManager의 싱글톤 인스턴스에 접근하기 위한 프로퍼티입니다.
        /// </summary>
        public static InventoryManager Instance => instance;

        // 인벤토리 데이터 저장 경로
        private string _dataPath_Inventory;
        
        // 캐싱된 아이템 리스트
        private List<ItemData> cachedItems = new List<ItemData>();

        // 인벤토리에서 사용할 이벤트 선언 (아이템이 추가될 때, 아이템이 삭제될 때)
        public event Action<ItemData> OnItemAdded;
        public event Action<int, int> OnItemDeleted;
        
        /// <summary>
        /// 게임 오브젝트가 활성화될 때 호출됩니다.
        /// 싱글톤 인스턴스를 설정하고, 데이터를 로드합니다.
        /// </summary>
        private async void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject); // 씬 전환 시 오브젝트가 파괴되지 않도록 설정
            }
            else
            {
                Destroy(gameObject); // 이미 인스턴스가 존재하면 중복 오브젝트를 파괴
                return;
            }
            
            // 인벤토리 데이터 저장 경로
            _dataPath_Inventory = Path.Combine(Application.persistentDataPath, "inventoryData.json");
            
            // 데이터 로드
            await LoadItemsAsync();
        }

        /// <summary>
        /// 아이템 데이터를 로컬 저장소에 비동기적으로 저장합니다.
        /// </summary>
        /// <param name="items">저장할 아이템 리스트</param>
        private async UniTask SaveItemsAsync()
        {
            try
            {
                ItemListWrapper wrapper = new ItemListWrapper { items = cachedItems };
                string jsonData = JsonUtility.ToJson(wrapper, true);
                await File.WriteAllTextAsync(_dataPath_Inventory, jsonData);
                DebugEx.Log("Inventory data saved successfully.");
            }
            catch (Exception ex)
            {
                DebugEx.LogWarning($"Failed to save inventory data: {ex.Message}");
            }
        }

        /// <summary>
        /// 로컬 저장소에서 아이템 데이터를 비동기적으로 불러옵니다.
        /// </summary>
        private async UniTask LoadItemsAsync()
        {
            if (File.Exists(_dataPath_Inventory))
            {
                try
                {
                    string jsonData = await File.ReadAllTextAsync(_dataPath_Inventory);
                    ItemListWrapper wrapper = JsonUtility.FromJson<ItemListWrapper>(jsonData);
                    cachedItems = wrapper?.items ?? new List<ItemData>();
                    DebugEx.Log("Inventory data loaded successfully.");
                }
                catch (Exception ex)
                {
                    DebugEx.LogWarning($"Failed to load inventory data: {ex.Message}");
                    cachedItems = new List<ItemData>(); // 기본적으로 빈 리스트 생성
                }
            }
            else
            {
                // 샘플 데이터 생성
                int[] sampleIds = { 0, 1, 2, 3, 100, 101, 102, 103, 200, 201, 202, 203, 300, 301, 302, 303 };
                var random = new System.Random();

                foreach (int id in sampleIds)
                {
                    cachedItems.Add(new ItemData
                    {
                        id = id,
                        amount = random.Next(1, 10) // 1에서 9사이의 무작위 수량
                    });
                }

                // id 기준으로 오름차순 정렬
                cachedItems.Sort((item1, item2) => item1.id.CompareTo(item2.id));

                DebugEx.Log("No inventory data found. Created sample inventory data.");

                // 샘플 데이터 저장
                await SaveItemsAsync();
            }
        }

        /// <summary>
        /// 현재 캐싱된 아이템 리스트를 반환합니다.
        /// </summary>
        public List<ItemData> GetItems()
        {
            return cachedItems;
        }
        
        /// <summary>
        /// 새로운 아이템을 추가.
        /// 동일한 id의 아이템이 이미 있는 경우 해당 아이템의 수량을 증가.
        /// </summary>
        /// <param name="newItem">추가할 아이템</param>
        public async UniTask AddItemAsync(ItemData newItem)
        {
            var existingItem = cachedItems.Find(item => item.id == newItem.id);
            if (existingItem != null)
            {
                existingItem.amount += newItem.amount;
                DebugEx.Log($"Amount of {existingItem.id} increased by {newItem.amount}. New amount: {existingItem.amount}");
            }
            else
            {
                cachedItems.Add(newItem);
                
                DebugEx.Log($"{newItem.id} added to inventory.");
            }

            OnItemAdded?.Invoke(newItem);
            // id 기준으로 오름차순 정렬
            cachedItems.Sort((item1, item2) => item1.id.CompareTo(item2.id));

            await SaveItemsAsync();
        }

        /// <summary>
        /// 특정 아이템을 삭제합니다.
        /// 아이템의 id와 갯수를 입력받아 해당 아이템의 갯수를 줄입니다.
        /// </summary>
        /// <param name="itemId">삭제할 아이템의 ID</param>
        /// <param name="amountToRemove">제거할 갯수</param>
        public async UniTask DeleteItemAsync(int itemId, int amountToRemove)
        {
            var existingItem = cachedItems.Find(item => item.id == itemId);
            if (existingItem != null)
            {
                // if (existingItem.amount > amountToRemove)
                // {
                //     existingItem.amount -= amountToRemove;
                //     DebugEx.Log($"{amountToRemove} of {existingItem.id} removed. Remaining amount: {existingItem.amount}");
                // }
                // else
                // {
                //     cachedItems.Remove(existingItem);
                //     DebugEx.Log($"{existingItem.id} removed from inventory.");
                // }

                OnItemDeleted?.Invoke(itemId, amountToRemove);

                // id 기준으로 오름차순 정렬
                //cachedItems.Sort((item1, item2) => item1.id.CompareTo(item2.id));

                await SaveItemsAsync();
            }
            else
            {
                DebugEx.LogWarning($"Failed to delete item: Item with ID {itemId} not found in inventory.");
            }
        }
        
        private async void OnApplicationQuit()
        {
            await SaveItemsAsync();
        }
        
        /// <summary>
        /// 아이템 리스트를 감싸는 클래스입니다. JSON 직렬화를 위해 사용됩니다.
        /// </summary>
        [Serializable]
        private class ItemListWrapper
        {
            public List<ItemData> items;
        }
    }
}