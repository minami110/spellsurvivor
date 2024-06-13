using System;
using Godot;

namespace fms.Projectile;

[Flags]
public enum AutoAimMode : uint
{
    Default = 0,

    // 最初一回きりの検索
    JustOnce = 1 << 0,

    // 検索に失敗した場合は Projectile を消す
    KillPrjWhenSearchFailed = 1 << 1
}

/// <summary>
///     Projectile の Direction を補正する Mod
///     Note: Godot はスクリプトベースでの CircleCast とかが簡単にできないので, Area2D 生成して.. でやってます
///     開始数フレームは安定しない などの問題が微妙にあります
/// </summary>
public partial class AutoAim : Area2D
{
    private CollisionShape2D _collision = null!;

    private bool _isFirstFrame = true;

    public AutoAimMode Mode { get; init; } = AutoAimMode.Default;
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
        // ToDo: 最初のフレームは判定が安定しないので無視する
        // シェイプキャストみたいなのあればそれがいいよ~~
        if (_isFirstFrame)
        {
            _isFirstFrame = false;
            return;
        }

        if (Mode.HasFlag(AutoAimMode.JustOnce))
        {
            _collision.Disabled = true;
            _collision.Hide();
            SetPhysicsProcess(false);
        }

        // 最も近い敵を検索する
        var distance = 999999f;
        Enemy? nearest = null;
        var bodies = GetOverlappingBodies();

        var parent = GetParent<BaseProjectile>();

        // もし HitInfo に Node2D が入っていたら WhiteList にいれる (同じ敵に反応し続けるのを防止するため)
        var prevHitNode = parent.HitInfo.HitNode;

        foreach (var body in bodies)
        {
            if (body is not Enemy enemy)
            {
                continue;
            }

            if (body == prevHitNode)
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
            parent.Direction = direction;
        }
        else
        {
            if (Mode.HasFlag(AutoAimMode.KillPrjWhenSearchFailed))
            {
                parent.OnDead(WhyDead.Short);
            }
        }
    }
}