using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using VInspector;

[Serializable]
public class Placeable : MonoBehaviour
{
    public int id;
    public Vector2Int size;         //(x,z)
    public Vector2Int position;     //(x,z)
    public int rotation;            // n * 90

    private void Awake()
    {
        InitializeRenderers();
    }
    
    #region Color
    
    private struct RendererInfo
    {
        public Renderer renderer;
        public Color originalColor;
    }

    private List<RendererInfo> renderers = new List<RendererInfo>();

    
    private void InitializeRenderers()
    {
        // 자신과 모든 자식 오브젝트에서 Renderer를 찾아 저장합니다.
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            RendererInfo info = new RendererInfo();
            info.renderer = renderer;

            // 원래 색상을 저장합니다.
            if (renderer.material.HasProperty("_Color"))
            {
                info.originalColor = renderer.material.color;
            }
            else
            {
                info.originalColor = Color.white; // 기본 색상 설정
            }

            renderers.Add(info);
        }
    }

    [Button] // 오브젝트를 초록색으로 만드는 함수
    public void SetGreenColor()
    {
        SetColor(Color.green);
    }

    [Button] // 오브젝트를 빨간색으로 만드는 함수
    public void SetRedColor()
    {
        SetColor(Color.red);
    }

    [Button] // 오브젝트를 원래 색상으로 복구하는 함수
    public void ResetColor()
    {
        foreach (RendererInfo info in renderers)
        {
            if (info.renderer.material.HasProperty("_Color"))
            {
                info.renderer.material.color = info.originalColor;
            }
        }
    }

    // 색상을 설정하는 내부 함수
    private void SetColor(Color color)
    {
        foreach (RendererInfo info in renderers)
        {
            if (info.renderer.material.HasProperty("_Color"))
            {
                info.renderer.material.color = color;
            }
        }
    }

    #endregion
}


[Serializable]
public class PlaceableData
{
    public string type; // 객체의 타입 정보 ("Placeable" 또는 "FarmBuilding")
    public int id;
    public Vector2Int size;
    public Vector2Int position;
    public int rotation;
}