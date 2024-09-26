using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private int money;
    private int cash;



    private static InventoryManager _instance;
    public static InventoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InventoryManager>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    _instance = singletonObject.AddComponent<InventoryManager>();
                    singletonObject.name = typeof(InventoryManager).ToString() + " (Singleton)";
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
       if(_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddMoney(int amount)
    {
        if (amount < 0) { }
        else if (money > int.MaxValue - amount) { }
        else { money += amount; }
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            return true;
        }
        else
        {
            return false;
        }
    }

    public int GetMoney()
    {
        return money;
    }
    private void SetUIMoney()
    {
        // 미구현, 상단 UI의 머니 갱신 
    }
}
