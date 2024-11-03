using System;
using fms.Projectile;
using Godot;

namespace fms.Weapon;

public partial class Gyrocopter : WeaponBase
{
    [Export]
    private PackedScene _mainProjectile = null!;

    [Export]
    private PackedScene _subProjectile = null!;

    private protected override void SpawnProjectile(uint level)
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

                prj.Damage = 40;
                prj.LifeFrame = 24;
                prj.Speed = 500;

                var spawnPos = GlobalPosition;
                var direction = enemies[i].GlobalPosition - GlobalPosition;
                AddProjectile(prj, spawnPos, direction);
            }
            // それ以外の敵にはサブの弾を撃つ， レベルに応じて対象に取れる数が増える
            else
            {
                if (i > level)
                {
                    break;
                }

                var prj = _subProjectile.Instantiate<BulletProjectile>();

                prj.Damage = 20;
                prj.LifeFrame = 39;
                prj.Speed = 300;

                var spawnPos = GlobalPosition;
                var direction = enemies[i].GlobalPosition - GlobalPosition;
                AddProjectile(prj, spawnPos, direction);
            }
        }

    }
}