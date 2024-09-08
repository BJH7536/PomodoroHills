using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TileMapManager : MonoBehaviour
{
    public static TileMapManager Instance { get; private set; }
    public CinemachineTargetGroup TargetGroup; //ī�޶� �ӽÿ�


    [SerializeField]
    GameObject tilePrefab;
    [SerializeField]
    GameObject maintilePrefab;
    GameObject maintile;
    

    public int gridX = 5;      // x,z �׸��� Ÿ�ϸ�
    public int gridZ = 5;      //

    public Dictionary<Vector2Int, Tile> tileMap = new Dictionary<Vector2Int, Tile>();      //Ÿ�� ��������üũ


    Dictionary<Vector2Int, GameObject> tileMapObject = new Dictionary<Vector2Int, GameObject>();    //1x1Ÿ�� �ð�ȭ(�̻��)





    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }   //�̱��� ����
        else
        {
            Destroy(gameObject);
        }


        CreateTileMap();
    }



    void CreateTileMap()    //Ÿ�ϸ� Ŭ���� �� ������Ʈ nxn�� ����
    {
        for(int x = 0; x < gridX; x++)
        {
            for(int z = 0; z < gridZ; z++)
            {
                Vector2Int position = new Vector2Int(x, z);// Ÿ�� ���� ����
                if (!tileMap.ContainsKey(position))
                {
                    Tile tile = new Tile(position);
                    tileMap[position] = tile;
                }

                
                /*
                Vector3 worldPosition = new Vector3(x, 0, z);// �ð������� ���̴� Ÿ�� ������Ʈ ����
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



    public bool GetEveryTileAvailable(Vector2Int size, Vector2Int position, int rotation)   //GetTileAvailable�� Ÿ�Ͽ� ���� ��� �ڸ����� ����
    {
        /*switch (rotation)
        {
            case 1: //90��
                for (int x = position.x; x < position.x + size.y; x++)
                {
                    for (int y = position.y - size.x + 1; y <= position.y; y++) 
                    {
                        if (!GetTileAvailable(new Vector2Int(x, y))) { return false; }
                    }
                }
                break;
            case 2: //180��
                for (int x = position.x - size.x + 1; x <= position.x; x++)
                {
                    for (int y = position.y - size.y + 1; y <= position.y; y++)
                    {
                        if (!GetTileAvailable(new Vector2Int(x, y))) { return false; }
                    }
                }
                break;
            case 3: //270��
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
            case 0: // �⺻ ȸ�� (0��)
                for (int x = position.x; x < position.x + size.x; x++)
                {
                    for (int y = position.y; y < position.y + size.y; y++)
                    {
                        if (!GetTileAvailable(new Vector2Int(x, y))) { return false; }
                    }
                }
                break;
            case 1: // ȸ�� 90��
                for (int x = position.x; x < position.x + size.y; x++)
                {
                    for (int y = position.y - size.x + 1; y <= position.y; y++)
                    {
                        if (!GetTileAvailable(new Vector2Int(x, y))) { return false; }
                    }
                }
                break;
            case 2: // ȸ�� 180��
                for (int x = position.x; x > position.x - size.x; x--)
                {
                    for (int y = position.y; y > position.y - size.y; y--)
                    {
                        if (!GetTileAvailable(new Vector2Int(x, y))) { return false; }
                    }
                }
                break;
            case 3: // ȸ�� 90��
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
    public bool GetTileAvailable(Vector2Int position)    //Ÿ���� �����ִ��� Ȯ���� - �������� ��ȯ
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
            case 0: // �⺻ ȸ�� (0��)
                for (int x = position.x; x < position.x + size.x; x++)
                {
                    for (int y = position.y; y < position.y + size.y; y++)
                    {
                        OccupyTile(new Vector2Int(x, y));
                    }
                }
                break;
            case 1: // ȸ�� 90��
                for (int x = position.x; x < position.x + size.y; x++)
                {
                    for (int y = position.y - size.x + 1; y <= position.y; y++)
                    {
                        OccupyTile(new Vector2Int(x, y));
                    }
                }
                break;
            case 2: // ȸ�� 180��
                for (int x = position.x; x > position.x - size.x; x--)
                {
                    for (int y = position.y; y > position.y - size.y; y--)
                    {
                        OccupyTile(new Vector2Int(x, y));
                    }
                }
                break;
            case 3: // ȸ�� 90��
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

    public void OccupyTile(Vector2Int position)     //Ÿ������ ����
    {
        tileMap[position].Occupy();
    }

    public void FreeTile(Vector2Int position)       //Ÿ������ ����
    {
        tileMap[position].Free();
    }

    private void UpdateTileVisual(Vector2Int position)      //Ÿ�� ������Ʈ�� ���־��� ������Ʈ �ϱ� ���� �޼ҵ�� ���� �̻��
    {
        // Ÿ���� �ð��� ��Ҹ� ������Ʈ
        GameObject tileObject = tileMapObject[position];
        Renderer renderer = tileObject.GetComponent<Renderer>();

        if (renderer != null)
        {
            Color tileColor = tileMap[position].isOccupied ? Color.red : Color.white; // ��: ���� ���¿� ���� ���� ����
            renderer.material.color = tileColor;
        }
    }

}
