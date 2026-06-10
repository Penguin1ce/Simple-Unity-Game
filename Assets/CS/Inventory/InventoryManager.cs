using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour, IInventoryService
{
    private static InventoryManager _instance;

    public static InventoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InventoryManager>();
            }
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        // 如果场景里已经有了（比如挂在玩家身上），就不自动创建
        if (FindObjectOfType<InventoryManager>() != null) return;

        GameObject go = new GameObject("InventoryManager");
        _instance = go.AddComponent<InventoryManager>();
        DontDestroyOnLoad(go);
    }

    [Header("Item Definitions")]
    public List<InventoryItemData> itemDefinitions = new List<InventoryItemData>();

    [SerializeField]
    private List<InventorySlot> slots = new List<InventorySlot>();

    public event Action OnInventoryChanged;
    public event Action<string, PotionEffectType, int> OnPotionUsed;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        LoadItemRegistry();
    }

    private void LoadItemRegistry()
    {
        InventoryItemRegistry registry = Resources.Load<InventoryItemRegistry>("ItemRegistry");
        if (registry == null)
        {
            Debug.Log("[InventoryManager] No ItemRegistry found, creating built-in defaults.");
            CreateBuiltInDefaults();
            return;
        }

        itemDefinitions.Clear();
        for (int i = 0; i < registry.items.Count; i++)
            itemDefinitions.Add(registry.items[i]);
        Debug.Log(string.Format("[InventoryManager] Loaded {0} item definitions from ItemRegistry.", itemDefinitions.Count));
    }

    private void CreateBuiltInDefaults()
    {
        itemDefinitions.Clear();
        itemDefinitions.Add(MakeDef("potion_health", "治理药水", ItemCategory.Potion, PotionEffectType.HealthRestore, 30));
        itemDefinitions.Add(MakeDef("potion_mana", "魔力药水", ItemCategory.Potion, PotionEffectType.ManaRestore, 20));
        itemDefinitions.Add(MakeDef("potion_attack_boost", "力量药水", ItemCategory.Potion, PotionEffectType.AttackBoost, 5));
        itemDefinitions.Add(MakeDef("quest_stone_heart", "石像心", ItemCategory.QuestItem));
        itemDefinitions.Add(MakeDef("quest_flame_demon_core", "炎魔之核", ItemCategory.QuestItem));
        itemDefinitions.Add(MakeDef("quest_skeleton_crown", "骷髅王冠", ItemCategory.QuestItem));
        itemDefinitions.Add(MakeDef("weapon_iron_blade", "铁刀", ItemCategory.Weapon, atk: 5));
        itemDefinitions.Add(MakeDef("weapon_steel_blade", "刚刀", ItemCategory.Weapon, atk: 10));
        itemDefinitions.Add(MakeDef("weapon_legendary_blade", "传说之刃", ItemCategory.Weapon, atk: 15));

        for (int i = 0; i < itemDefinitions.Count; i++)
            Debug.Log(string.Format("[InventoryManager] Built-in def: id={0}, name={1}, cat={2}",
                itemDefinitions[i].itemId, itemDefinitions[i].itemName, itemDefinitions[i].itemCategory));
    }

    private InventoryItemData MakeDef(string id, string name, ItemCategory cat,
        PotionEffectType effect = default, int effectVal = 0, int atk = 0)
    {
        InventoryItemData d = ScriptableObject.CreateInstance<InventoryItemData>();
        d.itemId = id;
        d.itemName = name;
        d.itemCategory = cat;
        d.maxStack = cat == ItemCategory.Weapon ? 1 : 99;
        if (cat == ItemCategory.Potion) { d.potionEffectType = effect; d.effectValue = effectVal; }
        if (cat == ItemCategory.Weapon) d.attackPower = atk;
        return d;
    }

    #region Basic Inventory

    public InventorySlot[] GetAllSlots()
    {
        return slots.ToArray();
    }

    public bool AddItem(string itemId, int count)
    {
        if (string.IsNullOrEmpty(itemId) || count <= 0) return false;

        int maxStack = GetMaxStackForItem(itemId);
        int remaining = count;

        for (int i = 0; i < slots.Count; i++)
        {
            if (remaining <= 0) break;

            if (slots[i].itemId == itemId && slots[i].count < maxStack)
            {
                int space = maxStack - slots[i].count;
                int toAdd = Mathf.Min(space, remaining);
                slots[i].AddAmount(toAdd);
                remaining -= toAdd;
            }
        }

        while (remaining > 0)
        {
            int toAdd = Mathf.Min(maxStack, remaining);
            InventorySlot newSlot = new InventorySlot();
            newSlot.Set(itemId, toAdd);
            slots.Add(newSlot);
            remaining -= toAdd;
        }

        if (OnInventoryChanged != null) OnInventoryChanged();
        return true;
    }

    public int GetItemCount(string itemId)
    {
        int total = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].itemId == itemId)
            {
                total += slots[i].count;
            }
        }
        return total;
    }

    public bool RemoveItem(string itemId, int count)
    {
        if (GetItemCount(itemId) < count) return false;

        int remaining = count;
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (remaining <= 0) break;

            if (slots[i].itemId == itemId)
            {
                int toRemove = Mathf.Min(slots[i].count, remaining);
                slots[i].RemoveAmount(toRemove);
                remaining -= toRemove;
            }
        }

        slots.RemoveAll(s => s.IsEmpty);

        if (OnInventoryChanged != null) OnInventoryChanged();
        return true;
    }

    public bool HasItem(string itemId, int count = 1)
    {
        return GetItemCount(itemId) >= count;
    }

    #endregion

    #region Potion Usage

    public bool UsePotion(string itemId)
    {
        InventoryItemData def = GetItemDefinition(itemId);
        if (def == null || def.itemCategory != ItemCategory.Potion)
        {
            Debug.LogWarning(string.Format("[InventoryManager] {0} is not a potion.", itemId));
            return false;
        }

        if (!HasItem(itemId))
        {
            Debug.LogWarning(string.Format("[InventoryManager] No {0} in inventory.", itemId));
            return false;
        }

        RemoveItem(itemId, 1);
        Debug.Log(string.Format("[InventoryManager] Used potion: {0}, effect: {1}, value: {2}",
            itemId, def.potionEffectType, def.effectValue));

        if (OnPotionUsed != null)
        {
            OnPotionUsed(itemId, def.potionEffectType, def.effectValue);
        }
        return true;
    }

    #endregion

    #region Query

    public InventoryItemData GetItemDefinition(string itemId)
    {
        for (int i = 0; i < itemDefinitions.Count; i++)
        {
            if (itemDefinitions[i].itemId == itemId)
            {
                return itemDefinitions[i];
            }
        }
        return null;
    }

    private int GetMaxStackForItem(string itemId)
    {
        InventoryItemData def = GetItemDefinition(itemId);
        if (def != null) return def.maxStack;
        return 99;
    }

    public void ForceRefreshUI()
    {
        if (OnInventoryChanged != null) OnInventoryChanged();
    }

    #endregion
}
