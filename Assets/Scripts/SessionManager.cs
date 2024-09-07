using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using VInspector;

public class SessionManager : MonoBehaviour
{
    [Tab("Information")]
    [SerializeField] private TMP_InputField inputField_user_id;
    [SerializeField] private TMP_InputField inputField_name;
    [SerializeField] private TMP_InputField inputField_age;
    [SerializeField] private TMP_InputField inputField_height;
    [SerializeField] private TMP_InputField inputField_weight;
    [SerializeField] private Toggle toggle_email_verified;
    [SerializeField] private TMP_InputField inputField_first_signed_at;
    [SerializeField] private TMP_InputField inputField_interests;
    [SerializeField] private TMP_InputField inputField_address_street;
    [SerializeField] private TMP_InputField inputField_address_city;
    [SerializeField] private TMP_InputField inputField_address_postal_code;
    [SerializeField] private TMP_InputField inputField_profile_picture;
    [SerializeField] private TMP_InputField inputField_regular_expression;
    [SerializeField] private TMP_InputField inputField_last_login_timestamp;
    [SerializeField] private Button button_GetDataFromServer;
    [SerializeField] private Button button_PostDataToServer;
    [SerializeField] private Button button_ClearInputFields;
    [SerializeField] private Button button_UpdateDataToServer;

    
    [Tab("Other")]
    [SerializeField] private GameObject loading;
    [SerializeField] string getUrl = $"http://211.188.54.196:5000/user";
    [SerializeField] string postUrl = $"http://211.188.54.196:5000/user";
    
    private void Awake()
    {
        Application.targetFrameRate = 120;
        
        button_GetDataFromServer.onClick.AddListener(GetDataFromServer);
        button_PostDataToServer.onClick.AddListener(() => PostDataToServer().Forget());
        button_UpdateDataToServer.onClick.AddListener(()=> UpdateDataToServer().Forget());
        button_ClearInputFields.onClick.AddListener(ClearInputFields);
    }

    /// <summary>
    /// 서버로부터 데이터 받아와서 UI에 적용
    /// </summary>
    async void GetDataFromServer()
    {
        string jsonData = await UnityWebRequestGet();

        Debug.Log($"Received jsonData : {jsonData}");
        
        // Information을 파싱하기 전에 address는 string으로 받아야 함
        Information info = JsonConvert.DeserializeObject<Information>(jsonData);

        // address 필드를 다시 Address 객체로 역직렬화
        Address address = JsonConvert.DeserializeObject<Address>(info.address);

        // 파싱된 데이터를 UI에 적용
        inputField_user_id.text = info.user_id;
        inputField_name.text = info.name;
        inputField_age.text = info.age.ToString();
        inputField_height.text = info.height.ToString(CultureInfo.CurrentCulture);
        inputField_weight.text = info.weight.ToString(CultureInfo.CurrentCulture);
        toggle_email_verified.isOn = info.email_verified;
        inputField_first_signed_at.text = info.first_signed_at.ToString(CultureInfo.CurrentCulture);

        // interests를 다시 리스트로 변환
        List<string> interests = JsonConvert.DeserializeObject<List<string>>(info.interests);
        inputField_interests.text = string.Join("|", interests);

        // 역직렬화된 address 데이터 적용
        inputField_address_street.text = address.street;
        inputField_address_city.text = address.city;
        inputField_address_postal_code.text = address.zipcode;
        inputField_profile_picture.text = info.profile_picture;
        inputField_regular_expression.text = info.regular_expression;
        inputField_last_login_timestamp.text = info.last_login.ToString(CultureInfo.CurrentCulture);
    }
    
    /// <summary>
    /// HTTP Get으로 서버로부터 데이터 받아오기
    /// </summary>
    /// <returns></returns>
    async UniTask<string> UnityWebRequestGet()
    {
        loading.SetActive(true);
        
        UnityWebRequest uwr = UnityWebRequest.Get($"{getUrl}/{inputField_user_id.text}");
        await uwr.SendWebRequest();
        
        if (uwr.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("ERROR: " + uwr.error);
            loading.SetActive(false);
            return string.Empty;
        }

        loading.SetActive(false);
        return uwr.downloadHandler.text;
    }
    
