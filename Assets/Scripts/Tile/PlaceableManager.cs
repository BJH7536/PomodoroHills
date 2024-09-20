using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

// placeable이 자신의 위치정보를 나타내지않아 transform 직접 참조 혹은 생성/확인 시transform을 따라가게합니다.
// unpack 절차 작성시 반영 필요
//버튼 등과 상화작용하는 메소드의 경우 가능한 Interaction에서 참조하도록 작성

public class PlaceableManager : MonoBehaviour
{
    public static PlaceableManager Instance { get; private set; }
    public ItemDB itemDB;


    public List<GameObject> placeables = new List<GameObject>();
    public GameObject selectedItem;
    public Vector2Int lastPosition = new Vector2Int(-1,-1);
    public int lastRotation = -1;
    //편집 등 상태관련 
    public bool isEdit { get; private set; }     //편집모드
    public bool isMoveEdit { get; private set; }
    public bool isNewEdit { get; private set; }

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
                newObj.tag = "Placeable";
                placeables.Add(newObj);
                Placeable plc = newObj.GetComponent<Placeable>();
                plc.position = position;
                plc.rotation = rotation;
            }
        }
    }

    public bool ConfirmEdit()   //(미완성)
    {
        if (selectedItem.TryGetComponent<Placeable>(out Placeable placeable))
        {
            if (TileMapManager.Instance.GetEveryTileAvailable(placeable.size, placeable.position, placeable.rotation))
            {
                TileMapManager.Instance.OccupyEveryTile(placeable.size, placeable.position, placeable.rotation);
                if (lastRotation != -1) // 기존에 있던 오브젝트의 경우
                    //Free는 옮기기 시작할때 해야합니다. 해당 부분 수정바랍니다.
                {
                    TileMapManager.Instance.FreeEveryTile(placeable.size, lastPosition, lastRotation);
                }
                else //새로 생성된 오브젝트의 경우 
                {
                    //
                }
                OffIsMoveEdit(); //새로 생성된것일 시...
                return true;
            }
            else
            {
                Debug.Log("cant place there");
                return false;
            }
        }
        Debug.Log("Cant find placeable");
        return false;
    }

    public void CancleEdit()
    {
        if (lastRotation == -1)     //새로 생성한 Placeable의 경우
        {
            Debug.Log("newob");
        }else if(selectedItem.TryGetComponent<Placeable>(out Placeable placeable))  //기존의 Placeable
        {
            TileMapManager.Instance.OccupyEveryTile(placeable.size,lastPosition,lastRotation);
            selectedItem.transform.position = new Vector3(lastPosition.x, 0f, lastPosition.y);
            selectedItem.transform.rotation = Quaternion.Euler(0, lastRotation*90f, 0);

            Debug.Log("oldob");
        }
        OffIsMoveEdit();
    }


    void DeletePlaceableObject()//미완성
    {
        if(selectedItem != null)
        {
            Item item = selectedItem.GetComponent<Item>();
            if(item != null)
            {
                Vector2Int position = item.position;
            }
            //특정 좌표 점유 해제
            //특정 좌표 부터 특정좌표까지 해제... FreeEveryTile(size,prepos,prerotation)사용 
            //인벤토리 구현 시 삭제된 오브젝트가 인벤토리로 돌아가도록 구현 
            //PackObject로 기능 이관 후 디버그용으로 유지
        }
    }



    //IsEdit이 true인 상태에서 타일맵 위의 Item을 클릭하면 주위로 UI가 활성화 되고 드래그하여 움직일 수 있다.
    //이때 확인, 회전, 보관(삭제)와 관련된 UI(버튼) 띄운다.
    public void OnIsEdit() { isEdit = true; }
    public void OffIsEdit() { isEdit = false; }
    public void OnIsNewEdit() { isNewEdit = true; }
    public void OffIsNewEdit() { isNewEdit = false; }
    public void OnIsMoveEdit() 
    {
        selectedItem.TryGetComponent<Placeable>(out Placeable placeable);
        
        lastRotation = placeable.rotation;
        lastPosition = placeable.position;
        
        isMoveEdit = true; 

        //새로 오브젝트를 생성하는 경우와 구분하여 작성
    }

    //
    public void OffIsMoveEdit() // 세분화(confirm, cancle 버튼 모두 이 메소드 사용중)(미완성)
    {                           // ConfirmEdit, CancleEdit,  해당 메소드가 제일 하위에 위치하도록 변경

        isMoveEdit = false;
        ResetLastLocation();
    }
    


    //
    
    private void ResetLastLocation()
    {
        lastPosition = new Vector2Int(-1, -1);
        lastRotation = -1;
    }


}
