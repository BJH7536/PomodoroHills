using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerInputManager : MonoBehaviour
{
    public TMP_InputField inputMainHour;
    public TMP_InputField inputMainMinute;
    public TMP_InputField inputMainSecond;
    public TMP_InputField inputRestHour;
    public TMP_InputField inputRestMinute;
    public TMP_InputField inputRestSecond;
    public Button buttonSubmit;
    public TimerWork timerWork;                     //
        
    private void Start()
    {                                                                   
        // 각 입력필드에 값 입력시 최대길이(2)입력 시 현재 입력필드의 입력값을 제한하고 다음 입력필드로 이동함
        inputMainHour.onValueChanged.AddListener(delegate { OnValueChanged(inputMainHour, inputMainMinute, 23); });
        inputMainMinute.onValueChanged.AddListener(delegate { OnValueChanged(inputMainMinute, inputMainSecond, 60); });
        inputMainSecond.onValueChanged.AddListener(delegate { OnValueChanged(inputMainSecond, inputRestHour, 60); });
        inputRestHour.onValueChanged.AddListener(delegate { OnValueChanged(inputRestHour, inputRestMinute, 23); });
        inputRestMinute.onValueChanged.AddListener(delegate { OnValueChanged(inputRestMinute, inputRestSecond, 60); });
        inputRestSecond.onValueChanged.AddListener(delegate { OnValueChanged(inputRestSecond, null, 60); });
        buttonSubmit.onClick.AddListener(OnSubmit);
    }





    void OnValueChanged(TMP_InputField currentField, TMP_InputField nextField, int maxValue)
    {
    // 각 입력필드에 값 입력시 최대길이(2)입력 시 현재 입력필드의 입력값을 제한하고 다음 입력필드로 이동함

        if (currentField.text.Length >= 2)
        {
            if (int.TryParse(currentField.text, out int value) && value >= 0 && value < maxValue)
            {
                if (nextField != null)
                {
                    nextField.Select();
                    nextField.ActivateInputField();
                }
            }
            else
            {
                // 유효한 값이 아닐 경우 최대값으로 설정
                currentField.text = (maxValue - 1).ToString("D2"); // 두 자리 형식으로 설정합니다.
            }
        }
    }

    public void OnSubmit()                      //타이머 설정 확인버튼에 사용
    {                                                
        float mainTime = 0f;
        float restTime = 0f;

        if (inputMainHour.text != "") { mainTime += float.Parse(inputMainHour.text) * 3600; }
        if (inputMainMinute.text != "") { mainTime += float.Parse(inputMainMinute.text) * 60; }
        if (inputMainSecond.text != "") { mainTime += float.Parse(inputMainSecond.text); }

        if (inputRestHour.text != "") { restTime += float.Parse(inputRestHour.text) * 3600; }
        if (inputRestMinute.text != "") { restTime += float.Parse(inputRestMinute.text) * 60; }
        if (inputRestSecond.text != "") { restTime += float.Parse(inputRestSecond.text) ; }

        if (mainTime != 0f && restTime!= 0f)
        {
            timerWork.GetSubmit(mainTime, restTime);    //두 타이머 값 확인 및 제출
            gameObject.SetActive(false);                //타이머 설정 창 종료
        }
        else
        {
            Debug.Log("they can't be zero");            //두 타이머 값이 0이면 타이머 설정 거부
        }
            
    }
}
