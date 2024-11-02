using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class ScrollSystem : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    #region Fields

    [SerializeField] private ScrollRect scrollRect;         // ScrollRect 참조, 스크롤 가능한 UI 영역을 정의
    [SerializeField] private GameObject itemPrefab;         // 스크롤 항목의 프리팹
    [SerializeField] private int minValue = 2024;           // 스크롤 항목의 최소값
    [SerializeField] private int maxValue = 2100;           // 스크롤 항목의 최대값
        
    [SerializeField] private float offsetY = 60;            // 스크롤의 Y축 오프셋
    [SerializeField] private float snapThreshold = 20f;     // 스냅 동작을 위한 속도 임계값
    [SerializeField] private float maxScrollSpeed = 1000f;  // 최대 스크롤 속도 제한
    [SerializeField] private int selectedValue;             // 현재 선택된 항목의 값

    private List<GameObject> items = new List<GameObject>();        // 스크롤 항목 오브젝트의 리스트
    private List<TMP_Text> itemTexts = new List<TMP_Text>();        // 항목 텍스트 캐싱
    private Queue<GameObject> itemPool = new Queue<GameObject>();   // 객체 풀링을 위한 큐
    private float itemHeight;                                       // 항목의 높이
    private int previousIndex = -1;                                 // 이전에 선택된 항목의 인덱스

    [SerializeField] private int BufferItemCount = 1; // 화면 밖에 추가로 생성할 버퍼 항목 수.
    private const float CheckInterval = 0.1f;               // 스크롤 체크 간격.
    private float lastCheckTime = 0;                        // 마지막 체크 시간.
    private int itemCount;                                  // 총 항목 수.
    private int visibleItemCount;                           // 화면에 보이는 항목 수.
    
    [SerializeField] private bool isScrolling = false;          // 스크롤 중인지 여부.
    [SerializeField] private bool isSnapping = false;           // 스냅 중인지 여부.
    [SerializeField] private bool isUserInteracting = false;    // 사용자가 드래그 중인지 여부.
    
    private event Action<int> onValueChanged;                   // 값 변경 시 호출되는 콜백.

    #endregion

    #region Setup

    /// <summary>
    /// ScrollSystem을 설정하는 함수.
    /// 최소값, 최대값, 값 변경 콜백을 설정하고 스크롤 항목을 초기화.
    /// </summary>
    /// <param name="minValue">스크롤 항목의 최소값</param>
    /// <param name="maxValue">스크롤 항목의 최대값</param>
    /// <param name="onValueChanged">값이 변경될 때 호출되는 콜백</param>
    /// <param name="value">초기 선택된 값</param>
    public void Setup(int minValue, int maxValue, Action<int> onValueChanged, int value)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.onValueChanged = onValueChanged;

        // 기존 아이템을 정리
        ClearItems();
        
        Initialize(value);
    }

    #endregion

    #region Unity Lifecycle
    
    /// <summary>
    /// Unity의 Update 메서드. 매 프레임마다 호출됨.
    /// 스크롤 속도 제한, 스냅 동작, 무한 스크롤을 처리함.
    /// </summary>
    void Update()
    {
        lastCheckTime += Time.deltaTime;

        // 사용자가 스크롤 중이거나 스냅 중일 경우 최적화를 위해 바로 반환
        if (isUserInteracting || isSnapping) return;
        
        if (lastCheckTime >= CheckInterval)
        {
            LimitScrollSpeed(); // 최대 스크롤 속도 제한

            // 스크롤이 멈췄을 때 가장 가까운 항목으로 스냅
            if (isScrolling && Mathf.Abs(scrollRect.velocity.y) < snapThreshold)
            {
                isScrolling = false;
                SnapToClosestItemAsync().Forget();
            }

            HandleInfiniteScroll(); // 무한 스크롤 처리
            lastCheckTime = 0;
        }
    }

    /// <summary>
    /// 스크롤 속도를 제한하는 메서드. 설정된 최대 스크롤 속도를 초과하지 않도록 함.
    /// </summary>
    private void LimitScrollSpeed()
    {
        if (Mathf.Abs(scrollRect.velocity.y) > maxScrollSpeed)
        {
            float clampedSpeed = Mathf.Sign(scrollRect.velocity.y) * maxScrollSpeed;
            scrollRect.velocity = new Vector2(scrollRect.velocity.x, clampedSpeed);
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 스크롤 중이던 애니메이션을 즉시 중지
        StopSnapping();
        scrollRect.velocity = Vector2.zero; // 스크롤을 즉시 멈춤
        isUserInteracting = true; // 사용자가 드래그 중임을 표시
    }

    /// <summary>
    /// 드래그 종료 시 호출됨.
    /// </summary>
    /// <param name="eventData">드래그 이벤트 데이터</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        isUserInteracting = false;      // 드래그 중지
        isScrolling = true;             // 스크롤 중임을 표시
    }
    
    #endregion

    #region Initialization

    /// <summary>
    /// ScrollSystem 초기화, 항목을 생성, 초기 스크롤 위치 설정.
    /// </summary>
    /// <param name="value">초기 선택할 값</param>
    private void Initialize(int value)
    {
        // PopulateItems();                                                // 항목 생성
        // SetInitialScrollPosition(value);                                // 초기 스크롤 위치 설정
        
        itemHeight = itemPrefab.GetComponent<RectTransform>().rect.height;  // 항목 높이
        itemCount = (maxValue - minValue + 1) * 3;                          // 항목 3배로 반복하여 무한 스크롤 구현
        visibleItemCount = Mathf.CeilToInt(scrollRect.viewport.rect.height / itemHeight) + BufferItemCount;

        PopulateItems();                                                    // 항목 생성
        scrollRect.onValueChanged.RemoveAllAndAddListener(OnScrollValueChanged);        // 스크롤 이벤트 등록
    
        Canvas.ForceUpdateCanvases();  // 레이아웃 업데이트 후 초기 스크롤 위치 설정
        SetInitialScrollPosition(value);                                    // 초기 스크롤 위치 설정
    }
    
    /// <summary>
    /// 특정 값을 받아 해당 값이 중앙에 위치하도록 스크롤 초기화.
    /// </summary>
    /// <param name="initialValue">초기 설정할 값</param>
    private void SetInitialScrollPosition(int initialValue)
    {
        Canvas.ForceUpdateCanvases();
        
        // initialValue가 범위 내에 있는지 확인하고 유효한 인덱스를 계산
        int targetIndex = Mathf.Clamp((initialValue - minValue), 0, maxValue - minValue) + (itemCount / 3);
        float positionYForInitialValue = targetIndex * itemHeight - (scrollRect.viewport.rect.height / 2) + (itemHeight / 2);

        // 스크롤 콘텐츠의 위치 설정
        scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, positionYForInitialValue);

        UpdateVisibleItems();               // 화면에 보이는 항목을 업데이트
        
        selectedValue = initialValue;
        HighlightSelectedItem(1);       // 중앙에 위치한 항목 강조
    }

    /// <summary>
    /// 스크롤에 표시될 항목 생성.
    /// </summary>
    private void PopulateItems()
    {
        for (int i = 0; i < visibleItemCount; i++)
        {
            GameObject newItem = GetPooledItem(); // 객체 풀에서 항목을 가져오거나 새로 생성
            TMP_Text itemText = newItem.GetComponentInChildren<TMP_Text>();
            itemTexts.Add(itemText);

            RectTransform itemRect = newItem.GetComponent<RectTransform>();
            itemRect.anchoredPosition = new Vector2(0, itemHeight * -(i) - (itemHeight / 2));

            items.Add(newItem);
        }

        // 스크롤 콘텐츠의 높이 설정
        float contentHeight = itemHeight * itemCount;
        scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, contentHeight);

        UpdateVisibleItems(); // 화면에 보이는 항목 업데이트
    }

    #endregion

    #region Scroll and Infinite Scroll Logic

    /// <summary>
    /// 현재 스크롤 위치를 기반으로 화면에 보이는 항목을 업데이트.
    /// </summary>
    private void UpdateVisibleItems()
    {
        // float scrollY = scrollRect.content.anchoredPosition.y;
        //
        // for (int i = 0; i < visibleItemCount; i++)
        // {
        //     int index = Mathf.FloorToInt(scrollY / itemHeight) + i;
        //     if (index < 0 || index >= itemCount) continue;
        //
        //     UpdateItem(i, index); // 해당 인덱스의 항목을 업데이트
        // }
        
        if (items.Count == 0)
        {
            // items 리스트가 아직 초기화되지 않은 경우 업데이트를 중단
            return;
        }

        float scrollY = scrollRect.content.anchoredPosition.y;

        for (int i = 0; i < visibleItemCount; i++)
        {
            int index = Mathf.FloorToInt(scrollY / itemHeight) + i;
            if (index < 0 || index >= itemCount) continue;

            UpdateItem(i, index); // 해당 인덱스의 항목을 업데이트
        }
    }

    /// <summary>
    /// 주어진 인덱스의 항목을 업데이트.
    /// </summary>
    /// <param name="i">항목의 인덱스</param>
    /// <param name="index">업데이트할 스크롤 인덱스</param>
    private void UpdateItem(int i, int index)
    {
        GameObject item = items[i];
        TMP_Text itemText = itemTexts[i];

        int value = minValue + (index % (maxValue - minValue + 1));
        itemText.text = value.ToString();

        if(item.TryGetComponent(out RectTransform itemRect))
        {
            itemRect.anchoredPosition = new Vector2(0, itemHeight * -(index) - (itemHeight / 2));
        }
    }

    /// <summary>
    /// 무한 스크롤 구현. 스크롤이 범위를 벗어나면 위치를 조정함.
    /// </summary>
    private void HandleInfiniteScroll()
    {
        float scrollY = scrollRect.content.anchoredPosition.y;
        float singleRangeHeight = itemHeight * (maxValue - minValue + 1);

        // 스크롤이 범위를 벗어나는 경우 위치를 조정하여 무한 스크롤을 구현
        if (scrollY < singleRangeHeight)
        {
            scrollRect.content.anchoredPosition += new Vector2(0, singleRangeHeight);
            UpdateVisibleItems();
        }
        else if (scrollY > singleRangeHeight * 2)
        {
            scrollRect.content.anchoredPosition -= new Vector2(0, singleRangeHeight);
            UpdateVisibleItems();
        }
    }

    /// <summary>
    /// 스크롤 값이 변경될 때 호출됨.
    /// </summary>
    /// <param name="scrollPosition">스크롤 위치</param>
    private void OnScrollValueChanged(Vector2 scrollPosition)
    {
        if (Mathf.Abs(scrollRect.velocity.y) > 25f)
        {
            isScrolling = true;
        }

        if (!isScrolling) return;

        HighlightSelectedItem(1);
        UpdateVisibleItems();
    }

    #endregion

    #region Snapping Logic

    /// <summary>
    /// 가장 가까운 항목으로 스냅하고 애니메이션 적용.
    /// </summary>
    private async UniTaskVoid SnapToClosestItemAsync()
    {
        if (isSnapping) return;

        onValueChanged?.Invoke(selectedValue); // 값이 변경되었을 때 콜백 호출
        
        isSnapping = true;
        float scrollY = scrollRect.content.anchoredPosition.y + offsetY;
        int closestIndex = Mathf.RoundToInt(scrollY / itemHeight);
        float targetY = closestIndex * itemHeight - offsetY;

        // 스크롤을 부드럽게 이동
        await scrollRect.content.DOAnchorPosY(targetY, 0.2f)
            .SetEase(Ease.OutCubic)
            .OnKill(() => isSnapping = false)
            .AsyncWaitForCompletion();

        isSnapping = false;
        CorrectScrollPosition();
    }

    /// <summary>
    /// 스크롤의 위치를 보정하여 무한 스크롤을 유지.
    /// </summary>
    private void CorrectScrollPosition()
    {
        float scrollY = scrollRect.content.anchoredPosition.y;
        float singleRangeHeight = itemHeight * (maxValue - minValue + 1);

        if (scrollY < singleRangeHeight)
        {
            scrollRect.content.anchoredPosition += new Vector2(0, singleRangeHeight);
        }
        else if (scrollY > singleRangeHeight * 2)
        {
            scrollRect.content.anchoredPosition -= new Vector2(0, singleRangeHeight);
        }

        UpdateVisibleItems();
    }

    #endregion

    #region Item Highlight and Pooling

    /// <summary>
    /// 현재 선택된 항목을 강조 표시.
    /// </summary>
    /// <param name="index">강조할 항목의 인덱스</param>
    private void HighlightSelectedItem(int index)
    {
        if (index < 0 || index >= items.Count) return;

        ResetPreviousItem();

        TMP_Text currentItemText = itemTexts[index];
        if (currentItemText.TryGetComponent(out RectTransform currentItemRect))
        {
            currentItemText.alpha = 1;
            currentItemRect.localScale = Vector3.one;
        }

        previousIndex = index;
        selectedValue = int.Parse(currentItemText.text);
    }

    /// <summary>
    /// 이전에 강조된 항목을 기본 상태로 복원.
    /// </summary>
    private void ResetPreviousItem()
    {
        if (previousIndex >= 0 && previousIndex < items.Count)
        {
            TMP_Text previousItemText = itemTexts[previousIndex];
            if (previousItemText.TryGetComponent(out RectTransform previousItemRect))
            {
                previousItemText.alpha = 0.7f;
                previousItemRect.localScale = Vector3.one * 0.7f;
            }
        }
    }

    /// <summary>
    /// 객체 풀에서 항목을 가져오거나 새로 생성.
    /// </summary>
    /// <returns>객체 풀에서 가져온 항목 또는 새로 생성된 항목</returns>
    private GameObject GetPooledItem()
    {
        if (itemPool.Count > 0)
        {
            var item = itemPool.Dequeue();
            item.SetActive(true);
            return item;
        }

        return Instantiate(itemPrefab, scrollRect.content);
    }

    /// <summary>
    /// 항목을 객체 풀로 반환.
    /// </summary>
    /// <param name="item">객체 풀로 반환할 항목</param>
    private void ReturnPooledItem(GameObject item)
    {
        item.SetActive(false);
        itemPool.Enqueue(item);
    }

    /// <summary>
    /// 기존에 생성된 아이템들을 정리.
    /// </summary>
    public void ClearItems()
    {
        foreach (var item in items)
        {
            ReturnPooledItem(item);
        }
        items.Clear();
        itemTexts.Clear();
    }
    
    #endregion

    #region Snapping Control

    /// <summary>
    /// 스냅 동작을 중지.
    /// </summary>
    private void StopSnapping()
    {
        // 모든 스크롤 애니메이션 중지
        scrollRect.content.DOKill();
        scrollRect.velocity = Vector2.zero; // 스크롤 속도를 0으로 설정하여 즉시 멈추도록 함
        isSnapping = false;
    }

    #endregion
}
