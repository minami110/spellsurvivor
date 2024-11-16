using System;
using fms.Projectile;
using Godot;

namespace fms.Weapon;

public partial class Gyrocopter : WeaponBase
{
    /// <summary>
    /// 攻撃を実行する際の敵の検索範囲
    /// </summary>
    [Export(PropertyHint.Range, "0,9999,1,suffix:px")]
    private float _maxRange = 200f;

    [ExportGroup("Main")]
    [Export]
    private PackedScene _mainProjectile = null!;

    [Export(PropertyHint.Range, "0,9999,1,suffix:px/s")]
    private float _speedMain = 500f;

    [Export(PropertyHint.Range, "0,7200,1,suffix:frames")]
    private uint _lifeMain = 24u;

    [ExportGroup("Sub")]
    [Export]
    private PackedScene _subProjectile = null!;

    [Export(PropertyHint.Range, "0,9999,1,suffix:px/s")]
    private float _speedSub = 300f;

    [Export(PropertyHint.Range, "0,7200,1,suffix:frames")]
    private uint _lifeSub = 39u;

    public override void _Ready()
    {
        GetNode<AimToNearEnemy>("AimToNearEnemy").SearchRadius = _maxRange;
    }

    private protected override void OnCoolDownComplete(uint level)
    {
        var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");

        if (aim is null)
        {
            throw new InvalidProgramException("AimToNearEnemy is not found.");
        }

        var enemies = aim.Enemies;
        if (enemies.Count == 0)
        {
            return;
        }

        // ToDo: 近い / 遠い 適当に実装しています
        for (var i = 0; i < enemies.Count; i++)
        {
            // 0番目の敵 (最も近い敵) にはメインの弾を撃つ
            if (i == 0)
            {
                var prj = _mainProjectile.Instantiate<BulletProjectile>();

                prj.Damage = BaseDamage;
                prj.Knockback = Knockback;
                prj.LifeFrame = _lifeMain;

                var dir = enemies[i].GlobalPosition - GlobalPosition;
                prj.ConstantForce = dir.Normalized() * _speedMain;
                AddProjectile(prj, GlobalPosition);
            }
            // それ以外の敵にはサブの弾を撃つ， レベルに応じて対象に取れる数が増える
            else
            {
                if (i > level)
                {
                    break;
                }

                var prj = _subProjectile.Instantiate<BulletProjectile>();

                prj.Damage = BaseDamage * 0.5f; // メインの半分の威力にする
                prj.Knockback = Knockback;
                prj.LifeFrame = _lifeSub;

                var dir = enemies[i].GlobalPosition - GlobalPosition;
                prj.ConstantForce = dir.Normalized() * _speedSub;
                AddProjectile(prj, GlobalPosition);
            }
        }
    }
}