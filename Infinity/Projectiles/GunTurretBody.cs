using fms.Projectile;
using Godot;

namespace fms.Weapon;

/// <summary>
/// GunTurretのクラス
/// Weaponからこのクラスを生成し、このクラスから弾を生成する
/// </summary>
public partial class GunTurretBody : AreaProjectile
{
    internal uint BulletAliveFrame;
    internal PackedScene BulletPackedScene = null!;
    internal uint BulletSpeed;
    internal uint DetectionRadius;

    public override void _EnterTree()
    {
        // Circle Shape の Radius を設定する
        var col = GetNode<CollisionShape2D>("CollisionShape2D");
        var circle = (CircleShape2D)col.Shape;
        circle.Radius = DetectionRadius;
    }

    /// <summary>
    /// Projectile のダメージ処理をオーバーライドして新しい弾を生成する
    /// </summary>
    private protected override void Attack()
    {
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
            var prj = BulletPackedScene.Instantiate<BaseProjectile>();
            {
                prj.Damage = Damage;
                prj.Knockback = Knockback;
                prj.LifeFrame = BulletAliveFrame;
                prj.ConstantForce = (enemy.GlobalPosition - GlobalPosition).Normalized() * BulletSpeed;
            }

            // 自身の兄弟階層に弾を生成する
            prj.Position = GlobalPosition;
            AddSibling(prj);
        }
    }
}