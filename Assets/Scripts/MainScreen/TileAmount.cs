using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class TileAmount : MonoBehaviour
{
    [SerializeField] public int ShownTileIdx = 8;
    [SerializeField] public List<TileController> Tiles;
    
    public void PlusShownTile()
    {
        if (ShownTileIdx == Tiles.Count - 1) return;

        ShownTileIdx++;
        Tiles[ShownTileIdx].gameObject.SetActive(true);
        Tiles[ShownTileIdx].gameObject.GetComponent<MMF_Player>().PlayFeedbacks();
    }

    public void MinusShownTile()
    {
        if (ShownTileIdx == 0) return;

        Tiles[ShownTileIdx].gameObject.SetActive(false);
        ShownTileIdx--;
        Tiles[ShownTileIdx].gameObject.SetActive(true);
    }
}
