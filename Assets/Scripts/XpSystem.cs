using UnityEngine;
using System;

public class XPSystem : MonoBehaviour
{
    [Header("XP Curve")]
    [SerializeField] private int baseXP = 50;
    [SerializeField] private float growth = 1.25f; // > 1.0 means increasing requirement

    public int Level { get; private set; } = 0;
    public int CurrentXP { get; private set; } = 0;
    public int RequiredXP { get; private set; }

    // Events
    public event Action<float, int, int> OnXPChanged; // normalized, current, required
    public event Action<int, int> OnLevelChanged; // level, new required

    void Awake()
    {
        RecalculateRequiredXP();
        RaiseXPChanged();
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;
        CurrentXP += amount;

        // Handle multiple level-ups if a big orb drops
        while (CurrentXP >= RequiredXP)
        {
            CurrentXP -= RequiredXP; // carryover
            Level++;
            RecalculateRequiredXP();
            OnLevelChanged?.Invoke(Level, RequiredXP);
        }
        RaiseXPChanged();
    }

    public void ResetXP(bool resetLevel = false)
    {
        if (resetLevel) Level = 0;
        CurrentXP = 0;
        RecalculateRequiredXP();
        RaiseXPChanged();
        OnLevelChanged?.Invoke(Level, RequiredXP);
    }

    private void RecalculateRequiredXP()
    {
        RequiredXP = Mathf.Max(1, Mathf.FloorToInt(baseXP * Mathf.Pow(growth, Level)));
    }

    private void RaiseXPChanged()
    {
        float norm = Mathf.Clamp01((RequiredXP > 0) ? (float)CurrentXP / RequiredXP : 0f);
        OnXPChanged?.Invoke(norm, CurrentXP, RequiredXP);
    }
}
