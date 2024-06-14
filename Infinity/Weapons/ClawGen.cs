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
        if (_enemyHitStacks == 0)
        {
            return;
        }

        // スタックの最大数は 5 にする
        _enemyHitStacks = Math.Min(_enemyHitStacks, 5);

        // 30 フレームに一回 スタックの数を 1 減らす
        _enemyHitReduceCounter++;
        if (_enemyHitReduceCounter >= 30)
        {
            _enemyHitStacks--;
            _enemyHitReduceCounter = 0;
        }

        if (_enemyHitStacks == 0)
        {
            return;
        }

        var newCoolDown = 60u - _enemyHitStacks * 10u;
        BaseCoolDownFrame = newCoolDown;
    }

    private protected override void SpawnProjectile(uint level)
    {
        var prj = _projectile.Instantiate<BaseProjectile>();
        {
            prj.GlobalPosition = GlobalPosition;
            prj.Direction = GlobalTransform.X;
        }

        prj.AddChild(new AutoAim
        {
            Mode = AutoAimMode.JustOnce | AutoAimMode.KillPrjWhenSearchFailed,
            SearchRadius = 100
        });

        FrameTimer.AddChild(prj);

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