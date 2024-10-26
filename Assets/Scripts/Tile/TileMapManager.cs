using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class TileMapManager : MonoBehaviour
{
    public static TileMapManager Instance { get; private set; }
    public CinemachineTargetGroup TargetGroup; //카메라 임시용


    [SerializeField]        // 미사용
    GameObject tilePrefab;  // 미사용
    [SerializeField]
    GameObject maintilePrefab;
    GameObject maintile;
    

    public int gridX = 5;      // x,z 그리드 타일맵
    public int gridZ = 5;      // 사용자 진척도에 따라 

    public Dictionary<Vector2Int, Tile> tileMap = new Dictionary<Vector2Int, Tile>();      //타일 점유여부체크


    Dictionary<Vector2Int, GameObject> tileMapObject = new Dictionary<Vector2Int, GameObject>();    //1x1타일 시각화(미사용)





    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }   //싱글톤 선언
        else
        {
            Destroy(gameObject);
        }


        CreateTileMap();
    }



    void CreateTileMap()    //타일맵 클래스 및 오브젝트 nxn개 생성 (gridx,gridz가 늘어날때도 사용)
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

            }
        }
        Vector3 mainTilePosition = new Vector3(gridX/2,-0.5f,gridZ/2);
        Vector3 targetScale = new Vector3(gridX,1f,gridZ);
        maintile = Instantiate(maintilePrefab,mainTilePosition,Quaternion.identity);
        maintile.transform.localScale = targetScale;
        if (TargetGroup != null) { TargetGroup.AddMember(maintile.transform, 3f, 3f); }
    }



    public bool GetEveryTileAvailable(Vector2Int size, Vector2Int position, int rotation)   //GetTileAvailable을 타일에 놓일 모든 자리에서 실행
    {
        switch (rotation)
        {
            case 0: // 기본 회전 (0도)
                for (int x = position.x; x < position.x + size.x; x++)
                {
                    for (int y = position.y; y < position.y + size.y; y++)
                    {
                        if (!GetTileAvailable(new Vector2Int(x, y))) { return false; }
                    }
                }
                break;
            case 1: // 회전 90도
                for (int x = position.x; x < position.x + size.y; x++)
                {
                    for (int y = position.y - size.x + 1; y <= position.y; y++)
                    {
                        if (!GetTileAvailable(new Vector2Int(x, y))) { return false; }
                    }
                }
                break;
            case 2: // 회전 180도
                for (int x = position.x; x > position.x - size.x; x--)
                {
                    for (int y = position.y; y > position.y - size.y; y--)
                    {
                        if (!GetTileAvailable(new Vector2Int(x, y))) { return false; }
                    }
                }
                break;
            case 3: // 회전 270도
                for (int x = position.x - size.y + 1; x <= position.x; x++)
                {
                    for (int y = position.y; y < position.y + size.x; y++)
                    {
                        if (!GetTileAvailable(new Vector2Int(x, y))) { return false; }
                    }
                }
                break;
            default:
                DebugEx.LogWarning("Invalid rotation value: " + rotation);
                break;
        }
        return true;
    }
    public bool GetTileAvailable(Vector2Int position)    //타일을 쓸수있는지 확인함 - 점유상태 반환
    {
        if (tileMap.ContainsKey(position))
        {
            tileMap.TryGetValue(position, out Tile tile);
            //DebugEx.Log($"Checking tile at position {position}: Occupied = {tile.isOccupied}");
            return !tile.isOccupied;
        }
        else
        {
            DebugEx.Log($"Tile not found at position {position}");
            return false;
        }
    }

    public void OccupyEveryTile(Vector2Int size, Vector2Int position, int rotation)
    {
        switch (rotation)
        {
            case 0: // 기본 회전 (0도)
                for (int x = position.x; x < position.x + size.x; x++)
                {
                    for (int y = position.y; y < position.y + size.y; y++)
                    {
                        OccupyTile(new Vector2Int(x, y));
                    }
                }
                break;
            case 1: // 회전 90도
                for (int x = position.x; x < position.x + size.y; x++)
                {
                    for (int y = position.y - size.x + 1; y <= position.y; y++)
                    {
                        OccupyTile(new Vector2Int(x, y));
                    }
                }
                break;
            case 2: // 회전 180도
                for (int x = position.x; x > position.x - size.x; x--)
                {
                    for (int y = position.y; y > position.y - size.y; y--)
                    {
                        OccupyTile(new Vector2Int(x, y));
                    }
                }
                break;
            case 3: // 회전 90도
                for (int x = position.x - size.y + 1; x <= position.x; x++)
                {
                    for (int y = position.y; y < position.y + size.x; y++)
                    {
                        OccupyTile(new Vector2Int(x, y));
                    }
                }
                break;
            default:
                DebugEx.LogWarning("Invalid rotation value: " + rotation);
                break;
        }
    }





    public void OccupyTile(Vector2Int position)     //타일점유 시작
    {
        tileMap[position].Occupy();
    }

    public void FreeEveryTile(Vector2Int size, Vector2Int prePosition, int preRotation)
    {
        switch (preRotation)
        {
            case 0: // 기본 회전 (0도)
                for (int x = prePosition.x; x < prePosition.x + size.x; x++)
                {
                    for (int y = prePosition.y; y < prePosition.y + size.y; y++)
                    {
                        FreeTile(new Vector2Int(x, y));
                    }
                }
                break;
            case 1: // 회전 90도
                for (int x = prePosition.x; x < prePosition.x + size.y; x++)
                {
                    for (int y = prePosition.y - size.x + 1; y <= prePosition.y; y++)
                    {
                        FreeTile(new Vector2Int(x, y));
                    }
                }
                break;
            case 2: // 회전 180도
                for (int x = prePosition.x; x > prePosition.x - size.x; x--)
                {
                    for (int y = prePosition.y; y > prePosition.y - size.y; y--)
                    {
                        FreeTile(new Vector2Int(x, y));
                    }
                }
                break;
            case 3: // 회전 90도
                for (int x = prePosition.x - size.y + 1; x <= prePosition.x; x++)
                {
                    for (int y = prePosition.y; y < prePosition.y + size.x; y++)
                    {
                        FreeTile(new Vector2Int(x, y));
                    }
                }
                break;
            default:
                DebugEx.LogWarning("Invalid rotation value: " + preRotation);
                break;
        }
    }
    public void FreeTile(Vector2Int position)       //타일점유 해제
    {
        if (tileMap.ContainsKey(position))
        {
            tileMap[position].Free();
        }
    }






    private void UpdateTileVisual(Vector2Int position)      //타일 오브젝트의 비주얼을 업데이트 하기 위한 메소드로 현재 미사용
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
