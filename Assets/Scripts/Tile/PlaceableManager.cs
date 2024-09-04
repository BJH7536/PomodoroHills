using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableManager : MonoBehaviour
{
    public static PlaceableManager Instance { get; private set; }
    public TileMapManager TileMapManager;
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
        
    }


    void Update()
    {
        
    }


    void FirstLoadForTest()
    {

    }

    private void LoadItem()//미완
    {
       
    }

    void DeletePlaceableObject()//미완
    {
        if(selectedItem != null)
        {
            Placeable placeable = selectedItem.GetComponent<Placeable>();
            if(placeable != null)
            {
                Vector2Int position = placeable.position;
            }
            //특정 좌표 점유 해제
            //특정 좌표 부터 특정좌표까지 해제... 이중 for문 사용 (미작성)
        }
    }

    


    public void OnIsEdit() { isEdit = true; }
    public void OffIsEdit() { isEdit = false; }




}
