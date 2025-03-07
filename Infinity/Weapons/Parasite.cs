﻿using Godot;

namespace fms.Weapon;

public partial class Parasite : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    /// <summary>
    /// Parasite の攻撃間隔
    /// </summary>
    [Export(PropertyHint.Range, "1,1000,1,suffix:frames")]
    private uint _attackSpan = 15u;

    /// <summary>
    /// パラサイトの移動速度
    /// </summary>
    [Export(PropertyHint.Range, "1,1000,1,suffix:px/s")]
    private float _speed = 120f;

    /// <summary>
    /// 移動速度の個体差の ± の幅
    /// </summary>
    [Export(PropertyHint.Range, "0,1000,1,suffix:px/s")]
    private float _randomSpeed = 20f;

    [Export]
    private uint _searchRadius = 150u;

    [Export]
    private uint _damageRadius = 20u;

    /// <summary>
    /// ウェーブ開始時に呼ばれるコールバック
    /// </summary>
    private protected override void OnStartAttack(uint level)
    {
        // レベルに応じて数が増える
        for (var i = 0; i < level; i++)
        {
            var prj = _projectile.Instantiate<ParasiteAreaDamage>();
            prj.Damage = State.Damage.CurrentValue;
            prj.Knockback = State.Knockback.CurrentValue;
            prj.DamageEveryXFrames = _attackSpan;

            var rand = (float)GD.Randfn(0f, _randomSpeed);
            var speed = _speed + rand;
            speed = Mathf.Max(speed, 0f);
            prj.FollowSpeed = speed;
            prj.SearchRadius = _searchRadius;
            prj.DamageRadius = _damageRadius;

            AddProjectile(prj, GlobalPosition);
        }
    }
}