using Godot;

// ReSharper disable InconsistentNaming

namespace fms;

public static class Constant
{
    public const uint LAYER_NONE = 0u;
    public const uint LAYER_PLAYER = 1u << 0;
    public const uint LAYER_DAMAGE = 1u << 1;
    public const uint LAYER_MOB = 1u << 2;
    public const uint LAYER_WALL = 1u << 4;

    public const uint MINION_MIN_LEVEL = 1u;
    public const uint MINION_MAX_LEVEL = 5u;
    public const uint MINION_MAX_TIER = 5u;

    public const uint SHOP_MAX_LEVEL = 8u;
    public const uint SHOP_MAX_ITEM_SLOT = 8u;

    public const uint PLAYER_INVENTORY_MAX_SLOT = 8u;

    public const uint FACTION_MAX_LEVEL = PLAYER_INVENTORY_MAX_SLOT;

    // ToDo: GroupNames に移動する
    public static readonly StringName GroupNameMinion = new("Minion");
    public static readonly StringName GroupNameFaction = new("Faction");
    public static readonly StringName GroupNameWeapon = new("Weapon");
    public static readonly StringName GroupNameProjectile = new("Projectile");
    public static readonly StringName GroupNamePickableItem = new("PickableItem");
    public static readonly StringName GroupNameEnemy = new("Enemy");
}

public static class GroupNames
{
    public static readonly StringName Player = new("Player");
    public static readonly StringName PlayerState = new("PlayerState");
    public static readonly StringName Pawn = new("Pawn");
    public static readonly StringName Effect = new("Effect");
}

public static class EntityAttributeNames
{
    public const string MaxHealth = "MaxHealth";
    public const string MoveSpeed = "MoveSpeed";
    public const string DodgeRate = "DodgeRate";
}

public static class WeaponAttributeNames
{
    public const string DamageRate = "DamageRate";
    public const string SpeedRate = "SpeedRate";

    // 武器固有のやつ
    public const string LifestealAmount = "LifestealAmount";
    public const string LifestealRate = "LifestealRate";
    public const string BounceCount = "BounceCount";
    public const string BounceDamageRate = "BounceDamageRate";
}

public static class FmsColors
{
    public static readonly Color TierCommon = new("#383838");
    public static readonly Color TierUncommon = new("#354232");
    public static readonly Color TierRare = new("#1e3252");
    public static readonly Color TierEpic = new("#5f2473");
    public static readonly Color TierLegendary = new("#9c5027");
}