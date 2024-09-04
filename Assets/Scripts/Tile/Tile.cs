using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Tile
{
    // �ϴ� �̰� �Ƚ��
    public bool isOccupied { get; set; }
    public Vector2Int position { get; set; }

    public Tile(Vector2Int position)
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
