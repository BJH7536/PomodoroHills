using System;
using System.Collections.Generic;
using UnityEngine;

namespace TodoList
{
    /// <summary>
    /// Todo 항목의 종류를 정의하는 열거형입니다.
    /// </summary>
    [Serializable]
    public enum ItemType
    {
        TimeBased,  // 시간형
        CheckBased  // 확인형
    }

    /// <summary>
    /// Todo 항목의 반복 여부를 정의하는 열거형입니다.
    /// </summary>
    [Serializable]
    public enum Recurrence
    {
        None,       // 반복 없음
        Daily,      // 매일 반복
        Weekly,     // 매주 반복
        Monthly,    // 매월 반복
        Yearly      // 매년 반복
    }

    /// <summary>
    /// Todo 항목의 상태를 정의하는 열거형입니다.
    /// </summary>
    [Serializable]
    public enum Status
    {
        Pending,        // 대기 중
        InProgress,     // 진행 중
        Completed       // 완료됨
    }

    /// <summary>
    /// 개별 Todo 항목을 나타내는 클래스입니다.
    /// </summary>
    [Serializable]
    public class TodoItem
    {
        public string Name;                          // 이름
        public string Description;                   // 설명
        public string StartDate;                     // 시작일 (string 형식)
        public string EndDate;                       // 종료일 (string 형식)
        public ItemType Type;                        // 종류 (시간형 또는 확인형)
        public int Priority;                         // 중요도 (1-10)
        public Recurrence Recurrence;                // 반복 여부
        public Status Status;                        // 상태 (대기 중, 진행 중, 완료됨)

        // 시간형 할 일에 필요한 추가 필드
        public float Duration;                       // 소요 시간 (시간형에만 필요, float으로 변경)
        public List<DayOfWeek> RecurrenceDays;       // 반복 요일 (주간 반복 시)
        public string ReminderTime;                  // 알림 시간 (string 형식)

        /// <summary>
        /// TodoItem 클래스의 생성자입니다.
        /// </summary>
        public TodoItem(string name, string description, DateTime? startDate, DateTime? endDate, ItemType type, int priority, Recurrence recurrence, Status status, float duration = 0f, List<DayOfWeek> recurrenceDays = null, DateTime? reminderTime = null)
        {
            Name = name;
            Description = description;
            StartDate = startDate?.ToString("yyyy-MM-dd");
            EndDate = endDate?.ToString("yyyy-MM-dd");
            Type = type;
            Priority = priority;
            Recurrence = recurrence;
            Status = status;
            Duration = duration;
            OriginalDuration = duration;
            RecurrenceDays = recurrenceDays ?? new List<DayOfWeek>();
            ReminderTime = reminderTime.HasValue ? reminderTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null;
        }

        /// <summary>
        /// TodoItem의 정보를 문자열로 반환합니다.
        /// </summary>
        public override string ToString()
        {
            string typeInfo = Type switch
            {
                ItemType.TimeBased => "시간형",
                ItemType.CheckBased => "확인형",
                _ => "알 수 없음"
            };

            string recurrenceInfo = Recurrence switch
            {
                Recurrence.None => "반복 없음",
                Recurrence.Daily => "매일 반복",
                Recurrence.Weekly => $"매주 반복: {string.Join(", ", RecurrenceDays)}",
                Recurrence.Monthly => "매월 반복",
                Recurrence.Yearly => "매년 반복",
                _ => "알 수 없음"
            };

            string durationInfo = Type == ItemType.TimeBased && Duration > 0 ? $"소요 시간: {Duration}시간" : "";
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

        /// <summary>
        /// 문자열을 DateTime으로 변환합니다.
        /// </summary>
        public DateTime? GetStartDateTime()
        {
            return DateTime.TryParse(StartDate, out DateTime result) ? result : (DateTime?)null;
        }

        public DateTime? GetEndDateTime()
        {
            return DateTime.TryParse(EndDate, out DateTime result) ? result : (DateTime?)null;
        }

        public DateTime? GetReminderDateTime()
        {
            return DateTime.TryParse(ReminderTime, out DateTime result) ? result : (DateTime?)null;
        }
        
        /// <summary>
        /// 소요 시간을 감소시킵니다.
        /// </summary>
        /// <param name="amount">감소시킬 시간 (시간 단위)</param>
        public void ReduceDuration(float amount)
        {
            if (Type == ItemType.TimeBased)
            {
                Duration -= amount;
                if (Duration < 0)
                    Duration = 0;
            }
            else
            {
                Debug.LogWarning("시간형이 아닌 할 일은 소요 시간을 감소시킬 수 없습니다.");
            }
        }
        
        /// <summary>
        /// 할 일을 시작하여 상태를 '진행 중'으로 변경합니다.
        /// </summary>
        public void StartTask()
        {
            if (Status == Status.Pending)
            {
                Status = Status.InProgress;
            }
            else
            {
                Debug.LogWarning("현재 상태에서 할 일을 시작할 수 없습니다.");
            }
        }

        /// <summary>
        /// 할 일을 완료하여 상태를 '완료됨'으로 변경합니다.
        /// </summary>
        public void CompleteTask()
        {
            if (Status == Status.InProgress || (Type == ItemType.CheckBased && Status == Status.Pending))
            {
                Status = Status.Completed;
            }
            else
            {
                Debug.LogWarning("현재 상태에서 할 일을 완료할 수 없습니다.");
            }
        }

        private float OriginalDuration; // 초기 소요 시간
        
        /// <summary>
        /// 전체 진척도를 퍼센티지로 반환합니다.
        /// </summary>
        /// <returns>진척도 (0 ~ 100%)</returns>
        public float GetProgressPercentage()
        {
            if (Status == Status.Completed)
            {
                return 100f;
            }
            else if (Status == Status.InProgress)
            {
                if (Type == ItemType.TimeBased && OriginalDuration > 0)
                {
                    float progress = ((OriginalDuration - Duration) / OriginalDuration) * 100f;
                    return Mathf.Clamp(progress, 0f, 99.9f);
                }
                else
                {
                    return 50f; // 진행 중이지만 시간형이 아닌 경우 임의로 50%로 설정
                }
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// 오늘의 할 일을 완료했는지 확인합니다.
        /// </summary>
        /// <param name="today">오늘 날짜</param>
        /// <returns>완료 여부</returns>
        public bool IsTodayTaskCompleted(DateTime today)
        {
            if (Status == Status.Completed)
            {
                return true;
            }
            else if (Type == ItemType.TimeBased)
            {
                if (IsTaskScheduledForDate(today))
                {
                    return Duration <= 0;
                }
            }
            else if (Type == ItemType.CheckBased)
            {
                return Status == Status.Completed;
            }
            return false;
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
                case Recurrence.Daily:
                    return true;
                case Recurrence.Weekly:
                    return RecurrenceDays.Contains(date.DayOfWeek);
                case Recurrence.Monthly:
                    return startDate.Value.Day == date.Day;
                case Recurrence.Yearly:
                    return startDate.Value.Month == date.Month && startDate.Value.Day == date.Day;
                default:
                    return false;
            }
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
