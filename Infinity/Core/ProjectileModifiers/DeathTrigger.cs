﻿using System;
using fms.Weapon;
using Godot;
using R3;

namespace fms.Projectile;

/// <summary>
/// Modifier, 死亡時に次の弾を連鎖する
/// </summary>
public partial class DeathTrigger : Node
{
    // ToDo: 生成されるかどうかわからんのに Tree 外のインスタンスもらうのやばい
    public required BaseProjectile Next { get; init; }

    public required WhyDead When { get; init; }

    public override void _EnterTree()
    {
        var projectile = GetParent<BaseProjectile>();
        projectile.Dead.Take(1).Subscribe(OnProjectileDead).AddTo(this);
    }

    private void OnProjectileDead(WhyDead why)
    {
        if (Next.IsInsideTree())
        {
            throw new ApplicationException("すでに次の弾が SceneTree 内に存在しています, この Modifier には Tree 内に存在する弾は渡さないでください");
        }

        if (!IsInstanceValid(Next))
        {
            throw new ApplicationException("次の弾がメモリから消されています");
        }

        // 指定された消滅理由ではない
        if (!When.HasFlag(why))
        {
            Next.CallDeferred(GodotObject.MethodName.Free);
            return;
        }

        var parent = GetParent<BaseProjectile>();
        var weapon = parent.Weapon;
        var hitInfo = parent.HitInfo;

        // Spawn Next 
        Next.Position = hitInfo.Position;

        weapon.CallDeferred(WeaponBase.MethodName.AddProjectile, Next);
    }
}