using System;
using UnityEngine;

public class DayAndNightSystem : MonoBehaviour
{
    public Transform sun;
    public Light sunLight;
    public Gradient sunColorGradient;           // 인스펙터에서 시간대별 색을 설정할 수 있는 Gradient
    public Gradient ambientColorGradient;       // 인스펙터에서 시간대별 색을 설정할 수 있는 Gradient

    private const float fullDayRotation = 360f;

    public bool rotation = true;
    
    public bool simulateTime = false;   // true로 설정하면 시뮬레이션 모드 활성화
    public int simulatedHour = 12;      // 시뮬레이션 시간
    
    void Update()
    {
        DateTime localTime;

        if (simulateTime)
        {
            localTime = new DateTime(2024, 1, 1, (int)simulatedHour % 24, 0, 0); // 시뮬레이션 시간 설정
        }
        else
        {
            DateTime utcNow = DateTime.UtcNow;
            TimeZoneInfo localZone = TimeZoneInfo.Local;
            localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localZone);
        }
        
        float hours = localTime.Hour + localTime.Minute / 60f; // 시간 + 분을 소수점으로 변환

        // 태양의 각도 계산 (반대 방향 회전, 00시에 밤, 12시에 정상)
        float sunAngle = -(hours / 24f) * fullDayRotation - 90f;
        if (rotation)
        {
            sun.localRotation = Quaternion.Euler(sunAngle, -30f, 0f);
        }
        
        // 시간을 0~1로 정규화하여 Gradient에서 색상을 뽑아냄
        float normalizedTime = hours / 24f;
        Color currentSunColor = sunColorGradient.Evaluate(normalizedTime);
        Color currentAmbientColor = ambientColorGradient.Evaluate(normalizedTime);

        sunLight.color = currentSunColor;
        RenderSettings.ambientLight = currentAmbientColor;
    }
}
