using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapManager : MonoBehaviour
{
    public CinemachineTargetGroup TargetGroup; //ī�޶� �ӽÿ�


    [SerializeField]
    GameObject tilePrefab;

    public int gridX = 5;      // x,z �׸��� Ÿ�ϸ�
    public int gridZ = 5;      //


    Dictionary<Vector2Int, Tile> tileMap = new Dictionary<Vector2Int, Tile>();      //Ÿ�� ��������üũ
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
                Vector2Int position = new Vector2Int(x, z);// Ÿ�� ���� ����
                if (!tileMap.ContainsKey(position))
                {
                    Tile tile = new Tile(position);
                    tileMap[position] = tile;
                }

                Vector3 worldPosition = new Vector3(x, 0, z);// �ð������� ���̴� Ÿ�� ������Ʈ ����
                if (!tileMapObject.ContainsKey(position))
                {
                    GameObject tileObject = Instantiate(tilePrefab, worldPosition, Quaternion.identity);
                    tileMapObject[position] = tileObject;

                    if (TargetGroup != null) { TargetGroup.AddMember(tileObject.transform, 0.5f, 0.5f); }
                }
            }
        }
    }



    public bool IsTileAvailable(Vector2Int position)    //Ÿ���� �����ִ��� Ȯ���� - �������� ��ȯ
    {
        return tileMap.ContainsKey(position) && !tileMap[position].isOccupied; // ���� ���� Ȯ��
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
