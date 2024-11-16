using Godot;

namespace fms.Weapon;

/// <summary>
/// ガンタレットを生成する
/// </summary>
public partial class GunTurret : WeaponBase
{
    /// <summary>
    /// タレット本体 の PackedScene
    /// </summary>
    [ExportGroup("Body Settings")]
    [Export]
    private PackedScene _body = null!;

    /// <summary>
    /// タレットの生存フレーム
    /// </summary>
    [Export]
    private uint _bodyAliveFrame = 600u;

    /// <summary>
    /// タレットの攻撃間隔
    /// </summary>
    [Export]
    private uint _bodyAttackSpan = 60u;

    /// <summary>
    /// タレット本体が敵を検知する範囲 (px)
    /// </summary>
    [Export]
    private uint _bodyEnemyDetectionRadius = 100u;

    [ExportGroup("Bullet Settings")]
    [Export]
    private PackedScene _bullet = null!;

    [Export]
    private uint _bulletAliveFrame = 15u;

    [Export]
    private uint _bulletSpeed = 800u;

    // Projectileじゃないけど、WeaponBaseの機構にタダ乗りするためにSpawnProjectileメソッドを利用
    private protected override void OnCoolDownComplete(uint level)
    {
        // タレット本体を生成する
        var prj = _body.Instantiate<GunTurretBody>();
        prj.Damage = BaseDamage;
        prj.Knockback = Knockback;
        prj.LifeFrame = _bodyAliveFrame; // タレット本体の生存フレーム
        prj.DamageEveryXFrames = _bodyAttackSpan; // タレット本体が攻撃を行う感覚
        prj.DetectionRadius = _bodyEnemyDetectionRadius;

        prj.BulletPackedScene = _bullet;
        prj.BulletAliveFrame = _bulletAliveFrame;
        prj.BulletSpeed = _bulletSpeed;

        // プレイヤーの位置にスポーンさせる
        AddProjectile(prj, GlobalPosition);
    }
}