    /// <summary>
    /// HTTP Post로 서버로 데이터 보내기
    /// </summary>
    async UniTaskVoid PostDataToServer()
    {
        // 전송할 데이터 생성
        Information info = new Information
        {
            user_id = inputField_user_id.text.Trim(),
            name = inputField_name.text.Trim(),
            age = int.Parse(inputField_age.text),
            height = float.Parse(inputField_height.text),
            weight = float.Parse(inputField_weight.text),
            email_verified = toggle_email_verified.isOn,
            first_signed_at = DateTime.Parse(inputField_first_signed_at.text),
            interests = JsonConvert.SerializeObject(inputField_interests.text.Trim().Split("|")),
            address = JsonConvert.SerializeObject(new Address
            {
                street = inputField_address_street.text.Trim(),
                city = inputField_address_city.text.Trim(),
                zipcode = inputField_address_postal_code.text.Trim()
            }),
            profile_picture = inputField_profile_picture.text.Trim(),
            regular_expression = inputField_regular_expression.text.Trim(),
            last_login = DateTime.Parse(inputField_last_login_timestamp.text.Trim())
        };

        string jsonData = JsonConvert.SerializeObject(info);
        Debug.Log(jsonData);
        
        // UnityWebRequest.Post 사용
        UnityWebRequest request = UnityWebRequest.PostWwwForm(postUrl, jsonData);
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        
        // 요청 전송 및 응답 대기
        await request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("데이터 전송 성공");
        }
        else
        {
            Debug.LogError("데이터 전송 실패: " + request.error);
        }
    }

    /// <summary>
    /// HTTP Put으로 서버 데이터 갱신하기
    /// </summary>
    async UniTaskVoid UpdateDataToServer()
    {
        // 전송할 데이터 생성 (기존의 데이터 수정)
        Information info = new Information
        {
            user_id = inputField_user_id.text.Trim(),
            name = inputField_name.text.Trim(),
            age = int.Parse(inputField_age.text),
            height = float.Parse(inputField_height.text),
            weight = float.Parse(inputField_weight.text),
            email_verified = toggle_email_verified.isOn,
            first_signed_at = DateTime.Parse(inputField_first_signed_at.text),
            interests = JsonConvert.SerializeObject(inputField_interests.text.Trim().Split("|")),
            address = JsonConvert.SerializeObject(new Address
            {
                street = inputField_address_street.text.Trim(),
                city = inputField_address_city.text.Trim(),
                zipcode = inputField_address_postal_code.text.Trim()
            }),
            profile_picture = inputField_profile_picture.text.Trim(),
            regular_expression = inputField_regular_expression.text.Trim(),
            last_login = DateTime.Parse(inputField_last_login_timestamp.text.Trim())
        };

        // JSON 데이터 직렬화
        string jsonData = JsonConvert.SerializeObject(info);
        Debug.Log(jsonData);
    
        // UnityWebRequest 사용하여 PUT 요청
        UnityWebRequest request = UnityWebRequest.Put($"{postUrl}/{inputField_user_id.text}", jsonData);
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // 요청 전송 및 응답 대기
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("데이터 갱신 성공");
        }
        else
        {
            Debug.LogError("데이터 갱신 실패: " + request.error);
        }
    }
    
    public void ClearInputFields()
    {
        inputField_user_id.text = string.Empty;
        inputField_name.text = string.Empty;
        inputField_age.text = string.Empty;
        inputField_height.text = string.Empty;
        inputField_weight.text = string.Empty;
        toggle_email_verified.isOn = false;
        inputField_first_signed_at.text = string.Empty;
        inputField_interests.text = string.Empty;
        inputField_address_street.text = string.Empty;
        inputField_address_city.text = string.Empty;
        inputField_address_postal_code.text = string.Empty;
        inputField_profile_picture.text = string.Empty;
        inputField_regular_expression.text = string.Empty;
        inputField_last_login_timestamp.text = string.Empty;
    }
}

[System.Serializable]
public class Information
{
    public string user_id;
    public string name;
    public int age;
    public float height;
    public float weight;
    public bool email_verified;
    public DateTime first_signed_at;
    public string interests;            // JSON 문자열로 파싱된 데이터를 나중에 List로 변환
    public string address;              // string으로 먼저 받아야 함
    public string profile_picture;
    public string regular_expression;
    public DateTime last_login;
}

[System.Serializable]
public class Address
{
    public string street;
    public string city;
    public string zipcode;
}