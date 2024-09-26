using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPlaceable : MonoBehaviour
{
    // placeable 처음으로 만들때(꺼낼) 쓸거에용
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





    void SelectObject() //이거 안되게 해야됨
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
