using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickRespondByMMFPlayer : MonoBehaviour, IPointerClickHandler
{
    private MMF_Player mmf_Player;
    private void Awake()
    {
        mmf_Player = GetComponent<MMF_Player>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(!PlaceableManager.Instance.IsEdit)
            mmf_Player.PlayFeedbacks();
    }
}
