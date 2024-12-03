using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "PomoDialogue", menuName = "PomodoroHills/PomoDialogue", order = 1)]
public class PomoDialogue : ScriptableObject
{
    [System.Serializable]
    public class DialogueEntry
    {
        public TimePeriod timePeriod;           // 시간대 이름 (오전, 점심 등)
        public List<Dialogue> dialogues;        // 해당 시간대의 대사 목록
    }

    [System.Serializable]
    public class Dialogue
    {
        public string Text;
        
        [Tooltip("해당 대사의 속도")]
        public float characterDelay = 0.05f;
        
        [Tooltip("해당 대사를 출력하면서 멈춰있을 시간 ")]
        public int Delay = 2;
    }
    
    [Header("시간대 별 대사 설정")]
    public List<DialogueEntry> dialogueEntries;

    /// <summary>
    /// 현재 시간에 맞는 대사를 반환하는 메서드
    /// </summary>
    /// <param name="hour">현재 시간 (0~23시)</param>
    /// <returns>랜덤 대사</returns>
    public Dialogue GetDialogueForCurrentTime(int hour)
    {
        TimePeriod currentPeriod = GetTimePeriod(hour);

        foreach (var entry in dialogueEntries)
        {
            if (entry.timePeriod == currentPeriod)
            {
                if (entry.dialogues.Count > 0)
                {
                    int randomIndex = Random.Range(0, entry.dialogues.Count);
                    return entry.dialogues[randomIndex];
                }
            }
        }

        return null; // 대사가 없을 경우
    }

    /// <summary>
    /// 현재 시간을 기준으로 시간대를 반환하는 메서드
    /// </summary>
    /// <param name="hour">현재 시간 (0~23시)</param>
    /// <returns>시간대 Enum</returns>
    private TimePeriod GetTimePeriod(int hour)
    {
        if (hour >= 5 && hour < 11) return TimePeriod.Morning;    // 오전
        if (hour >= 11 && hour < 14) return TimePeriod.Noon;      // 점심
        if (hour >= 14 && hour < 18) return TimePeriod.Afternoon; // 오후
        if (hour >= 18 && hour < 22) return TimePeriod.Evening;   // 저녁
        return TimePeriod.Night;                                  // 밤
    }
}

public enum TimePeriod
{
    Morning,    // 오전 (05:00 ~ 10:59)
    Noon,       // 점심 (11:00 ~ 13:59)
    Afternoon,  // 오후 (14:00 ~ 17:59)
    Evening,    // 저녁 (18:00 ~ 21:59)
    Night       // 밤/새벽 (22:00 ~ 04:59)
}

