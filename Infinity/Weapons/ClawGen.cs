using System;
using fms.Projectile;
using Godot;
using R3;

namespace fms.Weapon;

public partial class ClawGen : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;


    private uint _enemyHitReduceCounter;
    private uint _enemyHitStacks;

    public override void _PhysicsProcess(double delta)
    {
        if (_enemyHitStacks > 0)
        {
            // スタックの最大数は 5 にする
            _enemyHitStacks = Math.Min(_enemyHitStacks, 5);

            // スタックの数を 1 減らす
            // Note: スタックが貯まるたびに猶予時間が短くなるイメージの式
            _enemyHitReduceCounter++;
            if (_enemyHitStacks > 4)
            {
                if (_enemyHitReduceCounter >= 10)
                {
                    _enemyHitStacks--;
                    _enemyHitReduceCounter = 0;
                }
            }
            else if (_enemyHitStacks > 2)
            {
                if (_enemyHitReduceCounter >= 30)
                {
                    _enemyHitStacks--;
                    _enemyHitReduceCounter = 0;
                }
            }
            else
            {
                if (_enemyHitReduceCounter >= 60)
                {
                    _enemyHitStacks--;
                    _enemyHitReduceCounter = 0;
                }
            }
        }
        else
        {
            _enemyHitReduceCounter = 0;
        }

        var newCoolDown = 60u - _enemyHitStacks * 10u;
        BaseCoolDownFrame = newCoolDown;

        GD.Print(BaseCoolDownFrame);
    }

    private protected override void SpawnProjectile(uint level)
    {
        var prj = _projectile.Instantiate<BaseProjectile>();
        prj.AddChild(new AutoAim
        {
            Mode = AutoAimMode.JustOnce | AutoAimMode.KillPrjWhenSearchFailed,
            SearchRadius = 100
        });

        AddProjectile(prj, GlobalPosition);

        // Note: AddTo はシーンに入れたあとしかできないので
        // Projectile が Enemy にヒットしたら Stack を一つ貯める
        // Prj は貫通するが, 一つの Prj につき 1回だけ Stack を貯められるので Take(1) を入れている
        prj.Hit.Where(x => x.HitNode is Enemy)
            .Take(1)
            .Subscribe(hitInfo =>
            {
                _enemyHitStacks++;
                _enemyHitReduceCounter = 0;
            })
            .AddTo(prj);
    }
}