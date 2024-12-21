using System;
using UnityEngine;
using UnityEngine.EventSystems;
using VInspector;

public class Campfire : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool turn;
    [SerializeField] private GameObject VFX;
    [SerializeField] private GameObject LightSource;

    [Button]
    public void TurnOn()
    {
        VFX.SetActive(true);
        LightSource.SetActive(true);
        
        turn = true;
    }

    [Button]
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
