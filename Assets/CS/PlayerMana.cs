using System;
using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana Settings")]
    public float maxMana = 100f;
    public float currentMana;

    public event Action<float, float> ManaChanged;

    public float CurrentMana => currentMana;
    public float MaxMana => maxMana;

    private void Awake()
    {
        maxMana = Mathf.Max(1f, maxMana);
        currentMana = Mathf.Clamp(currentMana <= 0f ? maxMana : currentMana, 0f, maxMana);
        NotifyManaChanged();
    }

    public void RestoreMana(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentMana = Mathf.Min(maxMana, currentMana + amount);
        NotifyManaChanged();
    }

    public bool ConsumeMana(float amount)
    {
        if (amount <= 0f || currentMana < amount)
        {
            return false;
        }

        currentMana -= amount;
        NotifyManaChanged();
        return true;
    }

    private void NotifyManaChanged()
    {
        if (ManaChanged != null)
        {
            ManaChanged(currentMana, maxMana);
        }
    }
}