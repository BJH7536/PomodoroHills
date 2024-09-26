using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DataManagement;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 인벤토리 팝업을 관리하는 클래스입니다.
/// 팝업의 열림과 닫힘 애니메이션을 처리하고, 탭 전환을 관리하며,
/// 아이템 데이터를 로드하고 UI에 표시합니다.
/// </summary>
public class InventoryPopup : Popup
{
    #region Serialized Fields

    [Tab("Tweening")] 
    [SerializeField] private RectTransform panel;                       // 애니메이션을 적용할 패널
    [SerializeField] private RectTransform panelOpeningStartPosition;   // 애니메이션 시작 위치
    [SerializeField] private RectTransform panelOpeningEndPosition;     // 애니메이션 종료 위치
    [SerializeField] private Ease openingEase = Ease.OutQuad;           // 애니메이션 이징 함수
    [SerializeField] private float duration = 0.5f;                     // 애니메이션 지속 시간
    
    [Tab("Inventory")]
    [SerializeField] private List<Toggle> tabToggles;                   // 탭 토글 리스트
    [SerializeField] private List<GameObject> contentPanels;            // 탭에 대응하는 콘텐츠 패널 리스트
    [SerializeField] private HorizontalLayoutGroup layoutGroup;         // 레이아웃 그룹
    [SerializeField] private LoadingUI loadingUI;                       // 로딩 UI 참조

    [Header("Scroll Rects")]
    [SerializeField] private ScrollRect buildingScrollRect;
    [SerializeField] private ScrollRect decorationScrollRect;
    [SerializeField] private ScrollRect seedScrollRect;
    [SerializeField] private ScrollRect cropScrollRect;
    
    [Header("Item Management")]
    [SerializeField] private ItemUIPool itemUIPool; // ItemUIPool 참조

    #endregion

    #region Private Fields

    private Tweener openTweener;    // 열림 애니메이션 트위너
    private Tweener closeTweener;   // 닫힘 애니메이션 트위너

    #endregion

    #region Unity Lifecycle Methods

    /// <summary>
    /// 오브젝트가 활성화될 때 호출됩니다.
    /// 탭을 초기화하고 레이아웃 그룹을 비활성화하며,
    /// 아이템 데이터를 로드하고 UI에 표시합니다.
    /// </summary>
    private async void OnEnable()
    {
        // ItemUIPool 초기화 여부 확인
        if (!itemUIPool.isInitialized)
        {
            await WaitForUIPoolInitialization();  // 초기화가 완료될 때까지 대기
        }
        
        InitializeTabs();
        DisableLayoutGroupAfterFrameAsync().Forget();
        await LoadItemsAndPopulateUIAsync();
        
        // DataManager 이벤트 구독
        if (DataManager.Instance != null)
        {
            DataManager.Instance.OnItemAdded += HandleItemAdded;
            DataManager.Instance.OnItemDeleted += HandleItemDeleted;
        }
    }

    /// <summary>
    /// 오브젝트가 비활성화될 때 호출됩니다.
    /// 이벤트 리스너를 해제합니다.
    /// </summary>
    private void OnDisable()
    {
        foreach (var toggle in tabToggles)
        {
            toggle.onValueChanged.RemoveAllListeners();
        }

        // 모든 아이템 UI를 풀에 반환
        ClearAllItems();

        // DataManager 이벤트 구독 해제
        if (DataManager.Instance != null)
        {
            DataManager.Instance.OnItemAdded -= HandleItemAdded;
            DataManager.Instance.OnItemDeleted -= HandleItemDeleted;
        }
    }

    #endregion

    #region Initialization Methods

    /// <summary>
    /// 탭 토글을 초기화하고 이벤트 리스너를 설정합니다.
    /// </summary>
    private void InitializeTabs()
    {
        // 각 Toggle에 대해 OnValueChanged 이벤트를 등록
        for (int i = 0; i < tabToggles.Count; i++)
        {
            int index = i; // 클로저 문제를 방지하기 위해 로컬 변수에 인덱스 저장
            tabToggles[i].onValueChanged.AddListener(isOn => OnTabChanged(isOn, index));
        }

        // 시작 시 첫 번째 탭 활성화
        ActivateTab(0);
    }

    /// <summary>
    /// 지정된 인덱스의 탭을 활성화하고, 해당하는 콘텐츠 패널을 표시합니다.
    /// </summary>
    /// <param name="index">활성화할 탭의 인덱스</param>
    private void ActivateTab(int index)
    {
        if (tabToggles.Count == 0 || index < 0 || index >= tabToggles.Count)
            return;

        foreach (var toggle in tabToggles)
        {
            toggle.isOn = false;
        }

        tabToggles[index].isOn = true;
        UpdateContentPanels(index);
    }

