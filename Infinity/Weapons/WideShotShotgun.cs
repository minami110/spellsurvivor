using fms.Projectile;
using Godot;

namespace fms.Weapon;

public partial class WideShotShotgun : WeaponBase
{
    [Export]
    private PackedScene _projectile = null!;

    [Export]
    private uint _numberOfProjectiles = 5;

    [Export]
    private float _spreadAngle = 120f;

    private protected override void SpawnProjectile(uint level)
    {
        var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");
        var mazzle = GetNode<Node2D>("AimToNearEnemy/SpriteRoot/MazzlePoint");

        if (!aim.IsAiming)
        {
            return;
        }

        // プレイヤーの正面から spredAngle の範囲で均等に numberOfProjectiles の数だけ弾を発射する
        for (var i = 0; i < _numberOfProjectiles; i++)
        {
            var prj = _projectile.Instantiate<BaseProjectile>();

            prj.Damage = 12;
            prj.LifeFrame = 120;
            prj.Speed = 500;

            var angle = _spreadAngle * (i - _numberOfProjectiles / 2f) / _numberOfProjectiles;
            var dir = mazzle.GlobalTransform.X.Rotated(Mathf.DegToRad(angle));
            AddProjectile(prj, mazzle.GlobalPosition, dir);
        }
    }
}