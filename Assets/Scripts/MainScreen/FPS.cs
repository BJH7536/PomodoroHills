using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPS : MonoBehaviour
{
    [SerializeField] TMP_Text _fpsText = null;

    List<string> _dropDownOptionList = new();

    float _fpsUpdateTimer = 0f;

    void Start()
    {
        Application.targetFrameRate = 120;
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
