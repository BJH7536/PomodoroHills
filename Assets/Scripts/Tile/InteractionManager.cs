using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//<미해결>
//이동편집모드 진입시 편집모드 UI에서 빠져나오는 UI가 사라질 필요가 있음
//이동편집모드 진입시 UI 중 회전 UI가 생성되어야함


public class InteractionManager : MonoBehaviour       //해당 작업은 다른 탭을 이용 중일 때 비활성화 되어야합니다. -> 따라서 PlaceableManager와 분리합니다.
{
    public EventSystem eventSystem;
    public GameObject editOptionButton;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))    //조건을 isMoveEdit으로 구분해주세요
        {
            OnClickDetected();
        }    
    }

    void OnClickDetected()
    {
        if (PlaceableManager.Instance != null && PlaceableManager.Instance.isEdit)
        {
            if (!PlaceableManager.Instance.isMoveEdit)
            {
                if (!IsMouseOnUI())  //clickableUI 태그를 가진 UI와 오브젝트가 화면에서 겹칠 때 오브젝트 클릭을 무시합니다.
                {
                    SelectObject();
                }
            }
            else
            {
                if (PlaceableManager.Instance.selectedItem != null)
                {
                    Vector3 mousePosition = Input.mousePosition;
                    MovePlaceable(mousePosition);
                }
                else { Debug.Log("선택된 아이템이 없는데, 건물이동을 시도하고 있습니다."); }   
                //이부분을 분리해서 Update에서 isEdit, isMoveEdit의 상태에 따라 마우스 입력방식을 분리해 주세요
            }
        }
        else
        {
            //Placeable에서 클릭시 작동되는 애니메이션 혹은 상호작용 활성화를 이 부분과 연계
        }

    }
    void OnClickPerformer()
    {
        if (! (PlaceableManager.Instance != null))
        {
            Debug.Log("there is no PlaceableManager Instance.");
        }
        else
        {
            
            if (PlaceableManager.Instance.isEdit)
            {
                if (PlaceableManager.Instance.isMoveEdit)
                {
                    //이 상태에서는 MoveObject() 메서드 사용
                }
                else
                {
                    SelectObject();
                }
            }
            else
            {
                //  클릭 대상 Placeable의 상호작용 메서드 실행하기
            }
            
        }
    }


    void MovePlaceable(Vector3 position)   // 선택한 오브젝트를 이동할때 선택된 오브젝트와 그 아래 그리드맵을 향해서만 클릭등의 입력을 받도록 제한하여 구현합니다. 이때 클릭이 UI의 영향을 받는 것을 고려합니다.
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (ray.direction.y != 0)
        {
            float targetY = 0f;
            float t = (targetY - ray.origin.y) / ray.direction.y;
            if (t >= 0)
            {
                Vector3 intersectionPoint = ray.origin + (ray.direction * t);

                Vector3 placementPosition = Utility.SnapToGrid(intersectionPoint,1f);
            }
        }
    }

    private bool IsMouseOnUI()      //마우스가 UI 위에 있는지 검증합니다. 오브젝트 조작과 UI 조작이 겹칠 경우의 조작을 유효하게 하기 위한 절차입니다.
    {
        PointerEventData pointerEventData = new PointerEventData(eventSystem)
        {position = Input.mousePosition};
        
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        if (raycastResults.Count > 0)
        {
            return raycastResults[0].gameObject.CompareTag("ClickableUI");
        }
        return false;
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
                Debug.Log(PlaceableManager.Instance.selectedItem.name + "has been selected.");
                if (editOptionButton != null)
                { editOptionButton.SetActive(true); }
            }
            else
            {
                PlaceableManager.Instance.selectedItem = null;
                if(editOptionButton != null)
                { editOptionButton.SetActive(false); }
            }
        }
        else
        {
            PlaceableManager.Instance.selectedItem = null;// 중복 코드, 클릭 고려필요 
            if (editOptionButton != null)                   //별개의 함수로 분리
            { editOptionButton.SetActive(false); }
        }


    }
    
    public void LoseSelectedItem()     //UI가 클릭되었는지 아닌지를 확인하는 절차가 필요
    {                           //상기 절차로 ClickableUI tag를 가지는 UI 클릭시 해당 메소드 미실행
        PlaceableManager.Instance.selectedItem = null; 
        if (editOptionButton != null)
        { editOptionButton.SetActive(false); }
    }
    
    public void OnMoveSelectedItem()
    {
        if(PlaceableManager.Instance.selectedItem != null)
        {
            PlaceableManager.Instance.OnIsMoveEdit();
        }
    } 
    public void OffMoveSelectedItem()
    {
        if(PlaceableManager.Instance.selectedItem != null)
        {
            PlaceableManager.Instance.OffIsMoveEdit();
        }
    }
}
