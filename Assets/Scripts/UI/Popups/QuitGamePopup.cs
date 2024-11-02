using System;
using UnityEngine;

public class QuitGamePopup : ConfirmPopup
{
    private void Awake()
    {
        Setup("정말 종료하시겠습니까?", "정말로 종료하실거에요?", QuitGame,null);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }
}
