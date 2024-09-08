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


    //편집 등 상태관련 
    public bool isEdit;     //편집모드


    private void Awake()
    {
        itemDB = GetComponent<ItemDB>(); //임시 사용 itemDB
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }   //싱글톤 으로

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

    }//미완

    private void LoadItem()//미완 얘가 뭐하는거더라?
    {
       
    }

    private void PlaceTest(int index, Vector2Int position, int rotation)
    {   
        
        itemDB.itemTable.TryGetValue(index, out GameObject obj);
        Placeable placeable = obj.GetComponent<Placeable>();
        PlacePlaceable(obj, placeable.size, position, rotation);
    }

    public void UnpackPlaceable(int itemCode)   //인벤토리에서 배치요소를 꺼내는 메소드
    {
        itemDB.itemTable.TryGetValue(itemCode, out GameObject obj);
    }



    private void PlacePlaceable(GameObject Prefab, Vector2Int size,Vector2Int position, int rotation)
    {
        if (TileMapManager.Instance != null)
        {
            if (!TileMapManager.Instance.GetEveryTileAvailable(size, position, rotation)) {
                Debug.Log("이미 타일을 누가 쓰고있어요");
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

    void DeletePlaceableObject()//미완
    {
        if(selectedItem != null)
        {
            Item item = selectedItem.GetComponent<Item>();
            if(item != null)
            {
                Vector2Int position = item.position;
            }
            //특정 좌표 점유 해제
            //특정 좌표 부터 특정좌표까지 해제... 이중 for문 사용 (미작성) //서순 생각좀할래...
        }
    }

    

    //IsEdit이 true인 상태에서 타일맵 위의 Item을 클릭하면 주위로 UI가 활성화 되고 드래그하여 움직일 수 있다.
    //이때 확인, 회전, 보관(삭제)와 관련된 UI(버튼) 띄운다.
    public void OnIsEdit() { isEdit = true; }
    public void OffIsEdit() { isEdit = false; }




}
