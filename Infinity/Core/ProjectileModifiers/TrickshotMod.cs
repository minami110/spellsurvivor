using System;
using fms.Weapon;
using Godot;
using R3;

namespace fms.Projectile;

/// <summary>
/// 死亡時に次の Projectile を生成する Mod
/// Trickshot で使用される専用の機能が実装されている
/// - 最も近くの敵を検索してそちらに向かう機能
/// - 複製回数を保存している機能
/// </summary>
public partial class TrickshotMod : Node
{
    public required Func<PackedScene, Vector2, Vector2, uint, uint, BaseProjectile> ProjectileSpawnAction { get; init; }

    public required WhyDead When { get; init; }

    public required float SearchRadius { get; init; }

    public required uint Depth { get; init; }

    public required uint MaxDepth { get; init; }

    public required PackedScene Projectile { get; init; }

    public override void _EnterTree()
    {
        var projectile = GetParent<BaseProjectile>();
        projectile.Dead.Take(1).Subscribe(OnProjectileDead).AddTo(this);
    }

    private void OnProjectileDead(WhyDead why)
    {
        // 指定された消滅理由ではないばあいはそのまま消滅する
        if (!When.HasFlag(why))
        {
            return;
        }

        var parent = GetParent<BaseProjectile>();
        var weapon = parent.Weapon;
        var hitInfo = parent.HitInfo;

        // Note: スクリプトベースの CircleCast を行う
        // Circile Cast を使って敵を探す
        var shape = new CircleShape2D();
        shape.Radius = SearchRadius;
        var rid = shape.GetRid();
        var physicsParms = new PhysicsShapeQueryParameters2D
        {
            ShapeRid = rid,
            Transform = parent.Transform, // ToDo: 親の座標をつかう
            CollideWithAreas = false,
            CollideWithBodies = true,
            CollisionMask = Constant.LAYER_MOB
        };
        if (hitInfo.HitNode is CollisionObject2D col)
        {
            var ignoreRid = col.GetRid();
            physicsParms.Exclude = [ignoreRid];
        }

        var spaceState = parent.GetWorld2D().DirectSpaceState;
        var result = spaceState.IntersectShape(physicsParms);

        // ToDo: 使い終わった Shape とか消さなくていい?
        // RefCounted 継承だから勝手に消されるっぽい? しらべる

        if (result.Count > 0)
        {
            // ToDo: 一番近いやつを選ぶ
            var dict = result[0];
            var collider = (Node2D)dict["collider"];
            var vec = (collider.GlobalPosition - parent.GlobalPosition).Normalized();

            // Spawn Next 
            var next = ProjectileSpawnAction(Projectile, hitInfo.Position, vec, Depth + 1, MaxDepth);
            weapon.CallDeferred(WeaponBase.MethodName.AddProjectile, next);
        }
    }
}