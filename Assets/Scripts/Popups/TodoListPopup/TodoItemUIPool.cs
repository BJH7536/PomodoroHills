using System.Collections.Generic;
using UnityEngine;

public class TodoItemUIPool : MonoBehaviour
{
    [SerializeField] private GameObject todoItemUIPrefab; // TodoListPopup_TodoItemUI 프리팹 참조
    [SerializeField] private int initialPoolSize = 10;    // 초기 풀 크기
    [SerializeField] private Transform poolRoot;
    
    public bool IsInitialized { get; private set; } = false; // 초기화 여부 확인용 플래그

    private List<GameObject> todoItemUIPool = new List<GameObject>();

    private void Awake()
    {
        InitializePool();
    }

    /// <summary>
    /// TodoItem UI 풀을 초기화합니다.
    /// </summary>
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject itemUI = Instantiate(todoItemUIPrefab, poolRoot);
            itemUI.SetActive(false);
            todoItemUIPool.Add(itemUI);
        }

        IsInitialized = true; // 초기화 완료
    }

    /// <summary>
    /// 풀에서 TodoItem UI 인스턴스를 가져옵니다.
    /// </summary>
    public GameObject GetTodoItemUI()
    {
        // 비활성화된 UI 찾기
        foreach (var itemUI in todoItemUIPool)
        {
            if (!itemUI.activeInHierarchy)
            {
                itemUI.SetActive(true); // 비활성화된 아이템을 활성화
                return itemUI;
            }
        }

        // 풀에 더 이상 남은 UI가 없는 경우 새로 생성
        Debug.LogWarning("TodoItemUIPool: 풀에 아이템이 부족합니다. 새로 생성합니다.");
        ExpandPool(initialPoolSize); // 풀 사이즈를 초기 풀 사이즈만큼 늘림
        return GetTodoItemUI(); // 다시 시도하여 새로운 UI 반환
    }

    /// <summary>
    /// 풀 크기를 증가시킵니다.
    /// </summary>
    private void ExpandPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject itemUI = Instantiate(todoItemUIPrefab, poolRoot);
            itemUI.SetActive(false);
            todoItemUIPool.Add(itemUI);
        }

        Debug.Log($"TodoItemUIPool: 풀 크기를 {amount}만큼 확장했습니다.");
    }

    /// <summary>
    /// 사용한 TodoItem UI 인스턴스를 풀에 반환합니다.
    /// </summary>
    public void ReturnTodoItemUI(GameObject itemUI)
    {
        itemUI.SetActive(false);
        itemUI.transform.SetParent(poolRoot); // 풀의 자식으로 이동
    }
}
