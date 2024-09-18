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
//현재 스마트폰 환경에서의 다중입력을 생각하지 않았습니다. 조작방식도 스마트폰과 맞지않을겁니다.

//PlaceableManager와 입력 작업을 분리하여 다른 탭 사용 중 해당 스크립트를 비활성화합니다.

public class InteractionManager : MonoBehaviour       //해당 작업은 다른 탭을 이용 중일 때 비활성화 되어야합니다. -> 따라서 PlaceableManager와 분리합니다.
{
    public EventSystem eventSystem;
    public GameObject editOptionButton;

    private bool isDrag;                //드래그할때 해당하는 작업을 실시간으로 사용합니다.
    private Vector3 mouseDownPosition;  //클릭 된 위치를 기억합니다. (미사용될것같습니다.)
    private Vector3 startClickPosition;
    private float dragSpeed = 0.1f;
    public float dragThreshold = 0.5f;
    private float clickCheckTime = 0.5f;
    public float clickStartTime = 0f;

    // Update is called once per frame
    void Update()
    {
        InputMouse();
    }


    void InputMouse()
    {
        if (Input.GetMouseButtonDown(0))    //마우스 클릭될 때...
        {
            startClickPosition = Input.mousePosition;
            clickStartTime = Time.time;
            isDrag = false;
        }
        else if (Input.GetMouseButton(0))
        {


            Vector3 currentMousePosition = Input.mousePosition;
            float distance = Vector3.Distance(startClickPosition, Input.mousePosition);

            if (!isDrag && distance > dragThreshold)
            {
                isDrag = true;
            }

            if (isDrag)
            {
                OnDrag();
            }

        }
        else if (Input.GetMouseButtonUp(0))      //클릭이 취소될 때...
        {
            float clickDuration = Time.time - clickStartTime;
            if (!isDrag && clickDuration < clickCheckTime)
            {
                OnClick();
            }
        }

    }

    void OnClick()
    {
        if (!PlaceableManager.Instance.isEdit)              // 터치 상호작용...
        {

        }
        else if (!PlaceableManager.Instance.isMoveEdit)     // 편집대상 선택
        {
            SelectObject();
        }
        else                                                // 편집대상 이동
        {
            if (PlaceableManager.Instance.selectedItem != null)
            {
                MovePlaceable();
            }
            else { Debug.Log("선택된 아이템이 없는데, 건물이동을 시도하고 있습니다."); }
        }
    }

    private void OnDrag()
    {
        if (!PlaceableManager.Instance.isEdit)              // 터치 상호작용...
        {

        }
        else if (!PlaceableManager.Instance.isMoveEdit)     // 편집대상 선택
        {

        }
        else                                                // 편집대상 이동
        {
            if (PlaceableManager.Instance.selectedItem != null)     //실시간 이동으로....
            {
                MovePlaceable();
            }
        }
    }




    void MovePlaceable()   // 선택한 오브젝트를 이동할때 선택된 오브젝트와 그 아래 그리드맵을 향해서만 클릭등의 입력을 받도록 제한하여 구현합니다. 이때 클릭이 UI의 영향을 받는 것을 고려합니다.
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("TileMap"))
            {
                // 충돌 지점의 좌표 가져오기
                Vector3 hitPoint = hit.point;

                // x, z 좌표 정수로 반올림
                int nearestX = Mathf.RoundToInt(hitPoint.x);
                int nearestZ = Mathf.RoundToInt(hitPoint.z);

                // 결과 출력
                Debug.Log($"Clicked Point: {hitPoint}, Nearest Integer Coordinates: ({nearestX}, {nearestZ})");
                if (PlaceableManager.Instance.selectedItem != null)
                {
                    GameObject selectedObject = PlaceableManager.Instance.selectedItem;
                    Vector3 newPosition = new Vector3(nearestX, 0f, nearestZ);
                    selectedObject.transform.position = newPosition;
                }
                break;
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

            // 객체의 태그를 확인하여 특정 태그인 경우에만 처리
            if (clickedObject.CompareTag("Placeable"))
            {
                if(PlaceableManager.Instance.selectedItem != null)  //기존의 선택된 
                {

                }
                PlaceableManager.Instance.selectedItem = clickedObject;
                Debug.Log(PlaceableManager.Instance.selectedItem.name + " has been selected.");
                Debug.Log(PlaceableManager.Instance.selectedItem.name + " has been selected.");

                if (editOptionButton != null)
                {
                    editOptionButton.SetActive(true);
                }
            }
        }else
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
