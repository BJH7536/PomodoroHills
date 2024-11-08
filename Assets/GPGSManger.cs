using UnityEngine;
using GooglePlayGames;
using TMPro;

public class GPGSManager : MonoBehaviour
{
    public static GPGSManager Instance { get; private set; } // 싱글톤 인스턴스

    public string LoginFlagKey = "IsLoggedIn"; // 로그인 여부를 확인하는 키

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 파괴되지 않도록 설정
            Initialize();
        }
        else
        {
            Destroy(gameObject); // 중복 인스턴스는 파괴
        }
    }

    private void Initialize()
    {
        PlayGamesPlatform.Activate(); // Play Games 플랫폼 활성화

        // PlayerPrefs에 로그인 플래그가 존재하면 자동 로그인 시도
        if (PlayerPrefs.GetInt(LoginFlagKey, 0) == 1)
        {
            AutoLogin();
        }
    }
    
    private void AutoLogin()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }

    internal void ProcessAuthentication(bool status)
    {
        if (status)
        {
            // 로그인 성공 시 PlayerPrefs에 플래그 저장
            PlayerPrefs.SetInt(LoginFlagKey, 1);
            PlayerPrefs.Save();

            // 유저 이름 표시 업데이트
            if (GameObject.Find("Label-Name").TryGetComponent(out TMP_Text text))
            {
                text.text = PlayGamesPlatform.Instance.GetUserDisplayName();
            }
        }
        else
        {
            // 로그인 실패 시 처리
            Debug.Log("Login failed, showing login button.");
            // 필요 시 로그인 버튼 표시 등 처리
        }
    }
}