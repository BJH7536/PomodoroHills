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
    [SerializeField] private List<Toggle> tabToggles;                   // �� ��� ����Ʈ
    [SerializeField] private List<GameObject> ScrollViews;            // �ǿ� �����ϴ� ������ �г� ����Ʈ
    
    private void OnEnable()
    {
        // �� ��۰� Scroll View�� �����ϴ� �Լ�
        InitializeTabs();
        
        ReCalculateCanvasLayoutWithOpeningAnimation();
        

        //StoreManager.Instance.StoreTable.

    }

    #region Tab Management

    /// <summary>
    /// �� ����� �ʱ�ȭ�ϰ� �̺�Ʈ �����ʸ� ����
    /// </summary>
    private void InitializeTabs()
    {
        // �� Toggle�� ���� OnValueChanged �̺�Ʈ�� ���
        for (int i = 0; i < tabToggles.Count; i++)
        {
            int index = i; // Ŭ���� ������ �����ϱ� ���� ���� ������ �ε��� ����
            tabToggles[i].onValueChanged.AddListener(isOn => OnTabChanged(isOn, index));
        }

        // ���� �� ù ��° �� Ȱ��ȭ
        ActivateTab(0);
    }

    /// <summary>
    /// ������ �ε����� ���� Ȱ��ȭ�ϰ�, �ش��ϴ� ������ �г��� ǥ���մϴ�.
    /// </summary>
    /// <param name="index">Ȱ��ȭ�� ���� �ε���</param>
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
    /// ���� ���°� ����� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="isOn">��� ����</param>
    /// <param name="index">����� �ε���</param>
    private void OnTabChanged(bool isOn, int index)
    {
        if (isOn)
        {
            UpdateContentPanels(index);
        }
    }
    
    /// <summary>
    /// Ȱ��ȭ�� �ǿ� ���� ������ �г��� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="activeIndex">Ȱ��ȭ�� ���� �ε���</param>
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
    /// ���� ���ϸ� ���� ���� ���̾ƿ� ������ �����ϴ� ������Ʈ���� ��� �״� ����
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
    /// ���� ���ϸ� ���� ���� ���̾ƿ� ������ �����ϴ� ������Ʈ���� ��� �״� ����
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
