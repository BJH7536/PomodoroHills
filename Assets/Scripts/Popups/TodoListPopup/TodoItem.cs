using System;
using System.Collections.Generic;
using UnityEngine;

namespace TodoSystem
{
    /// <summary>
    /// 개별 Todo항목을 나타내는 클래스.
    /// </summary>
    [Serializable]
    public class TodoItem
    {
        /// <summary>
        /// 진척도로 Todo항목의 완료 여부를 검사할 tolerance값
        /// </summary>
        static float completeTolerance = 0.01f;
        
        // 기본 정보
        public string Id;                       // Guid 필드
        public string Name;                     // 이름
        public string Description;              // 설명
        public string StartDate;                // 시작일 ("yyyy-MM-dd" 형식의 문자열)
        public string EndDate;                  // 종료일 ("yyyy-MM-dd" 형식의 문자열)
        public ItemType Type;                   // 종류 (시간형 또는 확인형)
        public int Priority;                    // 중요도 (1-10)
        public Recurrence Recurrence;           // 반복 여부
        public Status Status;                   // 상태 (대기 중, 진행 중, 완료됨)

        // 시간형 할 일에 필요한 추가 정보
        public int DailyTargetDurationInMinutes;    // 매일의 목표 시간
        public List<DayOfWeek> RecurrenceDays;      // 반복 요일 (주간 반복 시)
        public string ReminderTime;                 // 알림 시간 ("yyyy-MM-dd HH:mm:ss" 형식의 문자열)

        // 진행 상황 추적을 위한 필드

        // 시간형 할 일의 날짜별 작업량 (분 단위)
        [SerializeField]
        private List<DateProgress> dailyProgressList;

        // 확인형 할 일의 완료된 날짜 목록
        [SerializeField]
        private List<string> completedDatesList;

        // 내부에서 사용하기 위한 데이터 구조 (직렬화되지 않음)
        private Dictionary<DateTime, int> dailyProgressDict;
        private HashSet<DateTime> completedDatesSet;

        /// <summary>
        /// OnTodoItemLink 클래스의 생성자입니다.
        /// </summary>
        public TodoItem(
            string id, // 새로운 ID 파라미터 추가
            string name,
            string description,
            DateTime? startDate,
            DateTime? endDate,
            ItemType type,
            int priority,
            Recurrence recurrence,
            Status status,
            int dailyTargetDurationInMinutes = 0,
            List<DayOfWeek> recurrenceDays = null,
            DateTime? reminderTime = null)
        {
            // 기본 정보 초기화
            Id = id;
            Name = name;
            Description = description;
            StartDate = startDate?.ToString("yyyy-MM-dd");
            EndDate = endDate?.ToString("yyyy-MM-dd");
            Type = type;
            Priority = priority;
            Recurrence = recurrence;
            Status = status;
            RecurrenceDays = recurrenceDays ?? new List<DayOfWeek>();
            ReminderTime = reminderTime?.ToString("yyyy-MM-dd HH:mm:ss");
 
            // 시간형 할 일 초기화
            DailyTargetDurationInMinutes = dailyTargetDurationInMinutes;

            // 진행 상황 초기화
            dailyProgressList = new List<DateProgress>();
            completedDatesList = new List<string>();

            // 내부 데이터 구조 초기화
            dailyProgressDict = new Dictionary<DateTime, int>();
            completedDatesSet = new HashSet<DateTime>();
        }
        
        #region 날짜 변환 메서드

