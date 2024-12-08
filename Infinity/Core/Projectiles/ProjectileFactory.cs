using Godot;

namespace fms.Projectile;

public readonly struct BulletProjectileFactory
{
    public required IEntity Instigator { get; init; }
    public required Node Causer { get; init; }
    public required string CauserPath { get; init; }
    public required float Damage { get; init; }
    public required uint Knockback { get; init; }
    public required uint Lifetime { get; init; }

    public Vector2 Position { get; init; }
    public Vector2 ConstantForce { get; init; }
    public BulletProjectile.PenetrateType PenetrateSettings { get; init; }

    public BulletProjectile Create(PackedScene scene)
    {
        var prj = scene.Instantiate<BulletProjectile>();

        prj.Instigator = Instigator;
        prj.Causer = Causer;
        prj.CauserPath = CauserPath;
        prj.Damage = Damage;
        prj.Knockback = Knockback;
        prj.LifeFrame = Lifetime;
        prj.PenetrateSettings = PenetrateSettings;
        prj.ConstantForce = ConstantForce;
        prj.Position = Position;
        return prj;
    }
}