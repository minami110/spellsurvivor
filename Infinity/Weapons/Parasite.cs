using fms.Projectile;
using Godot;

namespace fms.Weapon;

public partial class Parasite : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    private protected override void OnStartAttack()
    {
        // 開始時に寿命が長い Projectile を生成する
        var prj = _projectile.Instantiate<BaseProjectile>();
        AddProjectile(prj, GlobalPosition);
    }
}