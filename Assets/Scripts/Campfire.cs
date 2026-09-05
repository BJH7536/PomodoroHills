using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Campfire : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool turn;
    [SerializeField] private GameObject VFX;
    [SerializeField] private GameObject LightSource;

    public void TurnOn()
    {
        VFX.SetActive(true);
        LightSource.SetActive(true);
        
        turn = true;
    }

    public void TurnOff()
    {
        VFX.SetActive(false);
        LightSource.SetActive(false);
        
        turn = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (PlaceableManager.Instance.IsEdit) return;
        
        if (turn)
        {
            TurnOff();
        }
        else
        {
            TurnOn();
        }
    }
}
