using System;
using TMPro;
using UnityEngine;
using VInspector;

public class DatePickerUI : MonoBehaviour
{
    [Tab("information")]
    [SerializeField] private ScrollSystem yearScrollSystem;     // 연도 선택 스크롤 시스템
    [SerializeField] private ScrollSystem monthScrollSystem;    // 월 선택 스크롤 시스템
    [SerializeField] private ScrollSystem dayScrollSystem;      // 일 선택 스크롤 시스템
    [SerializeField] private TMP_Text selectedDateText;         // 선택된 날짜를 표시하는 텍스트
    
    [Tab("Caller")] 
    [SerializeField] private DatePickerCaller _datePickerCaller;
    
    private int selectedYear;  // 선택된 연도
    private int selectedMonth;  // 선택된 월
    private int selectedDay;  // 선택된 일
    
    public void Open(DatePickerCaller caller, DateTime dateTime)
    {
        _datePickerCaller = caller;
        
        selectedYear = dateTime.Year;
        selectedMonth = dateTime.Month;
        selectedDay = dateTime.Day;
        
        // 각각의 스크롤 시스템에 대한 초기화 작업
        InitializeYearScroll();
        InitializeMonthScroll();
        InitializeDayScroll();
        
        // 초기 선택된 날짜 표시
        UpdateSelectedDate();
        
        gameObject.SetActive(true);
    }

    public void Close()
    {
        _datePickerCaller.SetDateTime(new DateTime(selectedYear, selectedMonth, selectedDay));
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 연도 스크롤 시스템 초기화
    /// </summary>
    private void InitializeYearScroll()
    {
        yearScrollSystem.Setup(2024, 2100, OnYearSelected, selectedYear);
        yearScrollSystem.SetInitialScrollPosition(selectedYear);
    }

    /// <summary>
    /// 월 스크롤 시스템 초기화
    /// </summary>
    private void InitializeMonthScroll()
    {
        monthScrollSystem.Setup(1, 12, OnMonthSelected, selectedMonth);
        monthScrollSystem.SetInitialScrollPosition(selectedMonth);
    }

    /// <summary>
    /// 일 스크롤 시스템 초기화
    /// </summary>
    private void InitializeDayScroll()
    {
        UpdateDayScrollSystem();
        dayScrollSystem.SetInitialScrollPosition(selectedDay);
    }
    
    /// <summary>
    /// 연도가 선택되었을 때 호출되는 콜백
    /// </summary>
    private void OnYearSelected(int year)
    {
        selectedYear = year;
        UpdateDayScrollSystem();    // 월과 일의 유효 범위를 갱신
        UpdateSelectedDate();       // 선택된 날짜를 업데이트
    }

    /// <summary>
    /// 월이 선택되었을 때 호출되는 콜백
    /// </summary>
    private void OnMonthSelected(int month)
    {
        selectedMonth = month;
        UpdateDayScrollSystem();  // 선택된 월에 맞게 일 범위 업데이트
        UpdateSelectedDate();
    }

    /// <summary>
    /// 일이 선택되었을 때 호출되는 콜백
    /// </summary>
    private void OnDaySelected(int day)
    {
        selectedDay = day;
        UpdateSelectedDate();
    }
    
    /// <summary>
    /// 선택된 연도와 월에 맞춰 일의 유효 범위를 갱신
    /// </summary>
    private void UpdateDayScrollSystem()
    {
        int daysInMonth = GetDaysInMonth(selectedYear, selectedMonth);

        // 현재 선택된 일이 새로운 월의 최대 일수보다 크다면 조정.
        if (selectedDay > daysInMonth)
        {
            selectedDay = daysInMonth; // 현재 월의 최대 일수로 설정
        }

        // 기존 아이템을 제거
        dayScrollSystem.ClearItems();

        // 새로 설정
        dayScrollSystem.Setup(1, daysInMonth, OnDaySelected, selectedDay);
    }


    /// <summary>
    /// 연도와 월에 따른 해당 월의 마지막 날을 반환
    /// </summary>
    private int GetDaysInMonth(int year, int month)
    {
        return DateTime.DaysInMonth(year, month);
    }

    /// <summary>
    /// 선택된 날짜를 화면에 표시
    /// </summary>
    private void UpdateSelectedDate()
    {
        selectedDateText.text = $"{selectedYear}년 {selectedMonth}월 {selectedDay}일";
    }
}
