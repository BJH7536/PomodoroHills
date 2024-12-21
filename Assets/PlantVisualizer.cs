using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VInspector;

public class PlantVisualizer : MonoBehaviour
{
    [SerializeField] protected Transform[] plantPoints;

    protected FarmBuilding farmBuilding;
    
    // ���������� �ð�ȭ�� ���� �ܰ�
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

        // ���� ���� �ܰ� ���
        int currentStage = GetCurrentStage(
            farmBuilding.currentCrop.currentGrowthTime,
            farmBuilding.currentCrop.totalGrowthTime,
            cropData.GrowthSteps.Length - 1);

        // ������ �ð�ȭ�� ���� �ܰ�� �����ϸ� ����
        if (currentStage == lastVisualizedStage)
        {
            return;
        }
        
        // ���� �ܰ谡 ����� ��� ������Ʈ
        lastVisualizedStage = currentStage;

        // ���� �ܰ迡 ���� ������ ��������
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
        
        await UniTask.Yield(); // ���� ���������� �Ѱ� CPU ���� ��ȭ

        foreach (var tf in plantPoints)
        {
            Instantiate(vfx, tf);
        }
    }
    
    /// <summary>
    /// ���� ���� �ܰ� ���
    /// </summary>
    protected int GetCurrentStage(int currentGrowthTime, int totalGrowthTime, int growthStages)
    {
        if (totalGrowthTime <= 0 || currentGrowthTime < 0)
        {
            DebugEx.LogError("Invalid growth times.");
            return 0;
        }

        int stageDuration = totalGrowthTime / growthStages; // �� ���� �ܰ��� �ð�
        return Mathf.Min(currentGrowthTime / stageDuration, growthStages);
    }
    
    
    protected void DestroyAllChildren(Transform parent)
    {
        // �ڽ� ������Ʈ�� �ӽ� ����Ʈ�� ����
        var children = new List<GameObject>();

        foreach (Transform child in parent)
        {
            children.Add(child.gameObject);
        }

        // ����Ʈ�� ��ȸ�ϸ� ����
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
