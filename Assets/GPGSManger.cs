using UnityEngine;
using GooglePlayGames;
using TMPro;

public class GPGSManager : MonoBehaviour
{
    public static GPGSManager Instance { get; private set; } // �̱��� �ν��Ͻ�

    public string LoginFlagKey = "IsLoggedIn"; // �α��� ���θ� Ȯ���ϴ� Ű

    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �ÿ��� �ı����� �ʵ��� ����
            Initialize();
        }
        else
        {
            Destroy(gameObject); // �ߺ� �ν��Ͻ��� �ı�
        }
    }

    private void Initialize()
    {
        PlayGamesPlatform.Activate(); // Play Games �÷��� Ȱ��ȭ

        // PlayerPrefs�� �α��� �÷��װ� �����ϸ� �ڵ� �α��� �õ�
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
            // �α��� ���� �� PlayerPrefs�� �÷��� ����
            PlayerPrefs.SetInt(LoginFlagKey, 1);
            PlayerPrefs.Save();

            // ���� �̸� ǥ�� ������Ʈ
            if (GameObject.Find("Label-Name").TryGetComponent(out TMP_Text text))
            {
                text.text = PlayGamesPlatform.Instance.GetUserDisplayName();
            }
        }
        else
        {
            // �α��� ���� �� ó��
            Debug.Log("Login failed, showing login button.");
            // �ʿ� �� �α��� ��ư ǥ�� �� ó��
        }
    }
}