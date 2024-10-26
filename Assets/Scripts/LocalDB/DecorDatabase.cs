using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 로컬 Database에 저장되는 장식품 데이터.
/// 게임에 등장할 수 있는 장식품의 정적인 데이터를 저장한다.
/// </summary>
[System.Serializable]
public class DecorData
{
    [SerializeField] private string name;                   // 장식품의 이름
    public string Name => name;

    [SerializeField] private GameObject decorPrefab;        // 장식품의 프리팹
    [SerializeField] private int sizeX;                     // 장식품의 X 크기
    [SerializeField] private int sizeZ;                     // 장식품의 Z 크기
    public int SizeX => sizeX;
    public int SizeZ => sizeZ;

    [SerializeField] private List<string> tags;             // 장식품 태그 리스트
    public List<string> Tags => tags;
}

/// <summary>
/// 장식품에 대한 정보를 저장하는 로컬 Database.
/// 게임에 등장할 수 있는 장식품들의 정적인 데이터를 저장한다.
/// </summary>
[CreateAssetMenu(fileName = "NewDecorDatabase", menuName = "PomodoroHills/DecorDatabase")]
public class DecorDatabase : ScriptableObject
{
    /// <summary>
    /// Database에서 저장하는 장식품들에 대한 정보.
    /// Editor에서 Inspector View를 통해 새로운 데이터를 추가하면 이 List에 항목이 추가된다.
    /// </summary>
    [SerializeField] private List<DecorData> _decors = new List<DecorData>();

    /// <summary>
    /// 외부에서 장식품을 빠르게 찾을 수 있도록 Dictionary를 활용한다.
    /// 런타임에 List에 있는 장식품들이 이 Dictionary로 옮겨온다.
    /// </summary>
    private Dictionary<string, DecorData> _decorDictionary = new Dictionary<string, DecorData>();

    private void OnEnable()
    {
        // ScriptableObject가 로드될 때 Dictionary 초기화
        InitializeDictionary();
    }

    /// <summary>
    /// List에 담겨있는 장식품 데이터 정보들을 Dictionary로 옮겨오는 함수
    /// </summary>
    private void InitializeDictionary()
    {
        _decorDictionary = new Dictionary<string, DecorData>();
        foreach (var decor in _decors)
        {
            if (!_decorDictionary.TryAdd(decor.Name, decor))
            {
                DebugEx.LogWarning($"중복된 장식품 이름: {decor.Name} / 장식품 이름은 고유해야 합니다.");
            }
        }
    }

    /// <summary>
    /// 장식품 이름으로 데이터를 검색하는 함수
    /// </summary>
    /// <param name="decorName"> 찾고자 하는 장식품 데이터 </param>
    /// <returns></returns>
    public DecorData GetDecorByName(string decorName)
    {
        if (_decorDictionary == null)
        {
            InitializeDictionary();  // 만약 Dictionary가 null이라면 다시 초기화
        }

        if (_decorDictionary != null && _decorDictionary.TryGetValue(decorName, out var decorData))
        {
            return decorData;
        }
        else
        {
            DebugEx.LogWarning($"이름이 {decorName}인 장식품을 찾을 수 없습니다.");
            return null;
        }
    }

    /// <summary>
    /// 장식품들을 태그로 검색하는 함수
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public List<DecorData> GetDecorsByTag(string tag)
    {
        List<DecorData> result = new List<DecorData>();

        foreach (var decor in _decors)
        {
            if (decor.Tags.Contains(tag))
            {
                result.Add(decor);
            }
        }

        return result;
    }

    /// <summary>
    /// 여러 태그에 모두 일치하는 장식품을 찾는 함수 (AND 조건)
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    public List<DecorData> GetDecorsByTags(List<string> tags)
    {
        List<DecorData> result = new List<DecorData>();

        foreach (var decor in _decors)
        {
            if (tags.All(tag => decor.Tags.Contains(tag)))
            {
                result.Add(decor);
            }
        }

        return result;
    }

    /// <summary>
    /// 전체 장식품 데이터를 콘솔에 출력하는 함수
    /// </summary>
    public void PrintAllDecors()
    {
        foreach (var decor in _decors)
        {
            string tags = string.Join(", ", decor.Tags);
            DebugEx.Log($"장식품 이름: {decor.Name}, 크기: {decor.SizeX}x{decor.SizeZ}, 태그: [{tags}]");
        }
    }
}
