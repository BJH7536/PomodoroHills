using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

/// <summary>
/// 인벤토리 팝업을 관리하는 클래스입니다.
/// 팝업의 열림과 닫힘 애니메이션을 처리하고, 탭 전환을 관리합니다.
/// </summary>
public class InventoryPopup : Popup
{
    #region Serialized Fields

    [Tab("Tweening")] 
    [SerializeField] private RectTransform panel; // 애니메이션을 적용할 패널
    [SerializeField] private RectTransform panelOpeningStartPosition; // 애니메이션 시작 위치
    [SerializeField] private RectTransform panelOpeningEndPosition; // 애니메이션 종료 위치
    [SerializeField] private Ease openingEase = Ease.OutQuad; // 애니메이션 이징 함수
    [SerializeField] private float duration = 0.5f; // 애니메이션 지속 시간
    
    [Tab("Inventory")]
    [SerializeField] private List<Toggle> tabToggles; // 탭 토글 리스트
    [SerializeField] private List<GameObject> contentPanels; // 탭에 대응하는 콘텐츠 패널 리스트
    [SerializeField] private HorizontalLayoutGroup layoutGroup; // 레이아웃 그룹

    #endregion

    #region Private Fields

    private Tweener openTweener; // 열림 애니메이션 트위너
    private Tweener closeTweener; // 닫힘 애니메이션 트위너

    #endregion

    #region Unity Lifecycle Methods

    /// <summary>
    /// 오브젝트가 활성화될 때 호출됩니다.
    /// 탭을 초기화하고 레이아웃 그룹을 비활성화합니다.
    /// </summary>
    private void OnEnable()
    {
        InitializeTabs();
        DisableLayoutGroupAfterFrameAsync().Forget();
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
    /// 인벤토리 팝업을 설정합니다.
    /// 추가적인 초기화 작업이 필요한 경우 여기서 수행합니다.
    /// </summary>
    public void SetupInventory()
    {
        // TODO 인벤토리 관련 초기화 작업
    }

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
}
