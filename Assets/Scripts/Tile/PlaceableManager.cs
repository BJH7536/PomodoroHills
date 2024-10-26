using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

//최우선 과제, 배치 절차 진입시 현재 소재 Free하기
// placeable이 자신의 위치정보를 나타내지않아 transform 직접 참조 혹은 생성/확인 시transform을 따라가게합니다.
// unpack 절차 작성시 반영 필요
//버튼 등과 상화작용하는 메소드의 경우 가능한 Interaction에서 참조하도록 작성

public class PlaceableManager : MonoBehaviour
{
    public static PlaceableManager Instance { get; private set; }
    public ItemDB itemDB;


    public List<GameObject> placeables = new List<GameObject>();
    public GameObject selectedPlaceable;
    public Vector2Int lastPosition = new Vector2Int(-1, -1);
    public int lastRotation = -1;
    public UnityEngine.Color OriginColor;
    //편집 등 상태관련 

    public bool isEdit { get; private set; }     //편집모드
    public bool isChestEdit { get; private set; }
    public bool isMoveEdit { get; private set; }    //이동모드
    public bool isNewEdit { get; private set; }


    private void Awake()
    {
        ResetLastLocation();
        itemDB = GetComponent<ItemDB>(); //임시 PlaceablePrefabTable -> 차후 DB 연동 후 삭제
        if (Instance == null)           //싱글톤 선언
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
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
            DebugEx.Log($"Key: ({key.x}, {key.y}), Value: {value}");
        }*/

    }

    void Update()
    {
    }


    void LoadSavedPlaceable()
    {
        //리스트불러오기
        while (false)
        {
            
        }
        //unpack 과정과 유사하게 작성 select하는 UI안되게
    }

    private void LoadPlaceable(int PlaceableCode, Vector2Int position, int rotation)//
    {
        //수량 체크 후 -1

    }

    private void PlaceTest(int index, Vector2Int position, int rotation)
    {

        itemDB.itemTable.TryGetValue(index, out GameObject obj);
        Placeable placeable = obj.GetComponent<Placeable>();
        PlacePlaceableTest(obj, placeable.size, position, rotation);
    }

    private void PlacePlaceableTest(GameObject Prefab, Vector2Int size, Vector2Int position, int rotation)
    {
        if (TileMapManager.Instance != null)
        {
            if (!TileMapManager.Instance.GetEveryTileAvailable(size, position, rotation))
            {
                DebugEx.Log("이미 타일을 누가 쓰고있어요");
            }
            else
            {
                TileMapManager.Instance.OccupyEveryTile(size, position, rotation);
                Vector3 objPosition = new Vector3(position.x, 0f, position.y);
                Quaternion rotationQuaternion = Quaternion.Euler(0, rotation * 90, 0);
                GameObject newObj = Instantiate(Prefab, objPosition, rotationQuaternion);
                newObj.tag = "Placeable";
                placeables.Add(newObj);
                Placeable plc = newObj.GetComponent<Placeable>();
                plc.position = position;
                plc.rotation = rotation;
            
            }
        }
    }
    //상기 두 메소드는 테스트용으로 작성된 코드임
    //
    // 후술할 코드 목적 : 생성, 선택 등 의 분리
    
    public GameObject CreatePlaceable(int placeableCode) // 테이블에서 Placeable프리팹을 찾아서 생성
    {
        if (itemDB.itemTable.TryGetValue(placeableCode, out GameObject Prefab))
        {
            GameObject newObj = Instantiate(Prefab, Vector3.zero, Quaternion.identity);
            return newObj;
        }
        DebugEx.Log("Can't Find placeableCode in Table");
        return null;
    }

    public bool UnpackPlaceable(int placeableCode)   //인벤토리에서 배치요소를 꺼내는 메소드     //Gameobject 반환하도록 수정 초기 저장된 배치 불러오기와 통합 (미완성)
    {
        OnIsNewEdit();
        ResetLastLocation();//불필요하게 반복되는 부분이나 만약을 위해 작성
        selectedPlaceable = CreatePlaceable(placeableCode);
        if (selectedPlaceable != null)
        {
            Vector2Int position 
                = new Vector2Int(TileMapManager.Instance.gridX/2, TileMapManager.Instance.gridZ/2);
            Placeable placeable = selectedPlaceable.GetComponent<Placeable>();
            placeable.position = position;
            selectedPlaceable.transform.position = new Vector3(position.x, 0f,position.y) ;
            OnIsMoveEdit();
            OffisChestEdit();
            return true;
        }
        else
        {
            DebugEx.Log("Unpack failed");
            return false;
        }

    }

    public bool PackPlaceable()
    {
        if (selectedPlaceable != null)
        {
            selectedPlaceable.TryGetComponent<Placeable>(out Placeable placeable);
            TileMapManager.Instance.FreeEveryTile(placeable.size,placeable.position,placeable.rotation);
            //인벤토리 내 해당하는 수량 +1
            Destroy(selectedPlaceable);
            //삭제여부 검토 후 수량 재확인
            selectedPlaceable = null;
            return true;
        }
        return false;
    }

    public void RotatePlaceable()       //현재 SelectedPlaceable을 회전 (이관예정)
    {
        if (selectedPlaceable.TryGetComponent<Placeable>(out Placeable placeable))
        {
            placeable.rotation++;
            if (placeable.rotation > 3) placeable.rotation = 0;
            //오브젝트 회전을 placeable 스크립트 rotation*90으로 변경
            selectedPlaceable.transform.rotation = Quaternion.Euler(0f, placeable.rotation * 90f, 0f);
        }
        else
        {
            DebugEx.Log("There is no selectedPlaceable");
        }
    }

    public bool ConfirmEdit()   //(미완성)
    {
        if (selectedPlaceable.TryGetComponent<Placeable>(out Placeable placeable))
        {
            if (TileMapManager.Instance.GetEveryTileAvailable(placeable.size, placeable.position, placeable.rotation))
            {
                TileMapManager.Instance.OccupyEveryTile(placeable.size, placeable.position, placeable.rotation);
                if (!isNewEdit) // 기존에 있던 오브젝트의 경우
                {
                    //TileMapManager.Instance.FreeEveryTile(placeable.size, lastPosition, lastRotation); //수정중 240921/0349
                }
                else
                {
                    placeables.Add(selectedPlaceable);
                    OffIsNewEdit();
                }
                return true;
            }
            else
            {
                DebugEx.Log("cant place there");
                return false;
            }
        }
        DebugEx.Log("Cant find placeable");
        return false;
    }

    public void CancleEdit()
    {
        if (lastRotation == -1)     //새로 생성한 Placeable의 경우
        {
            // 인벤토리 구현시 해당 Placeable 수량 +1
            Destroy(selectedPlaceable);
            OffIsNewEdit();
            DebugEx.Log("selectedPlaceable is newPlaceable, we pack it up");
        }
        else if (selectedPlaceable.TryGetComponent<Placeable>(out Placeable placeable))  //기존의 Placeable
        {
            TileMapManager.Instance.OccupyEveryTile(placeable.size, lastPosition, lastRotation);
            selectedPlaceable.transform.position = new Vector3(lastPosition.x, 0f, lastPosition.y);
            selectedPlaceable.transform.rotation = Quaternion.Euler(0, lastRotation * 90f, 0);
            placeable.position = lastPosition;
            placeable.rotation = lastRotation;
            DebugEx.Log("oldob");
        }
    }


    void DeletePlaceableObject()//미완성
    {
        if (selectedPlaceable != null)
        {
            Placeable placeable = selectedPlaceable.GetComponent<Placeable>();
            if (placeable != null)
            {
                Vector2Int position = placeable.position;
            }
            //특정 좌표 점유 해제
            //특정 좌표 부터 특정좌표까지 해제... FreeEveryTile(size,prepos,prerotation)사용 
            //인벤토리 구현 시 삭제된 오브젝트가 인벤토리로 돌아가도록 구현 
            //PackObject로 기능 이관 후 디버그용으로 유지
        }
    }



    //IsEdit이 true인 상태에서 타일맵 위의 Placeable을 클릭하면 주위로 UI가 활성화 되고 드래그하여 움직일 수 있다.
    //이때 확인, 회전, 보관(삭제)와 관련된 UI(버튼) 띄운다.
    public void OnIsEdit() { isEdit = true; }
    public void OffIsEdit() { isEdit = false; }
    public void OnIsNewEdit() { isNewEdit = true; }
    public void OffIsNewEdit() { isNewEdit = false; }
    public void OnisChestEdit() { isChestEdit = true; }
    public void OffisChestEdit() { isChestEdit = false; }
    public void OnIsMoveEdit() // 오브젝트 이동 시작시 기존 위치 Free
    {
        StartColor();
        if (selectedPlaceable.TryGetComponent<Placeable>(out Placeable placeable))
        {
            TileMapManager.Instance.FreeEveryTile(placeable.size, placeable.position, placeable.rotation);
            if (isNewEdit)
            {
                ResetLastLocation();
            }
            else
            {
                lastRotation = placeable.rotation;
                lastPosition = placeable.position;
            }

            isMoveEdit = true;
        }
    }

    //
    public void OffIsMoveEdit() // 세분화(confirm, cancle 버튼 모두 이 메소드 사용중)(미완성)
    {                           // ConfirmEdit, CancleEdit,  해당 메소드가 제일 하위에 위치하도록 변경

        isMoveEdit = false;
        EndColor();
        ResetLastLocation();
    }



    
    public void StartColor()    //(미사용)
    {
        //Renderer renderer = selectedPlaceable.GetComponent<Renderer>();
        //OriginColor = renderer.material.color;
    }
    public void ChangePlaceableColor()  //배치가능,불가능 여부를 녹/적색으로 나타내기 위한 코드입니다. 컬러값이 따로 지정된 마테리얼을 사용하는 경우 StartColor()에서 기존 색을 저장해야 합니다.
    {
        if (selectedPlaceable != null)
        {
            Placeable placeable = selectedPlaceable.GetComponent<Placeable>();
            if (TileMapManager.Instance.GetEveryTileAvailable(placeable.size, placeable.position, placeable.rotation))
            {
                MeshRenderer[] renderers = selectedPlaceable.GetComponentsInChildren<MeshRenderer>();

                foreach (MeshRenderer renderer in renderers)
                {
                    renderer.material.color = UnityEngine.Color.green;
                }
            }
            else
            {
                MeshRenderer[] renderers = selectedPlaceable.GetComponentsInChildren<MeshRenderer>();

                foreach (MeshRenderer renderer in renderers)
                {
                    renderer.material.color = UnityEngine.Color.red;
                }
            }
        }

    }
    public void EndColor()  //변경된 색을 원래대로 바꿉니다. ChangePlaceableColor()의 주석 참고바랍니다.
    {
        if (selectedPlaceable != null)
        {
            MeshRenderer[] renderers = selectedPlaceable.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer renderer in renderers)
            {
                renderer.material.color = UnityEngine.Color.white;
            }
        }
    }





    private void ResetLastLocation()        //좌표 초기화
    {
        lastPosition = new Vector2Int(-1, -1);
        lastRotation = -1;
    }
}

