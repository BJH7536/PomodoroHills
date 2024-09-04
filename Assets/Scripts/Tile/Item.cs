using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public Vector2Int size { get; private set; }
    public Vector2Int position { get; private set; }
    public int itemCode { get; private set; }
    public bool isPlaced { get; private set; } = false;
    public bool isMoving { get; private set; } = false;

    public Item(int width, int height, int itemCode)
    {
        size = new Vector2Int(width, height);
        position = new Vector2Int(-1, -1);  //초기값(미배치)
    }

}
