using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using PomodoroHills;
using UnityEngine;
using UnityEngine.Serialization;
using VInspector;

//최우선 과제, 배치 절차 진입시 현재 소재 Free하기
// placeable이 자신의 위치정보를 나타내지않아 transform 직접 참조 혹은 생성/확인 시transform을 따라가게합니다.
// unpack 절차 작성시 반영 필요
//버튼 등과 상화작용하는 메소드의 경우 가능한 Interaction에서 참조하도록 작성
[DefaultExecutionOrder(-1)]
public class PlaceableManager : MonoBehaviour
{
    public static PlaceableManager Instance { get; private set; }

    public List<GameObject> placeables = new List<GameObject>();
    public GameObject SelectedPlaceable;
    public Vector2Int lastPosition = new Vector2Int(-1, -1);
    public int lastRotation = -1;
    
    //편집 등 상태관련 
    public bool IsEdit { get; private set; }     //편집모드

    public Action<bool> EditModeSwitched;
    
    /// <summary>
    /// 드래그로 Placeable을 옮길 수 있는지 여부에 대한 Flag
    /// </summary>
    public bool IsMoveEdit { get; private set; }
    
    /// <summary>
    /// 지금 편집하고있는게 새로 지은 건물인지 구분하는 Flag
    /// </summary>
    public bool IsNewEdit { get; private set; }
    
    private void Awake()
    {
        ResetLastLocation();
        if (Instance == null)           //싱글톤 선언
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        IsEdit = false;
    }
    
