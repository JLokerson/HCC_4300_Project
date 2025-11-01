using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages upgrade acquisition, stack tracking, and application to StatManager.
/// Stores runtime state separate from ScriptableObject assets.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StatManager statManager;

    [Header("Configuration")]
    [Tooltip("All upgrades available in the game")]
    [SerializeField] private List<UpgradeData> availableUpgrades = new List<UpgradeData>();

    // Runtime tracking: UpgradeData -> current stack count
    private Dictionary<UpgradeData, int> activeUpgradeStacks = new Dictionary<UpgradeData, int>();

    // Track applied modifiers for removal/updating
    private Dictionary<UpgradeData, Dictionary<StatType, StatModifier>> appliedModifiers = new Dictionary<UpgradeData, Dictionary<StatType, StatModifier>>();

    public event Action<UpgradeData, int> OnUpgradeAcquired;
    public event Action<UpgradeData, int> OnUpgradeStacked;

    private void Awake()
    {
        if (statManager == null)
        {
            statManager = GetComponent<StatManager>();
        }

        if (statManager == null)
        {
            Debug.LogError("UpgradeManager requires a StatManager component!");
        }
    }

    /// <summary>
    /// Attempts to acquire an upgrade. Returns true if successful.
    /// Handles both new acquisition and stacking.
    /// </summary>
    public bool AcquireUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null)
        {
            Debug.LogError("Cannot acquire null upgrade");
            return false;
        }

        // Check if we already have this upgrade
        if (activeUpgradeStacks.TryGetValue(upgrade, out int currentStack))
        {
            // Already have it - try to stack
            if (!upgrade.isStackable)
            {
                Debug.Log($"Upgrade '{upgrade.upgradeName}' is not stackable");
                return false;
            }

            if (currentStack >= upgrade.maxStacks)
            {
                Debug.Log($"Upgrade '{upgrade.upgradeName}' is already at max stacks ({upgrade.maxStacks})");
                return false;
            }

            // Increment stack
            activeUpgradeStacks[upgrade] = currentStack + 1;
            UpdateUpgradeModifier(upgrade, currentStack + 1);
            OnUpgradeStacked?.Invoke(upgrade, currentStack + 1);
        }
        else
        {
            // First time acquiring this upgrade
            activeUpgradeStacks[upgrade] = 1;
            ApplyUpgradeModifier(upgrade, 1);
            OnUpgradeAcquired?.Invoke(upgrade, 1);
        }

        return true;
    }

    /// <summary>
    /// Applies a new upgrade modifier to the stat system.
    /// </summary>
    private void ApplyUpgradeModifier(UpgradeData upgrade, int stackCount)
    {
        // Create modifiers for all affected stats
        var modifiers = upgrade.CreateModifiers(stackCount, upgrade);
        
        // Store references to modifiers for later updates
        appliedModifiers[upgrade] = modifiers;
        
        // Apply each modifier to the appropriate stat
        foreach (var kvp in modifiers)
        {
            StatType statType = kvp.Key;        // The stat being modified
            StatModifier modifier = kvp.Value;   // The modifier to apply
            statManager.AddModifier(statType, modifier);
        }

        Debug.Log($"Applied upgrade '{upgrade.upgradeName}' affecting {modifiers.Count} stat(s) (Stack: {stackCount})");
    }

    /// <summary>
    /// Updates existing upgrade modifier when stack count changes.
    /// More efficient than removing and readding.
    /// </summary>
    private void UpdateUpgradeModifier(UpgradeData upgrade, int newStackCount)
    {
        if (appliedModifiers.TryGetValue(upgrade, out var existingModifiers))
        {
            // Remove all old modifiers
            foreach (var kvp in existingModifiers)
            {
                StatType statType = kvp.Key;
                StatModifier modifier = kvp.Value;
                var stat = statManager.GetStat(statType);
                stat.RemoveModifier(modifier);
            }

            // Create and apply new modifiers with updated stack count
            var newModifiers = upgrade.CreateModifiers(newStackCount, upgrade);
            appliedModifiers[upgrade] = newModifiers;
            
            foreach (var kvp in newModifiers)
            {
                StatType statType = kvp.Key;
                StatModifier modifier = kvp.Value;
                statManager.AddModifier(statType, modifier);
            }

            Debug.Log($"Updated upgrade '{upgrade.upgradeName}' stack to {newStackCount}");
        }
    }

    /// <summary>
    /// Removes an upgrade entirely
    /// </summary>
    public bool RemoveUpgrade(UpgradeData upgrade)
    {
        if (!activeUpgradeStacks.ContainsKey(upgrade))
        {
            return false;
        }

        // Remove modifier from stat system
        statManager.RemoveAllModifiersFromSource(upgrade);

        // Remove from tracking dictionaries
        activeUpgradeStacks.Remove(upgrade);
        appliedModifiers.Remove(upgrade);

        Debug.Log($"Removed upgrade '{upgrade.upgradeName}'");
        return true;
    }

    /// <summary>
    /// Gets current stack count for an upgrade.
    /// </summary>
    public int GetUpgradeStack(UpgradeData upgrade)
    {
        return activeUpgradeStacks.TryGetValue(upgrade, out int stack) ? stack : 0;
    }

    /// <summary>
    /// Checks if upgrade can be acquired (not at max stacks).
    /// </summary>
    public bool CanAcquireUpgrade(UpgradeData upgrade)
    {
        int currentStack = GetUpgradeStack(upgrade);

        if (currentStack == 0)
        {
            return true; // Can always acquire for first time
        }

        if (!upgrade.isStackable)
        {
            return false; // Already have nonstackable upgrade
        }

        return currentStack < upgrade.maxStacks;
    }

    /// <summary>
    /// Gets random available upgrades for level-up choice UI.
    /// Filters out maxstacked and nonstackable already acquired upgrades.
    /// </summary>
    public List<UpgradeData> GetRandomAvailableUpgrades(int count)
    {
        var eligible = availableUpgrades.Where(u => CanAcquireUpgrade(u)).ToList();

        if (eligible.Count <= count)
        {
            return eligible;
        }

        // Shuffle and take random subset
        return eligible.OrderBy(x => UnityEngine.Random.value).Take(count).ToList();
    }

    /// <summary>
    /// Clears all upgrades (for game restart/new run).
    /// </summary>
    public void ResetAllUpgrades()
    {
        statManager.RemoveAllModifiersFromSource(this);
        activeUpgradeStacks.Clear();
        appliedModifiers.Clear();
    }

#if UNITY_EDITOR
    [ContextMenu("Debug: Print Active Upgrades")]
    private void DebugPrintActiveUpgrades()
    {
        foreach (var kvp in activeUpgradeStacks)
        {
            Debug.Log($"{kvp.Key.upgradeName}: Stack {kvp.Value}/{kvp.Key.maxStacks}");
        }
    }
#endif
}