    /// <summary>
    /// 활성화된 탭에 따라 콘텐츠 패널을 업데이트합니다.
    /// </summary>
    /// <param name="activeIndex">활성화된 탭의 인덱스</param>
    private void UpdateContentPanels(int activeIndex)
    {
        for (int i = 0; i < contentPanels.Count; i++)
        {
            contentPanels[i].SetActive(i == activeIndex);
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// 탭의 상태가 변경될 때 호출됩니다.
    /// </summary>
    /// <param name="isOn">토글 상태</param>
    /// <param name="index">토글의 인덱스</param>
    private void OnTabChanged(bool isOn, int index)
    {
        if (isOn)
        {
            UpdateContentPanels(index);
            PopulateItemsForCurrentTab();
        }
    }

    #endregion

    #region Layout Management
    
    /// <summary>
    /// 한 프레임 대기 후 레이아웃 그룹을 비활성화합니다.
    /// </summary>
    private async UniTaskVoid DisableLayoutGroupAfterFrameAsync()
    {
        await UniTask.Yield(PlayerLoopTiming.Update); // 한 프레임 대기

        // 강제로 레이아웃을 업데이트
        Canvas.ForceUpdateCanvases();
        
        if (layoutGroup != null)
        {
            layoutGroup.enabled = false; // LayoutGroup 비활성화
        }
    }

    #endregion

    #region Animation Methods

    /// <summary>
    /// 팝업을 애니메이션과 함께 여는 메서드입니다.
    /// 기존 애니메이션을 종료하고 새로운 열림 애니메이션을 시작합니다.
    /// </summary>
    public async Task OpenWithAnimation()
    {
        KillExistingTweeners();

        // 현재 위치에서 시작하여 종료 위치로 애니메이션 실행
        panel.anchoredPosition = panelOpeningStartPosition.anchoredPosition;
        openTweener = CreateTween(panelOpeningEndPosition.anchoredPosition, duration, openingEase);
        await openTweener.ToUniTask();
    }

    /// <summary>
    /// 팝업을 애니메이션과 함께 닫는 메서드입니다.
    /// 기존 애니메이션을 종료하고 새로운 닫힘 애니메이션을 시작합니다.
    /// </summary>
    public async Task CloseWithAnimation()
    {
        KillExistingTweeners();

        // 현재 위치에서 시작하여 시작 위치로 애니메이션 실행
        closeTweener = CreateTween(panelOpeningStartPosition.anchoredPosition, duration, openingEase)
            .OnComplete(() =>
            {
                base.Hide();
            });
        await closeTweener.ToUniTask();
    }

    /// <summary>
    /// 애니메이션 트위너를 생성하고 설정합니다.
    /// </summary>
    /// <param name="endValue">애니메이션 종료 위치</param>
    /// <param name="duration">애니메이션 지속 시간</param>
    /// <param name="ease">애니메이션 이징 함수</param>
    /// <returns>설정된 Tweener</returns>
    private Tweener CreateTween(Vector2 endValue, float duration, Ease ease)
    {
        return panel.DOAnchorPos(endValue, duration)
                    .SetEase(ease)
                    .SetUpdate(true); // 게임이 일시정지되었을 때도 트윈 실행
    }

    /// <summary>
    /// 기존에 실행 중인 트위너를 종료합니다.
    /// </summary>
    private void KillExistingTweeners()
    {
        openTweener?.Kill();
        closeTweener?.Kill();
    }

    #endregion

    #region Popup Control Methods

    /// <summary>
    /// 팝업을 표시하고, 애니메이션을 실행합니다.
    /// </summary>
    public override async void Show()
    {
        base.Show(); // 팝업 GameObject 활성화
        await OpenWithAnimation();
    }

    /// <summary>
    /// 팝업을 닫고, 애니메이션을 실행합니다.
    /// </summary>
    public override async void Hide()
    {
        await CloseWithAnimation();
    }

    #endregion

    #region Item Management Methods

    /// <summary>
    /// 아이템 데이터를 로드하고, UI에 표시합니다.
    /// </summary>
    private async Task LoadItemsAndPopulateUIAsync()
    {
        // 로딩 UI 표시
        loadingUI.ShowLoading();
        
        // 아이템 UI 업데이트
        PopulateItems();

        // 레이아웃 갱신을 위한 한 프레임 대기
        await UniTask.Yield();

        // 레이아웃 업데이트 강제 수행
        ForceRebuildLayout();

        // 로딩 UI 숨김
        loadingUI.HideLoading();
    }

    /// <summary>
    /// 로드된 아이템 데이터를 기반으로 UI를 업데이트합니다.
    /// </summary>
    private void PopulateItems()
    {
        // 기존 아이템을 클리어하고, 풀에 반환
        ClearAllItems();

        // DataManager에서 아이템 리스트를 가져와 UI에 표시
        List<DataManagement.Item> currentItems = DataManager.Instance.GetItems();

        if (currentItems == null || currentItems.Count == 0)
        {
            Debug.LogWarning("No items found to populate.");
            return;
        }

        // 아이템을 이름순으로 정렬합니다
        currentItems.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCulture));

        foreach (DataManagement.Item item in currentItems)
        {
            GameObject itemGO = itemUIPool.GetItemUI(item.Type);

            if (itemGO == null)
            {
                Debug.LogError($"Failed to get item UI for type: {item.Type}");
                continue;
            }

            itemGO.transform.SetParent(GetScrollContent(item.Type), false);
            InventoryPopup_ItemUI inventoryPopupItemUI = itemGO.GetComponent<InventoryPopup_ItemUI>();
            if (inventoryPopupItemUI != null)
            {
                inventoryPopupItemUI.Setup(item);
            }
            else
            {
                Debug.LogError("InventoryPopup_ItemUI component missing from the item UI prefab.");
            }
        }

        // 레이아웃 갱신 및 컴포넌트 비활성화
        EnableLayoutComponents();
        UniTask.Void(async () =>
        {
            await UniTask.Yield();
            ForceRebuildLayout();
            DisableLayoutComponents();
        });
    }


