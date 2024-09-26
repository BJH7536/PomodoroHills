using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DataManagement
{
    /// <summary>
    /// 아이템 데이터를 로컬 저장소에 저장하고 불러오는 기능을 제공하는 클래스입니다.
    /// 싱글톤 패턴을 사용하여 전역에서 접근 가능합니다.
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        /// <summary>
        /// DataManager의 단일 인스턴스를 유지하기 위한 변수입니다.
        /// </summary>
        private static DataManager instance;

        /// <summary>
        /// DataManager의 싱글톤 인스턴스에 접근하기 위한 프로퍼티입니다.
        /// </summary>
        public static DataManager Instance => instance;

        // 인벤토리 데이터 저장 경로
        private string _dataPath_Inventory;
        
        // 캐싱된 아이템 리스트
        private List<Item> cachedItems = new List<Item>();

        // 인벤토리에서 사용할 이벤트 선언 (아이템이 추가될 때, 아이템이 삭제될 때)
        public event Action<Item> OnItemAdded;
        public event Action<string> OnItemDeleted;
        
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
        private async Task SaveItemsAsync()
        {
            string json = JsonUtility.ToJson(new ItemListWrapper { items = cachedItems }, true);
            using (StreamWriter writer = new StreamWriter(_dataPath_Inventory, false))
            {
                await writer.WriteAsync(json);
            }
        }

        /// <summary>
        /// 로컬 저장소에서 아이템 데이터를 비동기적으로 불러옵니다.
        /// </summary>
        private async Task LoadItemsAsync()
        {
            if (!File.Exists(_dataPath_Inventory))
            {
                cachedItems = new List<Item>();
                return;
            }

            using (StreamReader reader = new StreamReader(_dataPath_Inventory))
            {
                string json = await reader.ReadToEndAsync();
                ItemListWrapper wrapper = JsonUtility.FromJson<ItemListWrapper>(json);
                if (wrapper != null && wrapper.items != null)
                {
                    cachedItems = wrapper.items;
                }
                else
                {
                    cachedItems = new List<Item>();
                }
            }
        }

        /// <summary>
        /// 현재 캐싱된 아이템 리스트를 반환합니다.
        /// </summary>
        public List<Item> GetItems()
        {
            return cachedItems;
        }
        
        /// <summary>
        /// 새로운 아이템을 추가합니다.
        /// </summary>
        /// <param name="newItem">추가할 아이템</param>
        public async Task AddItemAsync(Item newItem)
        {
            if (newItem == null)
            {
                Debug.LogWarning("AddItem 호출 시 null 아이템이 전달되었습니다.");
                return;
            }

            cachedItems.Add(newItem);
    
            // 아이템 추가 후 정렬 (예: 이름 기준 오름차순)
            cachedItems.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            try
            {
                await SaveItemsAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"AddItemAsync: 아이템 저장 중 오류 발생: {ex.Message}");
                return;
            }

            // 이벤트 발생
            OnItemAdded?.Invoke(newItem);
    
            Debug.Log($"Added Item: {newItem.Name}, Type: {newItem.Type}, Quantity: {newItem.Quantity}");
        }

        /// <summary>
        /// 특정 아이템을 삭제합니다.
        /// </summary>
        /// <param name="itemID">삭제할 아이템의 ID</param>
        public async Task DeleteItemAsync(string itemID)
        {
            Item itemToRemove = cachedItems.Find(item => item.ItemID == itemID);
            if (itemToRemove != null)
            {
                cachedItems.Remove(itemToRemove);
                
                try
                {
                    await SaveItemsAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"DeleteItemAsync: 아이템 저장 중 오류 발생: {ex.Message}");
                    return;
                }

                // 이벤트 발생
                OnItemDeleted?.Invoke(itemID);
                Debug.Log($"Deleted Item: {itemToRemove.Name}, Type: {itemToRemove.Type}, Quantity: {itemToRemove.Quantity}");
            }
            else
            {
                Debug.LogWarning($"DeleteItemAsync: 해당 ID의 아이템을 찾을 수 없습니다: {itemID}");
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
            public List<Item> items;
        }
    }
}