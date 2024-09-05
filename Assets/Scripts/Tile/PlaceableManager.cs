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

    private void LoadItem()//�̿� �갡 ���ϴ°Ŵ���?
    {
       
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
