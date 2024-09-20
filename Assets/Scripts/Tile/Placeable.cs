using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Placeable : MonoBehaviour
{
    public int placeableCode { get; private set; }
    public Vector2Int size;     //(x,z)
    public Vector2Int position; //(x,z)
    public int rotation; // n * 90

    public bool isPlaced;
    

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
