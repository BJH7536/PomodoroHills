using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 로컬 Database에 저장되는 씨앗 데이터.
/// 게임에 등장할 수 있는 씨앗의 정적인 데이터를 저장한다.
/// </summary>
[System.Serializable]
public class SeedData
{
    [SerializeField] private string name;                   // 씨앗의 이름
    public string Name => name;

    [SerializeField] private int growthTime;                // 성장 시간 (단위: 초)
    public int GrowthTime => growthTime;

    [SerializeField] private List<string> tags;             // 씨앗 태그 리스트
    public List<string> Tags => tags;
}

/// <summary>
/// 씨앗에 대한 정보를 저장하는 로컬 Database.
/// 게임에 등장할 수 있는 씨앗들의 정적인 데이터를 저장한다.
/// </summary>
[CreateAssetMenu(fileName = "NewSeedDatabase", menuName = "PomodoroHills/SeedDatabase")]
public class SeedDatabase : ScriptableObject
{
    /// <summary>
    /// Database에서 저장하는 씨앗들에 대한 정보.
    /// Editor에서 Inspector View를 통해 새로운 데이터를 추가하면 이 List에 항목이 추가된다.
    /// </summary>
    [SerializeField] private List<SeedData> _seeds = new List<SeedData>();

    /// <summary>
    /// 외부에서 씨앗을 빠르게 찾을 수 있도록 Dictionary를 활용한다.
    /// 런타임에 List에 있는 씨앗들이 이 Dictionary로 옮겨온다.
    /// </summary>
    private Dictionary<string, SeedData> _seedDictionary = new Dictionary<string, SeedData>();

    private void OnEnable()
    {
        // ScriptableObject가 로드될 때 Dictionary 초기화
        InitializeDictionary();
    }

    /// <summary>
    /// List에 담겨있는 씨앗 데이터 정보들을 Dictionary로 옮겨오는 함수
    /// </summary>
    private void InitializeDictionary()
    {
        _seedDictionary = new Dictionary<string, SeedData>();
        foreach (var seed in _seeds)
        {
            if (!_seedDictionary.TryAdd(seed.Name, seed))
            {
                Debug.LogWarning($"중복된 씨앗 이름: {seed.Name} / 씨앗 이름은 고유해야 합니다.");
            }
        }
    }

    /// <summary>
    /// 씨앗 이름으로 데이터를 검색하는 함수
    /// </summary>
    /// <param name="seedName"> 찾고자 하는 씨앗 데이터 </param>
    /// <returns></returns>
    public SeedData GetSeedByName(string seedName)
    {
        if (_seedDictionary == null)
        {
            InitializeDictionary();  // 만약 Dictionary가 null이라면 다시 초기화
        }

        if (_seedDictionary != null && _seedDictionary.TryGetValue(seedName, out var seedData))
        {
            return seedData;
        }
        else
        {
            Debug.LogWarning($"이름이 {seedName}인 씨앗을 찾을 수 없습니다.");
            return null;
        }
    }

    /// <summary>
    /// 씨앗들을 태그로 검색하는 함수
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public List<SeedData> GetSeedsByTag(string tag)
    {
        List<SeedData> result = new List<SeedData>();

        foreach (var seed in _seeds)
        {
            if (seed.Tags.Contains(tag))
            {
                result.Add(seed);
            }
        }

        return result;
    }

    /// <summary>
    /// 여러 태그에 모두 일치하는 씨앗을 찾는 함수 (AND 조건)
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    public List<SeedData> GetSeedsByTags(List<string> tags)
    {
        List<SeedData> result = new List<SeedData>();

        foreach (var seed in _seeds)
        {
            if (tags.All(tag => seed.Tags.Contains(tag)))
            {
                result.Add(seed);
            }
        }

        return result;
    }

    /// <summary>
    /// 전체 씨앗 데이터를 콘솔에 출력하는 함수
    /// </summary>
    public void PrintAllSeeds()
    {
        foreach (var seed in _seeds)
        {
            string tags = string.Join(", ", seed.Tags);
            Debug.Log($"씨앗 이름: {seed.Name}, 성장 시간: {seed.GrowthTime}일, 태그: [{tags}]");
        }
    }
}
