using UnityEngine;
using UnityEngine.EventSystems;
using VInspector;

public class House : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool turn;
    [SerializeField] private GameObject LightSource;

    [Button]
    public void TurnOn()
    {
        LightSource.SetActive(true);
        
        turn = true;
    }

    [Button]
    public void TurnOff()
    {
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
