using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapManager : MonoBehaviour
{
    public CinemachineTargetGroup TargetGroup; //카메라 임시용


    [SerializeField]
    GameObject tilePrefab;

    public int gridX = 5;      // x,z 그리드 타일맵
    public int gridZ = 5;      //


    Dictionary<Vector2Int, Tile> tileMap = new Dictionary<Vector2Int, Tile>();      //타일 점유여부체크
    Dictionary<Vector2Int, GameObject> tileMapObject = new Dictionary<Vector2Int, GameObject>();





    private void Awake()
    {
        CreateTileMap();
    }



    void CreateTileMap()
    {
        for(int x = 0; x < gridX; x++)
        {
            for(int z = 0; z < gridZ; z++)
            {
                Vector2Int position = new Vector2Int(x, z);// 타일 공간 생성
                if (!tileMap.ContainsKey(position))
                {
                    Tile tile = new Tile(position);
                    tileMap[position] = tile;
                }

                Vector3 worldPosition = new Vector3(x, 0, z);// 시각적으로 보이는 타일 오브젝트 생성
                if (!tileMapObject.ContainsKey(position))
                {
                    GameObject tileObject = Instantiate(tilePrefab, worldPosition, Quaternion.identity);
                    tileMapObject[position] = tileObject;

                    if (TargetGroup != null) { TargetGroup.AddMember(tileObject.transform, 0.5f, 0.5f); }
                }
            }
        }
    }



    public bool IsTileAvailable(Vector2Int position)    //타일을 쓸수있는지 확인함 - 점유상태 반환
    {
        return tileMap.ContainsKey(position) && !tileMap[position].isOccupied; // 점유 상태 확인
    }

    public void OccupyTile(Vector2Int position)
    {
        tileMap[position].Occupy();
    }

    public void FreeTile(Vector2Int position)
    {
        tileMap[position].Free();
    }

    private void UpdateTileVisual(Vector2Int position)
    {
        // 타일의 시각적 요소를 업데이트
        GameObject tileObject = tileMapObject[position];
        Renderer renderer = tileObject.GetComponent<Renderer>();

        if (renderer != null)
        {
            Color tileColor = tileMap[position].isOccupied ? Color.red : Color.white; // 예: 점유 상태에 따라 색상 변경
            renderer.material.color = tileColor;
        }
    }

}
