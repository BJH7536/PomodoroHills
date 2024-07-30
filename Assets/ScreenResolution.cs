using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScreenResolution : MonoBehaviour
{
    [SerializeField] private TMP_Text resolutionText;
    
    void Update()
    {
        // 현재 화면 해상도를 가져옴
        int width = Screen.width;
        int height = Screen.height;

        // 해상도를 텍스트로 설정
        resolutionText.text = $"Resolution: {width} x {height}";
    }

}
