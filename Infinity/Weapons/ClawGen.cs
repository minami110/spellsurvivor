using System;
using System.Diagnostics;
using fms.Projectile;
using Godot;
using R3;
namespace fms.Weapon;

public partial class ClawGen : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    private uint _enemyHitStacks = 0;
    
    private bool hitStackChaged = false;
    private protected override void SpawnProjectile(uint level)
    {
        var prj = _projectile.Instantiate<BaseProjectile>();
        {
            prj.GlobalPosition = GlobalPosition;
            prj.Direction = GlobalTransform.X;
        }

        prj.Hit
            //.Where(x => x.HitNode is Enemy)
            //.Take(1)
            .Subscribe(hitInfo =>
            {
                _enemyHitStacks++;
                GD.Print("hit");
                hitStackChaged = true;
            });
        
        prj.AddChild(new AutoAim
        {
            Mode = AutoAimMode.JustOnce | AutoAimMode.KillPrjWhenSearchFailed,
            SearchRadius = 100
        });

        // hit stack は最大値がある
        // hit stack は時間経過で0になる
        // hit stack の数だけCDが減少する
        
        
        FrameTimer.AddChild(prj);
    }

    // 一旦決め打ちで
    public override void _Process(double delta)
    {
        base._Process(delta);
        var enemyHitStacks = Math.Min(_enemyHitStacks, 10);
        // いったん決め打ち

        if (!hitStackChaged)
        {
            return;
        }
        
        GD.Print(BaseCoolDownFrame);
        UpdateBaseCoolDownFrame(300 - _enemyHitStacks * 30);

        hitStackChaged = false;
    }
}