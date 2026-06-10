using UnityEngine;

public enum ItemCategory
{
    Weapon,
    Potion,
    QuestItem
}

public enum PotionEffectType
{
    AttackBoost,
    HealthRestore,
    ManaRestore
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "ARPG/Inventory/Item Data")]
public class InventoryItemData : ScriptableObject
{
    [Header("Base")]
    public string itemId;
    public string itemName;
    public string description;
    public Sprite icon;
    public int maxStack = 99;
    public ItemCategory itemCategory;

    [Header("Weapon")]
    [Tooltip("Only used when ItemCategory = Weapon")]
    public int attackPower;
    [Tooltip("The 3D model prefab for this weapon, shown on the player's hand.")]
    public GameObject weaponPrefab;

    [Header("Potion")]
    [Tooltip("Only used when ItemCategory = Potion")]
    public PotionEffectType potionEffectType;
    [Tooltip("Effect value: attack bonus, HP restored, or MP restored")]
    public int effectValue;

    [Header("Quest Item")]
    [Tooltip("Associated quest stage that awards this item")]
    public int questStage;
}
