using UnityEngine;

public class DataBaseManager : MonoBehaviour
{
    [SerializeField] public BuildingDatabase BuildingDatabase;
    [SerializeField] public DecorDatabase DecorDatabase;
    [SerializeField] public SeedDatabase SeedDatabase;
    [SerializeField] public CropDatabase CropDatabase;

    [SerializeField] public ItemTable ItemTable;
    
    /// <summary>
    /// 싱글톤 인스턴스
    /// </summary>
    private static DataBaseManager instance;

    /// <summary>
    /// 싱글톤 인스턴스에 접근하기 위한 프로퍼티
    /// </summary>
    public static DataBaseManager Instance => instance;

    /// <summary>
    /// 싱글톤 인스턴스 설정.
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 오브젝트가 파괴되지 않도록 설정

        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 존재하면 중복 오브젝트를 파괴
            return;
        }
    }
}
