using System;
using System.IO;
using System.Threading;
using AssetKits.ParticleImage;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 사용자의 재화 정보를 로컬 저장소에 저장하고 불러오는 기능을 제공하는 클래스.
/// 싱글톤 패턴을 사용하여 전역에서 접근 가능.
/// </summary>
public class EconomyManager : MonoBehaviour
{
    /// <summary>
    /// EconomyManager의 단일 인스턴스를 유지하기 위한 변수.
    /// </summary>
    private static EconomyManager instance;

    /// <summary>
    /// EconomyManager의 싱글톤 인스턴스에 접근하기 위한 프로퍼티.
    /// </summary>
    public static EconomyManager Instance => instance;

    private const string CurrencyFileName = "currencyData.json";
    private string _dataPath_Currency;

    #region SerializeFields
    
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI gemText;

    [SerializeField] private ParticleImage attractorBurstCoin;
    [SerializeField] private ParticleImage attractorBurstGem;
    
    #endregion
    
    // 사용자가 보유한 재화 
    public long Coin { get; private set; }           // 무료 재화 
    public long Gem { get; private set; }        // 유료 재화

    // 재화가 변동될 때 호출되는 Action
    public event Action<long> OnCoinChanged;
    public event Action<long> OnGemChanged;
    
    float coinAnimationStartTime;
    float gemAnimationStartTime;
    
    private CancellationTokenSource coinAnimationTokenSource; // 금화 애니메이션 취소 토큰
    private CancellationTokenSource gemAnimationTokenSource;  // 보석 애니메이션 취소 토큰
    
    /// <summary>
    /// 게임 오브젝트가 활성화될 때 호출됩니다.
    /// 싱글톤 인스턴스를 설정하고, 데이터를 로드합니다.
    /// </summary>
    private async void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 재화 데이터 저장 경로
        _dataPath_Currency = Path.Combine(Application.persistentDataPath, CurrencyFileName);

        // 데이터 로드
        await LoadCurrencyAsync();
        
        // 초기 텍스트 설정
        UpdateCoinText(Coin);
        UpdateGemText(Gem);
        
