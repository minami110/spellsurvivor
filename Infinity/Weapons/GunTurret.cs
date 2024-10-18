using System.Data.SqlTypes;
using fms.Projectile;
using Godot;

namespace fms.Weapon;
/// <summary>
///     GunTurretのクラス
/// </summary>

public partial class GunTurret : BaseProjectile
{
    // クラス名的にProjectileだが、実際はタレットのクラス
    // タレットが発射する弾
    [Export]
    private PackedScene _projectileScene = null!;

    private protected override void OnDamageEveryXFrames()
    {
        var bodies = GetOverlappingBodies();
        if (bodies.Count == 0)
            return;
        var target = bodies[0];

        if (target is Enemy enemy)
        {
            var prj = _projectileScene.Instantiate<BaseProjectile>();
            prj.AddChild(new AutoAim
            {
                Mode = AutoAimMode.JustOnce | AutoAimMode.KillPrjWhenSearchFailed,
                SearchRadius = 1000
            });
            
            // BaseWeapon 系の処理を追加でかく
            prj.Weapon = this.Weapon;
            prj.Position = GlobalPosition;
            AddSibling(prj);
        }
    }
}