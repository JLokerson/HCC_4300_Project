using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages a single stat with base value and dynamic modifiers.
/// Uses dirty flag pattern for performance optimization.
/// </summary>
[Serializable]
public class CharacterStat
{
    public float BaseValue;

    // Event fired when final value changes (for UI updates)
    public event Action OnValueChanged;

    // Backing field for modifiers list
    private readonly List<StatModifier> statModifiers;

    // Cached final value to avoid recalculation every access
    private float _value;

    // Dirty flag indicates recalculation needed
    private bool isDirty = true;

    // Public read-only access to modifiers
    public ReadOnlyCollection<StatModifier> StatModifiers { get; }

    public CharacterStat()
    {
        statModifiers = new List<StatModifier>(4); // Pre-allocate for typical modifier count
        StatModifiers = statModifiers.AsReadOnly();
    }

    public CharacterStat(float baseValue) : this()
    {
        BaseValue = baseValue;
    }

    /// <summary>
    /// Gets the final calculated stat value.
    /// Only recalculates if dirty flag is set.
    /// </summary>
    public virtual float Value
    {
        get
        {
            if (isDirty)
            {
                _value = CalculateFinalValue();
                isDirty = false;
            }
            return _value;
        }
    }

    /// <summary>
    /// Adds a modifier and marks stat as dirty.
    /// Modifiers with same source can stack.
    /// </summary>
    public virtual void AddModifier(StatModifier modifier)
    {
        if (modifier == null)
        {
            Debug.LogError("Cannot add null modifier");
            return;
        }

        isDirty = true;
        statModifiers.Add(modifier);
        OnValueChanged?.Invoke();
    }

    /// <summary>
    /// Removes a specific modifier instance.
    /// Returns true if modifier was found and removed.
    /// </summary>
    public virtual bool RemoveModifier(StatModifier modifier)
    {
        if (statModifiers.Remove(modifier))
        {
            isDirty = true;
            OnValueChanged?.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes all modifiers from a specific source (e.g., unequipping an item).
    /// Critical for equipment systems and temporary buffs.
    /// </summary>
    public virtual bool RemoveAllModifiersFromSource(object source)
    {
        // Reverse iteration to safely remove during iteration
        bool didRemove = false;
        for (int i = statModifiers.Count - 1; i >= 0; i--)
        {
            if (statModifiers[i].Source == source)
            {
                isDirty = true;
                statModifiers.RemoveAt(i);
                didRemove = true;
            }
        }

        if (didRemove)
        {
            OnValueChanged?.Invoke();
        }

        return didRemove;
    }

    /// <summary>
    /// Removes all modifiers and resets to base value.
    /// </summary>
    public virtual void ClearModifiers()
    {
        if (statModifiers.Count > 0)
        {
            isDirty = true;
            statModifiers.Clear();
            OnValueChanged?.Invoke();
        }
    }

    /// <summary>
    /// Calculates final value using industry-standard modifier order:
    /// 1. Flat modifiers (additive)
    /// 2. PercentAdd modifiers (summed, then applied as multiplier)
    /// 3. PercentMult modifiers (each applied as separate multiplier)
    /// </summary>
    protected virtual float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        // Sort modifiers by Order to ensure correct calculation sequence
        // Uses LINQ for clarity; for performance-critical code, consider caching sorted list
        foreach (var modifier in statModifiers.OrderBy(m => m.Order))
        {
            switch (modifier.Type)
            {
                case ModifierType.Flat:
                    finalValue += modifier.Value;
                    break;

                case ModifierType.PercentAdd:
                    // Sum all PercentAdd modifiers together
                    sumPercentAdd += modifier.Value;
                    break;

                case ModifierType.PercentMult:
                    // Apply each PercentMult separately (multiplicative stacking)
                    finalValue *= 1 + modifier.Value;
                    break;
            }
        }

        // Apply summed PercentAdd modifiers
        finalValue *= 1 + sumPercentAdd;

        return finalValue;
    }
}