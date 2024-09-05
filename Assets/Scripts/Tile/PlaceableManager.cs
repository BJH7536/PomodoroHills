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

    private void LoadItem()//미완 얘가 뭐하는거더라?
    {
       
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
