using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TodoList;

public class TodoManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static TodoManager instance;

    // 싱글톤 인스턴스에 접근하기 위한 프로퍼티
    public static TodoManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TodoManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("TodoManager");
                    instance = go.AddComponent<TodoManager>();
                }
            }
            return instance;
        }
    }
    
    // Todo 리스트
    public List<TodoItem> TodoList { get; private set; } = new List<TodoItem>();
    
    public event Action<TodoItem> OnTodoItemAdded;
    
    // TodoList 저장 경로
    private string _filePath;

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 오브젝트가 파괴되지 않도록 설정
        }
        else if (instance != this)
        {
            Destroy(gameObject); // 이미 인스턴스가 존재하면 중복 오브젝트를 파괴
            return;
        }

        // 파일 경로 설정 (예: Application.persistentDataPath는 영구 저장 경로)
        _filePath = Path.Combine(Application.persistentDataPath, "todoList.json");

        // Todo 리스트 로드
        LoadOrCreateSampleTodoItems();
        
        // 오늘의 할 일 필터링
        List<TodoItem> todaysTasks = FilterTodoItemsForToday(TodoList, DateTime.Today);
        
        Debug.Log("불러온 Todo 항목들:");         // 불러온 Todo 항목 출력
        foreach (var todo in TodoList)
        {
            Debug.Log(todo.ToString());
        }
        Debug.Log("오늘의 할 일:");              // 오늘의 Todo 항목 출력
        foreach (var todo in todaysTasks)
        {
            Debug.Log(todo.ToString());
        }
    }
    
    /// <summary>
    /// Todo 리스트를 로드합니다.
    /// </summary>
    private void LoadOrCreateSampleTodoItems()
    {
        // JSON 파일에서 불러오기
        List<TodoItem> loadedTodoList = LoadTodoListFromJson(_filePath);

        if (loadedTodoList.Count == 0)
        {
            Debug.LogWarning("파일이 비어있음. 샘플 데이터를 생성함");
            
            // 파일이 없거나 비어 있으면 샘플 데이터 생성
            TodoList = CreateSampleTodoItems();
            // JSON 파일로 저장
            SaveTodoListToJson(TodoList, _filePath);
        }
        else
        {
            TodoList = loadedTodoList;
        }
    }
    
    /// <summary>
    /// 샘플 Todo 항목들을 생성합니다.
    /// </summary>
    List<TodoItem> CreateSampleTodoItems()
    {
        TodoItem codingStudy = new TodoItem(
            name: "코딩 공부",
            description: "매일 90분씩 코딩 공부하기",
            startDate: new DateTime(2024, 9, 27),
            endDate: new DateTime(2024, 10, 3),
            type: ItemType.TimeBased,
            priority: 5,
            recurrence: Recurrence.Daily,
            status: Status.Pending,
            durationInMinutes: 90
        );
        
        codingStudy.AddProgress(new DateTime(2024, 9, 27), 90);
        codingStudy.AddProgress(new DateTime(2024, 9, 28), 45);
        codingStudy.AddProgress(new DateTime(2024, 9, 29), 60);
        
        TodoItem drinkWater = new TodoItem(
            name: "물 마시기",
            description: "오늘 물 8잔 마시기",
            startDate: new DateTime(2024, 9, 27),
            endDate: new DateTime(2024, 9, 27),
            type: ItemType.CheckBased,
            priority: 3,
            recurrence: Recurrence.None,
            status: Status.Pending
        );

        drinkWater.MarkAsCompletedOnDate(new DateTime(2024, 9, 27));
        
        TodoItem healthCheck = new TodoItem(
            name: "건강 검진 받기",
            description: "건강 검진 센터 방문",
            startDate: new DateTime(2024, 9, 30),
            endDate: new DateTime(2024, 9, 30),
            type: ItemType.CheckBased,
            priority: 8,
            recurrence: Recurrence.None,
            status: Status.Pending
        );

        TodoItem yogaClass = new TodoItem(
            name: "요가 수업",
            description: "매주 화요일과 목요일에 요가 수업 참석",
            startDate: new DateTime(2024, 9, 27),
            endDate: new DateTime(2024, 10, 31),
            type: ItemType.TimeBased,
            priority: 6,
            recurrence: Recurrence.Weekly,
            status: Status.Pending,
            durationInMinutes: 60,
            recurrenceDays: new List<DayOfWeek> { DayOfWeek.Tuesday, DayOfWeek.Thursday }
        );
        
        return new List<TodoItem>
        {
            codingStudy,
            drinkWater,
            healthCheck,
            yogaClass
        };
    }

    #region Save&Load

    /// <summary>
    /// Todo 리스트를 JSON 파일로 저장합니다.
    /// </summary>
    void SaveTodoListToJson(List<TodoItem> todoList, string filePath)
    {
        TodoListWrapper wrapper = new TodoListWrapper(todoList);

        string jsonString = JsonUtility.ToJson(wrapper, prettyPrint: true);
        File.WriteAllText(filePath, jsonString);

        Debug.Log($"Todo 리스트가 JSON 파일로 저장되었습니다: {filePath}");
    }

    /// <summary>
    /// JSON 파일에서 Todo 리스트를 불러옵니다.
    /// </summary>
    List<TodoItem> LoadTodoListFromJson(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"파일이 존재하지 않습니다: {filePath}");
            return new List<TodoItem>();
        }

        string jsonString = File.ReadAllText(filePath);
        TodoListWrapper wrapper = JsonUtility.FromJson<TodoListWrapper>(jsonString);

        if (wrapper != null && wrapper.TodoItems != null)
        {
            Debug.Log($"Todo 리스트가 JSON 파일에서 불러와졌습니다: {filePath}");

            // 로드한 후 내부 데이터 구조 초기화
            foreach (var todo in wrapper.TodoItems)
            {
                todo.InitializeInternalDataStructures();
            }

            return wrapper.TodoItems;
        }
        else
        {
            Debug.LogWarning("Todo 리스트를 불러오는 데 실패했습니다.");
            return new List<TodoItem>();
        }
    }

    #endregion

    /// <summary>
    /// 특정 날짜에 해당하는 Todo 항목을 필터링합니다.
    /// </summary>
    public List<TodoItem> FilterTodoItemsForToday(List<TodoItem> todoList, DateTime today)
    {
        List<TodoItem> filteredList = new List<TodoItem>();

        foreach (var todo in todoList)
        {
            if (todo.IsTaskScheduledForDate(today))
            {
                filteredList.Add(todo);
            }
        }

        return filteredList;
    }

    #region Add&Delete

    /// <summary>
    /// 새로운 TodoItem을 추가하고 저장합니다.
    /// </summary>
    public void AddTodoItem(TodoItem todoItem)
    {
        TodoList.Add(todoItem);
        SaveTodoListToJson(TodoList, _filePath);
        
        Debug.Log($"새로운 Todo 항목이 생겼습니다. {todoItem}");
        
        // 이벤트 호출
        OnTodoItemAdded?.Invoke(todoItem);
    }

    /// <summary>
    /// TodoItem을 삭제하고 저장합니다.
    /// </summary>
    public void DeleteTodoItem(TodoItem todoItem)
    {
        if (TodoList.Contains(todoItem))
        {
            TodoList.Remove(todoItem);  // TodoItem 삭제
            SaveTodoListToJson(TodoList, _filePath);  // 리스트 업데이트 후 저장
            Debug.Log($"{todoItem.Name} 할 일이 삭제되었습니다.");
        }
        else
        {
            Debug.LogWarning("삭제하려는 TodoItem을 찾을 수 없습니다.");
        }
    }

    #endregion
}