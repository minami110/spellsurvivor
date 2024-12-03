using System;
using Godot;
using Godot.Collections;
using R3;

namespace fms.Projectile;

/// <summary>
/// Modifier, 指定された消滅理由の際に次の Projectile を生成する
/// </summary>
public partial class DeathTrigger : Node
{
    public required Func<Dictionary, BaseProjectile> Next { get; init; }

    public required WhyDead When { get; init; }

    public required Dictionary Payload { get; init; }

    public override void _EnterTree()
    {
        var projectile = GetParent<BaseProjectile>();
        projectile.Dead.Take(1).Subscribe(OnProjectileDead).AddTo(this);
    }

    private void OnProjectileDead(WhyDead why)
    {
        // 指定された消滅理由ではない
        if (!When.HasFlag(why))
        {
            return;
        }

        var parent = GetParent<BaseProjectile>();
        var weapon = parent.Weapon;

        // Payload に死亡理由を追加
        Payload["WhyDead"] = (ulong)why;
        Payload["DeadPosition"] = parent.Position;

        var prj = Next(Payload);
        weapon.CallDeferred(WeaponBase.MethodName.AddProjectile, prj);
    }
}