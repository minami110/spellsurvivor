using System;
using fms.Projectile;
using Godot;

namespace fms.Mob;

public partial class Turret : Node2D, IEntity
{
    [ExportGroup("Body Settings")]
    [Export]
    public uint Lifetime { get; set; } = 600u;

    [Export]
    public uint AttackSpeed { get; set; } = 60u;

    [Export]
    public uint DetectionRadius { get; set; } = 100u;

    [ExportGroup("Bullet Settings")]
    [Export]
    public PackedScene BulletScene = null!;

    [Export]
    public float Damage { get; set; } = 10f;

    [Export]
    public uint Knockback { get; set; }

    [Export]
    public uint BulletLifetime { get; set; } = 10u;

    [Export]
    public uint BulletSpeed { get; set; } = 10u;

    private uint _age;

    public string CauserPath { get; set; } = null!;

    private AimEntity AimEntity => GetNode<AimEntity>("AimEntity");

    public override void _Ready()
    {
        AimEntity.Mode = AimEntity.TargetMode.NearestEntity;
        AimEntity.MinRange = 0;
        AimEntity.MaxRange = DetectionRadius;
        AimEntity.RotateSensitivity = 0.3f;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_age++ >= Lifetime)
        {
            QueueFree();
        }

        if (_age % AttackSpeed == 0)
        {
            if (AimEntity.IsAiming)
            {
                var enemy = (Node2D)AimEntity.TargetEntity!;
                var factory = new BulletProjectileFactory
                {
                    Instigator = this,
                    Causer = this,
                    CauserPath = CauserPath,
                    Damage = Damage,
                    Knockback = Knockback,
                    Lifetime = BulletLifetime,
                    Position = GlobalPosition,
                    ConstantForce = (enemy.GlobalPosition - GlobalPosition).Normalized() * BulletSpeed
                };
                AddSibling(factory.Create(BulletScene));
            }
        }
    }

    void IGodotNode.AddChild(Node child)
    {
        AddChild(child);
    }

    bool IEntity.IsDead => throw new NotImplementedException();

    EntityState IEntity.State => throw new NotImplementedException();

    public void ApplayDamage(float amount, IEntity instigator, Node causer, string causerPath)
    {
        throw new NotImplementedException();
    }
}