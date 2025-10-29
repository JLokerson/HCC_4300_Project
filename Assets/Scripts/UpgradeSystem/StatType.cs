using UnityEngine;

/// <summary>
/// Defines all possible stat types in the game.
/// Using enums provides type safety and enables efficient Dictionary lookups.
/// </summary>
public enum StatType
{
    // Movement Stats
    MoveSpeed,

    // Combat Stats
    BulletDamage,

    AttackSpeed, //Measured in attacks per second
    CriticalChance,
    BulletPiercing,
    BulletSpeed,

    // Defensive Stats
    MaxHealth,

    // Add more stat types as needed
}