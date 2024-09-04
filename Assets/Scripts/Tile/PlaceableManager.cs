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
        
    }


    void Update()
    {
        
    }


    void FirstLoadForTest()
    {

    }

    private void LoadItem()//�̿�
    {
       
    }

    void DeletePlaceableObject()//�̿�
    {
        if(selectedItem != null)
        {
            Placeable placeable = selectedItem.GetComponent<Placeable>();
            if(placeable != null)
            {
                Vector2Int position = placeable.position;
            }
            //Ư�� ��ǥ ���� ����
            //Ư�� ��ǥ ���� Ư����ǥ���� ����... ���� for�� ��� (���ۼ�)
        }
    }

    


    public void OnIsEdit() { isEdit = true; }
    public void OffIsEdit() { isEdit = false; }




}
