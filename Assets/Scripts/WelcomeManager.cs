using System;
using UnityEngine;

public class WelcomeManager : MonoBehaviour
{
    [SerializeField] private GameObject welcomePopup;
    
    private const string LoggedInKey = "isLoggedIn";

    private void Awake()
    {
        // 기기에 로그인 기록이 남아있는지 여부에 따라 사용자 로그인 팝업을 띄우거나, 안띄운다.
        welcomePopup.SetActive(!IsLoggedIn());
    }

    // 기기에 로그인 기록이 있는지 확인하는 함수
    public bool IsLoggedIn()
    {
        return PlayerPrefs.GetInt(LoggedInKey, 0) == 1;
    }

    // 로그인 기록을 저장하는 함수
    public void SaveLoginStatus(bool isLoggedIn)
    {
        PlayerPrefs.SetInt(LoggedInKey, isLoggedIn ? 1 : 0);
        PlayerPrefs.Save(); // 저장을 즉시 적용
    }

    // 디버그를 위해 로그인 기록을 삭제하는 함수
    public void ClearLoginStatus()
    {
        PlayerPrefs.DeleteKey(LoggedInKey);
        PlayerPrefs.Save(); // 삭제 후 저장을 적용
    }

    public void DisableWelcomePopup()
    {
        welcomePopup.SetActive(false);
    }
}
