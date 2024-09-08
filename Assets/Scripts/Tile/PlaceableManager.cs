using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PlaceableManager : MonoBehaviour
{
    public static PlaceableManager Instance { get; private set; }
    public TileMapManager tileMapManager;
    public ItemDB itemDB;


    public List<GameObject> placeables = new List<GameObject>();
    public GameObject selectedItem;


    //���� �� ���°��� 
    public bool isEdit;     //�������


    private void Awake()
    {
        itemDB = GetComponent<ItemDB>(); //�ӽ� ��� itemDB
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }   //�̱��� ����

        else
        {
            Destroy(gameObject);
        }


        isEdit = false;
    }
    void Start()
    {
        PlaceTest(0, new Vector2Int(2, 2), 0);
        PlaceTest(1, new Vector2Int(1, 2), 3);
        PlaceTest(1, new Vector2Int(2, 1), 2);
        PlaceTest(1, new Vector2Int(2, 3), 0);
        PlaceTest(1, new Vector2Int(3, 2), 1);

        /*foreach (var kvp in TileMapManager.Instance.tileMap)
        {
            Vector2Int key = kvp.Key;
            TileMapManager.Instance.tileMap.TryGetValue(key, out Tile val);
            string value = val.isOccupied.ToString();
            Debug.Log($"Key: ({key.x}, {key.y}), Value: {value}");
        }*/

    }

    void Update()
    {
        
    }


    void FirstLoadForTest()
    {

    }//�̿�

    private void LoadItem()//�̿� �갡 ���ϴ°Ŵ���?
    {
       
    }

    private void PlaceTest(int index, Vector2Int position, int rotation)
    {   
        
        itemDB.itemTable.TryGetValue(index, out GameObject obj);
        Placeable placeable = obj.GetComponent<Placeable>();
        PlacePlaceable(obj, placeable.size, position, rotation);
    }

    public void UnpackPlaceable(int itemCode)   //�κ��丮���� ��ġ��Ҹ� ������ �޼ҵ�
    {
        itemDB.itemTable.TryGetValue(itemCode, out GameObject obj);
    }



    private void PlacePlaceable(GameObject Prefab, Vector2Int size,Vector2Int position, int rotation)
    {
        if (TileMapManager.Instance != null)
        {
            if (!TileMapManager.Instance.GetEveryTileAvailable(size, position, rotation)) {
                Debug.Log("�̹� Ÿ���� ���� �����־��");
            }
            else
            {
                TileMapManager.Instance.OccupyEveryTile(size, position, rotation);
                Vector3 objPosition = new Vector3(position.x, 0f, position.y);
                Quaternion rotationQuaternion = Quaternion.Euler(0, rotation * 90, 0);
                GameObject newObj = Instantiate(Prefab, objPosition,rotationQuaternion );
                placeables.Add(newObj);
            }
        }
    }

    void DeletePlaceableObject()//�̿�
    {
        if(selectedItem != null)
        {
            Item item = selectedItem.GetComponent<Item>();
            if(item != null)
            {
                Vector2Int position = item.position;
            }
            //Ư�� ��ǥ ���� ����
            //Ư�� ��ǥ ���� Ư����ǥ���� ����... ���� for�� ��� (���ۼ�) //���� �������ҷ�...
        }
    }

    

    //IsEdit�� true�� ���¿��� Ÿ�ϸ� ���� Item�� Ŭ���ϸ� ������ UI�� Ȱ��ȭ �ǰ� �巡���Ͽ� ������ �� �ִ�.
    //�̶� Ȯ��, ȸ��, ����(����)�� ���õ� UI(��ư) ����.
    public void OnIsEdit() { isEdit = true; }
    public void OffIsEdit() { isEdit = false; }




}
