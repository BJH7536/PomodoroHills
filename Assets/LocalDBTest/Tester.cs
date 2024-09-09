using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField] private CropDatabase _buildingDatabase;

    void Start()
    {
        _buildingDatabase.PrintAllCrops();
    }
    
}
