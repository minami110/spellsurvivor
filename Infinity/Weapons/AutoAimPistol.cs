using fms.Projectile;
using Godot;

namespace fms.Weapon;

public partial class AutoAimPistol : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    private protected override void SpawnProjectile(uint level)
    {
        var prj = _projectile.Instantiate<BaseProjectile>();
        prj.AddChild(new AutoAim
        {
            Mode = AutoAimMode.JustOnce | AutoAimMode.KillPrjWhenSearchFailed,
            SearchRadius = 100
        });

        AddProjectile(prj, GlobalPosition, GlobalTransform.X);
    }
}