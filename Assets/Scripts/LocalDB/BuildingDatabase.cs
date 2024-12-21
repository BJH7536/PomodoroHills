using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 로컬 Database에 저장되는 건물 데이터.
/// 게임에 등장할 수 있는 건물의 정적인 데이터를 저장한다.
/// </summary>
[System.Serializable]
public class BuildingData
{
    [SerializeField] private int id;                        // 식별자
    public int Id => id;
    [SerializeField] private string name;                   // 건물의 이름
    public string Name => name;
    [SerializeField] private GameObject buildingPrefab;     // 건물의 프리팹
    public GameObject Prefab => buildingPrefab;
    [SerializeField] private int sizeX;                     // 건물의 X크기
    [SerializeField] private int sizeZ;                     // 건물의 Z크기
    public int SizeX => sizeX;
    public int SizeZ => sizeZ;
    
    [SerializeField] private List<string> tags;             // 건물 태그 리스트 (여러 태그를 가질 수 있음)
    public List<string> Tags => tags;

    [SerializeField] private List<int> plantable;             // 재배 가능한 작물들의 리스트 (id를 저장)
    public List<int> Plantable => plantable;
}

/// <summary>
/// 건물에 대한 정보를 저장하는 로컬 Database.
/// 게임에 등장할 수 있는 건물들의 정적인 데이터를 저장한다.
/// </summary>
[CreateAssetMenu(fileName = "NewBuildingDatabase", menuName = "PomodoroHills/BuildingDatabase")]
public class BuildingDatabase : ScriptableObject
{
    /// <summary>
    /// Database에서 저장하는 건물들에 대한 정보.
    /// Editor에서 Inspector View를 통해 새로운 데이터를 추가하면 이 List에 항목이 추가된다.
    /// </summary>
    [SerializeField] private List<BuildingData> _buildings = new List<BuildingData>();
    
    /// <summary>
    /// 외부에서 건물을 빠르게 찾을 수 있도록 Dictionary를 활용한다.
    /// 런타임에 List에 있는 건물들이 이 Dictionary로 옮겨온다.
    /// </summary>
    private Dictionary<int, BuildingData> _buildingDictionary = new Dictionary<int, BuildingData>();

    private void OnEnable()
    {
        // ScriptableObject가 로드될 때 Dictionary 초기화
        InitializeDictionary();
    }

    /// <summary>
    /// List에 담겨있는 건물 데이터 정보들을 Dictionary로 옮겨오는 함수
    /// </summary>
    private void InitializeDictionary()
    {
        _buildingDictionary = new Dictionary<int, BuildingData>();
        foreach (var building in _buildings)
        {
            if (!_buildingDictionary.TryAdd(building.Id, building))
            {
                DebugEx.LogWarning($"중복된 건물의 식별자: {building.Id} / 건물의 식별자는 고유해야 합니다.");
            }
        }
    }

    /// <summary>
    /// 건물 이름으로 데이터를 검색하는 함수
    /// </summary>
    /// <param name="id"> 찾고자 하는 건물 데이터 </param>
    /// <returns></returns>
    public BuildingData GetBuildingById(int id)
    {
        if (_buildingDictionary == null)
        {
            InitializeDictionary();  // 만약 Dictionary가 null이라면 다시 초기화
        }

        if (_buildingDictionary != null && _buildingDictionary.TryGetValue(id, out var buildingData))
        {
            return buildingData;
        }
        else
        {
            DebugEx.LogWarning($"식별자가 {id}인 건물을 찾을 수 없습니다.");
            return null;
        }
    }
    
    /// <summary>
    /// 건물 이름으로 데이터를 검색하는 함수
    /// </summary>
    /// <param name="id"> 찾고자 하는 건물 데이터 </param>
    /// <returns></returns>
    public List<int> GetGrowablesById(int id)
    {
        BuildingData data = GetBuildingById(id);

        if (data != null)
            return data.Plantable;
        else
            return null;
    }
    
    /// <summary>
    /// 건물의 Id를 통해 해당 건물의 태그 리스트를 반환하는 함수
    /// </summary>
    /// <param name="id">찾고자 하는 건물의 Id</param>
    /// <returns>건물의 태그 리스트 (해당 건물이 없을 경우 null 반환)</returns>
    public List<string> GetTagsByBuildingId(int id)
    {
        if (_buildingDictionary == null)
        {
            InitializeDictionary();  // Dictionary가 null일 경우 초기화
        }

        if (_buildingDictionary != null && _buildingDictionary.TryGetValue(id, out var buildingData))
        {
            return buildingData.Tags;
        }
        else
        {
            DebugEx.LogWarning($"식별자가 {id}인 건물을 찾을 수 없습니다.");
            return null;
        }
    }
    
    /// <summary>
    /// 건물들을 태그로 검색하는 함수
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public List<BuildingData> GetBuildingsByTag(string tag)
    {
        List<BuildingData> result = new List<BuildingData>();

        foreach (var building in _buildings)
        {
            if (building.Tags.Contains(tag))
            {
                result.Add(building);
            }
        }

        return result;
    }

    /// <summary>
    /// 여러 태그에 모두 일치하는 건물을 찾는 함수 (AND 조건)
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    public List<BuildingData> GetBuildingsByTags(List<string> tags)
    {
        List<BuildingData> result = new List<BuildingData>();

        foreach (var building in _buildings)
        {
            if (tags.All(tag => building.Tags.Contains(tag)))
            {
                result.Add(building);
            }
        }

        return result;
    }
    
    /// <summary>
    /// 전체 건물 데이터를 콘솔에 출력하는 함수
    /// </summary>
    public void PrintAllBuildings()
    {
        foreach (var building in _buildings)
        {
            string tags = string.Join(", ", building.Tags);
            DebugEx.Log($"건물 이름: {building.Name}, 크기: {building.SizeX}x{building.SizeZ}, 태그: [{tags}]");
        }
    }
}
