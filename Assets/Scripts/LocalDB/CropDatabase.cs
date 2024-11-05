using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 로컬 Database에 저장되는 작물 데이터.
/// 게임에 등장할 수 있는 작물의 정적인 데이터를 저장한다.
/// </summary>
[System.Serializable]
public class CropData
{
    [SerializeField] private int id;                        // 식별자
    public int Id => id;
    [SerializeField] private string name;                    // 작물의 이름
    public string Name => name;
    
    [SerializeField] private int harvestTime;                // 수확 시간 (단위: 분)
    public int HarvestTime => harvestTime;

    [SerializeField] private List<string> tags;              // 작물 태그 리스트
    public List<string> Tags => tags;
}

/// <summary>
/// 작물에 대한 정보를 저장하는 로컬 Database.
/// 게임에 등장할 수 있는 작물들의 정적인 데이터를 저장한다.
/// </summary>
[CreateAssetMenu(fileName = "NewCropDatabase", menuName = "PomodoroHills/CropDatabase")]
public class CropDatabase : ScriptableObject
{
    /// <summary>
    /// Database에서 저장하는 작물들에 대한 정보.
    /// Editor에서 Inspector View를 통해 새로운 데이터를 추가하면 이 List에 항목이 추가된다.
    /// </summary>
    [SerializeField] private List<CropData> _crops = new List<CropData>();

    /// <summary>
    /// 외부에서 작물을 빠르게 찾을 수 있도록 Dictionary를 활용한다.
    /// 런타임에 List에 있는 작물들이 이 Dictionary로 옮겨온다.
    /// </summary>
    private Dictionary<int, CropData> _cropDictionary = new Dictionary<int, CropData>();

    private void OnEnable()
    {
        // ScriptableObject가 로드될 때 Dictionary 초기화
        InitializeDictionary();
    }

    /// <summary>
    /// List에 담겨있는 작물 데이터 정보들을 Dictionary로 옮겨오는 함수
    /// </summary>
    private void InitializeDictionary()
    {
        _cropDictionary = new Dictionary<int, CropData>();
        foreach (var crop in _crops)
        {
            if (!_cropDictionary.TryAdd(crop.Id, crop))
            {
                DebugEx.LogWarning($"중복된 작물 식별자: {crop.Id} / 작물의 식별자는 고유해야 합니다.");
            }
        }
    }

    /// <summary>
    /// 작물 이름으로 데이터를 검색하는 함수
    /// </summary>
    /// <param name="cropName"> 찾고자 하는 작물 데이터 </param>
    /// <returns></returns>
    public CropData GetCropById(int id)
    {
        if (_cropDictionary == null)
        {
            InitializeDictionary();  // 만약 Dictionary가 null이라면 다시 초기화
        }

        if (_cropDictionary != null && _cropDictionary.TryGetValue(id, out var cropData))
        {
            return cropData;
        }
        else
        {
            DebugEx.LogWarning($"식별자가 {id}인 작물을 찾을 수 없습니다.");
            return null;
        }
    }

    /// <summary>
    /// 작물들을 태그로 검색하는 함수
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public List<CropData> GetCropsByTag(string tag)
    {
        List<CropData> result = new List<CropData>();

        foreach (var crop in _crops)
        {
            if (crop.Tags.Contains(tag))
            {
                result.Add(crop);
            }
        }

        return result;
    }

    /// <summary>
    /// 여러 태그에 모두 일치하는 작물을 찾는 함수 (AND 조건)
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    public List<CropData> GetCropsByTags(List<string> tags)
    {
        List<CropData> result = new List<CropData>();

        foreach (var crop in _crops)
        {
            if (tags.All(tag => crop.Tags.Contains(tag)))
            {
                result.Add(crop);
            }
        }

        return result;
    }

    /// <summary>
    /// 전체 작물 데이터를 콘솔에 출력하는 함수
    /// </summary>
    public void PrintAllCrops()
    {
        foreach (var crop in _crops)
        {
            string tags = string.Join(", ", crop.Tags);
            DebugEx.Log($"작물 이름: {crop.Name}, 수확 시간: {crop.HarvestTime}일, 태그: [{tags}]");
        }
    }
}
