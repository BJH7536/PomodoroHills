using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPS : MonoBehaviour
{
    [SerializeField] TMP_Dropdown _dropdown = null;
    [SerializeField] TMP_Text _fpsText = null;

    List<string> _dropDownOptionList = new();

    float _fpsUpdateTimer = 0f;

    void Start()
    {
        Application.targetFrameRate = 120;
        
        // _dropDownOptionList.Add(Application.targetFrameRate.ToString());
        // _dropDownOptionList.Add("30");
        // _dropDownOptionList.Add("60");
        // _dropDownOptionList.Add("80");
        // _dropDownOptionList.Add("120");
        // _dropDownOptionList.Add("144");
        //
        // _dropdown.AddOptions(_dropDownOptionList);
        
        _dropdown.ClearOptions();       // 기존 옵션 제거

        Resolution[] resolutions = Screen.resolutions;      // 장치가 지원하는 모든 해상도 가져오기
        HashSet<string> frameRate = new HashSet<string>();        // 중복 제거를 위해 HashSet 사용 
        
        // 각 해상도의 프레임레이트를 HashSet에 추가
        foreach (var res in resolutions)
        {
            frameRate.Add(res.refreshRateRatio.ToString());
        }
        
        // 프레임레이트 옵션을 드롭다운에 추가
        foreach (string fr in frameRate)
        {
            _dropDownOptionList.Add(fr.ToString() + " Hz");
        }
        _dropdown.AddOptions(_dropDownOptionList);
        
        _dropdown.onValueChanged.AddListener(ChangeFPS);
    }

    void Update()
    {
        float fps = 1.0f / Time.deltaTime;
        float ms = Time.deltaTime * 1000.0f;

        _fpsUpdateTimer += Time.deltaTime;

        if (_fpsUpdateTimer >= 0.1f)
        {
            _fpsUpdateTimer = 0f;
            _fpsText.text = $"{(int)fps} FPS ({ms:.0}ms)";
        }
    }

    private void ChangeFPS(int idx)
    {
        int fps = int.Parse(_dropDownOptionList[idx].Substring(0, _dropDownOptionList[idx].Length - 3));
        Application.targetFrameRate = fps;
    }
}
