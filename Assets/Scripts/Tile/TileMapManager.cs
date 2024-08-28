using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapManager : MonoBehaviour
{
    [SerializeField] private GameObject floorObject;
    [SerializeField] private Transform floorTop;
    [SerializeField] private Transform floorBottom;
    public int tileLength; // n*n �ٴڿ����� n
    private bool[,] onFloor = new bool [100,100]; //Ÿ������ ������Ʈ�� �����ϰ� �ִ°�


    //[�ӽ�] ������Ʈ �ڵ������
    //�����ϴ� ������Ʈ ����Ʈ(x,y��ǥ,ȸ������,������Ʈ�ڵ�)
    //���� ����Ʈ�� �о ó���� �̴ϼ�������ϴ� �ڵ� �߰� �ʿ�

    private void Awake()
    {
        tileLength = 5;
        setScale();
        // Ÿ�ϸ� ���� ������Ʈ Ȯ��
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void setScale()
    {
        floorObject.transform.localScale = new Vector3(tileLength, 1,tileLength);
    }

    void initTile() //�� ������ Ÿ�� ���� �ʱ�ȭ
    {
        for(int i=0; i < tileLength; i++)
        {
            for(int j=0; j < tileLength; j++)
            {
                
            }
        }
    }
}
