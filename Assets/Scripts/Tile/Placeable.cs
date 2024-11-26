using System;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

[Serializable]
public class Placeable : MonoBehaviour
{
    public int id;
    public Vector2Int position;     //(x,z)
    public Vector2Int size;         //(x,z)
    public int rotation;            // n * 90

    private void Reset()
    {
        SetPosition();
    }

    private void Awake()
    {
        InitializeRenderers();
    }

    [ContextMenu("SetPosition")]
    public void SetPosition()
    {
        position = new Vector2Int((int) transform.position.x, (int) transform.position.z);
    }
    
    #region Color
    
    private class  RendererInfo
    {
        public Renderer renderer;           // 해당 Renderer 컴포넌트 참조
        public Material originalMaterial;   // 원본 Material 저장
        public Material instanceMaterial;   // 색상 변경을 위해 생성된 Material의 인스턴스를 저장
    }

    private List<RendererInfo> renderers = new List<RendererInfo>();

    
    private void InitializeRenderers()
    {
        renderers = new List<RendererInfo>();
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            RendererInfo info = new RendererInfo();
            info.renderer = renderer;
            info.originalMaterial = renderer.sharedMaterial; // 원본 Material 저장
            info.instanceMaterial = null;
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
            if (info.instanceMaterial != null)
            {
                info.renderer.material = info.originalMaterial; // 원본 Material로 복구
                Destroy(info.instanceMaterial); // 인스턴스 Material 제거
                info.instanceMaterial = null;
            }
        }
    }

    private void OnDestroy()
    {
        // 인스턴스 Material이 남아있다면 제거
        foreach (RendererInfo info in renderers)
        {
            if (info.instanceMaterial != null)
            {
                Destroy(info.instanceMaterial);
                info.instanceMaterial = null;
            }
        }
    }

    // 색상을 설정하는 내부 함수
    private void SetColor(Color color)
    {
        foreach (RendererInfo info in renderers)
        {
            // 이미 인스턴스 Material이 있으면 재사용
            Material material = info.instanceMaterial;
            if (material == null)
            {
                material = new Material(info.originalMaterial); // 인스턴스 Material 생성
                info.instanceMaterial = material;
            }

            if (material.HasProperty("_Color"))
            {
                material.color = color;
            }

            info.renderer.material = material; // 인스턴스 Material 적용
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