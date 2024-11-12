using fms.Projectile;
using Godot;

namespace fms.Weapon;

public partial class AutoAimPistol : WeaponBase
{
    [ExportGroup("Projectile Settings")]
    [Export]
    private PackedScene _projectile = null!;

    [Export(PropertyHint.Range, "0,9999,1,suffix:px/s")]
    private float _speed = 200f;

    [Export(PropertyHint.Range, "0,7200,1,suffix:frames")]
    private uint _life = 120;

    private protected override void SpawnProjectile(uint level)
    {
        // 範囲内に敵がいない場合は何もしない
        var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");
        if (!aim.IsAiming)
        {
            return;
        }

        var enemy = aim.NearestEnemy!;
        var prj = _projectile.Instantiate<BulletProjectile>();
        {
            prj.Damage = BaseDamage;
            prj.Knockback = Knockback;
            prj.LifeFrame = _life;
            prj.ConstantForce = (enemy.GlobalPosition - GlobalPosition).Normalized() * _speed;
            prj.PenetrateEnemy = false;
            prj.PenetrateWall = false;
        }

        AddProjectile(prj, GlobalPosition);
    }
}