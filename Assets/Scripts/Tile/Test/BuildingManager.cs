using System.Text;
using MoreMountains.Feedbacks;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public GameObject buildingPrefab;   // 건물 프리팹
    public float gridSize = 1.0f;       // 격자의 크기
    public Color gridColor = Color.green;   // 비어있는 격자 색상
    public Color occupiedGridColor = Color.red; // 점유된 격자 색상
    public float previewAlpha = 0.5f;   // 미리보기 상태의 투명도
    private GameObject previewBuilding; // 미리보기 건물
    private GameObject currentBuilding; // 현재 배치된 건물
    private GridManager gridManager;    // 격자 관리 시스템
    public int gridWidth = 20;          // 가로 그리드 개수
    public int gridHeight = 20;         // 세로 그리드 개수

    void Start()
    {
        // GridManager 초기화
        gridManager = new GridManager(gridSize);

        // 미리보기 건물 생성
        previewBuilding = Instantiate(buildingPrefab);

        // Collider를 비활성화하여 미리보기 건물이 충돌하지 않도록 설정
        foreach (var collider in previewBuilding.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        // 미리보기 건물의 Appearance 설정 (Material 복사본 사용)
        SetBuildingPreviewAppearance(previewBuilding);
        previewBuilding.SetActive(false); // 시작 시 미리보기 비활성화
    }

    void Update()
    {
        UpdatePreview(); // 마우스 위치에 따라 건물 미리보기 업데이트

        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭 시 건물 배치
        {
            PlaceBuilding();
        }
    }

    // 마우스 위치에 따라 건물 미리보기 업데이트
    void UpdatePreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            // 지면 위에 마우스가 있을 때만 미리보기 표시
            Vector3 placementPosition = Utility.SnapToGrid(hit.point, gridSize);
            previewBuilding.SetActive(true); // 미리보기 건물을 활성화
            previewBuilding.transform.position = placementPosition;

            // 투명도 설정
            SetBuildingPreviewAlpha(previewAlpha); // 투명도 50%
        }
        else
        {
            previewBuilding.SetActive(false); // 지면에 마우스가 없으면 미리보기를 비활성화
        }
    }

    // 마우스 클릭 시 건물 배치
    void PlaceBuilding()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit))
        {
            Vector3 placementPosition = Utility.SnapToGrid(hit.point, gridSize);
            
            // 충돌 확인 후 건물 배치
            if (gridManager.CanPlaceBuilding(placementPosition, buildingPrefab))
            {
                currentBuilding = Instantiate(buildingPrefab, placementPosition, Quaternion.identity);
                currentBuilding.GetComponent<MMF_Player>().PlayFeedbacks();
                gridManager.MarkGridAsOccupied(placementPosition, buildingPrefab);
            }
        }
    }

    // 미리보기 건물의 Appearance 설정: Material 복사본 사용
    void SetBuildingPreviewAppearance(GameObject building)
    {
        foreach (var renderer in building.GetComponentsInChildren<MeshRenderer>())
        {
            // Material의 복사본을 생성하여 미리보기 상태에서만 사용
            foreach (var material in renderer.materials)
            {
                Material newMaterial = new Material(material); // Material 복사본 생성
                newMaterial.SetFloat("_Surface", 1); // Surface Type을 Transparent로 설정 (1은 Transparent)
                newMaterial.SetFloat("_AlphaClip", 0); // Alpha 클리핑 비활성화
                newMaterial.renderQueue = 3000; // Render Queue 설정 (3000은 Transparent 큐)
                newMaterial.SetInt("_ZWrite", 0); // ZWrite 비활성화
                newMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                newMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                renderer.material = newMaterial; // 복사된 Material을 할당
            }
        }
    }

    // 미리보기 건물의 투명도를 설정
    void SetBuildingPreviewAlpha(float alphaValue)
    {
        foreach (var renderer in previewBuilding.GetComponentsInChildren<MeshRenderer>())
        {
            foreach (var material in renderer.materials)
            {
                // Material에서 Alpha 값을 조정하여 투명도 설정
                if (material.HasProperty("_BaseColor"))
                {
                    // _BaseColor에서 기존 색상을 가져와서 Alpha 값만 수정
                    Color originalColor = material.GetColor("_BaseColor");
                    originalColor.a = alphaValue; // Alpha 값 조정
                    material.SetColor("_BaseColor", originalColor);
                }
            }
        }
    }
    
    // 격자 시각화
    private void OnDrawGizmos()
    {
        if (gridManager == null)
        {
            return;
        }

        for (int x = 0 - gridWidth / 2; x < gridWidth / 2; x++)
        {
            for (int z = 0 - gridHeight / 2; z < gridHeight / 2; z++)
            {
                Vector3 start = new Vector3(x * gridSize, 0, z * gridSize);
                Vector3 endX = start + Vector3.right * gridSize;
                Vector3 endZ = start + Vector3.forward * gridSize;

                if (gridManager.IsGridOccupied(x, z))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(start + new Vector3(gridSize / 2, 0, gridSize / 2), new Vector3(gridSize, 0.1f, gridSize));
                }
                else
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(start, endX);
                    Gizmos.DrawLine(start, endZ);
                }
            }
        }
    }
    
    // Custom Editor에서 호출될 함수
    public void DisplayOccupiedGrids()
    {
        if (gridManager == null)
        {
            DebugEx.Log("GridManager가 초기화되지 않았습니다.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Occupied Grids:");

        foreach (var grid in gridManager.GetOccupiedGrids())
        {
            sb.AppendLine($"Grid Position: {grid.Key}, Occupied: {grid.Value}");
        }

        DebugEx.Log(sb.ToString());
    }
}
