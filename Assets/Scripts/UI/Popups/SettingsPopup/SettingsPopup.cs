using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : Popup
{
    [SerializeField] private Transform panel;
    
    private void OnEnable()
    {
        panel.localScale = Vector3.zero;
        panel.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }

    // private void InitializeSettings()
    // {
    //     // 예시: 저장된 설정 불러오기
    //     soundToggle.isOn = PlayerPrefs.GetInt("Sound", 1) == 1;
    //     musicToggle.isOn = PlayerPrefs.GetInt("Music", 1) == 1;
    //
    //     // 토글 이벤트 설정
    //     soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
    //     musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
    // }
    //
    // private void OnSoundToggleChanged(bool isOn)
    // {
    //     PlayerPrefs.SetInt("Sound", isOn ? 1 : 0);
    //     // 추가 로직: 사운드 설정 반영
    // }
    //
    // private void OnMusicToggleChanged(bool isOn)
    // {
    //     PlayerPrefs.SetInt("Music", isOn ? 1 : 0);
    //     // 추가 로직: 음악 설정 반영
    // }

    public void Close()
    {
        PopupManager.Instance.HidePopup();
    }
}