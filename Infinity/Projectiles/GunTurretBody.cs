using fms.Projectile;
using Godot;

namespace fms.Weapon;
/// <summary>
///     GunTurretのクラス
///     Weaponからこのクラスを生成し、このクラスから弾を生成する
/// </summary>

public partial class GunTurretBody : AreaProjectile
{ 
    // タレットが発射する弾
    [Export]
    private PackedScene _projectileScene = null!;

    /// <summary>
    /// Projectile のダメージ処理をオーバーライドして新しい弾を生成する
    /// </summary>
    private protected override void Attack()
    {
        var bodies = GetOverlappingBodies();
        if (bodies.Count == 0)
            return;

        // ToDo: 一番近い敵を選択する 
        var target = bodies[0];

        if (target is EnemyBase enemy)
        {
            // タレットが発射する弾を生成
            var prj = _projectileScene.Instantiate<BaseProjectile>();

            // 弾のパラメーターを設定する
            prj.Damage = 12;
            prj.LifeFrame = 15;
            prj.Speed = 800;

            // 一番近い敵に向かっていく
            prj.AddChild(new AutoAim
            {
                Mode = AutoAimMode.JustOnce | AutoAimMode.KillPrjWhenSearchFailed,
                SearchRadius = 1000
            });
            
            // BaseWeapon 系の処理を追加でかく
            prj.Position = GlobalPosition;
            AddSibling(prj);
        }
    }
}