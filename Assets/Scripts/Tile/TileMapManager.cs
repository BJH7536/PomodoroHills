using System.Text;
using UnityEngine;
using VInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileMapManager : MonoBehaviour
{
    public static TileMapManager Instance { get; private set; }

    public int gridSize = 1;      // 그리드 한 칸의 크기
    public int gridCountX = 25;   // x 방향 그리드 개수 (가로, 홀수)
    public int gridCountZ = 25;   // z 방향 그리드 개수 (세로, 홀수)

    private bool[,] tileStates;
    private int offsetX;
    private int offsetZ;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }   // 싱글톤 선언
        else
        {
            DestroyImmediate(gameObject);
        }
        CreateTileMap();
    }

    void CreateTileMap()
    {
        // 그리드의 최소 및 최대 인덱스 계산
        offsetX = gridCountX / 2;
        offsetZ = gridCountZ / 2;

        // 2차원 배열 초기화
        tileStates = new bool[gridCountX, gridCountZ];

        // 타일 상태 초기화
        for (int x = -offsetX; x <= offsetX; x++)
        {
            for (int z = -offsetZ; z <= offsetZ; z++)
            {
                int arrayX = x + offsetX;
                int arrayZ = z + offsetZ;
                tileStates[arrayX, arrayZ] = false;
            }
        }
    }

    // 그리드 인덱스를 배열 인덱스로 변환하는 함수
    private bool TryGetArrayIndices(Vector2Int gridPosition, out int arrayX, out int arrayZ)
    {
        arrayX = gridPosition.x + offsetX;
        arrayZ = gridPosition.y + offsetZ;

        if (arrayX >= 0 && arrayX < gridCountX && arrayZ >= 0 && arrayZ < gridCountZ)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public bool GetEveryTileAvailable(Vector2Int size, Vector2Int position, int rotation)   // GetTileAvailable을 타일에 놓일 모든 자리에서 실행
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

    public bool GetTileAvailable(Vector2Int position)
    {
        // tileStates가 null이면 초기화
        if (tileStates == null)
        {
            CreateTileMap();
        }

        if (TryGetArrayIndices(position, out int arrayX, out int arrayZ))
        {
            return !tileStates[arrayX, arrayZ];
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
            case 3: // 회전 270도
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

    public void OccupyTile(Vector2Int position)
    {
        // tileStates가 null이면 초기화
        if (tileStates == null)
        {
            CreateTileMap();
        }

        if (TryGetArrayIndices(position, out int arrayX, out int arrayZ))
        {
            tileStates[arrayX, arrayZ] = true;
        }
        else
        {
            Debug.Log($"Cannot occupy tile at position {position} - out of bounds");
        }
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
            case 3: // 회전 270도
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

    public void FreeTile(Vector2Int position)
    {
        // tileStates가 null이면 초기화
        if (tileStates == null)
        {
            CreateTileMap();
        }

        if (TryGetArrayIndices(position, out int arrayX, out int arrayZ))
        {
            tileStates[arrayX, arrayZ] = false;
        }
        else
        {
            Debug.Log($"Cannot free tile at position {position} - out of bounds");
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        // tileStates와 오프셋이 초기화되었는지 확인
        if (tileStates == null || offsetX == 0 || offsetZ == 0)
        {
            CreateTileMap();
        }

        // SceneView가 null인지 확인
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null || sceneView.camera == null)
        {
            return;
        }

        // 그리드 범위 설정
        int minGridX = -offsetX;
        int maxGridX = offsetX;
        int minGridZ = -offsetZ;
        int maxGridZ = offsetZ;

        // 현재 카메라 위치 기준으로 그리드 범위 계산
        Vector3 cameraPosition = sceneView.camera.transform.position;
        int drawRange = 10; // 카메라 주변 10칸만 그리기

        int minX = Mathf.Max(Mathf.FloorToInt(cameraPosition.x / gridSize - drawRange), minGridX);
        int maxX = Mathf.Min(Mathf.CeilToInt(cameraPosition.x / gridSize + drawRange), maxGridX);
        int minZ = Mathf.Max(Mathf.FloorToInt(cameraPosition.z / gridSize - drawRange), minGridZ);
        int maxZ = Mathf.Min(Mathf.CeilToInt(cameraPosition.z / gridSize + drawRange), maxGridZ);

        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                Vector3 center = new Vector3(x * gridSize, 0, z * gridSize);
                Vector3 size = new Vector3(gridSize, 0.1f, gridSize);

                if (GetTileAvailable(new Vector2Int(x, z)))
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(center, size);
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(center, size);
                }
            }
        }
    }
#endif

    // Custom Editor에서 호출될 함수
    [Button]
    public void DisplayOccupiedGrids()
    {
        // tileStates가 null이면 초기화
        if (tileStates == null)
        {
            CreateTileMap();
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Occupied Grids:");

        for (int x = -offsetX; x <= offsetX; x++)
        {
            for (int z = -offsetZ; z <= offsetZ; z++)
            {
                if (!GetTileAvailable(new Vector2Int(x, z)))
                {
                    sb.AppendLine($"Grid Position: ({x}, {z}), Occupied: true");
                }
            }
        }

        Debug.Log(sb.ToString());
    }
}
