using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPlaceable : MonoBehaviour
{
    // placeable ó������ ���鶧(����) ���ſ���
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (PlaceableManager.Instance !=null && PlaceableManager.Instance.isEdit)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SelectObject();
            }
        }
    }





    void SelectObject()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject clickedObject = hit.collider.gameObject;

            if (clickedObject.TryGetComponent<Placeable>(out var placeable))
            {
                PlaceableManager.Instance.selectedItem = clickedObject;
                Debug.Log(PlaceableManager.Instance.selectedItem.name);
                Debug.Log("Test");
            }
        }

    }
}
