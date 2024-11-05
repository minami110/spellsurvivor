using Godot;

namespace fms;

public static class Constant
{
    public const int LAYER_PLAYER = 1 << 0;
    public const int LAYER_DAMAGE = 1 << 1;
    public const int LAYER_MOB = 1 << 2;
    public const int LAYER_WALL = 1 << 4;

    public const uint MINION_MIN_LEVEL = 1u;
    public const uint MINION_MAX_LEVEL = 5u;
    public const uint MINION_MAX_TIER = 5u;

    public const uint SHOP_MAX_LEVEL = 8u;
    public const uint SHOP_MAX_ITEM_SLOT = 8u;

    public const uint PLAYER_INVENTORY_MAX_SLOT = 8u;

    public const uint FACTION_MAX_LEVEL = PLAYER_INVENTORY_MAX_SLOT;


    public static readonly StringName GroupNamePlayer = new("Player");
    public static readonly StringName GroupNamePlayerState = new("PlayerState");
    public static readonly StringName GroupNameMinion = new("Minion");
    public static readonly StringName GroupNameFaction = new("Faction");
    public static readonly StringName GroupNameWeapon = new("Weapon");
    public static readonly StringName GroupNameProjectile = new("Projectile");
    public static readonly StringName GroupNamePickableItem = new("PickableItem");
    public static readonly StringName GroupNameEnemy = new("Enemy");

}