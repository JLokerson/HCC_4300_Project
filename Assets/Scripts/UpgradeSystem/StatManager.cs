using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all stats for a character or entity.
/// Uses Dictionary for O(1) stat lookups by StatType enum.
/// </summary>
public class StatManager : MonoBehaviour
{
    // Serializable wrapper for Unity Inspector (Dictionaries don't serialize natively)
    [Serializable]
    private class StatEntry
    {
        public StatType statType;
        public float baseValue;
    }

    [Header("Base Stat Configuration")]
    [Tooltip("Configure base values for each stat in the Inspector")]
    [SerializeField] private List<StatEntry> baseStats = new List<StatEntry>();

    // Runtime Dictionary for fast stat access
    private Dictionary<StatType, CharacterStat> statDictionary;

    public event Action<StatType, float> OnStatChanged;

    private void Awake()
    {
        InitializeStats();
    }

    /// <summary>
    /// Converts serialized list to Dictionary and initializes stats.
    /// Called once at startup.
    /// </summary>
    private void InitializeStats()
    {
        // Pre-allocate Dictionary capacity for all enum values
        int statCount = Enum.GetValues(typeof(StatType)).Length;
        statDictionary = new Dictionary<StatType, CharacterStat>(statCount);

        // Initialize from Inspector-configured base stats
        foreach (var entry in baseStats)
        {
            var stat = new CharacterStat(entry.baseValue);
            stat.OnValueChanged += () => OnStatChanged?.Invoke(entry.statType, stat.Value);
            statDictionary[entry.statType] = stat;
        }

        // Initialize any missing stats with default value (0)
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            if (!statDictionary.ContainsKey(statType))
            {
                var stat = new CharacterStat(0);
                stat.OnValueChanged += () => OnStatChanged?.Invoke(statType, stat.Value);
                statDictionary[statType] = stat;
            }
        }
    }

    /// <summary>
    /// Gets the CharacterStat object for the given stat type.
    /// Returns null if stat doesn't exist (shouldn't happen if InitializeStats worked correctly).
    /// </summary>
    public CharacterStat GetStat(StatType statType)
    {
        if (statDictionary.TryGetValue(statType, out CharacterStat stat))
        {
            return stat;
        }

        Debug.LogError($"Stat {statType} not found in StatManager");
        return null;
    }

    /// <summary>
    /// Convenience method to get final stat value directly.
    /// </summary>
    public float GetStatValue(StatType statType)
    {
        return GetStat(statType)?.Value ?? 0f;
    }

    /// <summary>
    /// Adds a modifier to a specific stat.
    /// </summary>
    public void AddModifier(StatType statType, StatModifier modifier)
    {
        GetStat(statType)?.AddModifier(modifier);
    }

    /// <summary>
    /// Removes all modifiers from a specific source across all stats.
    /// Useful for equipment unequipping or buff expiration.
    /// </summary>
    public void RemoveAllModifiersFromSource(object source)
    {
        foreach (var stat in statDictionary.Values)
        {
            stat.RemoveAllModifiersFromSource(source);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Print All Stats")]
    private void DebugPrintAllStats()
    {
        foreach (var kvp in statDictionary)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value.Value} (Base: {kvp.Value.BaseValue}, Modifiers: {kvp.Value.StatModifiers.Count})");
        }
    }
#endif
}