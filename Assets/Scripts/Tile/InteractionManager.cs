using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-1)]
public class InteractionManager : MonoBehaviour
{
    private static InteractionManager instance;
    public static InteractionManager Instance => instance;

    [SerializeField] private EventSystem eventSystem;

    [SerializeField] private GameObject tileMapMainOptionUI;
    [SerializeField] private GameObject modeOptionUI;
    [SerializeField] private GameObject editOptionUI;
    [SerializeField] private GameObject selectOptionUI;
    [SerializeField] private GameObject moveOptionUI;
    [SerializeField] private GameObject placeableChestUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        TouchManager.Instance.OnClick += HandleClick;
        // TouchManager.Instance.OnClick += (vec)=> { DebugEx.Log($"OnClick");};

        TouchManager.Instance.OnDoubleClick += HandleDoubleClick;
        // TouchManager.Instance.OnDoubleClick += (vec)=> { DebugEx.Log($"OnDoubleClick");};
        
        TouchManager.Instance.OnDrag += HandleDrag;
        // TouchManager.Instance.OnDrag += (vec)=> { DebugEx.Log($"OnDrag");};

    }

    private void OnDisable()
    {
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.OnClick -= HandleClick;
            TouchManager.Instance.OnDoubleClick -= HandleDoubleClick;
            TouchManager.Instance.OnDrag -= HandleDrag;
        }
    }
    
    private void HandleClick(Vector2 position)
    {
        if (!PlaceableManager.Instance.IsEdit)
        {
            // 편집 모드가 아닐 때의 클릭 처리
        }
        else if (!PlaceableManager.Instance.IsMoveEdit)
        {
            SelectObject(position);
        }
        else
        {
            if (PlaceableManager.Instance.SelectedPlaceable != null)
            {
                MovePlaceable(position);
            }
            else
            {
                DebugEx.LogWarning("선택된 아이템이 없는데, 건물이동을 시도하고 있습니다.");
            }
        }
    }

    private void HandleDoubleClick(Vector2 position)
    {
        // 더블 클릭 시의 처리 로직을 여기에 구현하세요.
    }

    private void HandleDrag(Vector2 position)
    {
        if (PlaceableManager.Instance.IsMoveEdit && PlaceableManager.Instance.SelectedPlaceable != null)
        {
            MovePlaceable(position);
        }
    }

    private void MovePlaceable(Vector2 pointerPosition)
    {
        if (IsPointerOverUI(pointerPosition)) return;

        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("TileMap"))
            {
                Vector3 hitPoint = hit.point;
                int nearestX = Mathf.RoundToInt(hitPoint.x);
                int nearestZ = Mathf.RoundToInt(hitPoint.z);

                if (PlaceableManager.Instance.SelectedPlaceable != null)
                {
                    GameObject selectedObject = PlaceableManager.Instance.SelectedPlaceable;
                    Placeable placeable = selectedObject.GetComponent<Placeable>();
                    Vector3 newPosition = new Vector3(nearestX, 0f, nearestZ);
                    placeable.position = new Vector2Int(nearestX, nearestZ);
                    selectedObject.transform.position = newPosition;
                    
                    PlaceableManager.Instance.ChangePlaceableColor();
                }
                break;
            }
        }
    }

    private bool IsPointerOverUI(Vector2 pointerPosition)
    {
        PointerEventData pointerEventData = new PointerEventData(eventSystem)
        {
            position = pointerPosition
        };

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerEventData, raycastResults);

        return raycastResults.Count > 0 && raycastResults[0].gameObject.layer == LayerMask.NameToLayer("UI");
    }

    private void SelectObject(Vector2 pointerPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject clickedObject = hit.collider.transform.parent.gameObject;

            if (clickedObject.CompareTag("Placeable"))
            {
                PlaceableManager.Instance.SelectedPlaceable = clickedObject;
                DebugEx.Log($"{PlaceableManager.Instance.SelectedPlaceable.name}이(가) 선택되었습니다.");

                selectOptionUI?.SetActive(true);
            }
            else
            {
                DeselectPlaceable();
            }
        }
        else
        {
            DeselectPlaceable();
        }
    }

    public void UnpackPlaceable(int itemCode)
    {
        if (PlaceableManager.Instance.UnpackPlaceable(itemCode))
        {
            //ClosePlaceableChestUI();
            OnMoveSelectedPlaceable();
        }
    }

    public void PackPlaceable()
    {
        if (PlaceableManager.Instance.PackPlaceable())
        {
            selectOptionUI?.SetActive(false);
        }
    }

    public void DeselectPlaceable()
    {
        PlaceableManager.Instance.SelectedPlaceable = null;
        selectOptionUI?.SetActive(false);
        moveOptionUI?.SetActive(false);
    }

    public void OnMoveSelectedPlaceable()
    {
        if (PlaceableManager.Instance.SelectedPlaceable != null)
        {
            tileMapMainOptionUI?.SetActive(false);
            moveOptionUI?.SetActive(true);
            selectOptionUI?.SetActive(false);
            PlaceableManager.Instance.OnIsMoveEdit();
        }
    }

    public void OffMoveSelectedPlaceable()
    {
        if (PlaceableManager.Instance.SelectedPlaceable != null)
        {
            PlaceableManager.Instance.OffIsMoveEdit();
            moveOptionUI?.SetActive(false);
            tileMapMainOptionUI?.SetActive(true);
        }
    }

    #region Click Functions

    public void ClickConfirmEdit()
    {
        if (PlaceableManager.Instance.ConfirmEdit())
        {
            OffMoveSelectedPlaceable();
        }
        else
        {
            Debug.LogWarning("편집 확인 실패");
        }
    }

    public void ClickRotate()
    {
        PlaceableManager.Instance.RotatePlaceable();
    }

    public void ClickCancelEdit()
    {
        PlaceableManager.Instance.CancelEdit();
        OffMoveSelectedPlaceable();
    }

    public void ClickEdit()
    {
        PlaceableManager.Instance.OnEditMode();
        editOptionUI?.SetActive(true);
        modeOptionUI?.SetActive(false);
    }

    public void ClickEndEdit()
    {
        PlaceableManager.Instance.OffEditMode();
        editOptionUI?.SetActive(false);
        modeOptionUI?.SetActive(true);
    }

    public void ClickPlaceableChest()
    {
        placeableChestUI?.SetActive(true);
        selectOptionUI?.SetActive(false);
    }

    #endregion

    // public void ClosePlaceableChestUI()
    // {
    //     placeableChestUI?.SetActive(false);
    // }
}
