using System;
using UnityEngine;

/// <summary>
/// Represents a single modification to a stat value.
/// Modifiers are sorted by Order and applied in sequence.
/// </summary>
[Serializable]
public class StatModifier
{
    public float Value;
    public ModifierType Type;
    public int Order;
    public object Source; // Reference to what created this modifier (item, buff, etc.)

    /// <summary>
    /// Standard orders: Flat(100) -> Additive(200) -> Multiplicative(300)
    /// Custom orders can be placed between standard values (e.g., 150)
    /// </summary>
    public StatModifier(float value, ModifierType type, int order, object source)
    {
        Value = value;
        Type = type;
        Order = order;
        Source = source;
    }

    /// <summary>
    /// Convenience constructor using standard order values
    /// </summary>
    public StatModifier(float value, ModifierType type) : this(value, type, (int)type, null)
    {
    }

    public StatModifier(float value, ModifierType type, object source) : this(value, type, (int)type, source)
    {
    }
}

/// <summary>
/// Modifier types with built-in order values
/// </summary>
public enum ModifierType
{
    Flat = 100,          // Added directly to base (e.g., +10 damage)
    PercentAdd = 200,    // Percentage that sums with other PercentAdd (e.g., +20% damage)
    PercentMult = 300    // Percentage that multiplies separately (e.g., 50% MORE damage)
}
