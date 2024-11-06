using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class StorePopup : Popup
{
    [Header("Panel")]
    [SerializeField] private Transform panel;

    [Header("CalculateCanvasLayout")] 
    public HorizontalLayoutGroup HorizontalLayoutGroup;
    public GridLayoutGroup[] GridLayoutGroups;
    public ContentSizeFitter[] ContentSizeFitters;
    
    [Header("Tab")]
    [SerializeField] private List<Toggle> tabToggles;                   // 탭 토글 리스트
    [SerializeField] private List<GameObject> ScrollViews;            // 탭에 대응하는 콘텐츠 패널 리스트
    
    private void OnEnable()
    {
        // 탭 토글과 Scroll View를 연결하는 함수
        InitializeTabs();
        
        ReCalculateCanvasLayoutWithOpeningAnimation();
        

        //StoreManager.Instance.StoreTable.

    }

    #region Tab Management

    /// <summary>
    /// 탭 토글을 초기화하고 이벤트 리스너를 설정
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
    
    /// <summary>
    /// 활성화된 탭에 따라 콘텐츠 패널을 업데이트합니다.
    /// </summary>
    /// <param name="activeIndex">활성화된 탭의 인덱스</param>
    private void UpdateContentPanels(int activeIndex)
    {
        for (int i = 0; i < ScrollViews.Count; i++)
        {
            ScrollViews[i].SetActive(i == activeIndex);
            ReCalculateCanvasLayout();
        }
    }

    #endregion
    
    /// <summary>
    /// 성능 저하를 막기 위해 레이아웃 재계산을 수행하는 컴포넌트들을 잠깐만 켰다 끈다
    /// </summary>
    private async void ReCalculateCanvasLayoutWithOpeningAnimation()
    {
        HorizontalLayoutGroup.enabled = true;

        foreach (var layoutGroup in GridLayoutGroups)
        {
            layoutGroup.enabled = true;
        }

        foreach (var contentSizeFitter in ContentSizeFitters)
        {
            contentSizeFitter.enabled = true;
        }
        
        Canvas.ForceUpdateCanvases();
        
        // Animate
        panel.localScale = Vector3.zero;
        await panel.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).ToUniTask();
        
        HorizontalLayoutGroup.enabled = false;
        
        foreach (var layoutGroup in GridLayoutGroups)
        {
            layoutGroup.enabled = false;
        }

        foreach (var contentSizeFitter in ContentSizeFitters)
        {
            contentSizeFitter.enabled = false;
        }
    }
    
    /// <summary>
    /// 성능 저하를 막기 위해 레이아웃 재계산을 수행하는 컴포넌트들을 잠깐만 켰다 끈다
    /// </summary>
    private void ReCalculateCanvasLayout()
    {
        HorizontalLayoutGroup.enabled = true;

        foreach (var layoutGroup in GridLayoutGroups)
        {
            layoutGroup.enabled = true;
        }

        foreach (var contentSizeFitter in ContentSizeFitters)
        {
            contentSizeFitter.enabled = true;
        }
        
        Canvas.ForceUpdateCanvases();
        
        HorizontalLayoutGroup.enabled = false;
        
        foreach (var layoutGroup in GridLayoutGroups)
        {
            layoutGroup.enabled = false;
        }

        foreach (var contentSizeFitter in ContentSizeFitters)
        {
            contentSizeFitter.enabled = false;
        }
    }
}
