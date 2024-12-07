using System;
using Godot;
using Godot.Collections;
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
    public required Func<Dictionary, BaseProjectile> Next { get; init; }

    public required WhyDead When { get; init; }

    public required float SearchRadius { get; init; }

    public required Dictionary Payload { get; init; }

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
            var dir = (collider.GlobalPosition - parent.GlobalPosition).Normalized();

            // Update payload
            Payload["Direction"] = dir;

            // Trickshot の Iteration を更新する
            if (Payload.TryGetValue("Iter", out var iter))
            {
                Payload["Iter"] = (int)iter + 1;
            }
            else
            {
                Payload["Iter"] = 1;
            }

            Payload["WhyDead"] = (ulong)why;
            Payload["DeadPosition"] = parent.Position;

            // Spawn Next 
            var next = Next(Payload);
            GetParent().GetParent().CallDeferred(Node.MethodName.AddSibling, next);
        }
    }
}