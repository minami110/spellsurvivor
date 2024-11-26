using fms.Projectile;
using Godot;

namespace fms.Weapon;

/// <summary>
/// Weaponからこのクラスを生成し、このクラスから弾を生成する
/// </summary>
public partial class TentacleBody : AreaProjectile
{
    internal Vector2 DamageSize;
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
        // 範囲内の敵を検索する
        var bodies = GetOverlappingBodies();
        if (bodies.Count == 0)
        {
            return;
        }

        // ToDo: 一番近い敵を選択する 
        var target = bodies[0];

        if (target is EntityEnemy enemy)
        {
            // 触手が生成する範囲ダメージ
            var prj = new RectAreaProjectile();
            {
                prj.Damage = Damage; // 自身のものを継承
                prj.Knockback = Knockback; // 自身のものを継承
                prj.LifeFrame = 30u; // 一発しばいたら終わりなので短く適当に
                prj.DamageEveryXFrames = 0u; // 一度ダメージを与えて消滅する設定
                prj.Size = DamageSize;
                prj.Offset = new Vector2(DamageSize.X / 2f, 0f); // 原点に左辺が重なるような Offset を設定
            }

            // 敵の方向を向くような rotation を計算する
            var dir = enemy.GlobalPosition - GlobalPosition;
            var angle = dir.Angle();

            // 自分の位置にダメージエリアを生成する
            var pos = GlobalPosition + dir.Normalized();

            // 自身の兄弟階層に弾を生成する
            prj.Position = pos;
            prj.Rotation = angle;
            AddSibling(prj);
        }
    }
}