        // onFirstParticleFinished 이벤트 연결
        attractorBurstCoin.onFirstParticleFinished.AddListener(() => StartCoinAnimation(Coin));
        attractorBurstCoin.onLastParticleFinished.AddListener(()=>UpdateCoinText(Coin));
        attractorBurstGem.onFirstParticleFinished.AddListener(() => StartGemAnimation(Gem));
        attractorBurstGem.onLastParticleFinished.AddListener(()=>UpdateGemText(Gem));
    }

    #region Save & Load

    /// <summary>
    /// 재화 데이터를 로컬 저장소에 비동기적으로 저장합니다.
    /// </summary>
    private async UniTask SaveCurrencyAsync()
    {
        CurrencyData data = new CurrencyData { coin = Coin, gem = Gem };
        string json = JsonUtility.ToJson(data, true);
        try
        {
            using (StreamWriter writer = new StreamWriter(_dataPath_Currency, false))
            {
                await writer.WriteAsync(json);
            }
        }
        catch (Exception ex)
        {
            DebugEx.LogError($"SaveCurrencyAsync: 데이터 저장 중 오류 발생: {ex.Message}");
        }
    }

    /// <summary>
    /// 로컬 저장소에서 재화 데이터를 비동기적으로 불러옵니다.
    /// </summary>
    private async UniTask LoadCurrencyAsync()
    {
        if (!File.Exists(_dataPath_Currency))
        {
            Coin = 0;
            Gem = 0;
            return;
        }

        try
        {
            using (StreamReader reader = new StreamReader(_dataPath_Currency))
            {
                string json = await reader.ReadToEndAsync();
                CurrencyData data = JsonUtility.FromJson<CurrencyData>(json);
                Coin = data?.coin ?? 0;
                Gem = data?.gem ?? 0;

                // 초기값 설정 시 이벤트 호출
                OnCoinChanged?.Invoke(Coin);
                OnGemChanged?.Invoke(Gem);
            }
        }
        catch (Exception ex)
        {
            DebugEx.LogError($"LoadCurrencyAsync: 데이터 로드 중 오류 발생: {ex.Message}");
            Coin = 0;
            Gem = 0;
        }
    }

    #endregion

    #region Interface

    /// <summary>
    /// 골드 추가
    /// </summary>
    /// <param name="amount">추가할 금액</param>
    public async UniTask AddCoinAsync(long amount)
    {
        if (amount < 0)
        {
            DebugEx.LogWarning("AddCoinAsync: 음수 금액은 추가할 수 없습니다.");
            return;
        }

        attractorBurstCoin.SetBurst(0, 0, (int)(amount / 10));
        attractorBurstCoin.Play();
        coinAnimationStartTime = Time.time;
        
        Coin += amount;
        await SaveCurrencyAsync();
        OnCoinChanged?.Invoke(Coin); // Coin 변동 시 Action 호출
        DebugEx.Log($"Coin added: {amount}, Total: {Coin}");
    }

    /// <summary>
    /// 골드 소비
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public async UniTask<bool> SpendCoinAsync(long amount)
    {
        if (amount < 0)
        {
            DebugEx.LogWarning("SpendCoinAsync: 음수 금액은 사용할 수 없습니다.");
            return false;
        }

        if (Coin < amount)
        {
            PopupManager.Instance.ShowAlertPopup("SpendCoinAsync", "사용하려는 금액보다 보유한 Gold가 부족합니다.");
            DebugEx.LogWarning("SpendCoinAsync: 사용하려는 금액보다 보유한 Gold가 부족합니다.");
            return false;
        }

        Coin -= amount;
        await SaveCurrencyAsync();
        OnCoinChanged?.Invoke(Coin); // Coin 변동 시 Action 호출
        DebugEx.Log($"Coin spent: {amount}, Remaining: {Coin}");
        return true;
    }

    /// <summary>
    /// 다이아몬드 추가
    /// </summary>
    /// <param name="amount"></param>
    public async UniTask AddGemAsync(long amount)
    {
        if (amount < 0)
        {
            DebugEx.LogWarning("AddGemAsync: 음수 금액은 추가할 수 없습니다.");
            return;
        }

        attractorBurstGem.SetBurst(0, 0, (int)(amount / 10));
        attractorBurstGem.Play();
        gemAnimationStartTime = Time.time;
        
        Gem += amount;
        await SaveCurrencyAsync();
        OnGemChanged?.Invoke(Gem); // Gem 변동 시 Action 호출
        DebugEx.Log($"Gem added: {amount}, Total: {Gem}");
    }
    
    /// <summary>
    /// 재화 사용
    /// </summary>
    /// <param name="amount">사용할 금액</param>
    /// <returns>성공 여부</returns>
    public async UniTask<bool> SpendGemAsync(long amount)
    {
        if (amount < 0)
        {
            DebugEx.LogWarning("SpendGemAsync: 음수 금액은 사용할 수 없습니다.");
            return false;
        }

        if (Gem < amount)
        {
            PopupManager.Instance.ShowAlertPopup("SpendGemAsync", "사용하려는 금액보다 보유한 Diamond가 부족합니다.");
            DebugEx.LogWarning("SpendGemAsync: 사용하려는 금액보다 보유한 Diamond가 부족합니다.");
            return false;
        }

        Gem -= amount;
        await SaveCurrencyAsync();
        OnGemChanged?.Invoke(Gem); // Gem 변동 시 Action 호출
        DebugEx.Log($"Gem spent: {amount}, Remaining: {Gem}");
        return true;
    }

    #endregion

    private void UpdateCoinText(long coinAmount)
    {
        if (coinText != null)
            coinText.text = $"{coinAmount:N0}";
    }

    private void UpdateGemText(long gemAmount)
    {
        if (gemText != null)
            gemText.text = $"{gemAmount:N0}";
    }
    
    private void StartCoinAnimation(long targetAmount)
    {
        DOTween.Kill(coinText);
        float timeToFirstParticle = Time.time - coinAnimationStartTime;
        float duration = attractorBurstCoin.duration - timeToFirstParticle;
        
        coinText.DOCounter(int.Parse(coinText.text.Replace(",", "")), (int)targetAmount, duration).SetEase(Ease.OutCubic);
    }

    private void StartGemAnimation(long targetAmount)
    {
        DOTween.Kill(gemText);
        
        float timeToFirstParticle = Time.time - gemAnimationStartTime;
        float duration = attractorBurstGem.duration - timeToFirstParticle;
        
        gemText.DOCounter(int.Parse(gemText.text.Replace(",", "")), (int)targetAmount, duration).SetEase(Ease.OutCubic);
    }
    
    /// <summary>
    /// 앱이 종료될 때 데이터 저장
    /// </summary>
    private async void OnApplicationQuit()
    {
        await SaveCurrencyAsync();
    }

    public void ParticleStopped()
    {
        DebugEx.Log($"<color='red'>Particle Stopped!!!!!</color>");
    }
    
    /// <summary>
    /// 재화 데이터를 저장하기 위한 클래스.
    /// </summary>
    [Serializable]
    private class CurrencyData
    {
        public long coin;
        public long gem;
    }
}