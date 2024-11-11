using fms.Projectile;
using Godot;

namespace fms.Weapon;

/// <summary>
/// Weaponからこのクラスを生成し、このクラスから弾を生成する
/// </summary>
public partial class TentacleBody : AreaProjectile
{
    [Export]
    private PackedScene _projectile = null!;

    /// <summary>
    /// Projectile のダメージ処理をオーバーライドして新しい弾を生成する
    /// </summary>
    private protected override void Attack()
    {
        // 範囲内の敵を検索する
        var bodies = GetOverlappingBodies();
        if (bodies.Count == 0)
        {
            return;
        }

        // ToDo: 一番近い敵を選択する 
        var target = bodies[0];

        if (target is EnemyBase enemy)
        {
            // タレットが発射する弾を生成
            var prj = _projectile.Instantiate<BaseProjectile>();

            // 敵の方向を向くような rotation を計算する
            var dir = enemy.GlobalPosition - GlobalPosition;
            var angle = dir.Angle();

            // 自分の位置から angle 方向に 90 伸ばした位置を計算する
            // Note: プレイ間確かめながらスポーン位置のピクセル数は調整する
            var pos = GlobalPosition + dir.Normalized() * 90;

            // BaseWeapon 系の処理を追加でかく
            prj.Position = pos;
            prj.Rotation = angle;
            AddSibling(prj);
        }
    }
}