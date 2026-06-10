using System;
using UnityEngine;

[Serializable]
public class WeaponStageEntry
{
    [Tooltip("When quest stage reaches this value, this weapon auto-equips.")]
    public int questStage;
    public string weaponId;
}

public class WeaponManager : MonoBehaviour
{
    private static WeaponManager _instance;

    public static WeaponManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<WeaponManager>();
            }
            return _instance;
        }
    }

    [Header("Weapon Stage Progression")]
    [Tooltip("Weapon auto-equips when quest stage >= the configured threshold. Sorted low to high.")]
    public WeaponStageEntry[] weaponProgression = new WeaponStageEntry[]
    {
        new WeaponStageEntry { questStage = 0, weaponId = "weapon_iron_blade" },
        new WeaponStageEntry { questStage = 2, weaponId = "weapon_steel_blade" },
        new WeaponStageEntry { questStage = 3, weaponId = "weapon_legendary_blade" }
    };

    [Header("Weapon Socket")]
    [Tooltip("Drag the WeaponSlot or hand bone here. Leave empty to auto-find.")]
    public Transform weaponSocket;

    [Header("Weapon Offset")]
    [Tooltip("Position offset when spawning weapon onto the socket. Adjust if the weapon position is wrong.")]
    public Vector3 weaponPositionOffset = Vector3.zero;
    public Vector3 weaponRotationOffset = Vector3.zero;

    private string _currentWeaponId;
    private GameObject _spawnedWeapon;

    public string CurrentWeaponId
    {
        get { return _currentWeaponId; }
    }

    public InventoryItemData CurrentWeaponData
    {
        get { return GetWeaponDefinition(_currentWeaponId); }
    }

    public event Action<string> OnWeaponChanged;

    private bool _subscribed;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // 自动查找 WeaponSlot（如果没手动拖的话）
        if (weaponSocket == null)
        {
            FindWeaponSocket();
        }
    }

    private void Start()
    {
        // Start 时再找一次（手骨可能这时才加载完）
        if (weaponSocket == null)
        {
            FindWeaponSocket();
        }

        TrySubscribe();
        RefreshWeapon();
    }

    /// <summary>
    /// 自动在玩家骨骼中查找武器挂载点
    /// 优先找 WeaponSlot/RightHandSlot，找不到就用右手骨骼
    /// </summary>
    private void FindWeaponSocket()
    {
        // 1. 先找 WeaponSlot / RightHandSlot
        Transform found = FindChildRecursive(transform, "WeaponSlot");
        if (found == null)
        {
            found = FindChildRecursive(transform, "RightHandSlot");
        }

        // 2. 找不到就用右手骨骼
        if (found == null)
        {
            found = FindChildRecursive(transform, "RightHand");
        }
        if (found == null)
        {
            found = FindChildRecursive(transform, "hand_R");
        }
        if (found == null)
        {
            found = FindChildRecursive(transform, "Right Hand");
        }

        if (found != null)
        {
            weaponSocket = found;
            Debug.Log(string.Format("[WeaponManager] Auto-found weapon socket: {0}", found.gameObject.name));
        }
        else
        {
            Debug.LogWarning("[WeaponManager] Weapon socket not found! Please assign it manually in Inspector.");
        }
    }

    /// <summary>
    /// 递归查找子物体
    /// </summary>
    private Transform FindChildRecursive(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name) return child;

            Transform result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (_subscribed && MainQuestManager.Instance != null)
        {
            MainQuestManager.Instance.QuestStageChanged -= OnQuestStageChanged;
            _subscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (_subscribed) return;
        if (MainQuestManager.Instance == null) return;

        MainQuestManager.Instance.QuestStageChanged += OnQuestStageChanged;
        _subscribed = true;
    }

    private void OnQuestStageChanged(int newStage, string questText)
    {
        RefreshWeapon();
    }

    private void RefreshWeapon()
    {
        string newWeaponId = GetWeaponForCurrentStage();

        if (newWeaponId == _currentWeaponId) return;

        string oldWeapon = _currentWeaponId;
        _currentWeaponId = newWeaponId;

        Debug.Log(string.Format("[WeaponManager] Weapon changed: {0} -> {1}",
            oldWeapon ?? "null", newWeaponId ?? "null"));

        // 切换3D模型
        SpawnWeaponModel();

        if (OnWeaponChanged != null)
        {
            OnWeaponChanged(newWeaponId);
        }
    }

    private void SpawnWeaponModel()
    {
        // 删掉旧武器
        if (_spawnedWeapon != null)
        {
            Destroy(_spawnedWeapon);
            _spawnedWeapon = null;
        }

        // 没有武器插槽就不生成
        if (weaponSocket == null) return;

        // 获取武器数据
        InventoryItemData data = CurrentWeaponData;
        if (data == null || data.weaponPrefab == null) return;

        // 生成新武器到挂载点
        _spawnedWeapon = Instantiate(data.weaponPrefab, weaponSocket);
        _spawnedWeapon.transform.localPosition = weaponPositionOffset;
        _spawnedWeapon.transform.localRotation = Quaternion.Euler(weaponRotationOffset);
        _spawnedWeapon.transform.localScale = Vector3.one;

        Debug.Log(string.Format("[WeaponManager] Spawned weapon model: {0} on {1}", data.itemName, weaponSocket.name));
    }

    private string GetWeaponForCurrentStage()
    {
        if (MainQuestManager.Instance == null) return weaponProgression[0].weaponId;

        int currentStage = MainQuestManager.Instance.QuestStage;
        string bestWeapon = null;

        for (int i = 0; i < weaponProgression.Length; i++)
        {
            if (currentStage >= weaponProgression[i].questStage)
            {
                bestWeapon = weaponProgression[i].weaponId;
            }
        }

        return bestWeapon;
    }

    private InventoryItemData GetWeaponDefinition(string weaponId)
    {
        if (string.IsNullOrEmpty(weaponId)) return null;
        if (InventoryManager.Instance == null) return null;
        return InventoryManager.Instance.GetItemDefinition(weaponId);
    }
}