    /// <summary>
    /// 현재 활성화된 탭에 따라 아이템을 표시합니다.
    /// </summary>
    private void PopulateItemsForCurrentTab()
    {
        // Clear existing items
        ClearAllItems();
    
        List<DataManagement.Item> currentItems = DataManager.Instance.GetItems();
        if (currentItems == null || currentItems.Count == 0)
        {
            Debug.LogWarning("No items to display for the current tab.");
            return;
        }

        currentItems.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCulture));

        foreach (DataManagement.Item item in currentItems)
        {
            GameObject itemGO = itemUIPool.GetItemUI(item.Type); // 각 타입에 맞는 풀에서 UI 가져오기

            if (itemGO == null)
            {
                Debug.LogError($"Failed to get item UI for type: {item.Type}");
                continue;
            }

            itemGO.transform.SetParent(GetScrollContent(item.Type), false);

            InventoryPopup_ItemUI inventoryPopupItemUI = itemGO.GetComponent<InventoryPopup_ItemUI>();
            if (inventoryPopupItemUI != null)
            {
                inventoryPopupItemUI.Setup(item);
            }
            else
            {
                Debug.LogError("InventoryPopup_ItemUI component is missing on the item UI prefab.");
            }
        }

        // 레이아웃 갱신 및 컴포넌트 비활성화
        EnableLayoutComponents();
        UniTask.Void(async () =>
        {
            await UniTask.Yield();
            ForceRebuildLayout();
            DisableLayoutComponents();
        });
    }

    
    /// <summary>
    /// 아이템 타입에 따라 해당 ScrollRect의 콘텐츠를 반환합니다.
    /// </summary>
    /// <param name="type">아이템의 타입</param>
    /// <returns>해당 타입의 ScrollRect 콘텐츠</returns>
    private Transform GetScrollContent(ItemType type)
    {
        switch (type)
        {
            case ItemType.Building:
                return buildingScrollRect.content;
            case ItemType.Decoration:
                return decorationScrollRect.content;
            case ItemType.Seed:
                return seedScrollRect.content;
            case ItemType.Crop:
                return cropScrollRect.content;
            default:
                Debug.LogWarning($"알 수 없는 아이템 타입: {type}");
                return buildingScrollRect.content;
        }
    }

    /// <summary>
    /// 모든 아이템 UI를 풀에 반환하고, 콘텐츠를 정리합니다.
    /// </summary>
    private void ClearAllItems()
    {
        foreach (ScrollRect scrollRect in new ScrollRect[] { buildingScrollRect, decorationScrollRect, seedScrollRect, cropScrollRect })
        {
            if (scrollRect != null && scrollRect.content != null)
            {
                for (int i = scrollRect.content.childCount - 1; i >= 0; i--)
                {
                    Transform child = scrollRect.content.GetChild(i);
                    InventoryPopup_ItemUI inventoryPopupItemUI = child.GetComponent<InventoryPopup_ItemUI>();
                    if (inventoryPopupItemUI != null)
                    {
                        itemUIPool.ReturnItemUI(child.gameObject, inventoryPopupItemUI.GetItemType()); // 타입을 기준으로 반환
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// DataManager의 OnItemAdded 이벤트 핸들러
    /// </summary>
    /// <param name="newItem">추가된 아이템</param>
    private async void HandleItemAdded(DataManagement.Item newItem)
    {
        // 현재 활성화된 탭에 해당하는 ScrollRect에 아이템 추가
        GameObject itemGO = itemUIPool.GetItemUI(newItem.Type);
        itemGO.transform.SetParent(GetScrollContent(newItem.Type), false);
        InventoryPopup_ItemUI inventoryPopupItemUI = itemGO.GetComponent<InventoryPopup_ItemUI>();
        if (inventoryPopupItemUI != null)
        {
            inventoryPopupItemUI.Setup(newItem);
        }

        // 레이아웃 컴포넌트 활성화 후 업데이트, 이후 비활성화
        EnableLayoutComponents();
        await UniTask.Yield(); // 레이아웃이 제대로 적용될 수 있도록 한 프레임 대기
        ForceRebuildLayout();
        DisableLayoutComponents(); // 레이아웃 갱신 후 비활성화
    }

    /// <summary>
    /// DataManager의 OnItemDeleted 이벤트 핸들러
    /// </summary>
    /// <param name="itemID">삭제된 아이템의 ID</param>
    private async void HandleItemDeleted(string itemID)
    {
        // 모든 ScrollRect의 콘텐츠를 순회하며 해당 아이템을 찾아 풀에 반환
        foreach (ScrollRect scrollRect in new ScrollRect[] { buildingScrollRect, decorationScrollRect, seedScrollRect, cropScrollRect })
        {
            if (scrollRect != null && scrollRect.content != null)
            {
                foreach (Transform child in scrollRect.content)
                {
                    InventoryPopup_ItemUI inventoryPopupItemUI = child.GetComponent<InventoryPopup_ItemUI>();
                    if (inventoryPopupItemUI != null && inventoryPopupItemUI.itemID == itemID)
                    {
                        itemUIPool.ReturnItemUI(child.gameObject, inventoryPopupItemUI.GetItemType());
                        break; // 해당 아이템을 찾으면 더 이상 탐색할 필요 없음
                    }
                }
            }
        }

        // 레이아웃 컴포넌트 활성화 후 업데이트, 이후 비활성화
        EnableLayoutComponents();
        await UniTask.Yield(); // 레이아웃이 제대로 적용될 수 있도록 한 프레임 대기
        ForceRebuildLayout();
        DisableLayoutComponents(); // 레이아웃 갱신 후 비활성화
    }

    /// <summary>
    /// 모든 ScrollRect의 Content 레이아웃을 강제로 갱신합니다.
    /// </summary>
    private void ForceRebuildLayout()
    {
        List<ScrollRect> scrollRects = new List<ScrollRect>
        {
            buildingScrollRect,
            decorationScrollRect,
            seedScrollRect,
            cropScrollRect
        };

        foreach (ScrollRect scrollRect in scrollRects)
        {
            if (scrollRect != null && scrollRect.content != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content.GetComponent<RectTransform>());
            }
        }
    }
    
    private async UniTask WaitForUIPoolInitialization()
    {
        // 1초 이내로 풀 초기화 완료를 기다립니다.
        float timeout = 1f;
        float timeElapsed = 0f;

        while (!itemUIPool.isInitialized && timeElapsed < timeout)
        {
            await UniTask.Yield();
            timeElapsed += Time.deltaTime;
        }

        if (!itemUIPool.isInitialized)
        {
            Debug.LogError("ItemUIPool 초기화가 예상 시간 내에 완료되지 않았습니다.");
        }
    }

    #endregion

    #region Layout Component Management

    /// <summary>
    /// 모든 ScrollRect의 Content에 붙어있는 GridLayoutGroup과 ContentSizeFitter를 활성화합니다.
    /// </summary>
    private void EnableLayoutComponents()
    {
        foreach (ScrollRect scrollRect in new ScrollRect[] { buildingScrollRect, decorationScrollRect, seedScrollRect, cropScrollRect })
        {
            if (scrollRect != null && scrollRect.content != null)
            {
                GridLayoutGroup grid = scrollRect.content.GetComponent<GridLayoutGroup>();
                ContentSizeFitter fitter = scrollRect.content.GetComponent<ContentSizeFitter>();

                if (grid != null)
                    grid.enabled = true;

                if (fitter != null)
                    fitter.enabled = true;
            }
        }
    }

    /// <summary>
    /// 모든 ScrollRect의 Content에 붙어있는 GridLayoutGroup과 ContentSizeFitter를 비활성화합니다.
    /// </summary>
    private void DisableLayoutComponents()
    {
        foreach (ScrollRect scrollRect in new ScrollRect[] { buildingScrollRect, decorationScrollRect, seedScrollRect, cropScrollRect })
        {
            if (scrollRect != null && scrollRect.content != null)
            {
                GridLayoutGroup grid = scrollRect.content.GetComponent<GridLayoutGroup>();
                ContentSizeFitter fitter = scrollRect.content.GetComponent<ContentSizeFitter>();

                if (grid != null)
                    grid.enabled = false;

                if (fitter != null)
                    fitter.enabled = false;
            }
        }
    }
    
    #endregion
}
