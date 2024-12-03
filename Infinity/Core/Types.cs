using Godot;

namespace fms;

public enum TierType : uint
{
    Common = 1u,
    Uncommon = 2u,
    Rare = 3u,
    Epic = 4u,
    Legendary = 5u
}

public static class TierTypeExtension
{
    public static Color ToColor(this TierType tier)
    {
        return tier switch
        {
            TierType.Common => new Color("#383838"),
            TierType.Uncommon => new Color("#354232"),
            TierType.Rare => new Color("#1e3252"),
            TierType.Epic => new Color("#5f2473"),
            TierType.Legendary => new Color("#9c5027"),
            _ => Colors.Black
        };
    }
}

public enum WeaponStatusType : uint
{
    Level = 1u,
    Damage,
    AttackSpeed,
    Knockback,
    Lifesteal
}