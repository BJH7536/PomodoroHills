using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapManager : MonoBehaviour
{
    [SerializeField] private GameObject floorObject;
    [SerializeField] private Transform floorTop;
    [SerializeField] private Transform floorBottom;
    public int tileLength; // n*n 바닥에서의 n
    private bool[,] onFloor = new bool [100,100]; //타일위를 오브젝트가 점유하고 있는가


    //[임시] 오브젝트 코드저장용
    //존재하는 오브젝트 리스트(x,y좌표,회전여부,오브젝트코드)
    //위에 리스트을 읽어서 처음에 이니숄라이즈하는 코드 추가 필요

    private void Awake()
    {
        tileLength = 5;
        setScale();
        // 타일맵 위의 오브젝트 확인
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

    void initTile() //겜 켰을때 타일 위에 초기화
    {
        for(int i=0; i < tileLength; i++)
        {
            for(int j=0; j < tileLength; j++)
            {
                
            }
        }
    }
}
