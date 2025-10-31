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
    ReloadSpeed, //Measured in seconds to reload
    MagazineSize, //Measured in number of bullets
    CriticalChance, //Measured in percentage (0 to 1)
    CriticalDamageMultiplier, //Measured as a multiplier (e.g., 2.0 = double damage)
    BulletSpread, //Measured as a percentage (0 to 1)
    BulletPiercing,
    BulletSpeed,

    // Defensive Stats
    MaxHealth,

    // Add more stat types as needed
}