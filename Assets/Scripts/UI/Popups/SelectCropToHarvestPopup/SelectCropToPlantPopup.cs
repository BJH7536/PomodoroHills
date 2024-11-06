using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SelectCropToPlantPopup : Popup
{
    [SerializeField] private Transform panel;

    [SerializeField] private ScrollRect ScrollRect;
    [SerializeField] private HorizontalLayoutGroup HorizontalLayoutGroup;
    [SerializeField] private ContentSizeFitter ContentSizeFitter;

    [SerializeField] private GameObject prefab;
    
    private FarmBuilding targetBuilding;

    private void OnEnable()
    {
        ReCalculateCanvasLayoutWithOpeningAnimation();
    }

    public void SetUp(FarmBuilding target)
    {
        targetBuilding = target;
        
        // ScrollRect 내의 item들 전부 싹 밀고 새로운 애들로 초기화
        for (int i = ScrollRect.content.childCount - 1; i >= 0; i--)
            Destroy(ScrollRect.content.GetChild(i).gameObject);
        
        List<int> growables = DataBaseManager.Instance.BuildingDatabase.GetGrowablesById(targetBuilding.id);
        
        foreach (var id in growables)
        {
            GameObject item = Instantiate(prefab, ScrollRect.content);
            if (item.TryGetComponent(out SelectCropToHarvestPopup_Crop crop))
            {
                crop.SetUp(id, () =>
                {
                    target.PlantCrop(id);
                });
            }
        }
    }
    
    /// <summary>
    /// 성능 저하를 막기 위해 레이아웃 재계산을 수행하는 컴포넌트들을 잠깐만 켰다 끈다
    /// </summary>
    private async void ReCalculateCanvasLayoutWithOpeningAnimation()
    {
        HorizontalLayoutGroup.enabled = true;
        ContentSizeFitter.enabled = true;
        
        Canvas.ForceUpdateCanvases();
        
        // Animate
        panel.localScale = Vector3.zero;
        await panel.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).ToUniTask();
        
        HorizontalLayoutGroup.enabled = false;
        ContentSizeFitter.enabled = false;
    }
}
