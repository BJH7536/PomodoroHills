using System.Collections.Generic;
using UnityEngine;

public class GridManager
{
    private Dictionary<Vector2Int, bool> occupiedGrids = new Dictionary<Vector2Int, bool>();
    private float gridSize;

    public GridManager(float gridSize)
    {
        this.gridSize = gridSize;
    }

    // 건물이 차지하는 모든 격자를 점유 상태로 기록
    public void MarkGridAsOccupied(Vector3 position, GameObject building)
    {
        Bounds bounds = building.GetComponent<Renderer>().bounds;
        
        // position을 기준으로 건물의 Bound를 조정
        bounds.center = position; // 배치될 위치를 Bound 중심에 반영
        
        // 건물의 실제 Bound를 기준으로 격자를 점유합니다.
        int gridMinX = Mathf.FloorToInt(bounds.min.x / gridSize);
        int gridMaxX = Mathf.CeilToInt(bounds.max.x / gridSize);
        int gridMinZ = Mathf.FloorToInt(bounds.min.z / gridSize);
        int gridMaxZ = Mathf.CeilToInt(bounds.max.z / gridSize);

        for (int x = gridMinX; x < gridMaxX; x++)
        {
            for (int z = gridMinZ; z < gridMaxZ; z++)
            {
                Vector2Int gridPos = new Vector2Int(x, z);
                occupiedGrids[gridPos] = true; // 해당 격자를 점유 상태로 설정
            }
        }
    }

    // 해당 위치에 건물을 지을 수 있는지 확인
    public bool CanPlaceBuilding(Vector3 position, GameObject building)
    {
        // 건물의 크기를 기준으로 Bound 계산
        Bounds bounds = building.GetComponent<Renderer>().bounds;
    
        // position을 기준으로 건물의 Bound를 조정
        bounds.center = position; // 배치될 위치를 Bound 중심에 반영

        // 건물이 차지하는 격자의 범위 계산 (min ~ max)
        int gridMinX = Mathf.FloorToInt((bounds.min.x) / gridSize);
        int gridMaxX = Mathf.CeilToInt((bounds.max.x) / gridSize);
        int gridMinZ = Mathf.FloorToInt((bounds.min.z) / gridSize);
        int gridMaxZ = Mathf.CeilToInt((bounds.max.z) / gridSize);

        // 건물이 차지하는 모든 격자가 비어 있는지 확인
        for (int x = gridMinX; x < gridMaxX; x++)
        {
            for (int z = gridMinZ; z < gridMaxZ; z++)
            {
                Vector2Int gridPos = new Vector2Int(x, z);
                if (occupiedGrids.ContainsKey(gridPos) && occupiedGrids[gridPos])
                {
                    return false; // 이미 점유된 격자가 있으면 건물을 지을 수 없음
                }
            }
        }
        return true; // 모든 격자가 비어 있으면 건물 배치 가능
    }
    
    // 해당 격자가 점유되었는지 확인하는 함수
    public bool IsGridOccupied(int x, int z)
    {
        Vector2Int gridPos = new Vector2Int(x, z);
        return occupiedGrids.ContainsKey(gridPos) && occupiedGrids[gridPos];
    }
    
    // occupiedGrids 반환 메서드
    public Dictionary<Vector2Int, bool> GetOccupiedGrids()
    {
        return occupiedGrids;
    }
}