        /// <summary>
        /// 시작일을 DateTime 객체로 반환합니다.
        /// </summary>
        public DateTime? GetStartDateTime()
        {
            if (DateTime.TryParseExact(StartDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// 종료일을 DateTime 객체로 반환합니다.
        /// </summary>
        public DateTime? GetEndDateTime()
        {
            if (DateTime.TryParseExact(EndDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// 알림 시간을 DateTime 객체로 반환합니다.
        /// </summary>
        public DateTime? GetReminderDateTime()
        {
            if (DateTime.TryParseExact(ReminderTime, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            return null;
        }

        #endregion

        #region 작업량 및 완료 상태 기록 메서드

        /// <summary>
        /// 특정 날짜에 수행한 작업량을 추가합니다. (시간형 할 일)
        /// </summary>
        /// <param name="date">작업을 수행한 날짜</param>
        /// <param name="minutes">수행한 작업량 (분 단위)</param>
        public void AddProgress(DateTime date, int minutes)
        {
            // 이미 완료된 항목이라면 아무것도 안한다.
            if (Status == Status.Completed) return;
            
            if (Type != ItemType.TimeBased)
            {
                DebugEx.LogWarning("시간형이 아닌 할 일은 작업량을 기록할 수 없습니다.");
                return;
            }

            if (!IsTaskScheduledForDate(date))
            {
                DebugEx.LogWarning("해당 날짜에 작업이 예정되어 있지 않습니다.");
                return;
            }

            LoadDailyProgressDict();
            
            if (!dailyProgressDict.TryAdd(date.Date, minutes))
            {
                dailyProgressDict[date.Date] += minutes;
            }
            
            // 상태 변환
            if (Math.Abs(GetProgressPercentage() - 100) < completeTolerance)
                Status = Status.Completed;
            else if (Status == Status.Pending) 
                Status = Status.InProgress;
            
            SaveDailyProgressDict();
            
            TodoManager.Instance.SaveCurrentTodoList();         // 현재 시스템의 TodoList를 로컬 저장소에 저장
        }

        /// <summary>
        /// 특정 날짜의 작업을 완료로 표시한다. (확인형 할 일)
        /// </summary>
        /// <param name="date">작업을 완료한 날짜</param>
        public void MarkAsCompletedOnDate(DateTime date)
        {
            // 이미 완료된 항목이라면 아무것도 안한다.
            if (Status == Status.Completed) return;
            
            if (Type != ItemType.CheckBased)
            {
                DebugEx.LogWarning("확인형이 아닌 할 일은 완료 상태를 표시할 수 없습니다.");
                return;
            }

            if (!IsTaskScheduledForDate(date))
            {
                DebugEx.LogWarning("해당 날짜에 작업이 예정되어 있지 않습니다.");
                return;
            }

            // 상태 변환
            if (Status == Status.Pending) Status = Status.InProgress;
            
            LoadCompletedDatesSet();

            completedDatesSet.Add(date.Date);

            // 상태 변환
            if (Math.Abs(GetProgressPercentage() - 100) < completeTolerance)
                Status = Status.Completed;
            else if (Status == Status.Pending) 
                Status = Status.InProgress;
            
            SaveCompletedDatesSet();
            
            TodoManager.Instance.SaveCurrentTodoList();         // 현재 시스템의 TodoList를 로컬 저장소에 저장
        }

        #endregion

        #region 진척도 계산 메서드

        /// <summary>
        /// 전체 진척도를 퍼센티지로 반환합니다.
        /// </summary>
        /// <returns>진척도 (0 ~ 100%)</returns>
        public float GetProgressPercentage()
        {
            if (Type == ItemType.TimeBased)
            {
                int totalScheduledMinutes = GetTotalScheduledMinutes();
                int totalCompletedMinutes = GetTotalCompletedMinutes();

                if (totalScheduledMinutes > 0)
                {
                    float progress = (totalCompletedMinutes / (float)totalScheduledMinutes) * 100f;
                    return Mathf.Clamp(progress, 0f, 100f);
                }
                return 0f;
            }

            if (Type == ItemType.CheckBased)
            {
                int totalScheduledDays = GetAllScheduledDates().Count;
                int totalCompletedDays = GetTotalCompletedDays();

                if (totalScheduledDays > 0)
                {
                    float progress = (totalCompletedDays / (float)totalScheduledDays) * 100f;
                    return Mathf.Clamp(progress, 0f, 100f);
                }
                return 0f;
            }
            
            return 0f;
        }

        /// <summary>
        /// 오늘의 할 일을 완료했는지 확인합니다.
        /// </summary>
        /// <param name="today">오늘 날짜</param>
        /// <returns>완료 여부</returns>
        public bool IsTodayTaskCompleted(DateTime today)
        {
            if (!IsTaskScheduledForDate(today))
            {
                return false; // 오늘 할 일이 없으면 미완료로 간주
            }

            if (Type == ItemType.TimeBased)
            {
                if (GetRemainingTimeOfToday() > 0)
                    return false;
                else 
                    return true;
            }
            
            if (Type == ItemType.CheckBased)
            {
                LoadCompletedDatesSet();

                return completedDatesSet.Contains(today.Date);
            }
            
            return false;
        }

        /// <summary>
        /// 오늘 남은 시간을 구합니다. (시간형 할 일)
        /// </summary>
        /// <returns></returns>
        public int GetRemainingTimeOfToday()
        {
            if (!IsTaskScheduledForDate(DateTime.Today))
            {
                return 0; // 오늘 할 일이 없으면 미완료로 간주
            }

            if (Type == ItemType.TimeBased) 
            {
                LoadDailyProgressDict();

                // 딕셔너리에서 오늘자 진척도 찾기 - 있으면
                if (dailyProgressDict.TryGetValue(DateTime.Today, out var value))
                    return (int) MathF.Max(DailyTargetDurationInMinutes - value, 0);
                // 없으면
                else
                    return DailyTargetDurationInMinutes;
            }
            
            return 0;
        }

        #endregion

        #region 내부 데이터 처리 메서드

        /// <summary>
        /// 전체 예정된 날짜 목록을 반환합니다.
        /// </summary>
        /// <returns>예정된 날짜 리스트</returns>
        public List<DateTime> GetAllScheduledDates()
        {
            List<DateTime> scheduledDates = new List<DateTime>();

            DateTime? startDate = GetStartDateTime();
            DateTime? endDate = GetEndDateTime();

            if (!startDate.HasValue || !endDate.HasValue)
                return scheduledDates;

            DateTime currentDate = startDate.Value.Date;

            while (currentDate <= endDate.Value.Date)
            {
                if (IsTaskScheduledForDate(currentDate))
                {
                    scheduledDates.Add(currentDate);
                }

                currentDate = currentDate.AddDays(1);
            }

            return scheduledDates;
        }

        /// <summary>
        /// 특정 날짜에 할 일이 예정되어 있는지 확인합니다.
        /// </summary>
        /// <param name="date">확인할 날짜</param>
        /// <returns>예정 여부</returns>
        public bool IsTaskScheduledForDate(DateTime date)
        {
            DateTime? startDate = GetStartDateTime();
            DateTime? endDate = GetEndDateTime();

            if (!startDate.HasValue || !endDate.HasValue)
                return false;

            if (date.Date < startDate.Value.Date || date.Date > endDate.Value.Date)
                return false;

            switch (Recurrence)
            {
                case Recurrence.None:
                    return true; // 기간 내 모든 날짜 포함
                case Recurrence.Daily:
                    return true;
                case Recurrence.Weekly:
                    return RecurrenceDays.Contains(date.DayOfWeek);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 총 예정된 작업량을 계산합니다. (시간형 할 일)
        /// </summary>
        /// <returns>총 예정된 작업량 (분 단위)</returns>
        private int GetTotalScheduledMinutes()
        {
            if (Type != ItemType.TimeBased)
                return 0;

            int scheduledDaysCount = GetAllScheduledDates().Count;
            return scheduledDaysCount * DailyTargetDurationInMinutes;
        }

        /// <summary>
        /// 총 완료한 작업량을 계산합니다. (시간형 할 일)
        /// </summary>
        /// <returns>총 완료한 작업량 (분 단위)</returns>
        private int GetTotalCompletedMinutes()
        {
            LoadDailyProgressDict();

            int totalCompleted = 0;
            foreach (var entry in dailyProgressDict)
            {
                if (IsTaskScheduledForDate(entry.Key))
                {
                    totalCompleted += entry.Value;
                }
            }
            return totalCompleted;
        }

        /// <summary>
        /// 총 완료한 일수를 계산합니다. (확인형 할 일)
        /// </summary>
        /// <returns>총 완료한 일수</returns>
        private int GetTotalCompletedDays()
        {
            LoadCompletedDatesSet();

            return completedDatesSet.Count;
        }

        #endregion

        #region 데이터 직렬화 및 역직렬화 메서드

        /// <summary>
        /// dailyProgressList를 딕셔너리로 변환하여 로드합니다.
        /// </summary>
        private void LoadDailyProgressDict()
        {
            if (dailyProgressDict == null || dailyProgressDict.Count == 0)
            {
                dailyProgressDict = new Dictionary<DateTime, int>();
                foreach (var entry in dailyProgressList)
                {
                    if (DateTime.TryParseExact(entry.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                    {
                        dailyProgressDict[date] = entry.Minutes;
                    }
                }
            }
        }

        /// <summary>
        /// 딕셔너리를 dailyProgressList로 변환하여 저장합니다.
        /// </summary>
        private void SaveDailyProgressDict()
        {
            dailyProgressList = new List<DateProgress>();
            foreach (var kvp in dailyProgressDict)
            {
                dailyProgressList.Add(new DateProgress(kvp.Key.ToString("yyyy-MM-dd"), kvp.Value));
            }
        }

        /// <summary>
        /// completedDatesList를 HashSet으로 변환하여 로드합니다.
        /// </summary>
        private void LoadCompletedDatesSet()
        {
            if (completedDatesSet == null || completedDatesSet.Count == 0)
            {
                completedDatesSet = new HashSet<DateTime>();
                foreach (var dateString in completedDatesList)
                {
                    if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                    {
                        completedDatesSet.Add(date);
                    }
                }
            }
        }

        /// <summary>
        /// HashSet을 completedDatesList로 변환하여 저장합니다.
        /// </summary>
        private void SaveCompletedDatesSet()
        {
            completedDatesList = new List<string>();
            foreach (var date in completedDatesSet)
            {
                completedDatesList.Add(date.ToString("yyyy-MM-dd"));
            }
        }

        /// <summary>
        /// 내부 데이터 구조를 초기화합니다.
        /// </summary>
        public void InitializeInternalDataStructures()
        {
            // 직렬화된 리스트를 딕셔너리나 집합으로 변환
            dailyProgressDict = new Dictionary<DateTime, int>();
            foreach (var entry in dailyProgressList)
            {
                if (DateTime.TryParseExact(entry.Date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                {
                    dailyProgressDict[date] = entry.Minutes;
                }
            }

            completedDatesSet = new HashSet<DateTime>();
            foreach (var dateString in completedDatesList)
            {
                if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime date))
                {
                    completedDatesSet.Add(date);
                }
            }
        }

        
        #endregion

        /// <summary>
        /// TodoItem의 정보를 문자열로 반환합니다.
        /// </summary>
        public override string ToString()
        {
            string typeInfo = Type == ItemType.TimeBased ? "시간형" : "확인형";

            string recurrenceInfo = Recurrence switch
            {
                Recurrence.None => "반복 없음",
                Recurrence.Daily => "매일 반복",
                Recurrence.Weekly => $"매주 반복: {string.Join(", ", RecurrenceDays)}",
                _ => "알 수 없음"
            };

            string durationInfo = Type == ItemType.TimeBased && DailyTargetDurationInMinutes > 0 ? $"소요 시간: {DailyTargetDurationInMinutes}분" : "";
            string reminderInfo = !string.IsNullOrEmpty(ReminderTime) ? $"알림 시간: {ReminderTime}" : "";

            return $"이름: {Name}\n" +
                   $"설명: {Description}\n" +
                   $"{(!string.IsNullOrEmpty(StartDate) ? $"시작일: {StartDate}\n" : "")}" +
                   $"{(!string.IsNullOrEmpty(EndDate) ? $"종료일: {EndDate}\n" : "")}" +
                   $"종류: {typeInfo}\n" +
                   $"중요도: {Priority}\n" +
                   $"반복 여부: {recurrenceInfo}\n" +
                   $"{(string.IsNullOrEmpty(durationInfo) ? "" : durationInfo + "\n")}" +
                   $"{(string.IsNullOrEmpty(reminderInfo) ? "" : reminderInfo + "\n")}" +
                   $"상태: {Status}\n";
        }
    }

    /// <summary>
    /// 시간형 TodoItem의 날짜별 작업량을 저장하기 위한 클래스입니다.
    /// </summary>
    [Serializable]
    public class DateProgress
    {
        public string Date;   // 날짜 ("yyyy-MM-dd" 형식)
        public int Minutes;   // 작업량 (분 단위)

        public DateProgress(string date, int minutes)
        {
            Date = date;
            Minutes = minutes;
        }
    }
    
    [Serializable]
    public class TodoListWrapper
    {
        public List<TodoItem> TodoItems;

        public TodoListWrapper(List<TodoItem> todoItems)
        {
            TodoItems = todoItems;
        }
    }
}
