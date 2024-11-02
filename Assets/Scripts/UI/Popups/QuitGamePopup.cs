using System;
using UnityEngine;

public class QuitGamePopup : ConfirmPopup
{
    private void Awake()
    {
        Setup("���� �����Ͻðڽ��ϱ�?", "������ �����Ͻǰſ���?", QuitGame,null);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // ���ø����̼� ����
#endif
    }
}
