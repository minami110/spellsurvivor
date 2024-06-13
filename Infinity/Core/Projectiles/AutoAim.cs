using System;
using Godot;

namespace fms.Projectile;

public partial class AutoAim : Area2D
{
    [Flags]
    public enum ModeType : uint
    {
        Default = 0,
        JustOnce = 1 << 0,
        KillPrjWhenSearchFailed = 1 << 1
    }

    private CollisionShape2D _collision = null!;

    public ModeType Mode { get; init; } = ModeType.Default;
    public required int SearchRadius { get; init; }

    public override void _EnterTree()
    {
        // Collision Layer の設定
        CollisionLayer = 0;
        CollisionMask = 1u << 2; // Node: Mob

        // CircleShape2D を作る
        var shape = new CircleShape2D();
        shape.Radius = SearchRadius;

        // CollisionShape2D を作る
        _collision = new CollisionShape2D();
        _collision.Shape = shape;
        _collision.DebugColor = new Color(0, 1, 0, 0f);

        // 親子関係の構築
        AddChild(_collision);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Mode.HasFlag(ModeType.JustOnce))
        {
            _collision.Disabled = true;
            _collision.Hide();
            SetPhysicsProcess(false);
        }

        // 最も近い敵を検索する
        var distance = 999999f;
        Enemy? nearest = null;
        var bodies = GetOverlappingBodies();
        foreach (var body in bodies)
        {
            if (body is not Enemy enemy)
            {
                continue;
            }

            var d = GlobalPosition.DistanceTo(enemy.GlobalPosition);
            if (d < distance)
            {
                distance = d;
                nearest = enemy;
            }
        }

        if (nearest is not null)
        {
            // Parent (Projectile) の向きを変更する
            var direction = (nearest.GlobalPosition - GlobalPosition).Normalized();
            GetParent<BaseProjectile>().Direction = direction;
        }
        else
        {
            if (Mode.HasFlag(ModeType.KillPrjWhenSearchFailed))
            {
                GetParent<BaseProjectile>().OnDead(WhyDead.Short);
            }
        }
    }
}