    private void Update()
    {
        if (IsMoveEdit)
        {
            ChangePlaceableColor();
        }
    }
    
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SavePlaceables();
            SaveTimerState();
        }
        else
        {
            LoadPlaceables();
            
            // 게임이 재개될 때 각 FarmBuilding 인스턴스에 저장된 타이머 상태를 전달하여 성장 업데이트
            foreach (GameObject obj in placeables)
            {
                if (obj.TryGetComponent<FarmBuilding>(out var farmBuilding))
                {
                    farmBuilding.LoadTimerStateAndUpdateGrowth();
                }
            }
            
            // 모든 FarmBuilding의 성장 업데이트가 끝난 후, 저장된 데이터를 삭제합니다.
            DeleteSavedTimerState();
        }
    }
    
    private void OnApplicationQuit()
    {
        SavePlaceables();
        SaveTimerState();
    }

    [Button]
    public void DebugCurrentState()
    {
        DebugEx.Log($"IsEdit : {IsEdit}");
        DebugEx.Log($"IsMoveEdit : {IsMoveEdit}");
        DebugEx.Log($"IsNewEdit : {IsNewEdit}");
    }

    #region Data Save & Load
    
    /// <summary>
    /// 배치된 오브젝트를 모두 삭제하고 리스트를 비웁니다.
    /// </summary>
    public void ClearPlaceables()
    {
        foreach (GameObject obj in placeables)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        placeables.Clear();
        SelectedPlaceable = null;
    }
    
    private void SaveTimerState()
    {
        // 타이머 상태와 시간을 저장
        PlayerPrefs.SetString("LastCheckTime", DateTime.Now.ToString());
        PlayerPrefs.SetInt("LastTimerState", (int)TimerManager.Instance.CurrentTimerState);
        PlayerPrefs.SetInt("RemainingFocusTimeInSeconds", TimerManager.Instance.RemainingTimeInSeconds);
        PlayerPrefs.Save();
    }
    
    private void DeleteSavedTimerState()
    {
        PlayerPrefs.DeleteKey("LastCheckTime");
        PlayerPrefs.DeleteKey("LastTimerState");
        PlayerPrefs.DeleteKey("RemainingFocusTimeInSeconds");
        PlayerPrefs.Save();
    }
    
    public void SavePlaceables()
    {
        List<PlaceableData> dataList = new List<PlaceableData>();

        foreach (GameObject obj in placeables)
        {
            if(obj == null)  continue;
            
            Placeable placeable = obj.GetComponent<Placeable>();
            if (placeable != null)
            {
                PlaceableData data;

                if (placeable is FarmBuilding farmBuilding)
                {
                    // FarmBuildingData로 저장
                    FarmBuildingData farmData = new FarmBuildingData
                    {
                        type = "FarmBuilding",
                        id = farmBuilding.id,
                        size = farmBuilding.size,
                        position = farmBuilding.position,
                        rotation = farmBuilding.rotation,
                        isCropPlanted = farmBuilding.IsCropPlanted,
                        currentCrop = farmBuilding.currentCrop
                    };
                    data = farmData;
                }
                else
                {
                    // 일반 PlaceableData로 저장
                    data = new PlaceableData
                    {
                        type = "Placeable",
                        id = placeable.id,
                        size = placeable.size,
                        position = placeable.position,
                        rotation = placeable.rotation
                    };
                }

                dataList.Add(data);
            }
        }

        // JSON으로 직렬화
        string json = JsonConvert.SerializeObject(dataList, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore // 순환 참조 무시
        });

        // 파일로 저장
        string path = Application.persistentDataPath + "/placeables.json";
        File.WriteAllText(path, json);

        Debug.Log("Placeables saved to " + path);
    }

    public void LoadPlaceables()
    {
        string path = Application.persistentDataPath + "/placeables.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            // JSON을 역직렬화하여 PlaceableData 리스트로 변환
            List<PlaceableData> dataList = JsonConvert.DeserializeObject<List<PlaceableData>>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            // 기존 오브젝트 삭제 및 타일맵 점유 해제
            foreach (GameObject obj in placeables)
            {
                Placeable placeable = obj.GetComponent<Placeable>();
                if (placeable != null)
                {
                    TileMapManager.Instance.FreeEveryTile(placeable.position, placeable.size, placeable.rotation);
                }
                Destroy(obj);
            }
            placeables.Clear();

            // 데이터에 따라 오브젝트 재생성
            foreach (PlaceableData data in dataList)
            {
                GameObject obj = null;

                if (data.type == "FarmBuilding")
                {
                    obj = CreateFarmBuilding(data.id);
                }
                else if (data.type == "Placeable")
                {
                    obj = CreatePlaceable(data.id);
                }
                else
                {
                    Debug.LogWarning("Unknown Placeable type: " + data.type);
                    continue;
                }

                if (obj != null)
                {
                    Placeable placeable = obj.GetComponent<Placeable>();
                    if (placeable != null)
                    {
                        placeable.size = data.size;
                        placeable.position = data.position;
                        placeable.rotation = data.rotation;
                        obj.transform.position = new Vector3(data.position.x, 0f, data.position.y);
                        obj.transform.rotation = Quaternion.Euler(0f, data.rotation * 90f, 0f);

                        if (placeable is FarmBuilding farmBuilding && data is FarmBuildingData farmData)
                        {
                            farmBuilding.currentCrop = farmData.currentCrop;
                            farmBuilding.IsCropPlanted = farmData.isCropPlanted;

                            if (farmBuilding.IsCropPlanted && farmBuilding.currentCrop != null)
                            {
                                if (!farmBuilding.currentCrop.IsFullyGrown())
                                {
                                    farmBuilding.SubscribeToTimer();
                                }
                                else
                                {
                                    farmBuilding.ShowHarvestButton();
                                }
                            }
                        }

                        TileMapManager.Instance.OccupyEveryTile(placeable.position, placeable.size, placeable.rotation);

                        placeables.Add(obj);
                    }
                }
                else
                {
                    Debug.LogWarning("Failed to create Placeable with id " + data.id);
                }
            }

            Debug.Log("Placeables loaded from " + path);
        }
        else
        {
            Debug.LogWarning("No saved placeables found at " + path);
        }
    }


    #endregion
    
    private void Place(int id, Vector2Int position, int rotation)
    {
        var obj = DataBaseManager.Instance.BuildingDatabase.GetBuildingById(id).Prefab;
        // itemDB.itemTable.TryGetValue(id, out GameObject obj);
        Placeable placeable = obj.GetComponent<Placeable>();
        PlacePlaceable(obj, placeable.size, position, rotation);
    }

    private void PlacePlaceable(GameObject Prefab, Vector2Int size, Vector2Int position, int rotation)
    {
        if (TileMapManager.Instance != null)
        {
            if (!TileMapManager.Instance.GetEveryTileAvailable(position, size, rotation))
            {
                DebugEx.Log("이미 타일을 누가 쓰고있어요");
            }
            else
            {
                TileMapManager.Instance.OccupyEveryTile(position, size, rotation);
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
    // 후술할 코드 목적 : 생성, 선택 등 의 분리
    
    public GameObject CreatePlaceable(int id) // 테이블에서 Placeable프리팹을 찾아서 생성
    {
        ItemType type = (ItemType)(id / 100);

        GameObject obj = null;
        if (type == ItemType.Building)
        {
            obj = DataBaseManager.Instance.BuildingDatabase.GetBuildingById(id).Prefab;
            
        }
        else if (type == ItemType.Decoration)
        {
            obj = DataBaseManager.Instance.DecorDatabase.GetDecorById(id).Prefab;
        }
        
        if (obj != null)
        {
            GameObject newObj = Instantiate(obj, Vector3.zero, Quaternion.identity);
            return newObj;
        }
        
        DebugEx.Log("Can't Find id in Table");
        return null;
    }

    public GameObject CreateFarmBuilding(int id)
    {
        GameObject obj = DataBaseManager.Instance.BuildingDatabase.GetBuildingById(id).Prefab;

        if (obj != null)
        {
            GameObject newObj = Instantiate(obj, Vector3.zero, Quaternion.identity);

            if (!newObj.TryGetComponent(out FarmBuilding _))
            {
                newObj.AddComponent<FarmBuilding>();
            }

            return newObj;
        }
        Debug.Log("Can't Find id in Table");
        return null;
    }
    
    /// <summary>
    /// 인벤토리에서 Placeable을 지으려고 꺼내는 메서드.
    /// 인벤토리랑 연동 (인벤토리 내 수량검증, 인벤토리 내 수량 감소 및 오브젝트 배치) 과정 구현 필요
    /// </summary>
    /// <param name="placeableCode"></param>
    /// <returns></returns>
    public bool UnpackPlaceable(int placeableCode)   //인벤토리에서 배치요소를 꺼내는 메소드     //Gameobject 반환하도록 수정 초기 저장된 배치 불러오기와 통합 (미완성)
    {
        OnEditMode();
        OnIsNewEdit();
        ResetLastLocation();                        //불필요하게 반복되는 부분이나 만약을 위해 작성
        SelectedPlaceable = CreatePlaceable(placeableCode);
        if (SelectedPlaceable != null)
        {
            Vector2Int position = new Vector2Int(0, 0);
            Placeable placeable = SelectedPlaceable.GetComponent<Placeable>();
            placeable.position = position;
            SelectedPlaceable.transform.position = new Vector3(position.x, 0f,position.y) ;
            OnIsMoveEdit();
            return true;
        }
        else
        {
            DebugEx.Log("Unpack failed");
            return false;
        }
    }

    /// <summary>
    /// Placeable을 재포장(Pack)했을때 인벤토리에 수량을 늘려야함
    /// </summary>
    /// <returns></returns>
    public bool PackPlaceable()
    {
        if (SelectedPlaceable != null)
        {
            SelectedPlaceable.TryGetComponent<Placeable>(out Placeable placeable);
            TileMapManager.Instance.FreeEveryTile(placeable.position, placeable.size, placeable.rotation);
            
            ItemData newItem = new ItemData()
            {
                id = placeable.id,
                amount = 1
            };
            PomodoroHills.InventoryManager.Instance.AddItemAsync(newItem).Forget();
            
            Destroy(SelectedPlaceable);
            //삭제여부 검토 후 수량 재확인
            SelectedPlaceable = null;
            return true;
        }
        return false;
    }

    public void RotatePlaceable()       //현재 SelectedPlaceable을 회전 (이관예정)
    {
        if (SelectedPlaceable.TryGetComponent<Placeable>(out Placeable placeable))
        {
            placeable.rotation++;
            if (placeable.rotation > 3) placeable.rotation = 0;
            //오브젝트 회전을 placeable 스크립트 rotation*90으로 변경
            SelectedPlaceable.transform.rotation = Quaternion.Euler(0f, placeable.rotation * 90f, 0f);
        }
        else
        {
            DebugEx.Log("There is no SelectedPlaceable");
        }
    }

    /// <summary>
    /// UnpackPlaceable에서 Unpack과정을 수행할때, 인벤토리의 수량 감소 시점을 Unpack Placeable에 둘지, ComfirmEdit에 둘지 문제로 (미완성)이라고 표기
    /// </summary>
    /// <returns></returns>
    public bool ConfirmEdit()
    {
        if (SelectedPlaceable.TryGetComponent(out Placeable placeable))
        {
            if (TileMapManager.Instance.GetEveryTileAvailable(placeable.position, placeable.size, placeable.rotation))
            {
                TileMapManager.Instance.OccupyEveryTile(placeable.position, placeable.size, placeable.rotation);
                if (!IsNewEdit) // 기존에 있던 오브젝트의 경우
                {
                    //TileMapManager.Instance.FreeEveryTile(placeable.size, lastPosition, lastRotation); //수정중 240921/0349
                }
                else
                {
                    PomodoroHills.InventoryManager.Instance.DeleteItemAsync(placeable.id, 1).Forget();
                    
                    placeables.Add(SelectedPlaceable);
                    OffIsNewEdit();
                    OffEditMode();
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

    public void CancelEdit()
    {
        DebugEx.Log($"{nameof(CancelEdit)}");
        
        if (lastRotation == -1)     //새로 생성한 Placeable의 경우
        {
            // 인벤토리 구현시 해당 Placeable 수량 +1
            Destroy(SelectedPlaceable);
            OffIsNewEdit();
            DebugEx.Log("SelectedPlaceable is newPlaceable, we pack it up");
        }
        else if (SelectedPlaceable.TryGetComponent<Placeable>(out Placeable placeable))  //기존의 Placeable
        {
            TileMapManager.Instance.OccupyEveryTile(lastPosition, placeable.size, lastRotation);
            SelectedPlaceable.transform.position = new Vector3(lastPosition.x, 0f, lastPosition.y);
            SelectedPlaceable.transform.rotation = Quaternion.Euler(0, lastRotation * 90f, 0);
            placeable.position = lastPosition;
            placeable.rotation = lastRotation;
        }
    }
    
    /// <summary>
    /// DeletePlaceObject가 디버그용 빼고 쓸 이유가 없음, 아래 PackPlaceable이 실제로 사용되는 Placeable 삭제 기능 
    /// </summary>
    void DeletePlaceableObject()//미완성
    {
        if (SelectedPlaceable != null)
        {
            Placeable placeable = SelectedPlaceable.GetComponent<Placeable>();
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
    public void OnEditMode()
    {
        IsEdit = true; 
        EditModeSwitched?.Invoke(IsEdit);
    }

    public void OffEditMode()
    {
        IsEdit = false; 
        EditModeSwitched?.Invoke(IsEdit);
    }
    public void OnIsNewEdit() { IsNewEdit = true; }
    
    public void OffIsNewEdit()
    {
        if(IsNewEdit) OffEditMode();
        IsNewEdit = false; 
    }

    public void OnIsMoveEdit() // 오브젝트 이동 시작시 기존 위치 Free
    {
        if (SelectedPlaceable.TryGetComponent(out Placeable placeable))
        {
            if (IsNewEdit)
            {
                ResetLastLocation();
            }
            else
            {
                TileMapManager.Instance.FreeEveryTile(placeable.position, placeable.size, placeable.rotation);
                lastRotation = placeable.rotation;
                lastPosition = placeable.position;
            }

            IsMoveEdit = true;
        }
    }
    
    /// <summary>
    /// Color 변경 문제로 미완성, 나머지 이유는 뭐임?
    /// </summary>
    public void OffIsMoveEdit() // 세분화(confirm, cancle 버튼 모두 이 메소드 사용중)(미완성)
    {                           // ConfirmEdit, CancelEdit,  해당 메소드가 제일 하위에 위치하도록 변경

        IsMoveEdit = false;
        EndColor();
        ResetLastLocation();
    }
    
    public void ChangePlaceableColor()  //배치가능,불가능 여부를 녹/적색으로 나타내기 위한 코드입니다. 컬러값이 따로 지정된 마테리얼을 사용하는 경우 StartColor()에서 기존 색을 저장해야 합니다.
    {
        if (SelectedPlaceable != null)
        {
            Placeable placeable = SelectedPlaceable.GetComponent<Placeable>();
            if (TileMapManager.Instance.GetEveryTileAvailable(placeable.position, placeable.size, placeable.rotation))
            {
                // MeshRenderer[] renderers = SelectedPlaceable.GetComponentsInChildren<MeshRenderer>();
                //
                // foreach (MeshRenderer renderer in renderers)
                // {
                //     renderer.material.color = UnityEngine.Color.green;
                // }
                
                placeable.SetGreenColor();
            }
            else
            {
                // MeshRenderer[] renderers = SelectedPlaceable.GetComponentsInChildren<MeshRenderer>();
                //
                // foreach (MeshRenderer renderer in renderers)
                // {
                //     renderer.material.color = UnityEngine.Color.red;
                // }
                
                placeable.SetRedColor();
            }
        }

    }
    public void EndColor()  //변경된 색을 원래대로 바꿉니다. ChangePlaceableColor()의 주석 참고바랍니다.
    {
        if (SelectedPlaceable != null)
        {
            // MeshRenderer[] renderers = SelectedPlaceable.GetComponentsInChildren<MeshRenderer>();
            //
            // foreach (MeshRenderer renderer in renderers)
            // {
            //     renderer.material.color = UnityEngine.Color.white;
            // }
            
            SelectedPlaceable.GetComponent<Placeable>().ResetColor();
        }
    }
    
    private void ResetLastLocation()        //좌표 초기화
    {
        lastPosition = new Vector2Int(-1, -1);
        lastRotation = -1;
    }
}


[Serializable]
public class PlaceableDataList
{
    public List<PlaceableData> placeables;
}

