using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlantVisualizerForOrchard : PlantVisualizer
{
    [SerializeField] private GameObject[] randomTrees;
    
    public override async UniTaskVoid ResetVisualize()
    {
        foreach (var tf in plantPoints)
        {
            DestroyAllChildren(tf);
        }
        
        await UniTask.Yield(); // 다음 프레임으로 넘겨 CPU 부하 완화

        foreach (var tf in plantPoints)
        {
            Instantiate(randomTrees[Random.Range(0, randomTrees.Length)], tf);
        }
    }
}
