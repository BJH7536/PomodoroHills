using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using TodoSystem;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class TodoListPopup : Popup
{
    [Tab("TodoList")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TodoItemUIPool todoItemUIPool; // TodoItemUIPool 참조
    
    [Tab("Tweening")]
    [SerializeField] private float originScale = 0.7f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private Ease Ease = Ease.OutBack;
    
    [Tab("Other")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private string TitleBase = "Todo List";
    [SerializeField] private GameObject panel;
    [SerializeField] public CanvasGroup canvasGroup; // 투명도를 조정할 UI 그룹
    
    private List<GameObject> activeTodoItems = new List<GameObject>();
    
    private async void OnEnable()
    {
        PlayAnimation();

        titleText.text = $"{TitleBase} - {DateTime.Today:yyyy.MM.dd}";
        
        // TodoItemUIPool 초기화 여부 확인
        if (!todoItemUIPool.IsInitialized) await WaitForUIPoolInitialization();
        
        // 할 일 목록 표시
        DisplayTodoList();
        
        // TodoManager 이벤트 구독
        TodoManager.Instance.OnTodoItemAdded += OnTodoItemAdded;
    }
    
    private void OnDisable()
    {
        // 활성화된 TodoItemUI를 모두 풀에 반환
        ClearAllTodoItemsUI();
        
        // 이벤트 구독 해제
        TodoManager.Instance.OnTodoItemAdded -= OnTodoItemAdded;
        
        // TodoList 저장
        TodoManager.Instance.SaveCurrentTodoList();
    }
    
    private void PlayAnimation()
    {
        panel.transform.localScale = Vector3.one * originScale;
        panel.transform.DOScale(Vector3.one, duration).SetEase(Ease);
    }
    
    /// <summary>
    /// 할 일 목록을 표시합니다.
    /// </summary>
    private void DisplayTodoList()
    {
        List<TodoItem> todoList = TodoManager.Instance.FilterTodoItemsForToday(TodoManager.Instance.TodoList, DateTime.Today);
        
        if (todoList == null || todoList.Count == 0)
        {
            DebugEx.LogWarning("TodoListPopup: 표시할 할 일이 없습니다.");
            
            return;
        }

        // 기존 아이템 클리어
        ClearAllTodoItemsUI();

        foreach (var todo in todoList)
        {
            // 풀에서 객체 가져오기
            GameObject itemGO = todoItemUIPool.GetTodoItemUI();
            itemGO.transform.SetParent(scrollRect.content, false);

            // 데이터 설정
            TodoListPopup_TodoItemUI itemUI = itemGO.GetComponent<TodoListPopup_TodoItemUI>();
            if (itemUI != null)
            {
                itemUI.SetUp(todo, todoItemUIPool);
            }
            else
            {
                DebugEx.LogError("TodoListPopup: TodoListPopup_TodoItemUI 컴포넌트를 찾을 수 없습니다.");
            }

            // 활성화된 아이템 목록에 추가
            activeTodoItems.Add(itemGO);
        }

        // 레이아웃 갱신
        ForceRebuildLayout();
    }

    /// <summary>
    /// 활성화된 TodoItemUI를 모두 풀에 반환하고 리스트를 비웁니다.
    /// </summary>
    private void ClearAllTodoItemsUI()
    {
        foreach (var itemGO in activeTodoItems)
        {
            todoItemUIPool.ReturnTodoItemUI(itemGO);
        }
        activeTodoItems.Clear();
    }

    /// <summary>
    /// 레이아웃을 강제로 갱신합니다.
    /// </summary>
    private void ForceRebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
    }

    public void UpdateAllTodoItemsUI()
    {
        foreach (var go in activeTodoItems)
        {
            if (go.TryGetComponent<TodoListPopup_TodoItemUI>(out var ui))
            {
                ui.UpdateProgressUI();
            }
        }
    }
    
    /// <summary>
    /// TodoItemUIPool의 초기화를 대기합니다.
    /// </summary>
    private async UniTask WaitForUIPoolInitialization()
    {
        // 1초 이내로 풀 초기화 완료를 기다립니다.
        float timeout = 1f;
        float timeElapsed = 0f;

        while (!todoItemUIPool.IsInitialized && timeElapsed < timeout)
        {
            await UniTask.Yield();
            timeElapsed += Time.deltaTime;
        }

        if (!todoItemUIPool.IsInitialized)
        {
            DebugEx.LogError("TodoItemUIPool 초기화가 예상 시간 내에 완료되지 않았습니다.");
        }
    }
    
    // 새로운 TodoItem UI 생성 및 설정
    private void OnTodoItemAdded(TodoItem newTodo)
    {
        GameObject itemGO = todoItemUIPool.GetTodoItemUI();
        itemGO.transform.SetParent(scrollRect.content, false);

        TodoListPopup_TodoItemUI itemUI = itemGO.GetComponent<TodoListPopup_TodoItemUI>();
        if (itemUI != null)
        {
            itemUI.SetUp(newTodo, todoItemUIPool);
        }
        else
        {
            DebugEx.LogError("TodoListPopup: TodoListPopup_TodoItemUI 컴포넌트를 찾을 수 없습니다.");
        }

        activeTodoItems.Add(itemGO);

        // 레이아웃 갱신
        ForceRebuildLayout();
    }
    
    // 투명도를 1에서 0으로 서서히 줄이기
    public async UniTaskVoid FadeOutUI()
    {
        // 첫 번째 트윈: 투명도를 1에서 0으로 서서히 줄이기
        await canvasGroup.DOFade(0f, 1f).ToUniTask();
        canvasGroup.DOFade(1f, 1f).ToUniTask().Forget();
    }

    // 투명도를 0에서 1로 서서히 늘리기
    public void FadeInUI()
    {
        canvasGroup.DOFade(1f, 1f);  // 1초 동안 알파 값을 1로 변경
    }

    public void Close()
    {
        PopupManager.Instance.HidePopup();
    }
    
}
