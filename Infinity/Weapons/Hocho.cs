﻿using fms.Projectile;
using Godot;

namespace fms.Weapon;

/// <summary>
/// </summary>
public partial class Hocho : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    private protected override void SpawnProjectile(uint level)
    {
        var prj = _projectile.Instantiate<BaseProjectile>();
        {
            prj.GlobalPosition = GlobalPosition;
        }

        switch (level)
        {
            case 2:
                prj.AddChild(new SizeMod { Add = 100 }); // 100 => 200
                break;
            case 3:
                prj.AddChild(new SizeMod { Add = 200 }); // 100 => 300
                break;
        }

        FrameTimer.AddChild(prj);
    }
}