using UnityEngine;
using System.Collections.Generic; // <-- THIS WAS MISSING!

/// <summary>
/// Helper class to define individual stat changes
/// </summary>
[System.Serializable]
public class StatModification
{
    public StatType affectedStat;
    public ModifierType modifierType;
    [Tooltip("Value to add/multiply per stack. For percentages, use decimals (0.1 = 10%)")]
    public float valuePerStack;
}

/// <summary>
/// ScriptableObject defining an upgrade that can be acquired and stacked.
/// Separates data (this SO) from logic (UpgradeManager).
/// Can affect multiple stats at once.
/// </summary>
[CreateAssetMenu(fileName = "New Upgrade", menuName = "Game/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Display Information")]
    public string upgradeName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;

    [Header("Stat Modifications")]
    [Tooltip("List of stat changes this upgrade provides")]
    public List<StatModification> statModifications = new List<StatModification>();

    [Header("Stack Settings")]
    [Tooltip("Maximum number of times this upgrade can be acquired")]
    public int maxStacks = 5;

    [Tooltip("Can this upgrade be acquired multiple times?")]
    public bool isStackable = true;

    /// <summary>
    /// Creates all StatModifiers for this upgrade at a given stack count.
    /// Returns a dictionary mapping StatType to the modifier for that stat.
    /// </summary>
    public Dictionary<StatType, StatModifier> CreateModifiers(int stackCount, object source)
    {
        var modifiers = new Dictionary<StatType, StatModifier>();
        
        foreach (var statMod in statModifications)
        {
            float finalValue = statMod.valuePerStack * stackCount;
            modifiers[statMod.affectedStat] = new StatModifier(finalValue, statMod.modifierType, source);
        }
        
        return modifiers;
    }

    /// <summary>
    /// Validates upgrade configuration in the editor.
    /// </summary>
    private void OnValidate()
    {
        if (maxStacks < 1)
        {
            maxStacks = 1;
        }

        if (!isStackable && maxStacks > 1)
        {
            Debug.LogWarning($"Upgrade '{upgradeName}' is not stackable but has maxStacks > 1");
        }

        // Validate percentage values for each stat modification
        foreach (var statMod in statModifications)
        {
            if (statMod.modifierType != ModifierType.Flat)
            {
                if (Mathf.Abs(statMod.valuePerStack) > 10f)
                {
                    Debug.LogWarning($"Upgrade '{upgradeName}' has very large percentage value ({statMod.valuePerStack}) for {statMod.affectedStat}. Did you mean {statMod.valuePerStack / 100f}?");
                }
            }
        }
    }
}