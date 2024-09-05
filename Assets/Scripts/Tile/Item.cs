using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public Vector2Int size { get; private set; }    //xzũ��
    public Vector2Int position { get; private set; }
    public int rotation { get; private set; } //ȸ������
    public int itemCode { get; private set; }
    public bool isPlaced { get; private set; } = false;
    public bool isMoving { get; private set; } = false;

    public Item(int width, int height, int itemCode)
    {
        size = new Vector2Int(width, height);
        position = new Vector2Int(-1, -1);  //�ʱⰪ(�̹�ġ)
    }

}
