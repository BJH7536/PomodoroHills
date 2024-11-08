// using TMPro;
// using UnityEngine;
// using GooglePlayGames;
// using GooglePlayGames.BasicApi;
// using UnityEngine.UI;
//
// public class GPGSManager : MonoBehaviour
// {
//     [SerializeField] private WelcomeManager _welcomeManager;
//     
//     [SerializeField] private TMP_Text DebugText;
//     [SerializeField] private Button SignInButton;
//
//     private void Start()
//     {
//         SignInButton.onClick.AddListener(GPGS_LogIn);
//     }
//
//     /// <summary>
//     /// GPGS 로그인 시도
//     /// </summary>
//     public void GPGS_LogIn()
//     {
//         PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
//     }
//
//     private void ProcessAuthentication(SignInStatus status) {
//         
//         
//         if (status == SignInStatus.Success) {
//             // 로그인에 성공
//             
//             string displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
//             string userID = PlayGamesPlatform.Instance.GetUserId();
//             
//             DebugText.text = $"Login Success! : {displayName} / {userID}";
//
//             // 버튼 비활성화
//             SignInButton.interactable = false;
//             
//             // 로그인 정보를 기기에 기록
//             _welcomeManager.SaveLoginStatus(true);
//             
//             // 팝업 끄기
//             _welcomeManager.DisableWelcomePopup();
//         } 
//         else 
//         {
//             // Disable your integration with Play Games Services or show a login button
//             // to ask users to sign-in. Clicking it should call
//             // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication).
//
//             DebugText.text = "Login Failed!";
//         }
//     }
//
//     public void ShowAchievementUI()
//     {
//         // 전체 업적 표시
//         PlayGamesPlatform.Instance.ShowAchievementsUI();
//     }
//     
//     public void UnlockAchievement_Welcome()
//     {
//         // 기본 업적 잠금해제 및 공개
//         PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_welcome, (bool success) => { });
//     }
//     
//     public void ProcessAchievement_Test()
//     {
//         // 단계별 업적 증가
//         PlayGamesPlatform.Instance.IncrementAchievement(GPGSIds.achievement_test_achievement,1,
//             (bool success) => { });
//     }
//
//     public void UnlockAchievement_Hidden()
//     {
//         // 기본 업적 잠금해제 및 공개
//         PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_hidden_achievement, (bool success) => { });
//     }
// }