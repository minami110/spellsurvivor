using Godot;

namespace fms;

public static class Constant
{
    public const uint MINION_MIN_LEVEL = 1;
    public const uint MINION_MAX_LEVEL = 5;
    public const uint MINION_MAX_TIER = 5;

    public const uint SHOP_MAX_LEVEL = 8;
    public const uint SHOP_MAX_ITEM_SLOT = 8;

    public static readonly StringName GroupNamePlayer = new("Player");
    public static readonly StringName GroupNameMinion = new("Minion");
    public static readonly StringName GroupNameFaction = new("Faction");
    public static readonly StringName GroupNameWeapon = new("Weapon");
    public static readonly StringName GroupNameProjectile = new("Projectile");
    public static readonly StringName GroupNameEnemy = new("Enemy");
}