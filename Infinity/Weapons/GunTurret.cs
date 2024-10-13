using fms.Projectile;
using Godot;

namespace fms.Weapon;
/// <summary>
///     GunTurretのクラス
/// </summary>

public partial class GunTurret : BaseProjectile
{
    // タレットが発射する弾
    [Export]
    private PackedScene _projectile = null!;
    
    private protected override void OnDamageEveryXFrames()
    {
        var bodies = GetOverlappingBodies();
        if (bodies.Count == 0)
            return;
        var target = bodies[0];
    }
    // private protected override void SpawnProjectile(uint level)
    // {
    //     var aim = GetNode<AimToNearEnemy>("AimToNearEnemy");
    //     var mazzle = GetNode<Node2D>("AimToNearEnemy/SpriteRoot/MazzlePoint");
    //
    //     if (!aim.IsAiming)
    //     {
    //         return;
    //     }
    //
    //     var prj = _projectile.Instantiate<BaseProjectile>();
    //     var dir = mazzle.GlobalTransform.X;
    //     AddProjectile(prj, mazzle.GlobalPosition, dir);
    // }
}