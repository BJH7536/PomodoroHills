using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

public class PlantVisualizer : MonoBehaviour
{
    [SerializeField] protected Transform[] plantPoints;

    protected FarmBuilding farmBuilding;
    
    // 마지막으로 시각화된 성장 단계
    protected int lastVisualizedStage = -1;
    
    private void Awake()
    {
        farmBuilding = GetComponent<FarmBuilding>();
    }

    [Button]
    public virtual void Visualize()
    {
        if (!farmBuilding.IsCropPlanted) return;
        
        var cropData = DataBaseManager.Instance.CropDatabase.GetCropById(farmBuilding.currentCrop.cropId);

        if (cropData == null || cropData.GrowthSteps == null)
        {
            DebugEx.LogError("Invalid crop data or growth steps.");
            return;
        }

        // 현재 성장 단계 계산
        int currentStage = GetCurrentStage(
            farmBuilding.currentCrop.currentGrowthTime,
            farmBuilding.currentCrop.totalGrowthTime,
            cropData.GrowthSteps.Length - 1);

        // 마지막 시각화된 성장 단계와 동일하면 종료
        if (currentStage == lastVisualizedStage)
        {
            return;
        }
        
        // 성장 단계가 변경된 경우 업데이트
        lastVisualizedStage = currentStage;

        // 성장 단계에 따른 프리팹 가져오기
        GameObject vfx = cropData.GrowthSteps[currentStage];
        
        UpdatePlantVisualsAsync(vfx).Forget();
    }

    [Button]
    public virtual UniTaskVoid ResetVisualize()
    {
        foreach (var tf in plantPoints)
        {
            DestroyAllChildren(tf);
        }

        return default;
    }

    protected async UniTask UpdatePlantVisualsAsync(GameObject vfx)
    {
        foreach (var tf in plantPoints)
        {
            DestroyAllChildren(tf);
        }
        
        await UniTask.Yield(); // 다음 프레임으로 넘겨 CPU 부하 완화

        foreach (var tf in plantPoints)
        {
            Instantiate(vfx, tf);
        }
    }
    
    /// <summary>
    /// 현재 성장 단계 계산
    /// </summary>
    protected int GetCurrentStage(int currentGrowthTime, int totalGrowthTime, int growthStages)
    {
        if (totalGrowthTime <= 0 || currentGrowthTime < 0)
        {
            DebugEx.LogError("Invalid growth times.");
            return 0;
        }

        int stageDuration = totalGrowthTime / growthStages; // 각 성장 단계의 시간
        return Mathf.Min(currentGrowthTime / stageDuration, growthStages);
    }
    
    
    protected void DestroyAllChildren(Transform parent)
    {
        // 자식 오브젝트를 임시 리스트로 저장
        var children = new List<GameObject>();

        foreach (Transform child in parent)
        {
            children.Add(child.gameObject);
        }

        // 리스트를 순회하며 삭제
        foreach (GameObject child in children)
        {
#if UNITY_EDITOR && !UNITY_EDITORPLAYMODE
            DestroyImmediate(child);
#else
            Destroy(child);
#endif
        }
    }
}
