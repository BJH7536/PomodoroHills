using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemDB : MonoBehaviour
{
    public GameObject [] items;
    public Dictionary<int, GameObject> itemTable = new Dictionary<int, GameObject>();
    private void Awake()
    {
        for (int i=0; i<items.Count();i++)
        {
            itemTable.Add(i, items[i]);
        }
    }
}
