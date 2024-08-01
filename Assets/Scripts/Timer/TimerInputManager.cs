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
        // �� �Է��ʵ忡 �� �Է½� �ִ����(2)�Է� �� ���� �Է��ʵ��� �Է°��� �����ϰ� ���� �Է��ʵ�� �̵���
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
    // �� �Է��ʵ忡 �� �Է½� �ִ����(2)�Է� �� ���� �Է��ʵ��� �Է°��� �����ϰ� ���� �Է��ʵ�� �̵���

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
                // ��ȿ�� ���� �ƴ� ��� �ִ밪���� ����
                currentField.text = (maxValue - 1).ToString("D2"); // �� �ڸ� �������� �����մϴ�.
            }
        }
    }

    public void OnSubmit()                      //Ÿ�̸� ���� Ȯ�ι�ư�� ���
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
            timerWork.GetSubmit(mainTime, restTime);    //�� Ÿ�̸� �� Ȯ�� �� ����
            gameObject.SetActive(false);                //Ÿ�̸� ���� â ����
        }
        else
        {
            Debug.Log("they can't be zero");            //�� Ÿ�̸� ���� 0�̸� Ÿ�̸� ���� �ź�
        }
            
    }
}
