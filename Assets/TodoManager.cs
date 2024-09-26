using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TodoList;

public class TodoManager : MonoBehaviour
{
    private string filePath;

    private void Start()
    {
        // 파일 경로 설정 (예: Application.persistentDataPath는 영구 저장 경로)
        filePath = Path.Combine(Application.persistentDataPath, "todoList.json");

        // 예시 Todo 항목 생성
        List<TodoItem> todoList = CreateSampleTodoItems();

        // JSON 파일로 저장
        SaveTodoListToJson(todoList, filePath);

        // JSON 파일에서 불러오기
        List<TodoItem> loadedTodoList = LoadTodoListFromJson(filePath);

        // 불러온 Todo 항목 출력
        Debug.Log("불러온 Todo 항목들:");
        foreach (var todo in loadedTodoList)
        {
            Debug.Log(todo.ToString());
        }

        // 오늘의 할 일 필터링
        DateTime today = DateTime.Today;
        List<TodoItem> todaysTasks = FilterTodoItemsForToday(loadedTodoList, today);

        Debug.Log("오늘의 할 일:");
        foreach (var todo in todaysTasks)
        {
            Debug.Log(todo.ToString());
        }
    }

    /// <summary>
    /// 샘플 Todo 항목들을 생성합니다.
    /// </summary>
    List<TodoItem> CreateSampleTodoItems()
    {
        DateTime today = DateTime.Today;

        // 1. "쓰레기 버리기" - 확인형 일회용 할 일
        TodoItem trashTakeout = new TodoItem(
            name: "쓰레기 버리기",
            description: "당일 하루 내에 쓰레기를 버립니다.",
            startDate: today,
            endDate: today, // 당일 하루
            type: ItemType.CheckBased,
            priority: 5, // 중요도 1-10 중 5
            recurrence: Recurrence.None, // 반복 없음
            status: Status.Pending
        );

        // 2. "전공 공부" - 시간형 기간형 할 일, 매일 2시간씩
        TodoItem majorStudy = new TodoItem(
            name: "전공 공부",
            description: "오늘부터 한 달 동안 매일 2시간씩 전공 공부를 합니다.",
            startDate: today,
            endDate: today.AddMonths(1), // 한 달 동안
            type: ItemType.TimeBased,
            priority: 7, // 중요도 1-10 중 7
            recurrence: Recurrence.Daily, // 매일 반복
            status: Status.InProgress,
            duration: 2f, // 소요 시간 2시간
            reminderTime: today.AddHours(9) // 예: 매일 오전 9시에 알림 설정
        );

        // 3. "운동" - 시간형 기간형 할 일, 매주 화요일과 목요일에 2시간씩
        TodoItem exercise = new TodoItem(
            name: "운동",
            description: "오늘부터 1년 동안 매주 화요일과 목요일에 2시간씩 운동을 합니다.",
            startDate: today,
            endDate: today.AddYears(1), // 1년 동안
            type: ItemType.TimeBased,
            priority: 8, // 중요도 1-10 중 8
            recurrence: Recurrence.Weekly, // 매주 반복
            status: Status.Pending,
            duration: 2f, // 소요 시간 2시간
            recurrenceDays: new List<DayOfWeek> { DayOfWeek.Tuesday, DayOfWeek.Thursday }, // 화요일과 목요일 반복
            reminderTime: today.AddHours(18) // 예: 매주 화, 목 오후 6시에 알림 설정
        );

        // 4. "건강검진 받기" - 확인형 일회용 할 일, 2일 후
        TodoItem healthCheck = new TodoItem(
            name: "건강검진 받기",
            description: "2일 후에 건강검진을 받습니다.",
            startDate: today.AddDays(2),
            endDate: today.AddDays(2), // 2일 후 하루
            type: ItemType.CheckBased,
            priority: 9, // 중요도 1-10 중 9
            recurrence: Recurrence.None, // 반복 없음
            status: Status.Pending
        );

        // 추가 예시: "발표 준비하기" - 시간형 일회용 할 일
        TodoItem presentationPreparation = new TodoItem(
            name: "발표 준비하기",
            description: "당일 하루에 2시간 동안 발표를 준비합니다.",
            startDate: today,
            endDate: today, // 당일 하루
            type: ItemType.TimeBased,
            priority: 6, // 중요도 1-10 중 6
            recurrence: Recurrence.None, // 반복 없음
            status: Status.Pending,
            duration: 2f, // 소요 시간 2시간
            reminderTime: today.AddHours(14) // 예: 오후 2시에 알림 설정
        );

        // 추가 예시: "분리수거하기" - 시간형 주간 반복 할 일, 시간 측정 없음
        TodoItem recycling = new TodoItem(
            name: "분리수거하기",
            description: "오늘부터 1년 동안 매주 수요일과 토요일에 분리수거를 합니다.",
            startDate: today,
            endDate: today.AddYears(1), // 1년 동안
            type: ItemType.TimeBased,
            priority: 7, // 중요도 1-10 중 7
            recurrence: Recurrence.Weekly, // 매주 반복
            status: Status.Pending,
            duration: 0f, // 시간 측정 없음
            recurrenceDays: new List<DayOfWeek> { DayOfWeek.Wednesday, DayOfWeek.Saturday }, // 수요일과 토요일 반복
            reminderTime: today.AddHours(17) // 예: 매주 수, 토 오후 5시에 알림 설정
        );

        return new List<TodoItem>
        {
            trashTakeout,
            majorStudy,
            exercise,
            healthCheck,
            presentationPreparation,
            recycling
        };
    }

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
            return wrapper.TodoItems;
        }
        else
        {
            Debug.LogWarning("Todo 리스트를 불러오는 데 실패했습니다.");
            return new List<TodoItem>();
        }
    }

    /// <summary>
    /// 특정 날짜에 해당하는 Todo 항목을 필터링합니다.
    /// </summary>
    List<TodoItem> FilterTodoItemsForToday(List<TodoItem> todoList, DateTime today)
    {
        List<TodoItem> filteredList = new List<TodoItem>();

        foreach (var todo in todoList)
        {
            if (todo.Type == ItemType.CheckBased)
            {
                // 확인형 할 일: 시작일과 종료일이 오늘인 경우
                if (!string.IsNullOrEmpty(todo.StartDate) && !string.IsNullOrEmpty(todo.EndDate))
                {
                    DateTime startDate = DateTime.Parse(todo.StartDate);
                    DateTime endDate = DateTime.Parse(todo.EndDate);

                    if (startDate.Date == today.Date && endDate.Date == today.Date)
                    {
                        filteredList.Add(todo);
                    }
                }
            }
            else if (todo.Type == ItemType.TimeBased)
            {
                // 시간형 할 일: 오늘이 시작일과 종료일 사이에 있는 경우
                if (!string.IsNullOrEmpty(todo.StartDate) && !string.IsNullOrEmpty(todo.EndDate))
                {
                    DateTime startDate = DateTime.Parse(todo.StartDate);
                    DateTime endDate = DateTime.Parse(todo.EndDate);

                    if (today.Date >= startDate.Date && today.Date <= endDate.Date)
                    {
                        bool shouldInclude = false;

                        switch (todo.Recurrence)
                        {
                            case Recurrence.None:
                                // 반복 없음: 기간 내에 있으면 포함
                                shouldInclude = true;
                                break;
                            case Recurrence.Daily:
                                // 매일 반복: 기간 내에 있으면 포함
                                shouldInclude = true;
                                break;
                            case Recurrence.Weekly:
                                // 매주 반복: 오늘 요일이 RecurrenceDays에 포함되는지 확인
                                if (todo.RecurrenceDays != null && todo.RecurrenceDays.Contains(today.DayOfWeek))
                                {
                                    shouldInclude = true;
                                }
                                break;
                            case Recurrence.Monthly:
                                // 매월 반복: 시작일과 같은 날짜인 경우
                                if (startDate.Day == today.Day)
                                {
                                    shouldInclude = true;
                                }
                                break;
                            case Recurrence.Yearly:
                                // 매년 반복: 시작일과 같은 월과 날짜인 경우
                                if (startDate.Month == today.Month && startDate.Day == today.Day)
                                {
                                    shouldInclude = true;
                                }
                                break;
                        }

                        if (shouldInclude)
                        {
                            filteredList.Add(todo);
                        }
                    }
                }
            }
        }

        return filteredList;
    }
}
