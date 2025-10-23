using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;
    public int CurrentHealth { get; private set; }

    // UI-friendly events
    // normalized: 0..1, current, max
    public event Action<float, int, int> OnHealthChanged;
    public event Action OnDied;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
        RaiseHealthChanged();
    }

    public void Damage(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        RaiseHealthChanged();
        if (CurrentHealth == 0) OnDied?.Invoke();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        RaiseHealthChanged();
    }

    public void SetMaxHealth(int value, bool fill = true)
    {
        MaxHealth = Mathf.Max(1, value);
        if (fill) CurrentHealth = MaxHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        RaiseHealthChanged();
    }

    public void ReviveFull()
    {
        CurrentHealth = MaxHealth;
        RaiseHealthChanged();
    }

    private void RaiseHealthChanged()
    {
        float normalized = MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;
        OnHealthChanged?.Invoke(normalized, CurrentHealth, MaxHealth);
    }
}
