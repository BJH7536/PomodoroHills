using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using PomodoroHills;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public class FarmBuilding : Placeable
{
    /// <summary>
    /// 생산 건물 안에서 정의되는 작물
    /// </summary>
    [Serializable]
    public class Crop
    {
        public int cropId;              // 작물의 ID
        public string cropName;         // 작물 이름
        public int totalGrowthTime;     // 총 성장 시간 (초 단위)
        public int currentGrowthTime;   // 현재까지 성장한 시간 (초 단위)

        public Crop(int id, string name, int growthTime)
        {
            cropId = id;
            cropName = name;
            totalGrowthTime = growthTime;
            currentGrowthTime = 0;
        }

        // 성장 완료 여부 확인
        public bool IsFullyGrown()
        {
            if (currentGrowthTime == 0) return false;
            return currentGrowthTime >= totalGrowthTime;
        }

        // 성장 퍼센티지 계산
        public float GetGrowthPercentage()
        {
            return (float)currentGrowthTime / totalGrowthTime;
        }
    }
    
    [SerializeField] public Crop currentCrop; // 현재 심어져 있는 작물

    [SerializeField]
    private bool _isCropPlanted = false;
    public bool IsCropPlanted
    {
        get => _isCropPlanted;
        set
        {
            _isCropPlanted = value; 
            UpdateUIVisibility();
        }
    } // 작물이 심어졌는지 여부

    [Header("UI")] 
    [SerializeField] private RectTransform Canvas;
    [SerializeField] private Button plantButton;
    [SerializeField] private Button harvestButton;
    [SerializeField] private Ricimi.CircularProgressBar _circularProgressBar;
    [SerializeField] private Image circularProgressImage;

    [Space]
    
    [SerializeField] public PlantVisualizer plantVisualizer;
    
    private void Start()
    {
        plantButton.onClick.AddListener(ShowSelectCropToPlant);
        harvestButton.onClick.AddListener(HarvestCrop);
    }

    private void OnEnable()
    {
        if (PlaceableManager.Instance.IsEdit)
        {
            HideAllButtons();
        }
        else
        {
            UpdateUIVisibility();
        }

        PlaceableManager.Instance.EditModeSwitched += OnInstanceEditModeSwitched;
    }

    private void OnInstanceEditModeSwitched(bool b)
    {
        UpdateUIVisibility();
    }

    private void OnDisable()
    {
        UnsubscribeFromTimer();
        
        PlaceableManager.Instance.EditModeSwitched -= OnInstanceEditModeSwitched;
    }

    public void PlantCrop(int cropId)
    {
        if (!IsCropPlanted)
        {
            CropData cropData = DataBaseManager.Instance.CropDatabase.GetCropById(cropId);
            if (cropData != null)
            {
                currentCrop = new Crop(cropId, cropData.Name, cropData.HarvestSeconds);
                IsCropPlanted = true;
                SubscribeToTimer();
                DebugEx.Log($"{cropData.Name} 작물을 심었습니다.");
                
                // 작물을 심으면 성장 UI를 초기화하고 활성화
                _circularProgressBar.gameObject.SetActive(true);
                _circularProgressBar.UpdateProgress(0);
                Sprite image = DataBaseManager.Instance.ItemTable.GetItemInformById(currentCrop.cropId).image_noBackground;
                circularProgressImage.sprite = image;
                
                // 성장 VFX 업데이트
                plantVisualizer?.Visualize();
            }
            else
            {
                DebugEx.LogError("해당 ID의 작물을 찾을 수 없습니다.");
            }
        }
        else
        {
            DebugEx.LogWarning("이미 작물이 심어져 있습니다.");
        }
    }

    private void UpdateCropGrowth(int remainingTime)
    {
        if (IsCropPlanted && currentCrop != null && !currentCrop.IsFullyGrown())
        {
            currentCrop.currentGrowthTime++;
            float growthPercentage = currentCrop.GetGrowthPercentage();

            DebugEx.Log($"{currentCrop.cropName}이(가) 성장 중입니다: {growthPercentage * 100}%");

            // 성장 UI 업데이트
            _circularProgressBar.gameObject.SetActive(true);
            _circularProgressBar.UpdateProgress(growthPercentage * 100);
            
            // 성장 VFX 업데이트
            plantVisualizer?.Visualize();
            
            if (currentCrop.IsFullyGrown())
            {
                DebugEx.Log("작물이 완전히 성장했습니다!");
                ShowHarvestButton();
                UnsubscribeFromTimer();
                
                // 작물이 완전히 성장하면 성장 UI를 비활성화
                _circularProgressBar.gameObject.SetActive(false);
            }
        }
    }

    public void HarvestCrop()
    {
        if (IsCropPlanted && currentCrop != null && currentCrop.IsFullyGrown())
        {
            DebugEx.Log($"{currentCrop.cropName} 작물을 수확했습니다!");
            PomodoroHills.InventoryManager.Instance.AddItemAsync(new ItemData { id = currentCrop.cropId, amount = 1 }).Forget();

            currentCrop = null;
            IsCropPlanted = false;
            HideHarvestButton();
            ShowPlantButton();
            
            // 작물을 수확하면 성장 UI를 비활성화
            _circularProgressBar.gameObject.SetActive(false);
            
            // 성장 VFX 업데이트
            plantVisualizer?.ResetVisualize();
        }
        else
        {
            DebugEx.LogWarning("수확할 수 있는 작물이 없습니다.");
        }
    }

    public void SubscribeToTimer()
    {
        TimerManager.Instance.OnTimeUpdated += UpdateCropGrowth;
    }

    public void UnsubscribeFromTimer()
    {
        TimerManager.Instance.OnTimeUpdated -= UpdateCropGrowth;
    }

    /// <summary>
    /// 남은 성장 시간의 일정 퍼센티지를 줄여주는 메서드
    /// </summary>
    /// <param name="percentage">0 ~ 100 사이의 비율을 입력.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void ReduceGrowthTimeByPercentage(float percentage)
    {
        if (percentage <= 0 || percentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 100.");
        }

        // 남은 성장 시간 계산
        int remainingGrowthTime = currentCrop.totalGrowthTime - currentCrop.currentGrowthTime;

        // 줄일 시간 계산
        int reduceTime = Mathf.FloorToInt(remainingGrowthTime * (percentage / 100));

        // 현재 성장 시간 증가
        currentCrop.currentGrowthTime += reduceTime;

        // 성장 시간이 총 성장 시간을 초과하지 않도록 조정
        if (currentCrop.currentGrowthTime > currentCrop.totalGrowthTime)
            currentCrop.currentGrowthTime = currentCrop.totalGrowthTime;
        
        float growthPercentage = currentCrop.GetGrowthPercentage();

        _circularProgressBar.UpdateProgress(growthPercentage * 100);
        
        if (growthPercentage >= 1f)
        {
            _circularProgressBar.gameObject.SetActive(false);
            ShowHarvestButton();
        }
        else
        {
            _circularProgressBar.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// 버튼 및 UI의 표시 상태를 갱신
    /// </summary>
    private void UpdateUIVisibility()
    {
        Canvas.rotation = Camera.main.transform.rotation;
        
        if (PlaceableManager.Instance.IsEdit)
        {
            Canvas.gameObject.SetActive(false);
        }
        else
        {
            Canvas.gameObject.SetActive(true);
            
            if (!IsCropPlanted)
            {
                // 작물이 심어져 있지 않으면 Plant 버튼 표시
                ShowPlantButton();
                HideHarvestButton();
            
                // 성장 UI를 비활성화
                _circularProgressBar.gameObject.SetActive(false);
            }
            else if (currentCrop != null && currentCrop.IsFullyGrown())
            {
                // 작물이 완전히 성장했으면 Harvest 버튼 표시
                HidePlantButton();
                ShowHarvestButton();
            
                // 성장 UI를 비활성화
                _circularProgressBar.gameObject.SetActive(false);
            }
            else
            {
                // 작물이 성장 중이면 모든 버튼 숨김
                HideAllButtons();
            
                // 성장 UI를 활성화
                _circularProgressBar.gameObject.SetActive(true);
                Sprite image = DataBaseManager.Instance.ItemTable.GetItemInformById(currentCrop.cropId).image_noBackground;
                circularProgressImage.sprite = image;
                _circularProgressBar.UpdateProgress(currentCrop.GetGrowthPercentage() * 100.0f);
            }
        }
    }
    
    /// <summary>
    /// 저장된 타이머 상태와 시간을 로드하고 작물 성장 업데이트
    /// </summary>
    public void LoadTimerStateAndUpdateGrowth()
    {
        DateTime lastCheckTime = DateTime.Now;
        TimerState lastTimerState = TimerState.Stopped;
        int savedRemainingFocusTime = 0;
        
        if (PlayerPrefs.HasKey("LastCheckTime"))
        {
            string lastCheckTimeStr = PlayerPrefs.GetString("LastCheckTime");
            if (DateTime.TryParse(lastCheckTimeStr, out DateTime savedTime))
            {
                lastCheckTime = savedTime;
            }
        }

        if (PlayerPrefs.HasKey("LastTimerState"))
        {
            lastTimerState = (TimerState)PlayerPrefs.GetInt("LastTimerState");
        }
        
        if (PlayerPrefs.HasKey("RemainingFocusTimeInSeconds"))
        {
            savedRemainingFocusTime = PlayerPrefs.GetInt("RemainingFocusTimeInSeconds");
        }

        // 게임이 꺼져 있던 시간 계산
        TimeSpan elapsedTime = DateTime.Now - lastCheckTime;

        // 타이머가 집중 세션이었는지 확인
        if (lastTimerState == TimerState.FocusSessionRunning)
        {
            int elapsedFocusTimeInSeconds = (int)elapsedTime.TotalSeconds;

            // 추가할 성장 시간을 집중 세션의 남은 시간으로 제한
            int growthTimeToAdd = Mathf.Min(elapsedFocusTimeInSeconds, savedRemainingFocusTime);
            
            // 작물의 성장 시간에 추가
            if (currentCrop != null && !currentCrop.IsFullyGrown())
            {
                currentCrop.currentGrowthTime += growthTimeToAdd;

                // 성장 UI 업데이트
                UpdateCropGrowthUI();

                // 작물이 성장 완료되었는지 확인
                if (currentCrop.IsFullyGrown())
                {
                    ShowHarvestButton();
                    _circularProgressBar.gameObject.SetActive(false);
                }
            }
        }
    }
    
    /// <summary>
    /// 성장 UI를 업데이트
    /// </summary>
    private void UpdateCropGrowthUI()
    {
        float growthPercentage = currentCrop.GetGrowthPercentage();

        _circularProgressBar.UpdateProgress(growthPercentage * 100);

        if (growthPercentage >= 1f)
        {
            _circularProgressBar.gameObject.SetActive(false);
        }
        else
        {
            _circularProgressBar.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// 작물을 심는 버튼을 보이게 하기
    /// </summary>
    public void ShowPlantButton()
    {
        plantButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// 작물을 심는 버튼을 숨기기
    /// </summary>
    public void HidePlantButton()
    {
        plantButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// 작물을 수확하는 버튼을 보이게 하기
    /// </summary>
    public void ShowHarvestButton()
    {
        harvestButton.gameObject.SetActive(true);
        Sprite image = DataBaseManager.Instance.ItemTable.GetItemInformById(currentCrop.cropId).image_noBackground;
        
        Transform buttonChild = harvestButton.transform.GetChild(0);
        Image imageComponent = buttonChild.GetComponent<Image>();
        imageComponent.sprite = image;
    }

    /// <summary>
    /// 작물을 수확하는 버튼을 숨기기
    /// </summary>
    public void HideHarvestButton()
    {
        harvestButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// 모든 버튼을 숨깁니다.
    /// </summary>
    public void HideAllButtons()
    {
        HidePlantButton();
        HideHarvestButton();
    }

    private void OnDestroy()
    {
        UnsubscribeFromTimer();
    }
    
    public void ShowSelectCropToPlant()
    {
        PopupManager.Instance.ShowPopup<SelectCropToPlantPopup>().SetUp(this);
    }
}

[Serializable]
public class FarmBuildingData : PlaceableData
{
    public bool isCropPlanted;
    public FarmBuilding.Crop currentCrop;
}
