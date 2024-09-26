using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Tile //타일의 색이나 기타 정보 등의 변동 등에 사용됩니다.
{
    public bool isOccupied { get; set; }    //타일의 점유여부(타일위에 오브젝트가 이미 존재하고 있는지
    public Vector2Int position { get; set; }



    public Tile(Vector2Int position)        //생성자
    {
        this.position = position;
        isOccupied = false;
    }





    public void Occupy()
    {
        isOccupied = true;
    }

    public void Free()
    {
        isOccupied = false;
    }

}
