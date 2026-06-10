using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour, ICurrencyService
{
    private static CurrencyManager _instance;

    public static CurrencyManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<CurrencyManager>();
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        // 如果场景里已经有了（比如挂在玩家身上），就不自动创建
        if (FindObjectOfType<CurrencyManager>() != null) return;

        GameObject go = new GameObject("CurrencyManager");
        _instance = go.AddComponent<CurrencyManager>();
        DontDestroyOnLoad(go);
    }

    [Header("Initial Gold")]
    public int initialGold = 100;

    [SerializeField]
    private int _gold;

    public int Gold
    {
        get { return _gold; }
    }

    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        _gold = initialGold;
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return false;
        if (_gold < amount)
        {
            Debug.LogWarning(string.Format("[CurrencyManager] Not enough gold. Have: {0}, need: {1}", _gold, amount));
            return false;
        }
        _gold -= amount;
        Debug.Log(string.Format("[CurrencyManager] Spent {0} gold. Remaining: {1}", amount, _gold));
        if (OnGoldChanged != null) OnGoldChanged(_gold);
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        _gold += amount;
        Debug.Log(string.Format("[CurrencyManager] Added {0} gold. Total: {1}", amount, _gold));
        if (OnGoldChanged != null) OnGoldChanged(_gold);
    }

    public bool HasGold(int amount)
    {
        return _gold >= amount;
    }
}
