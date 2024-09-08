using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TileMapManager : MonoBehaviour
{
    public static TileMapManager Instance { get; private set; }
    public CinemachineTargetGroup TargetGroup; //카메라 임시용


    [SerializeField]
    GameObject tilePrefab;
    [SerializeField]
    GameObject maintilePrefab;
    GameObject maintile;
    

    public int gridX = 5;      // x,z 그리드 타일맵
    public int gridZ = 5;      //

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



    void CreateTileMap()    //타일맵 클래스 및 오브젝트 nxn개 생성
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

                
                /*
                Vector3 worldPosition = new Vector3(x, 0, z);// 시각적으로 보이는 타일 오브젝트 생성
                if (!tileMapObject.ContainsKey(position))
                {
                    GameObject tileObject = Instantiate(tilePrefab, worldPosition, Quaternion.identity);
                    tileMapObject[position] = tileObject;

                    if (TargetGroup != null) { TargetGroup.AddMember(tileObject.transform, 0.5f, 0.5f); }
                }*/
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
        /*switch (rotation)
        {
            case 1: //90도
                for (int x = position.x; x < position.x + size.y; x++)
                {
                    for (int y = position.y - size.x + 1; y <= position.y; y++) 
                    {
                        if (!GetTileAvailable(new Vector2Int(x, y))) { return false; }
                    }
                }
                break;
            case 2: //180도
                for (int x = position.x - size.x + 1; x <= position.x; x++)
                {
                    for (int y = position.y - size.y + 1; y <= position.y; y++)
                    {
                        if (!GetTileAvailable(new Vector2Int(x, y))) { return false; }
                    }
                }
                break;
            case 3: //270도
                for (int x = position.x - size.y + 1; x <= position.x; x++)
                {
                    for (int y = position.y; y < position.y + size.x; y++)
                    {
                        if (!GetTileAvailable(new Vector2Int(x, y))) { return false; }
                    }
                }
                break;
            default:
                for (int x = position.x; x < position.x + size.x; x++)
                {
                    for (int y = position.y; y < position.y + size.y; y++)
                    {
                        if (!GetTileAvailable(new Vector2Int(x, y))) { return false; }
                    }
                }
                break;
        }*/
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
            case 3: // 회전 90도
                for (int x = position.x - size.y + 1; x <= position.x; x++)
                {
                    for (int y = position.y; y < position.y + size.x; y++)
                    {
                        if (!GetTileAvailable(new Vector2Int(x, y))) { return false; }
                    }
                }
                break;
            default:
                Debug.LogWarning("Invalid rotation value: " + rotation);
                break;
        }
        Debug.Log("x");
        return true;
    }
    public bool GetTileAvailable(Vector2Int position)    //타일을 쓸수있는지 확인함 - 점유상태 반환
    {
        if (tileMap.ContainsKey(position))
        {
            tileMap.TryGetValue(position, out Tile tile);
            //Debug.Log($"Checking tile at position {position}: Occupied = {tile.isOccupied}");
            return !tile.isOccupied;
        }
        else
        {
            Debug.Log($"Tile not found at position {position}");
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
                Debug.LogWarning("Invalid rotation value: " + rotation);
                break;
        }
    }

    public void OccupyTile(Vector2Int position)     //타일점유 시작
    {
        tileMap[position].Occupy();
    }

    public void FreeTile(Vector2Int position)       //타일점유 해제
    {
        tileMap[position].Free();
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
