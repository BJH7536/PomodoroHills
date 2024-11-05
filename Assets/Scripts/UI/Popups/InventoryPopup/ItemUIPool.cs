using System.Collections.Generic;
using UnityEngine;

namespace PomodoroHills
{
    public class ItemUIPool : MonoBehaviour
    {
        [SerializeField] private GameObject itemUIPrefab;   // InventoryPopup_ItemUI 프리팹 참조
        [SerializeField] private int initialPoolSize = 10;  // 초기 풀 크기

        public bool isInitialized { get; private set; } = false;  // 초기화 여부 확인용 플래그

        private Dictionary<ItemType, List<GameObject>> itemUIPools = new Dictionary<ItemType, List<GameObject>>();

        private void Awake()
        {
            InitializePools();
        }

        /// <summary>
        /// 각 ItemType별로 객체 풀을 초기화합니다.
        /// </summary>
        private void InitializePools()
        {
            foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
            {
                List<GameObject> pool = new List<GameObject>();

                for (int i = 0; i < initialPoolSize; i++)
                {
                    GameObject itemUI = Instantiate(itemUIPrefab, transform);
                    itemUI.SetActive(false);
                    pool.Add(itemUI);
                }

                DebugEx.Log($"ItemUIPool: Initializing pool for {type}");
                itemUIPools.Add(type, pool);  // 각 ItemType에 대한 풀 등록
            }

            isInitialized = true;  // 초기화가 완료되면 플래그를 true로 설정
        }

        /// <summary>
        /// 특정 ItemType의 풀에서 InventoryPopup_ItemUI 인스턴스를 가져옵니다.
        /// </summary>
        public GameObject GetItemUI(ItemType type)
        {
            if (!itemUIPools.ContainsKey(type))
            {
                DebugEx.LogError($"ItemUIPool: 풀을 찾을 수 없습니다: {type}");
                return null;
            }

            // 해당 타입의 풀 가져오기
            List<GameObject> pool = itemUIPools[type];

            // 활성화된 UI 찾기
            foreach (var itemUI in pool)
            {
                if (!itemUI.activeInHierarchy)
                {
                    itemUI.SetActive(true); // 비활성화된 아이템을 활성화
                    return itemUI;
                }
            }

            // 풀에 더 이상 남은 UI가 없는 경우 새로 생성
            DebugEx.LogWarning($"ItemUIPool: {type} 타입의 풀에 아이템이 부족합니다. 새로 생성합니다.");
            ExpandPool(type);  // 풀 사이즈 확장
            return GetItemUI(type);  // 다시 시도하여 새로운 UI 반환
        }

        /// <summary>
        /// 풀 크기를 증가시킵니다.
        /// </summary>
        private void ExpandPool(ItemType type)
        {
            if (!itemUIPools.ContainsKey(type))
            {
                DebugEx.LogError($"ItemUIPool: 확장할 풀을 찾을 수 없습니다: {type}");
                return;
            }

            // 풀 사이즈 확장
            for (int i = 0; i < 10; i++)  // 풀 사이즈를 10개씩 늘림
            {
                GameObject itemUI = Instantiate(itemUIPrefab, transform);
                itemUI.SetActive(false);
                itemUIPools[type].Add(itemUI);
            }

            DebugEx.Log($"ItemUIPool: {type} 타입의 풀 크기를 10만큼 확장했습니다.");
        }

        /// <summary>
        /// 사용한 InventoryPopup_ItemUI 인스턴스를 특정 ItemType의 풀에 반환합니다.
        /// </summary>
        public void ReturnItemUI(GameObject itemUI, ItemType type)
        {
            if (!itemUIPools.ContainsKey(type))
            {
                DebugEx.LogError($"ItemUIPool: 반환할 풀을 찾을 수 없습니다: {type}");
                return;
            }

            itemUI.SetActive(false);
            itemUIPools[type].Add(itemUI);
        }
    }
}
