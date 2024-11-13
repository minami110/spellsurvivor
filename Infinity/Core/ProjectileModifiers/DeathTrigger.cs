using System;
using fms.Weapon;
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
        var info = new Dictionary
        {
            { "DeadPosition", parent.Position } // Projectile が消滅した位置
        };

        var prj = Next(info);
        weapon.CallDeferred(WeaponBase.MethodName.AddProjectile, prj);
    }
}